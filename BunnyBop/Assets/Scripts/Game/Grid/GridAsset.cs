using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridAsset : ScriptableObject
{
    public static readonly string s_GridAssetDirectory = "Assets/Resources/GridAssets/";
    public static readonly string s_GridAssetSuffix = "_Grid.asset";

    public List<Grid.GridCellInfo> GridCells;
}
