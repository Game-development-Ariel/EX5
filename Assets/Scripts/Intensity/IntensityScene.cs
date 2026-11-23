using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A hard scene that has two versions: Implied and Direct. On Awake it reads the current intensity
/// and instantiates the matching prefab under mountPoint. If this scene is the festival opening
/// and the player chose to skip it, nothing is instantiated and skippedOpeningEvent is raised so
/// the game can jump straight to the next scene (the cell).
/// </summary>
public class IntensityScene : MonoBehaviour
{
    [Tooltip("Pairs of prefabs. Each pair has one Implied and one Direct variant")]
    [SerializeField] private List<IntensityVariant> variants = new List<IntensityVariant>();

    [Tooltip("Where the chosen prefabs are instantiated. If empty, this transform is used")]
    [SerializeField] private Transform mountPoint;

    [Tooltip("True only for the festival opening scene. When true, skipOpening skips this scene")]
    [SerializeField] private bool isOpening;

    [Tooltip("The intensity settings. Falls back to ContentIntensityManager.Current if empty")]
    [SerializeField] private ContentIntensity intensity;

    [Header("Raises")]
    [Tooltip("Raised when the opening is skipped. Listeners load the next scene (the cell)")]
    [SerializeField] private GameEvent skippedOpeningEvent;

    /// <summary>The prefabs that were instantiated by this scene, in the order of variants.</summary>
    public List<GameObject> Instances { get; } = new List<GameObject>();

    /// <summary>True after Spawn ran and decided to skip the opening.</summary>
    public bool SkippedOpening { get; private set; }

    private void Awake()
    {
        Spawn();
    }

    /// <summary>
    /// Picks the right variant for every pair and instantiates it under mountPoint.
    /// In Implied mode it uses impliedPrefab, in Direct mode directPrefab. A null side is skipped.
    /// </summary>
    public void Spawn()
    {
        ContentIntensity current = intensity != null ? intensity : ContentIntensityManager.Current;
        if (current == null)
        {
            return;
        }

        if (isOpening && current.SkipOpening)
        {
            SkippedOpening = true;
            if (skippedOpeningEvent != null)
            {
                skippedOpeningEvent.Raise();
            }
            return;
        }

        Transform parent = mountPoint != null ? mountPoint : transform;
        foreach (IntensityVariant variant in variants)
        {
            GameObject chosen = current.Mode == IntensityLevel.Direct ? variant.directPrefab : variant.impliedPrefab;
            if (chosen == null)
            {
                continue;
            }
            Instances.Add(Instantiate(chosen, parent));
        }
    }
}
