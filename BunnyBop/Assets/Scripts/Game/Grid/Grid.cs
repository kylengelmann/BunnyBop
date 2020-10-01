using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    public static Grid CurrentGrid { get; private set; }

    [Serializable]
    public struct GridCellInfo
    {
        public Vector2Int Position;

        public int Height;

        [NonSerialized]
        public bool bHasOccupyingObject;

        [NonSerialized]
        public GridObject OccupyingObject;

        [NonSerialized]
        public bool bHasSpecialObject;

        [NonSerialized]
        public GridObject SpecialObject;

        public GridCellInfo(in Vector2Int InPosition, int InHeight)
        {
            Position = InPosition;
            Height = InHeight;

            bHasOccupyingObject = false;
            OccupyingObject = null;
            bHasSpecialObject = false;
            SpecialObject = null;
        }
    }

    Dictionary<Vector2Int, GridCellInfo> GridMap = new Dictionary<Vector2Int, GridCellInfo>();

    public static void CreateCurrentGrid(GridAsset Asset)
    {
        CurrentGrid = new Grid();
        foreach (GridCellInfo Cell in Asset.GridCells)
        {
            CurrentGrid.GridMap.Add(Cell.Position, new GridCellInfo(Cell.Position, Cell.Height));
        }
    }

    public void AddGridObject(in Vector2Int Pos, GridObject Object, bool bIsSpecialObject)
    {
        if (GridMap.TryGetValue(Pos, out GridCellInfo CellInfo))
        {
            if(bIsSpecialObject ? CellInfo.bHasSpecialObject : CellInfo.bHasOccupyingObject)
            {
                GridObject currentObject = bIsSpecialObject ? CellInfo.SpecialObject : CellInfo.OccupyingObject;
                Debug.LogErrorFormat("Pos {0} is not empty, has object {1}", Pos, currentObject);
                return;
            }

            if(bIsSpecialObject)
            {
                CellInfo.bHasSpecialObject = true;
                CellInfo.SpecialObject = Object;
            }
            else
            {
                CellInfo.bHasOccupyingObject = true;
                CellInfo.OccupyingObject = Object;
            }
        }
        else
        {
            Debug.LogErrorFormat("Pos {0} has no cell", Pos);
        }
    }

    public void RemoveGridObject(in Vector2Int Pos, bool bIsSpecialObject)
    {
        if (GridMap.TryGetValue(Pos, out GridCellInfo CellInfo))
        {
            if (!(bIsSpecialObject ? CellInfo.bHasSpecialObject : CellInfo.bHasOccupyingObject))
            {
                GridObject currentObject = bIsSpecialObject ? CellInfo.SpecialObject : CellInfo.OccupyingObject;
                Debug.LogErrorFormat("Pos {0} is empty", Pos);
                return;
            }

            if (bIsSpecialObject)
            {
                CellInfo.bHasSpecialObject = false;
                CellInfo.SpecialObject = null;
            }
            else
            {
                CellInfo.bHasOccupyingObject = false;
                CellInfo.OccupyingObject = null;
            }
        }
        else
        {
            Debug.LogErrorFormat("Pos {0} has no cell", Pos);
        }
    }

    public GridObject GetGridObject(in Vector2Int Pos, bool bIsSpecialObject)
    {
        if (GridMap.TryGetValue(Pos, out GridCellInfo CellInfo))
        {
            return bIsSpecialObject ? CellInfo.SpecialObject : CellInfo.OccupyingObject;
        }
        else
        {
            Debug.LogErrorFormat("Pos {0} has no cell", Pos);
        }
        
        return null;
    }

    public void MoveGridObject(in Vector2Int FromPos, in Vector2Int ToPos, bool bIsSpecialObject)
    {
        if (GridMap.TryGetValue(FromPos, out GridCellInfo FromCellInfo))
        {
            if (GridMap.TryGetValue(ToPos, out GridCellInfo ToCellInfo))
            {
                if(bIsSpecialObject ? FromCellInfo.bHasSpecialObject : FromCellInfo.bHasOccupyingObject)
                {
                    GridObject MovingObject = bIsSpecialObject ? FromCellInfo.SpecialObject : FromCellInfo.OccupyingObject;

                    if(bIsSpecialObject ? ToCellInfo.bHasSpecialObject : ToCellInfo.bHasOccupyingObject)
                    {
                        Debug.LogErrorFormat("ToPos {0} is not empty, contains object {1}", ToPos, bIsSpecialObject ? ToCellInfo.SpecialObject.name : ToCellInfo.OccupyingObject.name);
                        return;
                    }

                    if(bIsSpecialObject)
                    {
                        ToCellInfo.SpecialObject = MovingObject;
                        FromCellInfo.SpecialObject = null;
                    }
                    else
                    {
                        ToCellInfo.OccupyingObject = MovingObject;
                        FromCellInfo.OccupyingObject = null;
                    }
                }
                else
                {
                    Debug.LogErrorFormat("FromPos {0} is empty, cannot move object", FromPos);
                }
            }
            else
            {
                Debug.LogErrorFormat("ToPos {0} has no cell", ToPos);
            }
        }
        else
        {
            Debug.LogErrorFormat("FromPos {0} has no cell", FromPos);
        }
    }
}