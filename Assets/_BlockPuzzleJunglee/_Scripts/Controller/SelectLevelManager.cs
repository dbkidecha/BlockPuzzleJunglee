using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SelectLevelManager : BaseController {
    public TextMeshProUGUI worldText;
    public SnapScrollRect scroll;
     
    protected override void Start()
    {
        base.Start();
        worldText.text = Const.WORLD_NAME[GameState.chosenWorld - 1].ToString();
        scroll.SetPage((LevelManager.GetUnlockLevel(GameState.chosenWorld) - 1) / 20);
        CUtils.ShowInterstitialAd();
    }
}
