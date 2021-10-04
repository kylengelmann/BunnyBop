using System;
using System.CodeDom;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, BunnyBopControls.IGameplayActions
{
    public enum EPlayerState
    {
        Inactive,
        Idle,
        UnitSelected,
        CardSelected
    }

    EPlayerState PlayerState = EPlayerState.Idle;

    public Vector2Int TilePos;

    SpriteRenderer HighlightTileRenderer;

    Camera mainCamera;
    PixelPerfectCamera pixelPerfectCamera;
    bool bHasPixelPerfect = false;
    bool bInitialized = false;

    BunnyBopControls Controls;

    Character SelectedCharacter;

    void Awake()
    {
        Controls = new BunnyBopControls();
        Controls.Gameplay.SetCallbacks(this);
    }

    void OnEnable()
    {
        Controls.Gameplay.Enable();
    }

    void OnDisable()
    {
        Controls.Gameplay.Disable();
    }

    // Set up initial values
    void Initialize()
    {
        mainCamera = GameMode.Instance.MainCamera;
        pixelPerfectCamera = mainCamera.GetComponent<PixelPerfectCamera>();
        bHasPixelPerfect = pixelPerfectCamera;

        bInitialized = true;
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            switch (PlayerState)
            {
                case EPlayerState.Idle:
                {
                    if (Grid.bHasCurrentGrid && Grid.CurrentGrid.DoesTileExist(TilePos))
                    {
                        if (Grid.CurrentGrid.GetGridObject(TilePos, BunnyBopStatics.CharacterLayer, out GridObject Object))
                        {
                            if (Object is Character CharacterObject)
                            {
                                SelectedCharacter = CharacterObject;
                                PlayerState = EPlayerState.UnitSelected;
                            }
                        }
                    }
                    break;
                }
                case EPlayerState.UnitSelected:
                {
                    if (Grid.bHasCurrentGrid && Grid.CurrentGrid.DoesTileExist(TilePos))
                    {
                        if (SelectedCharacter)
                        {
                            SelectedCharacter.MoveToTile(TilePos);
                        }

                        SelectedCharacter = null;
                        PlayerState = EPlayerState.Idle;
                    }

                    break;
                }
            }
        }
    }

    void Update()
    {
        if (!GameMode.Instance.bHasMainCamera)
        {
            return;
        }
        else if (!bInitialized)
        {
            Initialize();
        }

        TilePos = Grid.TransformWorldToTile(GetCursorPixelLocation());
    }

    // Get the pixel perfect aligned location of the cursor
    Vector2 GetCursorPixelLocation()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (bHasPixelPerfect && pixelPerfectCamera.isActiveAndEnabled)
        {
            // Subtract camera rect offset
            Vector2 cameraMousePos = new Vector2(mousePos.x - mainCamera.pixelRect.x, mousePos.y - mainCamera.pixelRect.y);
            
            // Scale down to pixel perfect resolution
            cameraMousePos /= pixelPerfectCamera.pixelRatio;
            
            // Round to nearest pixel
            cameraMousePos.x = Mathf.Floor(cameraMousePos.x) + .5f;
            cameraMousePos.y = Mathf.Floor(cameraMousePos.y) + .5f;

            // Upscale to screen resolution
            cameraMousePos *= pixelPerfectCamera.pixelRatio;

            // Add back in the rect offset
            mousePos.x = cameraMousePos.x + mainCamera.pixelRect.x;
            mousePos.y = cameraMousePos.y + mainCamera.pixelRect.y;
        }

        return mainCamera.ScreenToWorldPoint(mousePos);
    }
}