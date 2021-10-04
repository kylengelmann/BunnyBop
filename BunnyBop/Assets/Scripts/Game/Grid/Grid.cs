using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Net.Configuration;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Grid
{
    static Matrix4x4 TileToLocal3DPos = new Matrix4x4(new Vector4(.5f, -.5f, 0f, 0f), 
                                                      new Vector4(0f, 0f, 0f, 0f), 
                                                      new Vector4(.5f, .5f, 0f, .5f), 
                                                      new Vector4(0f, 0f, 0f, 1f)).transpose;
    

    static Matrix4x4 Local3DToTilePos = new Matrix4x4(new Vector4(1f, 0f, 1f, -.5f),
                                                      new Vector4(-1f, 0f, 1f, -.5f), 
                                                      new Vector4(0f, 0f, 0f, 0f), 
                                                      new Vector4(0f, 0f, 0f, 1f)).transpose;


    static Matrix4x4 WorldToTilePos = new Matrix4x4(new Vector4(1f, 2f, 0f, -.5f),
                                                    new Vector4(-1f, 2f, 0f, -.5f), 
                                                    new Vector4(0f, 0f, 0f, 0f), 
                                                    new Vector4(0f, 0f, 0f, 1f)).transpose;

    static Matrix4x4 WorldToLocalPos = TileToLocal3DPos * WorldToTilePos;

    public static Vector2Int TransformWorldToTile(Vector3 WorldPos)
    {
        Vector3 TilePos = WorldToTilePos.MultiplyPoint3x4(WorldPos);
        return new Vector2Int(Mathf.RoundToInt(TilePos.x), Mathf.RoundToInt(TilePos.y));
    }

    public static Vector2Int TransformLocal3DToTile(Vector3 LocalPos)
    {
        Vector3 TilePos = Local3DToTilePos.MultiplyPoint3x4(LocalPos);
        return new Vector2Int(Mathf.RoundToInt(TilePos.x), Mathf.RoundToInt(TilePos.y));
    }

    public static Vector3 TransformTileToLocal3D(Vector2Int TilePos)
    {
        Vector3 TilePos3D = new Vector3(TilePos.x, TilePos.y, 0f);
        return TileToLocal3DPos.MultiplyPoint3x4(TilePos3D);
    }

    public static Grid CurrentGrid { get; private set; }
    public static bool bHasCurrentGrid { get; private set; }

    [Serializable]
    public struct GridCellInfo
    {
        public Vector2Int Position;

        public int Height;

        private Dictionary<int, GridObject> OccupyingObjects;

        // Layer mask of occupied layers. Used to quickly check whether any of multiple layers are occupied
        private int OccupiedLayersMask;

        // Set the object occupying this cell in the given layer
        public void SetObject(int ObjectLayer, GridObject Object)
        {
            // Check that the given layer is valid
            if (ObjectLayer >= 32 || ObjectLayer < 0)
            {
                Debug.LogErrorFormat("GridCellInfo::SetOccupyingObject: [{0}] Layer {1} is invalid", Position.ToString(), ObjectLayer);
                return;
            }

            // If the passed in object is null, remove this layer's entry from the dictionary and update the occupied layers bitmask
            if (Object == null)
            {
                OccupyingObjects.Remove(ObjectLayer);
                OccupiedLayersMask &= ~(1 << ObjectLayer);
                return;
            }

            // Check if this layer is in the dictionary
            if (OccupyingObjects.TryGetValue(ObjectLayer, out GridObject CurrentObject))
            {
                if (CurrentObject != null)
                {
                    Debug.AssertFormat(IsOccupied(1 << ObjectLayer), "GridCellInfo::SetOccupyingObject: [{0}] Layer {1} has an object but the mask say otherwise", Position.ToString(), ObjectLayer);

                    Debug.LogErrorFormat("GridCellInfo::SetOccupyingObject: [{0}] Layer {1} is already occupied by object {2}", Position.ToString(), ObjectLayer, CurrentObject.name);
                }
                else
                {
                    Debug.AssertFormat(!IsOccupied(1 << ObjectLayer), "GridCellInfo::SetOccupyingObject: [{0}] Layer {1} has no object but the mask say otherwise", Position.ToString(), ObjectLayer);
                    Debug.LogWarningFormat("GridCellInfo::SetOccupyingObject: [{0}] Layer {1} is in the dictionary, but is null", Position.ToString(), ObjectLayer);

                    OccupyingObjects[ObjectLayer] = Object;
                }

                return;
            }

            Debug.AssertFormat(!IsOccupied(1 << ObjectLayer), "GridCellInfo::SetOccupyingObject: [{0}] Layer {1} has no object but the mask say otherwise", Position.ToString(), ObjectLayer);
            OccupyingObjects.Add(ObjectLayer, Object);
            OccupiedLayersMask |= (1 << ObjectLayer);
        }

        // Return whether or not any of the layers in the layer mask are occupied
        public bool IsOccupied(int LayerMask)
        {
            return (OccupiedLayersMask & (1 << LayerMask)) != 0;
        }

        // Return the object occupying this cell in the given layer
        public GridObject GetObject(int layer)
        {
            if (OccupyingObjects.TryGetValue(layer, out GridObject occupyingObject))
            {
                return occupyingObject;
            }

            return null;
        }

        public GridCellInfo(in Vector2Int InPosition, int InHeight)
        {
            Position = InPosition;
            Height = InHeight;

            OccupyingObjects = new Dictionary<int, GridObject>();
            OccupiedLayersMask = 0;
        }
    }

    // The map of cells for this grid
    Dictionary<Vector2Int, GridCellInfo> GridMap = new Dictionary<Vector2Int, GridCellInfo>();

    // Create the currently active grid
    public static void CreateCurrentGrid(GridAsset Asset)
    {
        if (Asset != null)
        {
            CurrentGrid = new Grid();
            foreach (GridCellInfo Cell in Asset.GridCells)
            {
                CurrentGrid.GridMap.Add(Cell.Position, new GridCellInfo(Cell.Position, Cell.Height));
            }

            bHasCurrentGrid = true;
        }
    }

    // Destroy the currently active grid
    public static void DestroyCurrentGrid()
    {
        if (bHasCurrentGrid)
        {
            CurrentGrid = null;
            bHasCurrentGrid = false;
        }
    }

    // Add an object to the grid at the given location. 
    public bool AddGridObject(in Vector2Int Pos, GridObject Object)
    {
        if (GridMap.TryGetValue(Pos, out GridCellInfo CellInfo))
        {
            int ObjectLayer = Object.gameObject.layer;
            
            if(CellInfo.IsOccupied(1 << ObjectLayer))
            {
                GridObject currentObject = CellInfo.GetObject(ObjectLayer);
                Debug.LogErrorFormat("Pos {0} is not empty, has object {1}", Pos, currentObject);
                return false;
            }

            CellInfo.SetObject(ObjectLayer, Object);
            GridMap[Pos] = CellInfo;

            return true;
        }
        else
        {
            Debug.LogErrorFormat("Pos {0} has no cell", Pos);
        }

        return false;
    }

    // Remove an object in the given layer from the grid at the given location
    public void RemoveGridObject(in Vector2Int Pos, int layer)
    {
        if (GridMap.TryGetValue(Pos, out GridCellInfo CellInfo))
        {
            if (!CellInfo.IsOccupied(1 << layer))
            {
                Debug.LogErrorFormat("Pos {0} is empty", Pos);
                return;
            }

            CellInfo.SetObject(layer, null);

            GridMap[Pos] = CellInfo;
        }
        else
        {
            Debug.LogErrorFormat("Pos {0} has no cell", Pos);
        }
    }

    public bool HasGridObject(in Vector2Int Pos, int LayerMask)
    {
        if (GridMap.TryGetValue(Pos, out GridCellInfo CellInfo))
        {
            return CellInfo.IsOccupied(LayerMask);
        }
        else
        {
            Debug.LogErrorFormat("Pos {0} has no cell", Pos);
        }

        return false;
    }

    public bool GetGridObject(in Vector2Int Pos, int Layer, out GridObject Object)
    {
        if (GridMap.TryGetValue(Pos, out GridCellInfo CellInfo))
        {
            Object = CellInfo.GetObject(Layer);
            return true;
        }
        else
        {
            Debug.LogErrorFormat("Pos {0} has no cell", Pos);
        }

        Object = null;
        return false;
    }

    public int GetTileHeight(in Vector2Int Pos)
    {
        if (GridMap.TryGetValue(Pos, out GridCellInfo CellInfo))
        {
            return CellInfo.Height;
        }

        return 0;
    }

    public bool MoveGridObject(in Vector2Int FromPos, in Vector2Int ToPos, int Layer)
    {
        if (GridMap.TryGetValue(FromPos, out GridCellInfo FromCellInfo))
        {
            if (GridMap.TryGetValue(ToPos, out GridCellInfo ToCellInfo))
            {
                if(FromCellInfo.IsOccupied(Layer))
                {
                    GridObject MovingObject = FromCellInfo.GetObject(Layer);

                    if(ToCellInfo.IsOccupied(Layer))
                    {
                        return false;
                    }

                    FromCellInfo.SetObject(Layer, null);
                    ToCellInfo.SetObject(Layer, MovingObject);

                    GridMap[FromPos] = FromCellInfo;
                    GridMap[ToPos] = ToCellInfo;

                    return true;
                }
                else
                {
                    Debug.LogErrorFormat("FromPos {0} is empty, cannot move object", FromPos);
                }
            }
        }
        else
        {
            Debug.LogErrorFormat("FromPos {0} has no cell", FromPos);
        }

        return false;
    }

    public bool DoesTileExist(in Vector2Int Pos)
    {
        return GridMap.ContainsKey(Pos);
    }

    public int GetNumTiles()
    {
        return GridMap.Count;
    }
}