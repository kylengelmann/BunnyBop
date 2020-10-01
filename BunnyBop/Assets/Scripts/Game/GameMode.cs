using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GameMode : Singleton<GameMode>
{
    public LevelScript CurrentLevelScript { get; private set; }
    public bool bHasCurrentLevelScript { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;
    }

    void SetupGridForCurrentLevel()
    {
        if(!bHasCurrentLevelScript) return;

        GridAsset LevelGridAsset = AssetDatabase.LoadAssetAtPath(Path.Combine(GridAsset.s_GridAssetDirectory, CurrentLevelScript.gameObject.scene.path + GridAsset.s_GridAssetSuffix), typeof(GridAsset)) as GridAsset;
        if (LevelGridAsset)
        {
            Grid.CreateCurrentGrid(LevelGridAsset);
        }
    }

    public void InitializeLevel()
    {
        
        CurrentLevelScript = FindObjectOfType<LevelScript>();
        bHasCurrentLevelScript = CurrentLevelScript;

        if (bHasCurrentLevelScript)
        {
            SetupGridForCurrentLevel();
        }
    }
}
