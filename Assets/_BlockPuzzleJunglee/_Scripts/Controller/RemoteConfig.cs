using Newtonsoft.Json;
using OneSignalSDK;
using UnityEngine;
using TMPro;
using Unity.RemoteConfig;
using System.Collections.Generic;

public class RemoteConfig : MonoBehaviour
{
    public static RemoteConfig instance;

    [SerializeField] private GameObject appUpdate;
    [SerializeField] private GameObject appMaintenance;
    [SerializeField] private TextMeshProUGUI message;

    public bool adsEnable = true;
    public int adsOnClick = 6;
    public struct userAttributes { }
    public struct appAttributes { }

    private string environmentName = "production";
    private string environmentId = "748b08fa-7a6d-41e9-a37b-08f19e65eb1c";

#if UNITY_DEVELOPMENT_BUILD
        private string environmentName = "development";
        private string environmentId = "bf0aad2a-c8ce-4625-bf25-4a63050781f2";
#endif

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        OneSignal.Default.Initialize("6455eada-3865-47e1-a360-391d95c332e7");
                
        ConfigManager.FetchCompleted += SetRemoteData;
        ConfigManager.SetEnvironmentID(environmentId);
        ConfigManager.FetchConfigs(new userAttributes(), new appAttributes());
    }

    private void SetRemoteData(ConfigResponse response)
    {
        switch (response.requestOrigin)
        {
            case ConfigOrigin.Default:
                Debug.Log("Default values will be returned");
                break;
            case ConfigOrigin.Cached:
                Debug.Log("Cached values loaded");
                break;
            case ConfigOrigin.Remote:
                Debug.Log("Remote Values changed");
                //Debug.Log("===== RemoteConfigService.Instance.appConfig fetched: " + ConfigManager.appConfig.config.ToString());

                adsEnable = ConfigManager.appConfig.GetBool("adsEnable");
                adsOnClick = ConfigManager.appConfig.GetInt("adsOnClick");
                float appVersion = ConfigManager.appConfig.GetFloat("appVersion");
                string jsonString = ConfigManager.appConfig.GetJson("appMaintenance");

                Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

                if (data["status"].ToString().Equals("true"))
                {
                    message.text = data["message"].ToString();
                    appMaintenance.SetActive(true);
                    Invoke(nameof(PauseApp), 0.1f);
                }
                else if (appVersion > float.Parse(Application.version))
                {
                    appUpdate.SetActive(true);
                    Invoke(nameof(PauseApp), 0.1f);
                }
                break;
        }
    }

    private void PauseApp()
    {
        Time.timeScale = 0f;
    }

    public void UpdateNow()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
        CloseApp();
    }

    public void NotNow()
    {
        appUpdate.SetActive(false);
        Time.timeScale = 1f;
    }

    public void MoreGame()
    {
        Application.OpenURL("https://artboxinfotech.app.link/artbox-games");
        CloseApp();
    }

    public void CloseApp()
    {
        Application.Quit();
    }
}