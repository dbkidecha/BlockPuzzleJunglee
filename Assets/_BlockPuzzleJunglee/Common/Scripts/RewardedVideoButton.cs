using UnityEngine;

public class RewardedVideoButton : MonoBehaviour
{
    public void OnClick(bool isx2)
    {
        GameConfigManager.Config.rewardedVideoAmount = isx2 ? 2 : 1;
#if UNITY_EDITOR
        OnUserEarnedReward();
#else        
            AdmobManager.onUserEarnedReward = OnUserEarnedReward;
            AdmobManager.instance.ShowRewardedAd();        
#endif
        Sound.instance.PlayButton();
    }

    public void OnUserEarnedReward()
    {
        int amount = GameConfigManager.Config.rewardedVideoAmount;
        GameState.hint.ChangeValue(amount);

        string unit = amount == 1 ? " hint" : " hints";
        Toast.instance.ShowMessage("You've received " + amount + unit, 3);

        if (amount >= 2)
        {
            DialogController.instance.CloseDialog(DialogType.LevelGift);
        }
    }

    private bool IsAdAvailable()
    {
        return AdmobManager.instance.rewardedAd.IsLoaded();
    }
}
