using System;
using UnityEngine;

/// <summary>
/// A pair of prefabs for one scene: the Implied version and the Direct version. IntensityScene picks
/// the right one at runtime. Either side can be null, which means that this scene does not change
/// in that mode (the other prefab is used, or nothing is spawned).
/// </summary>
[Serializable]
public struct IntensityVariant
{
    [Tooltip("The Implied prefab, suggested by sound and cutaways. Shown in Implied mode")]
    public GameObject impliedPrefab;

    [Tooltip("The Direct prefab, shown in Direct mode. Follows the limits in the design doc")]
    public GameObject directPrefab;
}
