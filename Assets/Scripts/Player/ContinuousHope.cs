using System;
using UnityEngine;

/// <summary>
/// A decimal hope value on a fixed 0..100 scale, sitting alongside the integer <see cref="Hope"/>
/// so slow effects like natural decay per minute can land in fractions without flipping the integer
/// every frame. When the fractional value crosses an integer boundary the integer <see cref="Hope"/>
/// is updated through <see cref="Hope.Add"/> / <see cref="Hope.Lose"/> so existing listeners keep working.
///
/// Master plan 6.1, problem 2: hope must be decimal with a fixed 0..100 scale and a separate ceiling.
/// This component is the decimal side of that fix without changing the existing integer API.
/// </summary>
[RequireComponent(typeof(Hope))]
public class ContinuousHope : MonoBehaviour
{
    // Hope runs from 0 to FullScale. A change smaller than Epsilon is not reported, so listeners don't get noise.
    public const float FullScale = 100f;
    public const float Epsilon = 0.001f;

    private Hope hope;

    /// <summary>The decimal hope value on the fixed 0..100 scale.</summary>
    public float Value { get; private set; }

    /// <summary>Fires whenever the decimal value changes, with (old, new).</summary>
    public event Action<float, float> ValueChanged;

    private void Awake()
    {
        hope = GetComponent<Hope>();
        Value = hope.Current; // Hope sets itself in its own Awake. Start reads it again, in case Hope woke up later.
    }

    private void OnEnable()
    {
        hope.Changed += OnIntegerHopeChanged;
    }

    private void OnDisable()
    {
        hope.Changed -= OnIntegerHopeChanged;
    }

    private void Start()
    {
        Value = hope.Current;
    }

    // A memory or a capture changes the integer hope directly. The decimal value follows it,
    // so the two never drift apart. A change that came from this component already matches.
    private void OnIntegerHopeChanged(int newHope)
    {
        if (Mathf.Abs(Value - newHope) < 1f)
        {
            return; // Less than a whole point apart: the decimal value is only ahead by its fraction.
        }

        float previous = Value;
        Value = newHope;
        ValueChanged?.Invoke(previous, Value);
    }

    /// <summary>Add decimal hope. Flushes to the integer <see cref="Hope"/> when a whole point accumulates.</summary>
    public void Add(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        SetValue(Value + amount);
    }

    /// <summary>Lose decimal hope. Flushes to the integer <see cref="Hope"/> when a whole point accumulates.</summary>
    public void Lose(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        SetValue(Value - amount);
    }

    private void SetValue(float raw)
    {
        float clamped = Mathf.Clamp(raw, 0f, FullScale);
        float previous = Value;
        Value = clamped;

        FlushToInteger(previous);

        if (Mathf.Abs(Value - previous) > Epsilon)
        {
            ValueChanged?.Invoke(previous, Value);
        }
    }

    // The integer hope moves only when a whole point has gathered since it last moved,
    // so a slow decay of 0.9 changes nothing and the next 0.2 takes one point.
    private void FlushToInteger(float previous)
    {
        int delta = (int)(Value - hope.Current); // The cast drops the fraction, toward zero.

        if (delta > 0)
        {
            hope.Add(delta);
        }
        else if (delta < 0)
        {
            hope.Lose(-delta);
        }
    }
}
