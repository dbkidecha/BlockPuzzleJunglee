using UnityEngine;
using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System.Collections.Generic;
using UnityEngine.Advertisements;

public class AdmobManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    private BannerView bannerView;
    public InterstitialAd interstitial;
    public RewardedAd rewardedAd;

    private AppOpenAd ad;
    private bool isShowingAd = false;
    private DateTime loadTime;
    public int adsCount = 0;
    public int remoteAdsCount = 1;

    public static Action onRewardedAdClosed, onUserEarnedReward;
    public static AdmobManager instance;

    private string gameId = "4928105";
    private string interstitialAdUnitId_Unity = "Interstitial_Android";
    private string rewardAdUnitId_Unity = "Rewarded_Android";

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Advertisement.Initialize(gameId, false, this);

        // Listen to application foreground and background events.
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;

        // Configure TagForChildDirectedTreatment and test device IDs.
        RequestConfiguration requestConfiguration =
            new RequestConfiguration.Builder().build();

        MobileAds.SetRequestConfiguration(requestConfiguration);
        MobileAds.SetiOSAppPauseOnBackground(true);

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(HandleInitCompleteAction);

        //remoteAdsCount = RemoteConfig.instance.adsOnClick;
    }

    private void HandleInitCompleteAction(InitializationStatus initstatus)
    {
        // Callbacks from GoogleMobileAds are not guaranteed to be called on
        // main thread.
        // In this example we use MobileAdsEventExecutor to schedule these calls on
        // the next Update() loop.
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            if (!CUtils.IsAdsRemoved())
            {
                RequestInterstitial();
                RequestBanner();
                LoadAppOpenAd();
            }
            RequestAndLoadRewardedAd();
        });
    }

    public void RequestBanner()
    {
        // These ad units are configured to always serve test ads.
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = GameConfigManager.Config.admob.androidBanner.Trim();
#elif UNITY_IPHONE
        string adUnitId = GameConfigManager.Config.admob.iosBanner.Trim();
#else
        string adUnitId = "unexpected_platform";
#endif

        // Clean up banner ad before creating a new one.
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

        // Register for ad events.
        bannerView.OnAdLoaded += HandleAdLoaded;
        bannerView.OnAdFailedToLoad += HandleAdFailedToLoad;
        bannerView.OnAdOpening += HandleAdOpened;
        bannerView.OnAdClosed += HandleAdClosed;

        // Load a banner ad.
        bannerView.LoadAd(CreateAdRequest());
    }

    public void RequestInterstitial()
    {
        // These ad units are configured to always serve test ads.
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = GameConfigManager.Config.admob.androidInterstitial.Trim();
#elif UNITY_IPHONE
        string adUnitId = GameConfigManager.Config.admob.iosInterstitial.Trim();
#else
        string adUnitId = "unexpected_platform";
#endif

        // Clean up interstitial ad before creating a new one.
        if (interstitial != null)
        {
            interstitial.Destroy();
        }

        // Create an interstitial.
        interstitial = new InterstitialAd(adUnitId);

        // Register for ad events.
        interstitial.OnAdLoaded += HandleInterstitialLoaded;
        interstitial.OnAdFailedToLoad += HandleInterstitialFailedToLoad;
        interstitial.OnAdOpening += HandleInterstitialOpened;
        interstitial.OnAdClosed += HandleInterstitialClosed;

        // Load an interstitial ad.
        interstitial.LoadAd(CreateAdRequest());
    }

    public void RequestAndLoadRewardedAd()
    {
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = GameConfigManager.Config.admob.androidRewarded.Trim();
#elif UNITY_IPHONE
        string adUnitId = GameConfigManager.Config.admob.iosRewarded.Trim();
#else
        string adUnitId = "unexpected_platform";
#endif

        // Create new rewarded ad instance.
        rewardedAd = new RewardedAd(adUnitId);

        // Called when an ad request has successfully loaded.
        rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        // Create an empty ad request.
        AdRequest request = CreateAdRequest();
        // Load the rewarded ad with the request.
        rewardedAd.LoadAd(request);
    }

    private void LoadAppOpenAd()
    {
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = GameConfigManager.Config.admob.androidAppOpen.Trim();
#elif UNITY_IPHONE
        string adUnitId = GameConfigManager.Config.admob.iosAppOpen.Trim();
#else
        string adUnitId = "unexpected_platform";
#endif
        AdRequest request = new AdRequest.Builder().Build();

        // Load an app open ad for portrait orientation
        AppOpenAd.LoadAd(adUnitId, ScreenOrientation.Portrait, request, ((appOpenAd, error) =>
        {
            if (error != null)
            {
                // Handle the error.
                Debug.LogFormat("Failed to load the ad. (reason: {0})", error.LoadAdError.GetMessage());
                return;
            }

            // App open ad is loaded.
            ad = appOpenAd;
            loadTime = DateTime.UtcNow;
        }));
    }

    private bool IsAdAvailable
    {
        get
        {
            return ad != null && (System.DateTime.UtcNow - loadTime).TotalHours < 4;
        }
    }

    public void ShowAdIfAvailable()
    {
        if (!IsAdAvailable || isShowingAd)
        {
            return;
        }

        ad.OnAdDidDismissFullScreenContent += HandleAdDidDismissFullScreenContent;
        ad.OnAdFailedToPresentFullScreenContent += HandleAdFailedToPresentFullScreenContent;
        ad.OnAdDidPresentFullScreenContent += HandleAdDidPresentFullScreenContent;
        ad.OnAdDidRecordImpression += HandleAdDidRecordImpression;
        ad.OnPaidEvent += HandlePaidEvent;

        ad.Show();
    }

    // Returns an ad request with custom ad targeting.
    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
                .AddKeyword("game")
                .Build();
    }

    public void ShowInterstitial(InterstitialAd ad)
    {
        if (ad != null && ad.IsLoaded())
        {
            ad.Show();
        }
    }

    public void ShowBanner()
    {
        if (bannerView != null) bannerView.Show();
        else RequestBanner();
    }

    public void DestroyBanner()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }
    }

    public bool ShowInterstitial()
    {
        if (interstitial == null)
            return false;

        if (interstitial.IsLoaded())
        {
            interstitial.Show();
            return true;
        }
        else {
            RequestInterstitial();
            ShowUnityInterstitialAd();
        }
        return false;
    }

    public bool ShowRewardedAd()
    {
        if (rewardedAd.IsLoaded())
        {
            rewardedAd.Show();
            return true;
        }
        else
        {
            Debug.Log("Rewarded ad is not ready yet");
            RequestAndLoadRewardedAd();
            return ShowUnityRewardedVideo();
        }
    }

    private void OnAppStateChanged(AppState state)
    {
        // Display the app open ad when the app is foregrounded.
        //Debug.Log("======= App State is " + state);
        if (state == AppState.Foreground)
        {
            ShowAdIfAvailable();
        }
    }

    #region Banner callback handlers

    public void HandleAdLoaded(object sender, EventArgs args)
    {
        print("HandleAdLoaded event received.");
    }

    public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        print("HandleFailedToReceiveAd event received with message: " + args.LoadAdError);
    }

    public void HandleAdOpened(object sender, EventArgs args)
    {
        print("HandleAdOpened event received");
    }

    public void HandleAdClosed(object sender, EventArgs args)
    {
        print("HandleAdClosed event received");
    }

    public void HandleAdLeftApplication(object sender, EventArgs args)
    {
        print("HandleAdLeftApplication event received");
    }

    #endregion

    #region Interstitial callback handlers

    public void HandleInterstitialLoaded(object sender, EventArgs args)
    {
        print("HandleInterstitialLoaded event received.");
    }

    public void HandleInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        print("HandleInterstitialFailedToLoad event received with message: " + args.LoadAdError);
    }

    public void HandleInterstitialOpened(object sender, EventArgs args)
    {
        print("HandleInterstitialOpened event received");
    }

    public void HandleInterstitialClosed(object sender, EventArgs args)
    {
        print("HandleInterstitialClosed event received");
        RequestInterstitial();
    }

    public void HandleInterstitialLeftApplication(object sender, EventArgs args)
    {
        print("HandleInterstitialLeftApplication event received");
    }

    #endregion

    #region RewardedAd callback handlers

    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        print("HandleRewardedAdLoaded event received");
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        print("HandleRewardedAdFailedToLoad event received with message: " + args.LoadAdError);
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        print("HandleRewardedAdOpening event received");
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        print("HandleRewardedAdFailedToShow event received with message: " + args.AdError);
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        print("HandleRewardedAdClosed event received");

        onRewardedAdClosed?.Invoke();
        RequestAndLoadRewardedAd();
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        print("HandleRewardedAdRewarded event received for " + amount.ToString() + " " + type);

        onUserEarnedReward?.Invoke();
    }

    #endregion

    #region AppOpen Ad callback handlers
    private void HandleAdDidDismissFullScreenContent(object sender, EventArgs args)
    {
        Debug.Log("Closed app open ad");
        // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
        ad = null;
        isShowingAd = false;
        LoadAppOpenAd();
    }

    private void HandleAdFailedToPresentFullScreenContent(object sender, AdErrorEventArgs args)
    {
        Debug.LogFormat("Failed to present the ad (reason: {0})", args.AdError.GetMessage());
        // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
        ad = null;
        LoadAppOpenAd();
    }

    private void HandleAdDidPresentFullScreenContent(object sender, EventArgs args)
    {
        Debug.Log("Displayed app open ad");
        isShowingAd = true;
    }

    private void HandleAdDidRecordImpression(object sender, EventArgs args)
    {
        Debug.Log("Recorded ad impression");
    }

    private void HandlePaidEvent(object sender, AdValueEventArgs args)
    {
        Debug.LogFormat("Received paid event. (currency: {0}, value: {1}",
                args.AdValue.CurrencyCode, args.AdValue.Value);
    }

    #endregion

    public void OnInitializationComplete()
    {
        Debug.Log("=== OnInitializationComplete");

        LoadUnityInterstitialAd();
        LoadUnityRewardedAds();
    }

    public void LoadUnityInterstitialAd()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        Advertisement.Load(interstitialAdUnitId_Unity, this);
    }

    // Show the loaded content in the Ad Unit: 
    public void ShowUnityInterstitialAd()
    {
        // Note that if the ad content wasn't previously loaded, this method will fail
        Advertisement.Show(interstitialAdUnitId_Unity, this);
    }

    public void LoadUnityRewardedAds()
    {
        Advertisement.Load(rewardAdUnitId_Unity, this);
    }

    public bool ShowUnityRewardedVideo()
    {        
        Advertisement.Show(rewardAdUnitId_Unity, this);
        return true;
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("OnUnityAdsAdLoaded : " + placementId);
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log("OnUnityAdsFailedToLoad : " + placementId);
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log("OnUnityAdsShowFailure : " + placementId);
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log("OnUnityAdsShowStart : " + placementId);
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("OnUnityAdsShowClick : " + placementId);
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log("OnUnityAdsShowComplete : " + placementId);

        if (placementId.Equals(interstitialAdUnitId_Unity))
        {
            LoadUnityInterstitialAd();
        }
        else if (placementId.Equals(rewardAdUnitId_Unity))
        {
            onUserEarnedReward?.Invoke();
            LoadUnityRewardedAds();
        }
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log("OnInitializationFailed : " + message);
    }
}