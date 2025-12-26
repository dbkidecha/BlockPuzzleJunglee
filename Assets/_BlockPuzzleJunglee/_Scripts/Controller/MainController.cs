using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ArtboxGames;
using TMPro;

public class MainController : BaseController {
    public TileRegion tileRegion;
    public int level = 1;
    public int world = 1;
    public LevelPrefs levelPrefs;
    public TextMeshProUGUI levelText;

    [HideInInspector]
    public GameLevel gameLevel;
    public static System.Action<int> tutorialSteps;

    [SerializeField] private GameObject[] tutorialObj;
    [SerializeField] private GameObject handObj;

    public static MainController instance;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
    }

    protected override void Start()
    {
        base.Start();

        //gameLevel = Resources.Load<GameLevel>("Levels/World_" + world + "/Level_" + level);
        //level = GameState.chosenLevel;
        //world = GameState.chosenWorld;
        //gameLevel = Resources.Load<GameLevel>("Levels/World_" + world + "/Level_" + level);

        int currLevel = LevelManager.GetCurrentLevel();
        if (currLevel > 250)
        {
            int level = Random.Range(11, 251);
            gameLevel = Resources.Load<GameLevel>("Levels/Level_" + level);
        }
        else
        {
            gameLevel = Resources.Load<GameLevel>("Levels/Level_" + currLevel);
        }
        GameState.chosenLevel = currLevel;

        //string strLevelPrefs = Utils.GetLevelData(world, level);
        //if (string.IsNullOrEmpty(strLevelPrefs))
        //{
        //    levelPrefs = new LevelPrefs();
        //    levelPrefs.piecesPrefs = new List<PiecePrefs>();
        //}
        //else
        //{
        //    levelPrefs = JsonUtility.FromJson<LevelPrefs>(strLevelPrefs);
        //}

        tileRegion.Load(gameLevel);
        GameState.canPlay = true;
        levelText.text = "Level " + currLevel.ToString();

        //ProcessLevelGift();

        Utils.IncreaseNumMoves(world, level);

        if (currLevel.Equals(1))
        {
            tutorialObj[0].SetActive(true);
            tutorialObj[1].SetActive(true);

            tutorialSteps = ShowTutorial;
        }
    }

    private void ShowTutorial(int step)
    {
        if (step.Equals(0))
        {
            tutorialObj[1].SetActive(true);
        }
        else if (step.Equals(1))
        {
            tutorialObj[1].SetActive(false);
        }
        else if (step.Equals(2))
        {
            tutorialObj[0].SetActive(false);
            tutorialObj[1].SetActive(false);

            tutorialSteps = null;
        }
    }

    public void Replay()
    {
        GameState.canPlay = true;

        foreach (var piece in tileRegion.pieces)
        {
            piece.MoveToBottom();
        }

        Sound.instance.Play(Sound.Others.Replay);
    }

    public void ShowHint()
    {
        if (GameState.hint.GetValue() <= 0)
        {
            Toast.instance.ShowMessage("Watch video ads to get FREE hint", 2f);
            handObj.SetActive(true);
            Invoke(nameof(DisableHand), 3f);
            return;
        }
        bool isShown = tileRegion.ShowHint();
        if (isShown) AddHint(-1);
    }

    public void AddHint(int num)
    {
        GameState.hint.ChangeValue(num);
    }

    public void ProcessLevelGift()
    {
        if (level % 4 == 3)
        {
            bool received = Utils.IsGiftReceived(world, level);
            if (!received)
            {
                Utils.ReceiveGift(world, level);
                Timer.Schedule(this, 0.5f, () =>
                {
                    DialogController.instance.ShowDialog(DialogType.LevelGift);
                });
            }
        }
    }

    public void OnComplete(int numTile)
    {
        GameState.canPlay = false;
        SavePrefs();

        int unlockedLevel = LevelManager.GetUnlockLevel(world);
        if (level == unlockedLevel)
        {
            LevelManager.SetUnlockLevel(world, unlockedLevel + 1);
        }

        Timer.Schedule(this, numTile * 0.03f + 0.7f, () =>
        {
            DialogController.instance.ShowDialog(DialogType.Complete);
        });

        Sound.instance.Play(Sound.Others.Complete);
    }

    private void SavePrefs()
    {
        string data = JsonUtility.ToJson(levelPrefs);
        Utils.SetLevelData(world, level, data);
    }

    private void DisableHand()
    {
        handObj.SetActive(false);
    }
}
