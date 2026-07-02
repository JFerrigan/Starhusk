using System;
using UnityEngine;

public class PlacedBuilding : MonoBehaviour
{
    public BuildingType buildingType = BuildingType.Mine;
    public BuildingTier buildingTier = BuildingTier.Tier1;

    public event Action<PlacedBuilding> MoveRequested;

    private SpriteRenderer spriteRenderer;
    private Color baseColor = Color.white;
    private bool hasBaseColor;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            baseColor = spriteRenderer.color;
            hasBaseColor = true;
        }
    }

    public virtual void SetSelected(bool selected)
    {
        if (!hasBaseColor || spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.color = selected
            ? new Color(
                Mathf.Clamp01(baseColor.r + 0.16f),
                Mathf.Clamp01(baseColor.g + 0.16f),
                Mathf.Clamp01(baseColor.b + 0.16f),
                baseColor.a)
            : baseColor;
    }

    public void RequestMove()
    {
        MoveRequested?.Invoke(this);
    }
}
