using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using ArtboxGames;

public class LevelCompleteDialog : Dialog
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image fillAmount;

    protected override void Start()
    {
        messageText.text = "Level " + GameState.chosenLevel + " Completed";
        ShowLevelProgress();
        LevelManager.SetCurrentLevel(GameState.chosenLevel + 1);
        Leadersboard.instance?.ReportHighScore(GameState.chosenLevel);

        if (AdmobManager.instance.adsCount >= AdmobManager.instance.remoteAdsCount)
        {
            AdmobManager.instance.adsCount = 0;
            AdmobManager.instance.ShowInterstitial();
        }
        else
            AdmobManager.instance.adsCount++;
    }

    private void ShowLevelProgress()
    {
        if (GameState.chosenLevel % 10 == 0)
        {
            fillAmount.fillAmount = 1f;
            Invoke(nameof(ProcessLevelGift), 0.2f);
        }
        else
        {
            string level = ((float)GameState.chosenLevel / 10f).ToString().Split('.')[1];
            fillAmount.fillAmount = (float.Parse(level) / 10f);
        }
    }

    private void ProcessLevelGift()
    {
        bool received = Utils.IsGiftReceived(GameState.chosenWorld, GameState.chosenLevel);
        if (!received)
        {
            Utils.ReceiveGift(GameState.chosenWorld, GameState.chosenLevel);
            Timer.Schedule(this, 0.5f, () =>
            {
                DialogController.instance.ShowDialog(DialogType.LevelGift, DialogShow.OVER_CURRENT);
            });
        }
        else
        {
            fillAmount.transform.parent.gameObject.SetActive(false);
        }
    }

    public void OnReplayClick()
    {
        Close();
        Sound.instance.PlayButton();
        MainController.instance.Replay();
    }

    public void OnNextClick()
    {
        Close();
        Sound.instance.PlayButton();
        //if (GameState.chosenLevel == Const.MAX_LEVEL)
        //{
        //    CUtils.LoadScene(1, true);
        //}
        //else
        //{
        //GameState.chosenLevel++;
        CUtils.LoadScene(2, true);
        //}
    }
}