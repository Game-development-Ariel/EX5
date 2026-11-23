using System;
using UnityEngine;

/// <summary>
/// Helper for raising events in a way that isolates a single failing listener. The project's
/// built-in <see cref="GameEvent.Raise"/> relies on the C# delegate invocation path, where a
/// thrown exception from any listener silently cancels the rest. The master plan calls this
/// out as problem 3 under section 6.1 — new systems should use this helper to call their
/// listeners, so one broken listener does not take the rest down with it.
///
/// Prefer <see cref="RaiseIsolated{T}"/> on typed events; use <see cref="RaiseIsolated"/> for
/// parameterless <see cref="Action"/> lists.
/// </summary>
public static class SafeEventRaise
{
    /// <summary>
    /// Invokes every listener on a parameterless event. Exceptions from one listener are logged
    /// and the remaining listeners still run.
    /// </summary>
    public static void RaiseIsolated(Action evt)
    {
        if (evt == null)
        {
            return;
        }

        foreach (Delegate listener in evt.GetInvocationList())
        {
            try
            {
                ((Action)listener).Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    /// <summary>
    /// Invokes every listener on a typed event. Exceptions from one listener are logged and
    /// the remaining listeners still run.
    /// </summary>
    public static void RaiseIsolated<T>(Action<T> evt, T payload)
    {
        if (evt == null)
        {
            return;
        }

        foreach (Delegate listener in evt.GetInvocationList())
        {
            try
            {
                ((Action<T>)listener).Invoke(payload);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    /// <summary>
    /// Invokes every listener on an event with two typed parameters. Exceptions from one
    /// listener are logged and the remaining listeners still run.
    /// </summary>
    public static void RaiseIsolated<T1, T2>(Action<T1, T2> evt, T1 payloadA, T2 payloadB)
    {
        if (evt == null)
        {
            return;
        }

        foreach (Delegate listener in evt.GetInvocationList())
        {
            try
            {
                ((Action<T1, T2>)listener).Invoke(payloadA, payloadB);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
