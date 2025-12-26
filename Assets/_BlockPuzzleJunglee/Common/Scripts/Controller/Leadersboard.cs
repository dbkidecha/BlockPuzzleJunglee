using UnityEngine;
using GooglePlayGames.BasicApi;
using GooglePlayGames;

public class Leadersboard : MonoBehaviour
{
	public static Leadersboard instance;

	// Use this for initialization
	void Start()
	{
		instance = this;
		InitAndSignin();
	}

	void InitAndSignin()
	{
		PlayGamesPlatform.DebugLogEnabled = true;
		PlayGamesPlatform.Activate();
		PlayGamesPlatform.Instance.Authenticate(OnSignInResult);
	}

	private void OnSignInResult(SignInStatus signInStatus)
	{
		if (signInStatus == SignInStatus.Success)
		{
			Debug.Log("=== GPG Authenticated. Hello, " + Social.localUser.userName + " (" + Social.localUser.id + ")");
		}
		else
		{
			Debug.Log("*** GPG Failed to authenticate with " + signInStatus);
		}
	}

	public void ReportHighScore(long time)
	{
		Social.ReportScore(time, "CgkI2uKVldYNEAIQAA", (bool success) =>
		{
			// handle success or failure
			if (success)
			{
				Debug.Log("==== Score reporting succes");
			}
			else
			{
				Debug.Log("==== Score reporting failed");
			}
		});
	}

	public void ShowLeadersboard()
	{
		if (Social.localUser.authenticated)
		{
			Social.ShowLeaderboardUI();
		}
		else
		{
			InitAndSignin();
		}
	}
}