using System.Collections.Generic;
using UnityEngine;

public static class GridSearchUtils
{
    public struct PathPointData
    {
        public Vector2Int PathPoint;

        public Vector2Int PreviousPathPoint;

        public int PathDistance;

        public PathPointData(in Vector2Int InPathPoint, in Vector2Int InPreviousPathPoint, int Distance)
        {
            PathPoint = InPathPoint;
            PreviousPathPoint = InPreviousPathPoint;
            PathDistance = Distance;
        }
    }

    public delegate bool GridSearchDelegate(in Vector2Int TilePos, in Vector2Int FromTilePos, int PathDistance);

    public delegate bool CanMoveToCellDelegate(in Vector2Int TilePos, in Vector2Int FromTilePos);

    public static bool SearchTiles(in Vector2Int StartPos, ref Dictionary<Vector2Int, PathPointData> PathPoints,
        GridSearchDelegate ShouldEnqueueTile, GridSearchDelegate ShouldEndSearch)
    {
        bool bWasEndConditionSatisfied = false;
        if (Grid.bHasCurrentGrid)
        {
            if (Grid.CurrentGrid.DoesTileExist(StartPos))
            {
                Queue<Vector2Int> SearchPointsQueue = new Queue<Vector2Int>(Grid.CurrentGrid.GetNumTiles());

                void EnqueuePoint(in Vector2Int InPoint, in Vector2Int InPreviousPoint, int Distance,
                    ref Dictionary<Vector2Int, PathPointData> PathPointsMap)
                {
                    SearchPointsQueue.Enqueue(InPoint);
                    PathPointsMap.Add(InPoint, new PathPointData(InPoint, InPreviousPoint, Distance));
                }

                EnqueuePoint(StartPos, StartPos, 0, ref PathPoints);

                while (SearchPointsQueue.Count > 0 && !bWasEndConditionSatisfied)
                {
                    Vector2Int CurrentPoint = SearchPointsQueue.Dequeue();
                    if (PathPoints.TryGetValue(CurrentPoint, out PathPointData currentPointData))
                    {
                        const int NumDirections = 4;
                        for (int i = 0; i < NumDirections; ++i)
                        {
                            Vector2Int DirectionSearching = Vector2Int.zero;
                            switch (i)
                            {
                                case 0:
                                    DirectionSearching = Vector2Int.up;
                                    break;
                                case 1:
                                    DirectionSearching = Vector2Int.down;
                                    break;
                                case 2:
                                    DirectionSearching = Vector2Int.left;
                                    break;
                                case 3:
                                    DirectionSearching = Vector2Int.right;
                                    break;
                            }

                            Vector2Int NextPoint = CurrentPoint + DirectionSearching;
                            int NextPathDistance = currentPointData.PathDistance + 1;

                            if (!PathPoints.ContainsKey(NextPoint) &&
                                ShouldEnqueueTile(NextPoint, CurrentPoint, NextPathDistance))
                            {
                                EnqueuePoint(NextPoint, CurrentPoint, NextPathDistance, ref PathPoints);

                                if (ShouldEndSearch(NextPoint, CurrentPoint, NextPathDistance))
                                {
                                    bWasEndConditionSatisfied = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogErrorFormat("Path Searching: Point data missing for point {0}",
                            CurrentPoint);
                    }
                }
            }
        }

        return bWasEndConditionSatisfied;
    }

    public static void FindPath(in Vector2Int FromPos, in Vector2Int ToPos, CanMoveToCellDelegate CanMoveToCell, ref List<Vector2Int> Path)
    {
        Path.Clear();

        if (Grid.bHasCurrentGrid)
        {
            if (Grid.CurrentGrid.DoesTileExist(FromPos))
            {
                if (Grid.CurrentGrid.DoesTileExist(ToPos))
                {
                    Dictionary<Vector2Int, PathPointData> PathPoints = new Dictionary<Vector2Int, PathPointData>(Grid.CurrentGrid.GetNumTiles());

                    bool ShouldEnqueueTile(in Vector2Int TilePos, in Vector2Int FromTilePos, int PathDistance)
                    {
                        // Search starts at the end point of the path for easy adding to the returned path list
                        // As a result, we need to check moving from the current tile pos into the last tile pos
                        return CanMoveToCell(TilePos, FromTilePos);
                    }

                    Vector2Int EndPoint = FromPos;
                    bool ShouldEndSearch(in Vector2Int TilePos, in Vector2Int FromTilePos, int PathDistance)
                    {
                        return TilePos == EndPoint;
                    }

                    if (SearchTiles(ToPos, ref PathPoints, ShouldEnqueueTile, ShouldEndSearch))
                    {
                        if (PathPoints.TryGetValue(FromPos, out PathPointData CurrentPointData))
                        {
                            int PathDistance = CurrentPointData.PathDistance;
                            for (int i = PathDistance; i > 0; --i)
                            {
                                Vector2Int PreviousPathPoint = CurrentPointData.PreviousPathPoint;
                                if (!PathPoints.TryGetValue(PreviousPathPoint, out CurrentPointData))
                                {
                                    Debug.LogErrorFormat("Path Building: Point data missing for point {0}",
                                        PreviousPathPoint);
                                    break;
                                }

                                Path.Add(CurrentPointData.PathPoint);
                            }
                        }
                        else
                        {
                            Debug.LogErrorFormat("Final point data missing for point {0}", FromPos);
                        }
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

    public static void FindTilesInRange(in Vector2Int FromPos, int Range, CanMoveToCellDelegate CanMoveToCell, ref List<Vector2Int> TilesInRange)
    {
        TilesInRange.Clear();

        if (Grid.bHasCurrentGrid)
        {
            if (Grid.CurrentGrid.DoesTileExist(FromPos))
            {
                bool ShouldEnqueueTile(in Vector2Int TilePos, in Vector2Int FromTilePos, int PathDistance)
                {
                    return CanMoveToCell(FromTilePos, TilePos);
                }

                bool ShouldEndSearch(in Vector2Int TilePos, in Vector2Int FromTilePos, int PathDistance)
                {
                    return PathDistance > Range;
                }

                int Diameter = Range * 2 + 1;
                Dictionary<Vector2Int, PathPointData> PathPoints = new Dictionary<Vector2Int, PathPointData>(Mathf.Min(Diameter*Diameter, Grid.CurrentGrid.GetNumTiles()));
                if (SearchTiles(FromPos, ref PathPoints, ShouldEnqueueTile, ShouldEndSearch))
                {
                    foreach (PathPointData PointData in PathPoints.Values)
                    {
                        TilesInRange.Add(PointData.PathPoint);
                    }
                }
            }
        }
    }
}
