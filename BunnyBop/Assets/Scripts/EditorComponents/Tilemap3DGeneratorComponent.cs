﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

[ExecuteInEditMode]
public class Tilemap3DGeneratorComponent : MonoBehaviour
{
    /** Struct containing data about 3D representations of sprites for lighting calculations */
    [Serializable]
    struct Sprite3DData
    {
        /** The sprites this data pertains to */
        public Sprite[] m_Sprites;

        /** The mesh to use to represent these sprite for lighting calculations */
        public Mesh m_Mesh;

        public int m_Height;
    }

    /** Sprite data to auto-generate a 3D representation of the tilemap */
    [SerializeField] Sprite3DData[] m_Sprite3DData;

    [SerializeField] GameObject m_3DObjectPrefab;

    [SerializeField] float m_MeshDistanceFromMap = 100f;

    [SerializeField] Transform m_MeshParent;

    GameObject m_InternalMeshParent;

    public void Generate3DGeometry()
    {
        if (!m_3DObjectPrefab)
        {
            Debug.LogErrorFormat("No 3D object prefab specified for object {0}", gameObject.name);
            return;
        }

        Tilemap tilemap;
        if (!TryGetComponent(out tilemap))
        {
            Debug.LogErrorFormat("No tilemap component on object {0}", gameObject.name);
            return;
        }

        if (m_InternalMeshParent != null)
        {
            DestroyImmediate(m_InternalMeshParent);
        }

        GridAsset LevelGridAsset =
            AssetDatabase.LoadAssetAtPath(Path.Combine(GridAsset.s_GridAssetDirectory,
                gameObject.scene.path + GridAsset.s_GridAssetSuffix), typeof(GridAsset)) as GridAsset;

        if (!LevelGridAsset)
        {
            LevelGridAsset = ScriptableObject.CreateInstance<GridAsset>();
            AssetDatabase.CreateAsset(LevelGridAsset, Path.Combine(GridAsset.s_GridAssetDirectory, gameObject.scene.name + GridAsset.s_GridAssetSuffix));
        }

        if (LevelGridAsset)
        {
            if (LevelGridAsset.GridCells == null)
            {
                LevelGridAsset.GridCells = new List<Grid.GridCellInfo>();
            }
            LevelGridAsset.GridCells.Clear();
        }

        string parentName = String.Concat(gameObject.name, "_3DGeometryParent");
        m_InternalMeshParent = new GameObject(parentName);

        m_InternalMeshParent.transform.position = transform.position + Vector3.back * m_MeshDistanceFromMap;
        m_InternalMeshParent.transform.SetParent(m_MeshParent);
        m_InternalMeshParent.transform.localRotation = Quaternion.identity;

        Matrix4x4 tilemapToMeshCoords = Matrix4x4.identity;
        tilemapToMeshCoords.m00 = .5f;
        tilemapToMeshCoords.m01 = -.5f;

        tilemapToMeshCoords.m11 = 0f;

        tilemapToMeshCoords.m20 = .5f;
        tilemapToMeshCoords.m21 = .5f;
        tilemapToMeshCoords.m23 = .5f;

        foreach (Vector3Int tilePosition in tilemap.cellBounds.allPositionsWithin)
        {
            Sprite tileSprite = tilemap.GetSprite(tilePosition);
            if (tileSprite)
            {
                bool bFoundSprite = false;
                Mesh FoundMesh = null;
                int FoundHeight = 0;
                foreach (Sprite3DData sprite3DData in m_Sprite3DData)
                {
                    foreach (Sprite sprite in sprite3DData.m_Sprites)
                    {
                        if (sprite == tileSprite)
                        {
                            bFoundSprite = true;
                            break;
                        }
                    }

                    if (bFoundSprite)
                    {
                        FoundMesh = sprite3DData.m_Mesh;
                        FoundHeight = sprite3DData.m_Height;
                        break;
                    }
                }

                if (bFoundSprite)
                {
                    Vector3 meshPos = tilemapToMeshCoords * new Vector4(tilePosition.x, tilePosition.y, tilePosition.z, 1f);
                    GameObject MeshGO = Instantiate(m_3DObjectPrefab, m_InternalMeshParent.transform);
                    MeshGO.transform.localPosition = meshPos;

                    MeshFilter meshFilter;
                    if (MeshGO.TryGetComponent(out meshFilter))
                    {
                        meshFilter.mesh = FoundMesh;
                    }

                    if (LevelGridAsset)
                    {
                        LevelGridAsset.GridCells.Add(new Grid.GridCellInfo(new Vector2Int(tilePosition.x, tilePosition.y), FoundHeight));
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }
    }
}
