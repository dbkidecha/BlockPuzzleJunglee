using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonMoreGames : MyButton {

    public override void OnButtonClick()
    {
        base.OnButtonClick();
#if UNITY_ANDROID
        Application.OpenURL("https://artboxinfotech.app.link/artbox-games");
#elif UNITY_IPHONE
        Application.OpenURL("https://apps.apple.com/us/developer/dixit-rathod/id1533112999");
#endif
    }
}