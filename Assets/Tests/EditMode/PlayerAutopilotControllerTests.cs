#if UNITY_INCLUDE_TESTS
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class PlayerAutopilotControllerTests
{
    [TearDown]
    public void TearDown()
    {
        GameSettings.SetMovementControl(MovementControlType.NewtonianPhysics);

        GameObject[] gameObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        for (int i = 0; i < gameObjects.Length; i++)
        {
            Object.DestroyImmediate(gameObjects[i]);
        }
    }

    [Test]
    public void SetDestinationStoresDestinationAndRouteEndpoints()
    {
        PlayerAutopilotController autopilot = CreateAutopilot(Vector2.zero);

        autopilot.SetDestination(new Vector2(20f, 0f));

        Assert.IsTrue(autopilot.HasDestination);
        Assert.That(autopilot.Destination.x, Is.EqualTo(20f).Within(0.001f));
        Assert.That(autopilot.Waypoints.Count, Is.GreaterThanOrEqualTo(2));
        Assert.That(autopilot.Waypoints[0], Is.EqualTo(Vector2.zero));
        Assert.That(autopilot.Waypoints[autopilot.Waypoints.Count - 1].x, Is.EqualTo(20f).Within(0.001f));
    }

    [Test]
    public void ManualMovementInputCancelsAutopilot()
    {
        PlayerAutopilotController autopilot = CreateAutopilot(Vector2.zero, true);
        PlayerMovement movement = autopilot.GetComponent<PlayerMovement>();
        typeof(PlayerMovement)
            .GetField("thrustInput", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(movement, 1f);

        autopilot.SetDestination(new Vector2(30f, 0f));
        autopilot.SendMessage("FixedUpdate");

        Assert.IsFalse(autopilot.HasDestination);
    }

    [Test]
    public void ArrivalWithinRadiusClearsDestination()
    {
        PlayerAutopilotController autopilot = CreateAutopilot(Vector2.zero);
        autopilot.arrivalRadius = 5f;

        autopilot.SetDestination(new Vector2(2f, 0f));
        autopilot.SendMessage("FixedUpdate");

        Assert.IsFalse(autopilot.HasDestination);
    }

    [Test]
    public void RouteGenerationInsertsWaypointAroundBlockingCollider()
    {
        PlayerAutopilotController autopilot = CreateAutopilot(new Vector2(-10f, 0f));
        autopilot.routeProbeRadius = 1f;
        autopilot.obstacleClearance = 4f;
        CreateBlocker("Blocker", Vector2.zero, 2f);
        Physics2D.SyncTransforms();

        autopilot.SetDestination(new Vector2(10f, 0f));

        Assert.That(autopilot.Waypoints.Count, Is.GreaterThanOrEqualTo(3));
        Assert.That(Mathf.Abs(autopilot.Waypoints[1].y), Is.GreaterThan(0.1f));
    }

    [Test]
    public void DestinationInsideBlockingColliderIsPushedOutsideClearance()
    {
        PlayerAutopilotController autopilot = CreateAutopilot(new Vector2(-10f, 0f));
        autopilot.routeProbeRadius = 1f;
        autopilot.obstacleClearance = 4f;
        CreateBlocker("Destination Blocker", Vector2.zero, 2f);
        Physics2D.SyncTransforms();

        autopilot.SetDestination(Vector2.zero);

        Assert.That(Vector2.Distance(Vector2.zero, autopilot.Destination), Is.GreaterThanOrEqualTo(6.999f));
    }

    [Test]
    public void MultipleBlockersCanProduceMultipleRouteWaypoints()
    {
        PlayerAutopilotController autopilot = CreateAutopilot(new Vector2(-20f, 0f));
        autopilot.routeProbeRadius = 1f;
        autopilot.obstacleClearance = 4f;
        autopilot.maxRouteIterations = 6;
        CreateBlocker("Left Blocker", new Vector2(-6f, 0f), 2f);
        CreateBlocker("Right Blocker", new Vector2(6f, 0f), 2f);
        Physics2D.SyncTransforms();

        autopilot.SetDestination(new Vector2(20f, 0f));

        Assert.That(autopilot.Waypoints.Count, Is.GreaterThanOrEqualTo(4));
        Assert.That(autopilot.Waypoints.Count, Is.LessThanOrEqualTo(autopilot.maxRouteIterations + 2));
    }

    [Test]
    public void SimpleModeAcceleratesUsingPlayerMovementAcceleration()
    {
        GameSettings.SetMovementControl(MovementControlType.Simple);
        PlayerAutopilotController autopilot = CreateAutopilot(Vector2.zero, true);
        autopilot.slowdownRadius = 1f;
        PlayerMovement movement = autopilot.GetComponent<PlayerMovement>();
        movement.simpleAcceleration = 12f;
        movement.simpleStopDeceleration = 44f;
        Rigidbody2D rb = autopilot.GetComponent<Rigidbody2D>();

        autopilot.SetDestination(new Vector2(0f, 100f));
        InvokeFixedUpdate(autopilot);

        Vector2 expected = PlayerMovement.CalculateSimpleVelocity(
            Vector2.zero,
            Vector2.up,
            1f,
            movement.simpleAcceleration,
            movement.simpleStopDeceleration,
            Time.fixedDeltaTime);
        Assert.That(rb.linearVelocity.x, Is.EqualTo(expected.x).Within(0.001f));
        Assert.That(rb.linearVelocity.y, Is.EqualTo(expected.y).Within(0.001f));
    }

    [Test]
    public void PlayerMovementDoesNotDampenSimpleAutopilotMovement()
    {
        GameSettings.SetMovementControl(MovementControlType.Simple);
        PlayerAutopilotController autopilot = CreateAutopilot(Vector2.zero, true);
        autopilot.slowdownRadius = 1f;
        PlayerMovement movement = autopilot.GetComponent<PlayerMovement>();
        movement.simpleAcceleration = 12f;
        movement.simpleStopDeceleration = 44f;
        Rigidbody2D rb = autopilot.GetComponent<Rigidbody2D>();

        autopilot.SetDestination(new Vector2(0f, 100f));
        autopilot.gameObject.SendMessage("FixedUpdate");

        Vector2 expected = PlayerMovement.CalculateSimpleVelocity(
            Vector2.zero,
            Vector2.up,
            1f,
            movement.simpleAcceleration,
            movement.simpleStopDeceleration,
            Time.fixedDeltaTime);
        Assert.That(rb.linearVelocity.x, Is.EqualTo(expected.x).Within(0.001f));
        Assert.That(rb.linearVelocity.y, Is.EqualTo(expected.y).Within(0.001f));
    }

    [Test]
    public void NewtonianModeAppliesPlayerMovementThrustForce()
    {
        GameSettings.SetMovementControl(MovementControlType.NewtonianPhysics);
        PlayerAutopilotController autopilot = CreateAutopilot(Vector2.zero, true);
        autopilot.slowdownRadius = 1f;
        PlayerMovement movement = autopilot.GetComponent<PlayerMovement>();
        movement.thrustForce = 50f;
        Rigidbody2D rb = autopilot.GetComponent<Rigidbody2D>();

        autopilot.SetDestination(new Vector2(0f, 100f));
        InvokeFixedUpdate(autopilot);
        SimulatePhysicsStep();

        Assert.That(rb.linearVelocity.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(rb.linearVelocity.y, Is.EqualTo(movement.thrustForce * Time.fixedDeltaTime / rb.mass).Within(0.001f));
    }

    [Test]
    public void SimpleModeDoesNotClampVelocityToOldCruiseSpeed()
    {
        GameSettings.SetMovementControl(MovementControlType.Simple);
        PlayerAutopilotController autopilot = CreateAutopilot(Vector2.zero, true);
        autopilot.slowdownRadius = 1f;
        PlayerMovement movement = autopilot.GetComponent<PlayerMovement>();
        movement.simpleAcceleration = 1000f;
        Rigidbody2D rb = autopilot.GetComponent<Rigidbody2D>();

        autopilot.SetDestination(new Vector2(0f, 100f));
        InvokeFixedUpdate(autopilot);

        Assert.That(rb.linearVelocity.y, Is.GreaterThan(18f));
    }

    [Test]
    public void SimpleModeBrakesNearDestinationUsingPlayerStopDeceleration()
    {
        GameSettings.SetMovementControl(MovementControlType.Simple);
        PlayerAutopilotController autopilot = CreateAutopilot(Vector2.zero, true);
        autopilot.arrivalRadius = 1f;
        autopilot.slowdownRadius = 10f;
        PlayerMovement movement = autopilot.GetComponent<PlayerMovement>();
        movement.simpleAcceleration = 100f;
        movement.simpleStopDeceleration = 20f;
        Rigidbody2D rb = autopilot.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.up * 10f;

        autopilot.SetDestination(new Vector2(0f, 3.2f));
        InvokeFixedUpdate(autopilot);

        Assert.That(rb.linearVelocity.y, Is.EqualTo(10f - (movement.simpleStopDeceleration * Time.fixedDeltaTime)).Within(0.001f));
    }

    [Test]
    public void NewtonianModeBrakesNearDestinationUsingPlayerBrakeDeceleration()
    {
        GameSettings.SetMovementControl(MovementControlType.NewtonianPhysics);
        PlayerAutopilotController autopilot = CreateAutopilot(Vector2.zero, true);
        autopilot.arrivalRadius = 1f;
        autopilot.slowdownRadius = 10f;
        PlayerMovement movement = autopilot.GetComponent<PlayerMovement>();
        movement.thrustForce = 100f;
        movement.brakeDeceleration = 20f;
        Rigidbody2D rb = autopilot.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.up * 10f;

        autopilot.SetDestination(new Vector2(0f, 3.2f));
        InvokeFixedUpdate(autopilot);

        Assert.That(rb.linearVelocity.y, Is.EqualTo(10f - (movement.brakeDeceleration * Time.fixedDeltaTime)).Within(0.001f));
    }

    [Test]
    public void NewtonianModeBrakesBeforeSlowdownRadiusWhenStoppingDistanceRequiresIt()
    {
        GameSettings.SetMovementControl(MovementControlType.NewtonianPhysics);
        PlayerAutopilotController autopilot = CreateAutopilot(Vector2.zero, true);
        autopilot.arrivalRadius = 1f;
        autopilot.slowdownRadius = 10f;
        PlayerMovement movement = autopilot.GetComponent<PlayerMovement>();
        movement.thrustForce = 100f;
        movement.brakeDeceleration = 20f;
        Rigidbody2D rb = autopilot.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.up * 70f;

        autopilot.SetDestination(new Vector2(0f, 100f));
        InvokeFixedUpdate(autopilot);

        Assert.That(rb.linearVelocity.y, Is.EqualTo(70f - (movement.brakeDeceleration * Time.fixedDeltaTime)).Within(0.001f));
    }

    [Test]
    public void NewtonianModeStillAcceleratesTowardNearbyDestinationWhenStopped()
    {
        GameSettings.SetMovementControl(MovementControlType.NewtonianPhysics);
        PlayerAutopilotController autopilot = CreateAutopilot(Vector2.zero, true);
        autopilot.arrivalRadius = 1f;
        autopilot.slowdownRadius = 10f;
        PlayerMovement movement = autopilot.GetComponent<PlayerMovement>();
        movement.thrustForce = 50f;
        Rigidbody2D rb = autopilot.GetComponent<Rigidbody2D>();

        autopilot.SetDestination(new Vector2(0f, 5f));
        InvokeFixedUpdate(autopilot);
        SimulatePhysicsStep();

        Assert.That(rb.linearVelocity.y, Is.EqualTo(movement.thrustForce * Time.fixedDeltaTime / rb.mass).Within(0.001f));
    }

    private static PlayerAutopilotController CreateAutopilot(Vector2 position, bool includeMovement = false)
    {
        GameObject player = new GameObject("Player");
        player.transform.position = position;
        player.AddComponent<Rigidbody2D>();
        player.AddComponent<BoxCollider2D>();
        if (includeMovement)
        {
            player.AddComponent<PlayerMovement>();
        }

        PlayerAutopilotController autopilot = player.AddComponent<PlayerAutopilotController>();
        Physics2D.SyncTransforms();
        return autopilot;
    }

    private static void CreateBlocker(string name, Vector2 position, float radius)
    {
        GameObject blocker = new GameObject(name);
        blocker.transform.position = position;
        CircleCollider2D collider = blocker.AddComponent<CircleCollider2D>();
        collider.radius = radius;
    }

    private static void InvokeFixedUpdate(PlayerAutopilotController autopilot)
    {
        typeof(PlayerAutopilotController)
            .GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(autopilot, null);
    }

    private static void SimulatePhysicsStep()
    {
        SimulationMode2D originalMode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        Physics2D.Simulate(Time.fixedDeltaTime);
        Physics2D.simulationMode = originalMode;
    }
}
#endif
