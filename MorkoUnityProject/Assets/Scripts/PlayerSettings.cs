using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Character normal form")]
    [Range(1f, 10f)] public float movementSpeed = 5f;
    [Header("Set multiplier to 1 for no effect")]
    [Range(0.1f, 1f)] public float sneakMultiplier = 0.5f;
    [Range(1f, 2f)] public float runMultiplier = 1.5f;

    [Header("Mörkö")]
    [Space(10)]
    
    [Range(1f, 10f)] public float morkoMovementSpeed = 5f;
    [Header("Set multiplier to 1 for no effect")]
    [Range(0.1f, 1f)] public float morkoSneakMultiplier = 0.3f;
    [Range(1f, 2f)] public float morkoRunMultiplier = 1.2f;
}