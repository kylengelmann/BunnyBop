using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CursorCast : MonoBehaviour
{
    public float MaxDistance = 100f;
    public LayerMask LayerMask = -1;
    public GameObject HighlightTile;

    public Vector2Int TilePos;

    SpriteRenderer HighlightTileRenderer;

    Camera mainCamera;

    PixelPerfectCamera pixelPerfectCamera;
    bool bHasPixelPerfect = false;

    void Start()
    {
        if (HighlightTile == null)
        {
            Destroy(this);
            return;
        }

        GameObject HighlightTileGO = Instantiate(HighlightTile, transform);
        if (HighlightTileGO)
        {
            HighlightTileRenderer = HighlightTileGO.GetComponent<SpriteRenderer>();

            if (!HighlightTileRenderer)
            {
                Destroy(HighlightTileGO);
                Destroy(this);
                return;
            }

            HighlightTileRenderer.enabled = false;
        }

        mainCamera = Camera.main;
        pixelPerfectCamera = mainCamera.GetComponent<PixelPerfectCamera>();
        bHasPixelPerfect = pixelPerfectCamera;
    }

    void OnDestroy()
    {
        if (HighlightTileRenderer)
        {
            Destroy(HighlightTileRenderer.gameObject);
        }
    }

    void Update()
    {
        Vector3 castLocation = GetCursorLocation();
        if (Physics.Raycast(castLocation, mainCamera.transform.forward, out RaycastHit hitInfo, MaxDistance, LayerMask, QueryTriggerInteraction.Ignore))
        {
            Collider collider = hitInfo.collider;
            if(collider)
            {
                Vector3 position = collider.transform.position;
                position.z = 0f;

                HighlightTileRenderer.transform.position = position;
                HighlightTileRenderer.enabled = true;

                TilePos = Grid.TransformWorldToTile(castLocation);
            }
        }
        else
        {
            HighlightTileRenderer.enabled = false;
        }

    }

    Vector3 GetCursorLocation()
    {
        Vector3 mousePos = Input.mousePosition;

        if (bHasPixelPerfect && pixelPerfectCamera.isActiveAndEnabled)
        {
            Vector2 cameraMousePos = new Vector2(mousePos.x - mainCamera.pixelRect.x, mousePos.y - mainCamera.pixelRect.y);

            cameraMousePos /= pixelPerfectCamera.pixelRatio;
            cameraMousePos.x = Mathf.Floor(cameraMousePos.x) + .5f;
            cameraMousePos.y = Mathf.Floor(cameraMousePos.y) + .5f;
            cameraMousePos *= pixelPerfectCamera.pixelRatio;

            mousePos.x = cameraMousePos.x + mainCamera.pixelRect.x;
            mousePos.y = cameraMousePos.y + mainCamera.pixelRect.y;
        }

        return mainCamera.ScreenToWorldPoint(mousePos);
    }
}
