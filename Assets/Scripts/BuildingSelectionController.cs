using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingSelectionController : MonoBehaviour
{
    private const float PanelWidth = 220f;
    private const float FreighterPanelWidth = 380f;
    private const float BasePanelHeight = 116f;
    private const float FreighterPanelHeight = 440f;
    private const float HeaderHeight = 28f;

    private Texture2D pixel;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private Rect panelRect;
    private PlacedBuilding selectedBuilding;
    private CollectorAutomaton selectedCollector;
    private FreighterAutomaton selectedFreighter;
    private SatelliteFactory selectedSatelliteFactory;
    private PowerRelay selectedPowerRelay;
    private CollectorHub selectedHub;
    private ResourceStorage selectedStorage;
    private GameObject selectedObject;
    private SpriteRenderer selectedRenderer;
    private Color selectedRendererBaseColor;
    private bool hasSelectedRendererBaseColor;
    private bool sourceDropdownOpen;
    private bool destinationDropdownOpen;
    private bool cargoPriorityDropdownOpen;
    private Vector2 sourceScrollPosition;
    private Vector2 destinationScrollPosition;
    private Vector2 panelPosition;
    private bool hasPanelPosition;
    private bool draggingPanel;
    private Vector2 dragOffset;

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

        if (PointerOverBuildMenu())
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
        float panelWidth = PanelWidthForSelection();
        EnsurePanelPosition(panelWidth, panelHeight);
        panelRect = new Rect(panelPosition.x, panelPosition.y, panelWidth, panelHeight);
        HandlePanelDrag();

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

        if (selectedFreighter != null)
        {
            DrawFreighterPanel(selectedFreighter);
            return;
        }

        if (selectedSatelliteFactory != null)
        {
            DrawSatelliteFactoryPanel(selectedSatelliteFactory);
            return;
        }

        if (selectedPowerRelay != null)
        {
            DrawPowerRelayPanel(selectedPowerRelay);
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
                SetSelectedObject(building.gameObject, building, null, null, null, null, null, null);
                return;
            }

            CollectorAutomaton collector = hits[i].GetComponentInParent<CollectorAutomaton>();
            if (collector != null)
            {
                SetSelectedObject(collector.gameObject, null, collector, null, null, null, null, collector.Cargo);
                return;
            }

            FreighterAutomaton freighter = hits[i].GetComponentInParent<FreighterAutomaton>();
            if (freighter != null)
            {
                SetSelectedObject(freighter.gameObject, null, null, freighter, null, null, null, freighter.Cargo);
                return;
            }

            SatelliteFactory satelliteFactory = hits[i].GetComponentInParent<SatelliteFactory>();
            if (satelliteFactory != null)
            {
                SetSelectedObject(satelliteFactory.gameObject, null, null, null, satelliteFactory, null, null, satelliteFactory.Storage);
                return;
            }

            PowerRelay powerRelay = hits[i].GetComponentInParent<PowerRelay>();
            if (powerRelay != null)
            {
                SetSelectedObject(powerRelay.gameObject, null, null, null, null, powerRelay, null, null);
                return;
            }

            CollectorHub hub = hits[i].GetComponentInParent<CollectorHub>();
            if (hub != null)
            {
                SetSelectedObject(hub.gameObject, null, null, null, null, null, hub, hub.Storage);
                return;
            }

            ResourceStorage storage = hits[i].GetComponentInParent<ResourceStorage>();
            if (storage != null)
            {
                SetSelectedObject(storage.gameObject, null, null, null, null, null, null, storage);
                return;
            }
        }

        SetSelectedObject(null, null, null, null, null, null, null, null);
    }

public void SelectFreighter(FreighterAutomaton freighter)
{
    if (freighter == null)
    {
        return;
    }

    SetSelectedObject(freighter.gameObject, null, null, freighter, null, null, null, freighter.Cargo);
}

