using UnityEngine;
using UnityEngine.InputSystem;

public class HoverNameDisplay : MonoBehaviour
{
    public Vector2 labelOffset = new Vector2(18f, 20f);
    public Vector2 labelPadding = new Vector2(10f, 6f);

    private GUIStyle labelStyle;
    private Texture2D pixel;
    private ObjectIdentity hoveredIdentity;

    private void Awake()
    {
        pixel = Texture2D.whiteTexture;
        labelStyle = new GUIStyle
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
    }

    private void Update()
    {
        hoveredIdentity = FindHoveredIdentity();
    }

    private void OnGUI()
    {
        if (hoveredIdentity == null || Mouse.current == null)
        {
            return;
        }

        string hoverName = hoveredIdentity.HoverName;
        if (string.IsNullOrWhiteSpace(hoverName))
        {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 guiPosition = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
        Vector2 contentSize = labelStyle.CalcSize(new GUIContent(hoverName));
        Rect rect = new Rect(
            guiPosition.x + labelOffset.x,
            guiPosition.y + labelOffset.y,
            contentSize.x + (labelPadding.x * 2f),
            contentSize.y + (labelPadding.y * 2f));

        rect.x = Mathf.Min(rect.x, Screen.width - rect.width - 4f);
        rect.y = Mathf.Min(rect.y, Screen.height - rect.height - 4f);
        rect.x = Mathf.Max(4f, rect.x);
        rect.y = Mathf.Max(4f, rect.y);

        DrawRect(rect, new Color(0f, 0f, 0f, 0.78f));
        DrawRectOutline(rect, new Color(0.55f, 0.88f, 1f, 0.9f), 2f);
        GUI.Label(rect, hoverName, labelStyle);
    }

    private ObjectIdentity FindHoveredIdentity()
    {
        if (Mouse.current == null || Camera.main == null)
        {
            return null;
        }

        Vector2 screen = Mouse.current.position.ReadValue();
        Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -Camera.main.transform.position.z));
        Collider2D[] hits = Physics2D.OverlapPointAll(new Vector2(world.x, world.y));
        ObjectIdentity bestIdentity = null;
        int bestSortingOrder = int.MinValue;

        for (int i = 0; i < hits.Length; i++)
        {
            ObjectIdentity identity = hits[i].GetComponentInParent<ObjectIdentity>();
            if (identity == null || !IsHoverVisible(identity))
            {
                continue;
            }

            int sortingOrder = SortingOrderFor(identity);
            if (sortingOrder < bestSortingOrder)
            {
                continue;
            }

            bestSortingOrder = sortingOrder;
            bestIdentity = identity;
        }

        return bestIdentity;
    }

    private static bool IsHoverVisible(ObjectIdentity identity)
    {
        MapMarker marker = identity.GetComponent<MapMarker>();
        if (marker != null && !marker.IsVisible)
        {
            return false;
        }

        DiscoveryState discovery = identity.GetComponent<DiscoveryState>();
        return discovery == null || discovery.discovered;
    }

    private static int SortingOrderFor(ObjectIdentity identity)
    {
        SpriteRenderer renderer = identity.GetComponentInChildren<SpriteRenderer>();
        return renderer == null ? 0 : renderer.sortingOrder;
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
