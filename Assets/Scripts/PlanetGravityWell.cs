using UnityEngine;

public class PlanetGravityWell : MonoBehaviour
{
    public float surfaceRadius = 12f;
    public float influenceRadius = 42f;
    public float maxAcceleration = 1.35f;
    public float landingAssistBand = 8f;
    public float landingDamping = 1.8f;

    private Rigidbody2D playerBody;

    private void FixedUpdate()
    {
        if (playerBody == null)
        {
            ResourceInventory player = FindFirstObjectByType<ResourceInventory>();
            if (player == null)
            {
                return;
            }

            playerBody = player.GetComponent<Rigidbody2D>();
            if (playerBody == null)
            {
                return;
            }
        }

        ApplyGravity(playerBody, Time.fixedDeltaTime);
    }

    public void ApplyGravity(Rigidbody2D body, float deltaTime)
    {
        Vector2 toPlanet = (Vector2)transform.position - body.position;
        float distance = toPlanet.magnitude;
        float acceleration = CalculateAcceleration(distance, surfaceRadius, influenceRadius, maxAcceleration);

        if (acceleration <= 0f || distance <= Mathf.Epsilon)
        {
            return;
        }

        Vector2 direction = toPlanet / distance;
        body.AddForce(direction * acceleration * body.mass, ForceMode2D.Force);

        ApplyLandingDamping(body, direction, distance, deltaTime);
    }

    public static float CalculateAcceleration(float distance, float surfaceRadius, float influenceRadius, float maxAcceleration)
    {
        if (distance >= influenceRadius || maxAcceleration <= 0f)
        {
            return 0f;
        }

        float gravitySpan = Mathf.Max(0.01f, influenceRadius - surfaceRadius);
        float normalizedDistance = Mathf.Clamp01((distance - surfaceRadius) / gravitySpan);
        float strength = 1f - Mathf.SmoothStep(0f, 1f, normalizedDistance);
        return maxAcceleration * strength;
    }

    private void ApplyLandingDamping(Rigidbody2D body, Vector2 directionToPlanet, float distance, float deltaTime)
    {
        float distanceFromSurface = distance - surfaceRadius;
        if (distanceFromSurface < 0f || distanceFromSurface > landingAssistBand)
        {
            return;
        }

        float assist = 1f - Mathf.Clamp01(distanceFromSurface / Mathf.Max(0.01f, landingAssistBand));
        Vector2 velocity = body.linearVelocity;
        float inwardSpeed = Vector2.Dot(velocity, directionToPlanet);

        if (inwardSpeed <= 0f)
        {
            return;
        }

        Vector2 radialVelocity = directionToPlanet * inwardSpeed;
        float dampingStep = landingDamping * assist * deltaTime;
        body.linearVelocity = velocity - (radialVelocity * Mathf.Clamp01(dampingStep));
    }
}
