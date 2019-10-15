using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [FormerlySerializedAs("movementSpeed")]
    [Header("Character normal form")]
    [Range(1f, 10f)] public float maxSpeed = 5f;
    [Header("Set multiplier to 1 for no effect")]
    [Range(0.1f, 1f)] public float sneakMultiplier = 0.5f;
    [Range(1f, 2f)] public float runMultiplier = 1.5f;
    [Range(0f, 5f)] public float accelerationTime = 1.5f;
    [Range(0f, 5f)] public float decelerationTime = 1.5f;
    [Range(0f, 5f)] public float accelerationRunTime = 1f;
    [Range(0f, 5f)] public float decelerationRunTime = 3f;

    [FormerlySerializedAs("morkoMovementSpeed")]
    [Header("Mörkö")]
    [Space(10)]
    
    [Range(1f, 10f)] public float morkoMaxSpeed = 5f;
    [Header("Set multiplier to 1 for no effect")]
    [Range(0.1f, 1f)] public float morkoSneakMultiplier = 0.3f;
    [Range(1f, 2f)] public float morkoRunMultiplier = 1.2f;
    [Range(0f, 5)] public float morkoAccelerationTime = 1.5f;
    [Range(0f, 5)] public float morkoDecelerationTime = 1.5f;
}