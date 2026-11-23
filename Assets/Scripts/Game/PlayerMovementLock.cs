using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The master-plan fix for the "every frame overwrite" of <see cref="PlayerMovement"/>.enabled
/// (master plan section 6.1, problem 1). Any system that needs to freeze the player adds a
/// reason to this lock, and removes it when its work is done. The player moves only when the
/// lock is empty. The reason string is also useful for debugging — a developer can inspect
/// <see cref="ActiveReasons"/> to see why movement is frozen right now.
///
/// The existing <see cref="LevelFreeze"/> stays as-is until the repo-wide refactor lands —
/// new systems (dig, prayer, journal) should prefer this lock so they don't fight each other.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerMovementLock : MonoBehaviour
{
    private readonly HashSet<string> reasons = new HashSet<string>();
    private PlayerMovement movement;
    private bool desiredEnabled = true;

    /// <summary>The reasons currently holding the player still, for debugging.</summary>
    public IReadOnlyCollection<string> ActiveReasons => reasons;

    /// <summary>True when no reasons are held, so the player can move.</summary>
    public bool IsFree => reasons.Count == 0;

    /// <summary>Fires whenever a reason is added or removed, with the new <see cref="IsFree"/>.</summary>
    public event Action<bool> FreeChanged;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        Apply();
    }

    /// <summary>
    /// Add a reason to freeze the player. Idempotent per reason — adding the same reason twice
    /// still requires one <see cref="Release"/> to clear it. Case-sensitive.
    /// </summary>
    public void Hold(string reason)
    {
        if (string.IsNullOrEmpty(reason))
        {
            throw new ArgumentException("A reason is required", nameof(reason));
        }

        bool wasFree = IsFree;
        reasons.Add(reason);

        Apply();

        if (wasFree)
        {
            FreeChanged?.Invoke(IsFree);
        }
    }

    /// <summary>
    /// Remove a reason. If the same reason was never added, this is a no-op.
    /// </summary>
    public void Release(string reason)
    {
        if (string.IsNullOrEmpty(reason))
        {
            return;
        }

        bool wasFree = IsFree;
        if (!reasons.Remove(reason))
        {
            return;
        }

        Apply();

        if (!wasFree && IsFree)
        {
            FreeChanged?.Invoke(IsFree);
        }
    }

    /// <summary>
    /// Clear every reason. Use with care — this cancels every system's freeze at once.
    /// </summary>
    public void ReleaseAll()
    {
        if (reasons.Count == 0)
        {
            return;
        }

        bool wasFree = IsFree;
        reasons.Clear();
        Apply();

        if (!wasFree)
        {
            FreeChanged?.Invoke(IsFree);
        }
    }

    private void Apply()
    {
        if (movement == null)
        {
            return;
        }

        movement.enabled = desiredEnabled && IsFree;
    }

    /// <summary>
    /// Set the "outside" enabled state. The player ends up enabled only when this is true AND
    /// no reasons are held. Used by level opening and ending flows.
    /// </summary>
    public void SetDesiredEnabled(bool enabled)
    {
        desiredEnabled = enabled;
        Apply();
    }
}
