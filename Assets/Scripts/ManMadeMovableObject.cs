using UnityEngine;
using UnityEngine.InputSystem;

public class ManMadeMovableObject : MonoBehaviour
{
    public float ghostAlpha = 0.35f;
    public float ghostSortingOffset = 25f;
    public bool updateStationarySatelliteOrbitData = true;

    private static ManMadeMovableObject activeEditor;

    private SpriteRenderer sourceRenderer;
    private GameObject ghostObject;
    private SpriteRenderer ghostRenderer;
    private bool waitingForInitialRelease;

    public bool IsEditing => activeEditor == this;

    private void Awake()
    {
        sourceRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (IsEditing)
        {
            UpdateEditorMode();
            return;
        }

        if (activeEditor == null && Mouse.current.leftButton.wasPressedThisFrame && PointerHitsThisObject())
        {
            EnterEditorMode();
        }
    }

    private void OnDisable()
    {
        if (IsEditing)
        {
            ExitEditorMode();
        }
    }

    public void EnterEditorMode()
    {
        if (activeEditor != null && activeEditor != this)
        {
            activeEditor.ExitEditorMode();
        }

        activeEditor = this;
        waitingForInitialRelease = Mouse.current != null && Mouse.current.leftButton.isPressed;
        EnsureGhost();
        UpdateGhostPosition(CursorWorldPosition());
    }

    public void PlaceAt(Vector2 worldPosition)
    {
        transform.position = PlacementPosition(worldPosition, transform.position.z);

        if (updateStationarySatelliteOrbitData)
        {
            DysonSatellite satellite = GetComponent<DysonSatellite>();
            if (satellite != null && !satellite.IsDynamic)
            {
                satellite.SetStationaryPosition(transform.position);
            }
        }
    }

    public static Vector3 PlacementPosition(Vector2 worldPosition, float z)
    {
        return new Vector3(worldPosition.x, worldPosition.y, z);
    }

    private void UpdateEditorMode()
    {
        Vector2 cursorPosition = CursorWorldPosition();
        UpdateGhostPosition(cursorPosition);

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ExitEditorMode();
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            ExitEditorMode();
            return;
        }

        if (waitingForInitialRelease)
        {
            if (!Mouse.current.leftButton.isPressed)
            {
                waitingForInitialRelease = false;
            }

            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceAt(cursorPosition);
            ExitEditorMode();
        }
    }

    private bool PointerHitsThisObject()
    {
        Collider2D ownCollider = GetComponent<Collider2D>();
        if (ownCollider == null)
        {
            return false;
        }

        Vector2 cursorPosition = CursorWorldPosition();
        Collider2D[] hits = Physics2D.OverlapPointAll(cursorPosition);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == ownCollider || (hit != null && hit.transform.IsChildOf(transform)))
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 CursorWorldPosition()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return transform.position;
        }

        Vector3 screenPosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

    private void EnsureGhost()
    {
        if (ghostObject != null)
        {
            ghostObject.SetActive(true);
            return;
        }

        ghostObject = new GameObject(name + " Placement Preview");
        ghostObject.transform.localScale = transform.lossyScale;
        ghostObject.transform.rotation = transform.rotation;

        ghostRenderer = ghostObject.AddComponent<SpriteRenderer>();
        if (sourceRenderer != null)
        {
            ghostRenderer.sprite = sourceRenderer.sprite;
            ghostRenderer.color = GhostColor(sourceRenderer.color, ghostAlpha);
            ghostRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            ghostRenderer.sortingOrder = sourceRenderer.sortingOrder + Mathf.RoundToInt(ghostSortingOffset);
        }
    }

    private void UpdateGhostPosition(Vector2 worldPosition)
    {
        EnsureGhost();
        ghostObject.transform.position = PlacementPosition(worldPosition, transform.position.z);
        ghostObject.transform.rotation = transform.rotation;
        ghostObject.transform.localScale = transform.lossyScale;
    }

    private void ExitEditorMode()
    {
        if (ghostObject != null)
        {
            Destroy(ghostObject);
        }

        ghostObject = null;
        ghostRenderer = null;
        waitingForInitialRelease = false;

        if (activeEditor == this)
        {
            activeEditor = null;
        }
    }

    private static Color GhostColor(Color source, float alpha)
    {
        return new Color(source.r, source.g, source.b, Mathf.Clamp01(alpha));
    }
}
