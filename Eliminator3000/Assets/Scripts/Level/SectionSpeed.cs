using UnityEngine;

/// <summary>Per-section speed multiplier for the scrolling track. Edited in the Level Manager.</summary>
[DisallowMultipleComponent]
public class SectionSpeed : MonoBehaviour
{
    public const float MaxMultiplier = 10f;

    [Range(0.1f, MaxMultiplier)] public float speedMultiplier = 1f;
}