using UnityEngine;

/// <summary>
/// Looks for the target inside a view cone, with rays that stop at walls, and raises an event when it sees it.
/// It looks along the object's "up" direction. Whoever turns the object (like CameraSweep) decides where it looks.
/// </summary>
public class VisionSensor : MonoBehaviour
{
    [SerializeField] private VisionStats stats;
    [SerializeField] private Transform target;

    [Tooltip("Raised when the target is seen")]
    [SerializeField] private GameEvent targetSeen;

    // A view can't be wider than a full turn, in degrees.
    private const float MaxViewAngle = 360f;

    // Closer than this, in world units, the target is right on the sensor.
    private const float OnSensorDistance = 0.001f;

    // More than 0 for a short time after seeing the target.
    private float alarmTimer;

    /// <summary>Multiplies the width of the view. The difficulty sets it: a narrower cone leaves more dark to hide in.</summary>
    public float AngleFactor { get; set; } = 1f;

    /// <summary>How wide it sees right now, in degrees: the stats' angle times the difficulty's factor.</summary>
    public float ViewAngle => Mathf.Min(stats.viewAngle * AngleFactor, MaxViewAngle);

    public VisionStats Stats => stats;
    public bool IsAlarmed => alarmTimer > 0f;

    private void Update()
    {
        if (IsAlarmed)
        {
            alarmTimer -= Time.deltaTime;
        }
        else if (CanSeeTarget())
        {
            alarmTimer = stats.alarmSeconds;
            targetSeen.Raise();
        }
    }

    /// <summary>
    /// How far a ray from here goes before it hits a wall, up to the view distance.
    /// The cone drawing uses this too, so what the cone shows is exactly what the sensor sees.
    /// </summary>
    public float DistanceToWall(Vector3 direction)
    {
        // The player is on the Ignore Raycast layer and triggers (like the bottle) are skipped,
        // so only walls can stop the ray.
        bool hitWall = Physics.Raycast(transform.position, direction, out RaycastHit hit, stats.viewDistance,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        return hitWall ? hit.distance : stats.viewDistance;
    }

    private bool CanSeeTarget()
    {
        if (target == null)
        {
            return false;
        }

        Vector2 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;
        if (distance < OnSensorDistance)
        {
            return true; // Right on the sensor. There is no direction to check, and nothing can hide it.
        }
        bool insideCone = distance <= stats.viewDistance && Vector2.Angle(transform.up, toTarget) <= ViewAngle / 2f;
        if (!insideCone)
        {
            return false;
        }

        // Seen only if no wall is closer than the target along the line between them.
        return DistanceToWall(toTarget / distance) >= distance;
    }
}
