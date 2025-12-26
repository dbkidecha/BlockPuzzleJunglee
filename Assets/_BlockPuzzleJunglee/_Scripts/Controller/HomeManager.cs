using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HomeManager : BaseController
{
    public void OnClick()
    {
        //new NativeShare().SetSubject("Share").SetText("I realy enjoy this game download and play Block Puzzle Junglee!").SetUrl("https://artboxinfotech.app.link/block-puzzle-junglee")
        //.SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
        //.Share();
        Leadersboard.instance?.ShowLeadersboard();
        Sound.instance.PlayButton();
    }
}