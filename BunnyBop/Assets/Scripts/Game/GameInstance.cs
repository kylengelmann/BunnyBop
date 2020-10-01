using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CheatSystem;
using UnityEditor;
using UnityEngine;

public class GameInstance : Singleton<GameInstance>
{
    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;

        DontDestroyOnLoad(gameObject);

        CreateCheatSystem();

        CheckStartFromLevel();
    }

    [Conditional("USING_CHEAT_SYSTEM")]
    void CreateCheatSystem()
    {
        gameObject.AddComponent<CheatSystem.CheatSystem>();
    }

    [Conditional("DEBUG_BUILD")]
    void CheckStartFromLevel()
    {
        if (GameMode.bHasInstance) return;

        if (FindObjectOfType<LevelScript>())
        {
            gameObject.AddComponent<GameMode>();
            if (GameMode.bHasInstance)
            {
                GameMode.Instance.InitializeLevel();
            }
        }
    }
}
