using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A hard scene that has two versions: Implied and Direct. On Awake it reads the current intensity
/// and instantiates the matching prefab under mountPoint.
/// </summary>
public class IntensityScene : MonoBehaviour
{
    [Tooltip("Pairs of prefabs. Each pair has one Implied and one Direct variant")]
    [SerializeField] private List<IntensityVariant> variants = new List<IntensityVariant>();

    [Tooltip("Where the chosen prefabs are instantiated. If empty, this transform is used")]
    [SerializeField] private Transform mountPoint;

    [Tooltip("The intensity settings. Falls back to ContentIntensityManager.Current if empty")]
    [SerializeField] private ContentIntensity intensity;

    /// <summary>The prefabs that were instantiated by this scene, in the order of variants.</summary>
    public List<GameObject> Instances { get; } = new List<GameObject>();

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
