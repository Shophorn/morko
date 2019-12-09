using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings")]
public class PlayerSettings : ScriptableObject //, IPlayerFields
{
    [Range(1f, 10f)] public float walkSpeed = 5f;
    [Header("Set multiplier to 1 for no effect")]
    [Range(0.1f, 1f)] public float sneakMultiplier = 0.5f;
    [Range(1f, 10f)] public float runMultiplier = 1.5f;
    [Range(0f, 1f)] public float sideMultiplier = 1;
    [Range(0f, 1f)] public float sideRunMultiplier = 1;
    [Range(0f, 1f)] public float backwardMultiplier = 1f;
    [Range(0f, 1f)] public float backwardRunMultiplier = 1f;
    public float rotationSpeed = 600f;
    
    public float sneakSpeed => walkSpeed * sneakMultiplier;
    public float runSpeed => walkSpeed * runMultiplier;
    public float sideSpeed => walkSpeed * sideMultiplier;
    public float backwardSpeed => walkSpeed * backwardMultiplier;
    public float sideRunSpeed => runSpeed * sideMultiplier;
    public float backwardRunSpeed => runSpeed * backwardMultiplier;

    [Space]
    [Range(0f, 5f)] public float accelerationTime = 1.5f;
    [Range(0f, 5f)] public float decelerationTime = 1.5f;
    [Range(0f, 5f)] public float accelerationRunTime = 1f;
    [Range(0f, 5f)] public float decelerationRunTime = 3f;
    
    public float accelerateSneak => walkSpeed / (accelerationTime + sneakSpeed);
    public float accelerationWalk => walkSpeed / accelerationTime;
    public float accelerationRun => walkSpeed / accelerationRunTime;
    public float decelerationWalk => walkSpeed / decelerationTime;
    public float decelerationRun => walkSpeed / decelerationRunTime;
    
    [Range(0f, 20f)] public float sprintSpeed = 2f;
    [Range(0f, 5f)] public float sprintDuration = 1.5f;
    [Range(0f, 10f)] public float sprintCooldown = 2f;
    public float sprintRotationSpeed = 180f;
    public float rotationBackToNormalSpeedInSecods = 0.25f;
    
    [Space]
    [Header("Animation Speeds")]
    public float minWalkAnimationSpeed = 1f;
    public float maxWalkAnimationSpeed = 1f;
    [Space]
    public float minSidewaysWalkAnimationSpeed = 1f;
    public float maxSidewaysWalkAnimationSpeed = 1f;
    [Space]
    public float minSidewaysRunAnimationSpeed = 1f;
    public float maxSidewaysRunAnimationSpeed = 1f;
    [Space]
    public float minBackwardsWalkAnimationSpeed = 1f;
    public float maxBackwardsWalkAnimationSpeed = 1f;
    [Space]
    public float minBackwardsRunAnimationSpeed = 1f;
    public float maxBackwardsRunAnimationSpeed = 1f;
    [Space]
    public float minRunAnimationSpeed = 1f;
    public float maxRunAnimationSpeed = 1f;
    [Space]
    public float sprintAnimationSpeed = 1f;

    [Header("TEST")]
    public bool rotateTowardsMove = true;
}