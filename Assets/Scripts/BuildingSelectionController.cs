using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingSelectionController : MonoBehaviour
{
    private const float PanelWidth = 220f;
    private const float BasePanelHeight = 116f;

    private Texture2D pixel;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private Rect panelRect;
    private PlacedBuilding selectedBuilding;
    private CollectorAutomaton selectedCollector;
    private CollectorHub selectedHub;
    private ResourceStorage selectedStorage;
    private GameObject selectedObject;
    private SpriteRenderer selectedRenderer;
    private Color selectedRendererBaseColor;
    private bool hasSelectedRendererBaseColor;

    private void Awake()
    {
        pixel = Texture2D.whiteTexture;
        titleStyle = new GUIStyle
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        bodyStyle = new GUIStyle
        {
            fontSize = 13,
            normal = { textColor = Color.white }
        };
    }

    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (BuildingPlacementController.Instance != null && BuildingPlacementController.Instance.IsPlacing)
        {
            return;
        }

        if (AutomatonPlacementController.Instance != null && AutomatonPlacementController.Instance.IsPlacing)
        {
            return;
        }

        if (PointerOverPanel())
        {
            return;
        }

        SelectAtPointer();
    }

    private void OnGUI()
    {
        if (selectedObject == null)
        {
            return;
        }

        float panelHeight = PanelHeight();
        panelRect = new Rect(12f, Screen.height - panelHeight - 12f, PanelWidth, panelHeight);
        DrawRect(panelRect, new Color(0f, 0f, 0f, 0.78f));
        DrawRectOutline(panelRect, new Color(0.24f, 0.84f, 0.95f, 0.95f), 2f);

        if (selectedBuilding is PlanetResourceExtractorBuilding extractor)
        {
            DrawExtractorPanel(extractor);
            return;
        }

        if (selectedCollector != null)
        {
            DrawCollectorPanel(selectedCollector);
            return;
        }

        if (selectedHub != null)
        {
            DrawStoragePanel("Cargo Hub", selectedHub.Storage, panelRect.y + 36f);
            return;
        }

        if (selectedStorage != null)
        {
            DrawStoragePanel(selectedObject.name, selectedStorage, panelRect.y + 36f);
        }
    }

    private void SelectAtPointer()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        Vector3 world = camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D[] hits = Physics2D.OverlapPointAll(new Vector2(world.x, world.y));

        for (int i = 0; i < hits.Length; i++)
        {
            PlacedBuilding building = hits[i].GetComponentInParent<PlacedBuilding>();
            if (building != null)
            {
                SetSelectedObject(building.gameObject, building, null, null, null);
                return;
            }

            CollectorAutomaton collector = hits[i].GetComponentInParent<CollectorAutomaton>();
            if (collector != null)
            {
                SetSelectedObject(collector.gameObject, null, collector, null, collector.Cargo);
                return;
            }

            CollectorHub hub = hits[i].GetComponentInParent<CollectorHub>();
            if (hub != null)
            {
                SetSelectedObject(hub.gameObject, null, null, hub, hub.Storage);
                return;
            }

            ResourceStorage storage = hits[i].GetComponentInParent<ResourceStorage>();
            if (storage != null)
            {
                SetSelectedObject(storage.gameObject, null, null, null, storage);
                return;
            }
        }

        SetSelectedObject(null, null, null, null, null);
    }

    private void SetSelectedObject(GameObject targetObject, PlacedBuilding building, CollectorAutomaton collector, CollectorHub hub, ResourceStorage storage)
    {
        if (selectedObject == targetObject)
        {
            return;
        }

        if (selectedBuilding != null)
        {
            selectedBuilding.SetSelected(false);
        }

        if (selectedRenderer != null && hasSelectedRendererBaseColor)
        {
            selectedRenderer.color = selectedRendererBaseColor;
        }

        selectedObject = targetObject;
        selectedBuilding = building;
        selectedCollector = collector;
        selectedHub = hub;
        selectedStorage = storage;
        selectedRenderer = null;
        hasSelectedRendererBaseColor = false;

        if (selectedBuilding != null)
        {
            selectedBuilding.SetSelected(true);
            return;
        }

        if (selectedObject != null)
        {
            selectedRenderer = selectedObject.GetComponentInChildren<SpriteRenderer>();
        }

        if (selectedRenderer != null)
        {
            selectedRendererBaseColor = selectedRenderer.color;
            hasSelectedRendererBaseColor = true;
            selectedRenderer.color = new Color(
                Mathf.Clamp01(selectedRendererBaseColor.r + 0.16f),
                Mathf.Clamp01(selectedRendererBaseColor.g + 0.16f),
                Mathf.Clamp01(selectedRendererBaseColor.b + 0.16f),
                selectedRendererBaseColor.a
            );
        }
    }

    private bool PointerOverPanel()
    {
        if (selectedObject == null || Mouse.current == null)
        {
            return false;
        }

        Vector2 position = Mouse.current.position.ReadValue();
        Vector2 guiPosition = new Vector2(position.x, Screen.height - position.y);
        return panelRect.Contains(guiPosition);
    }

    private void DrawExtractorPanel(PlanetResourceExtractorBuilding extractor)
    {
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 10f, 170f, 20f), extractor.DisplayName, titleStyle);
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 36f, 180f, 18f), "Planet: " + extractor.PlanetName, bodyStyle);
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 56f, 180f, 18f), "Resource: " + extractor.Storage.ResourceType, bodyStyle);
        GUI.Label(
            new Rect(panelRect.x + 12f, panelRect.y + 76f, 190f, 18f),
            "Stored: " + extractor.Storage.CurrentAmount + " / " + extractor.Storage.Capacity,
            bodyStyle
        );

        if (GUI.Button(new Rect(panelRect.x + PanelWidth - 76f, panelRect.y + panelRect.height - 36f, 64f, 24f), "Move"))
        {
            if (BuildingPlacementController.Instance != null)
            {
                BuildingPlacementController.Instance.BeginMove(extractor);
            }
        }
    }

    private void DrawCollectorPanel(CollectorAutomaton collector)
    {
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 10f, 190f, 20f), "Collector", titleStyle);
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 36f, 190f, 18f), "Goal: " + collector.goal, bodyStyle);
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 56f, 190f, 18f), "State: " + collector.State, bodyStyle);
        DrawStorageContents(collector.Cargo, panelRect.y + 80f);
    }

    private void DrawStoragePanel(string title, ResourceStorage storage, float contentY)
    {
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 10f, 190f, 20f), title, titleStyle);
        DrawStorageContents(storage, contentY);
    }

    private void DrawStorageContents(ResourceStorage storage, float startY)
    {
        if (storage == null)
        {
            GUI.Label(new Rect(panelRect.x + 12f, startY, 190f, 18f), "No storage", bodyStyle);
            return;
        }

        GUI.Label(
            new Rect(panelRect.x + 12f, startY, 190f, 18f),
            "Stored: " + storage.TotalAmount + " / " + storage.Capacity,
            bodyStyle
        );

        IReadOnlyList<ResourceStack> stacks = storage.GetResources();
        if (stacks.Count <= 0)
        {
            GUI.Label(new Rect(panelRect.x + 12f, startY + 22f, 190f, 18f), "Empty", bodyStyle);
            return;
        }

        for (int i = 0; i < stacks.Count; i++)
        {
            ResourceStack stack = stacks[i];
            GUI.Label(
                new Rect(panelRect.x + 12f, startY + 22f + (i * 18f), 190f, 18f),
                stack.type + ": " + stack.amount,
                bodyStyle
            );
        }
    }

    private float PanelHeight()
    {
        if (selectedCollector != null)
        {
            return Mathf.Max(BasePanelHeight, 124f + (ResourceLineCount(selectedCollector.Cargo) * 18f));
        }

        if (selectedHub != null)
        {
            return Mathf.Max(BasePanelHeight, 78f + (ResourceLineCount(selectedHub.Storage) * 18f));
        }

        if (selectedStorage != null && selectedBuilding == null)
        {
            return Mathf.Max(BasePanelHeight, 78f + (ResourceLineCount(selectedStorage) * 18f));
        }

        return BasePanelHeight;
    }

    private static int ResourceLineCount(ResourceStorage storage)
    {
        if (storage == null)
        {
            return 1;
        }

        return Mathf.Max(1, storage.GetResources().Count);
    }

    private void DrawRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, pixel);
        GUI.color = previous;
    }

    private void DrawRectOutline(Rect rect, Color color, float thickness)
    {
        DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        DrawRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        DrawRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        DrawRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
    }
}
