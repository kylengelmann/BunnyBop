using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[assembly:CheatSystem.CheatClass(typeof(GameMode))]

public class GameMode : Singleton<GameMode>
{
    public LevelScript CurrentLevelScript { get; private set; }
    public bool bHasCurrentLevelScript { get; private set; }

    public Camera MainCamera { get; private set; }
    public bool bHasMainCamera { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;

        StartCoroutine(WaitForMainCamera());
    }

    IEnumerator WaitForMainCamera()
    {
        while (!bHasMainCamera)
        {
            MainCamera = Camera.main;
            bHasMainCamera = MainCamera;
            yield return null;
        }
    }

    void SetupGridForCurrentLevel()
    {
        if(!bHasCurrentLevelScript) return;

        GridAsset LevelGridAsset = AssetDatabase.LoadAssetAtPath(Path.Combine(Path.GetDirectoryName(CurrentLevelScript.gameObject.scene.path), CurrentLevelScript.gameObject.scene.name + GridAsset.s_GridAssetSuffix), typeof(GridAsset)) as GridAsset;
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

        InitializePlayer();

        if (GameplayQueue.bHasInstance)
        {
            GameplayQueue.Instance.StartProcessingTasks();
        }
    }

    public void InitializePlayer()
    {
        GameObject PlayerControllerGO = new GameObject("PlayerController");
        PlayerControllerGO.AddComponent<PlayerController>();
    }
}
