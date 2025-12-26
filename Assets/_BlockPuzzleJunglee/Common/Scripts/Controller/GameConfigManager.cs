using UnityEngine;
using System.Collections;

public class GameConfigManager : MonoBehaviour {
    public GameConfig config;

    public static GameConfig Config
    {
        get { return instance.config; }
    }

    public static GameConfigManager instance;

    private void Awake()
    {
        instance = this;
    }
}
