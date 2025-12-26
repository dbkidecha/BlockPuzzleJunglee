using UnityEngine;
using System.Collections;

public class SceneController : MonoBehaviour
{
	private void Awake()
	{
		Application.targetFrameRate = 60;
        CPlayerPrefs.useRijndael(CommonConst.ENCRYPTION_PREFS);

		if (!CUtils.IsGameInitialzied())
		{
			CUtils.SetInitGame();
		}
	}

	private void Update()
    {
#if !UNITY_WSA
        if (Input.GetKeyDown(KeyCode.Escape) && !DialogController.instance.IsDialogShowing())
		{
			QuitGame.instance.ShowConfirmDialog();
        }
#endif
    }
}
