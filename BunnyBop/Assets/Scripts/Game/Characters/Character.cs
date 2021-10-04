using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;

public class Character : GridObject
{
    public int MoveSpeed;

    public AnimationCurve MoveStartCurve;
    public AnimationCurve MoveEndCurve;
    public float MoveCurveTimeScale = 1f;
    public float MoveToNewTileTime = .5f;

    Coroutine PerformMoveCoroutine = null;

    public void FindTilesCanMoveTo()
    {
        List<Vector2Int> TilesInRange = new List<Vector2Int>();
        GridSearchUtils.FindTilesInRange(GridPosition, MoveSpeed, CanMoveToPosition, ref TilesInRange);
    }

    public bool MoveToTile(in Vector2Int MoveTo)
    {
        List<Vector2Int> Path = new List<Vector2Int>();
        GridSearchUtils.FindPath(GridPosition, MoveTo, CanMoveToPosition, ref Path);

        if (Path.Count <= 0 || Path.Count > MoveSpeed) return false;

        StartCoroutine(PerformMove(Path));

        return true;
    }

    IEnumerator PerformMove(List<Vector2Int> Path)
    {
        float CurrentAnimTime = 0f;
        int CurrentPathIndex = 0;

        float MoveStartCurveEndTime = MoveStartCurve[MoveStartCurve.length-1].time;
        float MoveEndCurveEndTime = MoveEndCurve[MoveEndCurve.length-1].time;

        Vector2Int NextPathPos = Path[CurrentPathIndex];
        Vector3 NextPathPos3D = Grid.TransformTileToLocal3D(NextPathPos);
        Vector3 CurrentPathPos3D = Grid.TransformTileToLocal3D(GridPosition);

        Debug.Log("Moving From " + GridPosition.ToString() + " To " + NextPathPos.ToString());

        //while (CurrentAnimTime * MoveCurveTimeScale < MoveStartCurveEndTime)
        //{
        //    yield return null;

        //    CurrentAnimTime += Time.deltaTime;

        //    transform.localPosition = Vector3.Lerp(CurrentPathPos3D, NextPathPos3D, MoveStartCurve.Evaluate(CurrentAnimTime * MoveCurveTimeScale)/2f);
        //}

        if (Path.Count > 1)
        {
            //CurrentAnimTime = MoveToNewTileTime / 2f;
            while (CurrentPathIndex < Path.Count)
            {
                while (CurrentAnimTime < MoveToNewTileTime)
                {
                    yield return null;

                    CurrentAnimTime += Time.deltaTime;

                    transform.localPosition = Vector3.Lerp(CurrentPathPos3D, NextPathPos3D, CurrentAnimTime/MoveToNewTileTime);
                }

                CurrentAnimTime -= MoveToNewTileTime;

                ++CurrentPathIndex;
                MoveObject(NextPathPos);

                NextPathPos = Path[CurrentPathIndex];
                NextPathPos3D = Grid.TransformTileToLocal3D(NextPathPos);
                CurrentPathPos3D = Grid.TransformTileToLocal3D(GridPosition);

                Debug.Log("Moving From " + GridPosition.ToString() + " To " + NextPathPos.ToString());
            }

            //while (CurrentAnimTime < MoveToNewTileTime / 2f)
            //{
            //    yield return null;

            //    CurrentAnimTime += Time.deltaTime;

            //    transform.localPosition = Vector3.Lerp(CurrentPathPos3D, NextPathPos3D, CurrentAnimTime / MoveToNewTileTime);
            //}
        }

        CurrentAnimTime = 0f;

        //while (CurrentAnimTime * MoveCurveTimeScale < MoveEndCurveEndTime)
        //{
        //    yield return null;

        //    CurrentAnimTime += Time.deltaTime;

        //    transform.localPosition = Vector3.Lerp(CurrentPathPos3D, NextPathPos3D, .5f + (MoveEndCurve.Evaluate(CurrentAnimTime * MoveCurveTimeScale) / 2f));
        //}
        
        MoveObject(NextPathPos);
    }
}
