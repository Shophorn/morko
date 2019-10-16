using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Character normal form")]
    [Range(1f, 10f)] public float walkSpeed = 5f;
    [Range(1f, 10f)] public float sideSpeed = 3f;
    [Range(1f, 10f)] public float backwardSpeed = 2.5f;
    [Header("Set multiplier to 1 for no effect")]
    [Range(0.1f, 1f)] public float sneakMultiplier = 0.5f;
    [Range(1f, 2f)] public float runMultiplier = 1.5f;
    
    public float sneakSpeed => walkSpeed * sneakMultiplier;
    public float runSpeed => walkSpeed * runMultiplier;

    [Range(0f, 5f)] public float accelerationTime = 1.5f;
    [Range(0f, 5f)] public float decelerationTime = 1.5f;
    [Range(0f, 5f)] public float accelerationRunTime = 1f;
    [Range(0f, 5f)] public float decelerationRunTime = 3f;

    [Header("Mörkö")]
    [Space(10)]
    
    [Range(1f, 10f)] public float morkoWalkSpeed = 5f;
    [Range(1f, 10f)] public float morkoSideSpeed = 3f;
    [Range(1f, 10f)] public float morkoBackwardSpeed = 2.5f;
    [Header("Set multiplier to 1 for no effect")]
    [Range(0.1f, 1f)] public float morkoSneakMultiplier = 0.3f;
    [Range(1f, 2f)] public float morkoRunMultiplier = 1.2f;
    
    public float morkoSneakSpeed => morkoWalkSpeed * morkoSneakMultiplier;
    public float morkoRunSpeed => morkoWalkSpeed * morkoRunMultiplier;
    
    [Range(0f, 5)] public float morkoAccelerationTime = 1.5f;
    [Range(0f, 5)] public float morkoDecelerationTime = 1.5f;
    [Range(0f, 5f)] public float morkoAccelerationRunTime = 1f;
    [Range(0f, 5f)] public float morkoDecelerationRunTime = 3f;
}