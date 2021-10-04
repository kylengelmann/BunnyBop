using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridObject : MonoBehaviour
{
    /** The position this object is at on the grid */
    public Vector2Int GridPosition { get; protected set; }

    /** The Layers that block this object's movement on the grid. Should include this object's layer */
    [SerializeField] protected LayerMask BlockingLayerMask;

    /** Adds this object to the current grid */
    protected void Start()
    {
        if (Grid.bHasCurrentGrid)
        {
            GridPosition = Grid.TransformWorldToTile(transform.position);

            if (!Grid.CurrentGrid.HasGridObject(GridPosition, BlockingLayerMask))
            {
                if (!Grid.CurrentGrid.AddGridObject(GridPosition, this))
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    /** Moves this object to the given position if CanMoveToPosition returns true */
    public bool MoveObject(in Vector2Int Position)
    {
        if (CanMoveToPositionFromCurrent(Position))
        {
            if (Grid.CurrentGrid.MoveGridObject(GridPosition, Position, gameObject.layer))
            {
                GridPosition = Position;
                return true;
            }
        }

        return false;
    }

    /** Checks if this object can move from its current location to the passed in location */
    public virtual bool CanMoveToPositionFromCurrent(in Vector2Int Position)
    {
        return CanMoveToPosition(GridPosition, Position);
    }

    /** Checks if this object can move from FromPosition to ToPosition */
    public virtual bool CanMoveToPosition(in Vector2Int FromPosition, in Vector2Int ToPosition)
    {
        // Check that this position is adjacent, not including diagonals
        bool bIsAdjecentInX = Mathf.Abs(FromPosition.x - ToPosition.x) == 1;
        bool bIsAdjecentInY = Mathf.Abs(FromPosition.y - ToPosition.y) == 1;
        if (bIsAdjecentInX ^ bIsAdjecentInY)
        {
            int CurrentTileHeight = Grid.CurrentGrid.GetTileHeight(FromPosition);
            int NextTileHeight = Grid.CurrentGrid.GetTileHeight(ToPosition);

            return !Grid.CurrentGrid.HasGridObject(ToPosition, BlockingLayerMask) && CurrentTileHeight > 0 && NextTileHeight > 0 && CurrentTileHeight + 1 >= NextTileHeight;
        }

        return false;
    }

    /** Teleport this object to the given position, ignoring movement rules such as adjacency and tile height */
    public virtual bool TeleportObject(in Vector2Int Position)
    {
        if (!Grid.CurrentGrid.HasGridObject(Position, BlockingLayerMask))
        {
            if (Grid.CurrentGrid.MoveGridObject(GridPosition, Position, gameObject.layer))
            {
                GridPosition = Position;
                return true;
            }
        }

        return false;
    }
}