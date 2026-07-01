using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float thrustForce = 8f;
    public float rotationSpeed = 180f;
    public float maxSpeed = 8f;
    public float reverseThrustMultiplier = 0.5f;
    public float brakeDamping = 4f;
    public float rotationResponsiveness = 12f;

    private Rigidbody2D rb;
    private float rotateInput;
    private float thrustInput;
    private bool brakeInput;
    private float currentRotationSpeed;

    public Vector2 Velocity => rb == null ? Vector2.zero : rb.linearVelocity;
    public float Speed => Velocity.magnitude;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        rotateInput = 0f;
        thrustInput = 0f;
        brakeInput = false;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            rotateInput = 1f;
        }
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            rotateInput = -1f;
        }

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            thrustInput = 1f;
        }
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            thrustInput = -reverseThrustMultiplier;
            brakeInput = true;
        }

    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        float targetRotationSpeed = rotateInput * rotationSpeed;
        currentRotationSpeed = Mathf.Lerp(
            currentRotationSpeed,
            targetRotationSpeed,
            rotationResponsiveness * Time.fixedDeltaTime
        );

        rb.MoveRotation(rb.rotation + (currentRotationSpeed * Time.fixedDeltaTime));

        if (!Mathf.Approximately(thrustInput, 0f))
        {
            rb.AddForce(transform.up * thrustForce * thrustInput);
        }

        if (brakeInput && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            rb.linearVelocity = Vector2.Lerp(
                rb.linearVelocity,
                Vector2.zero,
                brakeDamping * Time.fixedDeltaTime
            );
        }

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
    }
}