public void ClearSelection()
{
    if (selectedBuilding != null)
    {
        selectedBuilding.SetSelected(false);
    }

    if (selectedRenderer != null && hasSelectedRendererBaseColor)
    {
        selectedRenderer.color = selectedRendererBaseColor;
    }

    selectedObject = null;
    selectedBuilding = null;
    selectedCollector = null;
    selectedFreighter = null;
    selectedSatelliteFactory = null;
    selectedPowerRelay = null;
    selectedHub = null;
    selectedStorage = null;
    selectedRenderer = null;
    hasSelectedRendererBaseColor = false;
    sourceDropdownOpen = false;
    destinationDropdownOpen = false;
    cargoPriorityDropdownOpen = false;
}

    private void SetSelectedObject(GameObject targetObject, PlacedBuilding building, CollectorAutomaton collector, FreighterAutomaton freighter, SatelliteFactory satelliteFactory, PowerRelay powerRelay, CollectorHub hub, ResourceStorage storage)
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
        selectedFreighter = freighter;
        selectedSatelliteFactory = satelliteFactory;
        selectedPowerRelay = powerRelay;
        selectedHub = hub;
        selectedStorage = storage;
        selectedRenderer = null;
        hasSelectedRendererBaseColor = false;
        sourceDropdownOpen = false;
        destinationDropdownOpen = false;
        cargoPriorityDropdownOpen = false;

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

    private bool PointerOverBuildMenu()
    {
        if (Mouse.current == null)
        {
            return false;
        }

        Vector2 position = Mouse.current.position.ReadValue();
        Vector2 guiPosition = new Vector2(position.x, Screen.height - position.y);
        return BuildOptionsMenu.ContainsGuiPoint(guiPosition);
    }

    private void EnsurePanelPosition(float panelWidth, float panelHeight)
    {
        if (!hasPanelPosition)
        {
            panelPosition = new Vector2(12f, Screen.height - panelHeight - 12f);
            hasPanelPosition = true;
            return;
        }

        panelPosition = ClampToScreen(panelPosition, new Vector2(panelWidth, panelHeight));
    }

    private void HandlePanelDrag()
    {
        Event current = Event.current;
        if (current == null)
        {
            return;
        }

        Rect dragRect = new Rect(panelRect.x, panelRect.y, panelRect.width, HeaderHeight);
        if (current.type == EventType.MouseDown && current.button == 0 && dragRect.Contains(current.mousePosition))
        {
            draggingPanel = true;
            dragOffset = current.mousePosition - panelPosition;
            current.Use();
        }
        else if (current.type == EventType.MouseDrag && draggingPanel)
        {
            panelPosition = ClampToScreen(current.mousePosition - dragOffset, panelRect.size);
            current.Use();
        }
        else if (current.type == EventType.MouseUp && current.button == 0)
        {
            draggingPanel = false;
        }
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
        DrawPowerStatus(collector, panelRect.y + 76f);
        DrawStorageContents(collector.Cargo, panelRect.y + 100f);
    }

    private void DrawFreighterPanel(FreighterAutomaton freighter)
    {
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 10f, panelRect.width - 24f, 20f), selectedObject.name, titleStyle);
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 36f, panelRect.width - 24f, 18f), "State: " + freighter.State, bodyStyle);
        DrawPowerStatus(freighter, panelRect.y + 56f);
        DrawStorageContents(freighter.Cargo, panelRect.y + 80f);

        float dropdownY = panelRect.y + 158f + (ResourceLineCount(freighter.Cargo) * 18f);
        DrawStorageDropdown(
            new Rect(panelRect.x + 12f, dropdownY, panelRect.width - 24f, 28f),
            "Source",
            freighter,
            freighter.SourceStorage,
            true);

        DrawStorageDropdown(
            new Rect(panelRect.x + 12f, dropdownY + 132f, panelRect.width - 24f, 28f),
            "Destination",
            freighter,
            freighter.DestinationStorage,
            false);

        DrawCargoPriorityDropdown(
            new Rect(panelRect.x + 12f, dropdownY + 264f, panelRect.width - 24f, 28f),
            freighter);
    }

    private void DrawCargoPriorityDropdown(Rect rect, FreighterAutomaton freighter)
    {
        GUI.Label(new Rect(rect.x, rect.y - 18f, rect.width, 18f), "Cargo Priority", bodyStyle);

        if (GUI.Button(rect, freighter.cargoPriority.ToString()))
        {
            cargoPriorityDropdownOpen = !cargoPriorityDropdownOpen;
            sourceDropdownOpen = false;
            destinationDropdownOpen = false;
        }

        if (!cargoPriorityDropdownOpen)
        {
            return;
        }

        FreighterCargoPriority[] priorities = (FreighterCargoPriority[])System.Enum.GetValues(typeof(FreighterCargoPriority));
        Rect listRect = new Rect(rect.x, rect.y + rect.height + 2f, rect.width, 120f);

        DrawRect(listRect, new Color(0.02f, 0.03f, 0.04f, 0.96f));
        DrawRectOutline(listRect, new Color(0.24f, 0.84f, 0.95f, 0.95f), 1f);

        for (int i = 0; i < priorities.Length; i++)
        {
            if (GUI.Button(new Rect(listRect.x, listRect.y + (i * 20f), listRect.width, 20f), priorities[i].ToString()))
            {
                freighter.SetCargoPriority(priorities[i]);
                cargoPriorityDropdownOpen = false;
            }
        }
    }

    private void DrawSatelliteFactoryPanel(SatelliteFactory factory)
    {
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 10f, panelRect.width - 24f, 20f), selectedObject.name, titleStyle);
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 36f, panelRect.width - 24f, 18f), "Produced: " + factory.ProducedCount, bodyStyle);
        DrawPowerStatus(factory, panelRect.y + 56f);
        GUI.Label(
            new Rect(panelRect.x + 12f, panelRect.y + 76f, panelRect.width - 24f, 18f),
            factory.IsBuilding
                ? "Building: " + Mathf.RoundToInt(factory.BuildProgress * 100f) + "%  " + factory.BuildTimeRemaining.ToString("0.0") + "s"
                : "Building: Waiting for resources",
            bodyStyle
        );

        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 102f, panelRect.width - 24f, 18f), "Recipe Pools", bodyStyle);
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 124f, panelRect.width - 24f, 18f), "Ore: " + factory.OrePool + " / " + SatelliteFactory.OreCost, bodyStyle);
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 142f, panelRect.width - 24f, 18f), "Silicate: " + factory.SilicatePool + " / " + SatelliteFactory.SilicateCost, bodyStyle);
        DrawStorageContents(factory.Storage, panelRect.y + 168f);
    }

    private void DrawPowerRelayPanel(PowerRelay relay)
    {
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 10f, panelRect.width - 24f, 20f), selectedObject.name, titleStyle);
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 36f, panelRect.width - 24f, 18f), relay.IsConnected ? "Powered" : "Unpowered", bodyStyle);
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 56f, panelRect.width - 24f, 18f), "Range: " + Mathf.RoundToInt(relay.Range), bodyStyle);
    }

    private void DrawPowerStatus(IPowerConsumer consumer, float y)
    {
        string status = consumer.IsPowered ? "Powered" : "Unpowered";
        GUI.Label(new Rect(panelRect.x + 12f, y, panelRect.width - 24f, 18f), status + "  Demand: " + consumer.PowerDemand, bodyStyle);
    }

    private void DrawStorageDropdown(Rect rect, string label, FreighterAutomaton freighter, ResourceStorage selectedStorageValue, bool isSource)
    {
        GUI.Label(new Rect(rect.x, rect.y - 18f, rect.width, 18f), label, bodyStyle);

        bool isOpen = isSource ? sourceDropdownOpen : destinationDropdownOpen;
        if (GUI.Button(rect, StorageLabel(selectedStorageValue)))
        {
            sourceDropdownOpen = isSource ? !sourceDropdownOpen : false;
            destinationDropdownOpen = isSource ? false : !destinationDropdownOpen;
            cargoPriorityDropdownOpen = false;
        }

        if (!isOpen)
        {
            return;
        }

        ResourceStorage[] storages = FindObjectsByType<ResourceStorage>(FindObjectsSortMode.None);
        Rect listRect = new Rect(rect.x, rect.y + rect.height + 2f, rect.width, 96f);
        Rect contentRect = new Rect(0f, 0f, rect.width - 18f, Mathf.Max(96f, storages.Length * 26f));
        Vector2 scrollPosition = isSource ? sourceScrollPosition : destinationScrollPosition;

        DrawRect(listRect, new Color(0.02f, 0.03f, 0.04f, 0.96f));
        DrawRectOutline(listRect, new Color(0.24f, 0.84f, 0.95f, 0.95f), 1f);

        scrollPosition = GUI.BeginScrollView(listRect, scrollPosition, contentRect);

        float y = 0f;
        for (int i = 0; i < storages.Length; i++)
        {
            ResourceStorage storage = storages[i];
            if (!IsSelectableFreighterStorage(freighter, storage, isSource))
            {
                continue;
            }

            if (GUI.Button(new Rect(0f, y, contentRect.width, 24f), StorageLabel(storage)))
            {
                if (isSource)
                {
                    freighter.AssignEndpoints(storage, freighter.DestinationStorage);
                    sourceDropdownOpen = false;
                }
                else
                {
                    freighter.AssignEndpoints(freighter.SourceStorage, storage);
                    destinationDropdownOpen = false;
                }
            }

            y += 26f;
        }

        GUI.EndScrollView();

        if (isSource)
        {
            sourceScrollPosition = scrollPosition;
        }
        else
        {
            destinationScrollPosition = scrollPosition;
        }
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
        if (selectedFreighter != null)
        {
            return Mathf.Max(FreighterPanelHeight + 130f, 420f + (ResourceLineCount(selectedFreighter.Cargo) * 18f));
        }

        if (selectedSatelliteFactory != null)
        {
            return Mathf.Max(240f, 148f + (ResourceLineCount(selectedSatelliteFactory.Storage) * 18f));
        }

        if (selectedCollector != null)
        {
            return Mathf.Max(BasePanelHeight, 144f + (ResourceLineCount(selectedCollector.Cargo) * 18f));
        }

        if (selectedPowerRelay != null)
        {
            return BasePanelHeight;
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

    private float PanelWidthForSelection()
    {
        return selectedFreighter == null ? PanelWidth : FreighterPanelWidth;
    }

    private static bool IsSelectableFreighterStorage(FreighterAutomaton freighter, ResourceStorage storage, bool isSource)
    {
        if (freighter == null || storage == null || storage == freighter.Cargo)
        {
            return false;
        }

        if (isSource)
        {
            return storage != freighter.DestinationStorage;
        }

        return storage != freighter.SourceStorage;
    }

    private static string StorageLabel(ResourceStorage storage)
    {
        if (storage == null)
        {
            return "None";
        }

        ObjectIdentity identity = storage.GetComponent<ObjectIdentity>();
        string storageName = identity == null ? storage.name : identity.HoverName;
        return storageName + " (" + storage.TotalAmount + " / " + storage.Capacity + ")";
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

    private static Vector2 ClampToScreen(Vector2 position, Vector2 size)
    {
        return new Vector2(
            Mathf.Clamp(position.x, 4f, Mathf.Max(4f, Screen.width - size.x - 4f)),
            Mathf.Clamp(position.y, 4f, Mathf.Max(4f, Screen.height - size.y - 4f)));
    }
}
