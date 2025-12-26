using UnityEngine;
using UnityEditor;

public class ArtboxGamesWindowEditor
{
    [MenuItem("ArtboxGames/Clear all playerprefs")]
    static void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    [MenuItem("ArtboxGames/Unlock all levels")]
    static void UnlockAllLevel()
    {
        CPlayerPrefs.useRijndael(CommonConst.ENCRYPTION_PREFS);

        for (int i = 1; i <= Const.WORLD_NAME.Length; i++)
        {
            LevelManager.SetUnlockLevel(i, Const.MAX_LEVEL + 1);
        }
        LevelManager.SetCurrentLevel(251);
    }

    [MenuItem("ArtboxGames/Credit balance (ruby, hint..)")]
    static void AddRuby()
    {
        CPlayerPrefs.useRijndael(CommonConst.ENCRYPTION_PREFS);
        GameState.hint.ChangeValue(1000);
    }

    [MenuItem("ArtboxGames/Set balance to 0")]
    static void SetBalanceZero()
    {
        CPlayerPrefs.useRijndael(CommonConst.ENCRYPTION_PREFS);
        CurrencyManager.SetBalance(0);
    }
}