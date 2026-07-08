using UnityEngine;

public class PlayerShipVisual : MonoBehaviour
{
    public Sprite thrustSprite;
    public Sprite idleSprite;

    private SpriteRenderer spriteRenderer;
    private PlayerMovement movement;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponentInParent<PlayerMovement>();
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            return;
        }

        if (movement == null)
        {
            movement = GetComponentInParent<PlayerMovement>();
        }

        spriteRenderer.sprite = SelectSprite(
            movement != null && movement.IsActivelyThrusting,
            idleSprite,
            thrustSprite,
            spriteRenderer.sprite);
    }

    public void Configure(Sprite idle, Sprite thrust)
    {
        idleSprite = idle;
        thrustSprite = thrust;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = SelectSprite(false, idleSprite, thrustSprite, spriteRenderer.sprite);
        }
    }

    public static Sprite SelectSprite(bool isThrusting, Sprite idleSprite, Sprite thrustSprite, Sprite fallbackSprite)
    {
        if (isThrusting)
        {
            return thrustSprite != null ? thrustSprite : fallbackSprite;
        }

        if (idleSprite != null)
        {
            return idleSprite;
        }

        return thrustSprite != null ? thrustSprite : fallbackSprite;
    }
}
