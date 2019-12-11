using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MaskController : MonoBehaviourPun
{
    public List<Transform> characterTransforms;
    public NavMeshAgent navMeshAgent;
    
    [Space]
    [Header("Mask Settings")]
    public float secondsBeforeMaskMovesAtStart;
    public float secondsBeforeMaskMovesToNewTarget;
    public float afterCollisionFreezeTime;
    public float startMovementSpeed;
    public float changingMovementSpeed;
    public float acceleration;
    public float jumpMinDistanceFromCharacter;
    [Range(0, 1)]
    public float jumpInterpolationCutOff = 0.9f;
    public float jumpParabolaSize;
    public Transform jumpSmokeEffectOnCollision;
    
    [Space]
    [Header("Animation Speeds")]
    public float moveSpeed = 1f;
    public float walkSpeed = 1f;
    public float runSpeed = 1f;
    public float jumpSpeed = 3f;
    public float breathingSpeed = 1f;

    [Space]
    [Header("Morko Info")]
    public Transform currentMorko;
    public Transform nextMorko;

    private Animator animator;
    private bool startWaitDurationWaited = false;
    private bool lookingForStartingMorko = false;
    private bool maskMovingToNewMorko = false;
    private bool maskJumpingOn = false;
    private bool maskJumpingOff = false;
    private bool maskIsBeingPutOn = false;

    private Vector3 startJumpingPosition;
    private Vector3 targetJumpingPosition;

    private Transform morkoHeadJoint;
    private Transform morkoNeckJoint;

    private PlayerController currenMorkoController;

    public bool startMaskControllerInStartMethod = true;
    public Vector3 maskOnToNeckOffset;
    public Vector3 maskOnToNeckRotation;

    // Todo(Leo): HACKHACKACK
    public bool IsTransferingToOtherCharacter { get; private set; }

    public enum AnimatorBooleans
    {
        Breathing,
        Idle,
        Move,
        Walk,
        WalkSidewaysLeft,
        WalkSidewaysRight,
        WalkBackwards,
        Run
    }

    private void Awake()
    {
        GameManager.RegisterMask(this);
    }

    private void Start()
    {
        navMeshAgent = transform.GetComponent<NavMeshAgent>();
        navMeshAgent.Warp(transform.position);
        animator = transform.GetComponent<Animator>();

        if (startMaskControllerInStartMethod)
            this.InvokeAfter (FindStartingCharacter, secondsBeforeMaskMovesAtStart);

        enabled = false;
    }

    private void Update()
    {
        if (photonView.IsMine == false)
            return;
        
        if (lookingForStartingMorko && startWaitDurationWaited && !maskJumpingOn && !maskIsBeingPutOn)
            FindStartingCharacter();
        
        else if (maskMovingToNewMorko && !maskJumpingOn && !maskJumpingOff && !maskIsBeingPutOn)
            CheckMaskDistanceFromCharacter(nextMorko);

        else if (maskJumpingOn)
            JumpToHead(nextMorko);
        
        else if (maskJumpingOff)
            JumpOffHead(Vector3.zero);

        if (!lookingForStartingMorko)
            SetAnimatorState();
    }

    public void StartMaskController()
    {
        this.InvokeAfter (FindStartingCharacter, secondsBeforeMaskMovesAtStart);
    }

    private void FindStartingCharacter()
    {
        enabled = true;

        startWaitDurationWaited = true;
        lookingForStartingMorko = true;
        nextMorko = FindClosestCharacter();
        
        morkoNeckJoint = nextMorko.transform.GetChild(1).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1);
        morkoHeadJoint = morkoNeckJoint.GetChild(0);
        
        MoveMaskToTarget(nextMorko, startMovementSpeed);
        CheckMaskDistanceFromCharacter(nextMorko);
    }

    private Transform FindClosestCharacter()
    {
        float distance = float.MaxValue;
        Transform closestCharacter = characterTransforms[Random.Range(0, characterTransforms.Count)];
        
        foreach (var c in characterTransforms)
        {
            var characterDistance = Vector3.Distance(transform.position, c.position);
            if (characterDistance < distance)
            {
                distance = characterDistance;
                closestCharacter = c;
            }
        }
        return closestCharacter;
    }
    
    public void SwitchMorko(Transform newMorko)
    {
        IsTransferingToOtherCharacter = true;

        nextMorko = newMorko;
        currentMorko = null;
        currenMorkoController.isMorko = false;
        currenMorkoController = null;
        
        maskJumpingOn = false;
        maskIsBeingPutOn = false;
        
        JumpOffHead(nextMorko.position);
    }
    
    private void MoveMaskToTarget(Transform target, float speed)
    {
        maskMovingToNewMorko        = true;
        animator.applyRootMotion    = true;
        SetAnimatorState(AnimatorBooleans.Move);
        
        navMeshAgent.enabled        = true;
        navMeshAgent.speed          = speed;
        navMeshAgent.acceleration   = acceleration;
        navMeshAgent.destination    = target.position;
    }
    
    private void CheckMaskDistanceFromCharacter(Transform target)
    {
        var distance = Vector3.Distance(transform.position, target.position);
        if (distance <= jumpMinDistanceFromCharacter)
            JumpToHead(target);
    }
    
    // Hack(Leo): HACKHACKHACK
    // Note(Leo): timeToJumpToHead must be the length of attack animation. It is't because animation triggering does not work...
    float timeToJumpToHead = 0.4f;
    float currentJumpInterpolation = 0.0f;
    Vector3 jumpStartPosition;
    Transform jumpTargetTransform;

    private void JumpToHead(Transform REMOVE_ME_IM_UNUSED)
    {
        navMeshAgent.enabled = false;
        if (maskJumpingOn == false)
        {
            maskJumpingOn = true;

            jumpStartPosition           = transform.position;
            currentJumpInterpolation    = 0.0f;
            jumpTargetTransform         = morkoHeadJoint.transform;

            animator.SetTrigger("JumpToHead");
        }

        // Todo(Leo): Add parabola, but this is not how. Also wont bother now, because it is not seen in game anyway        
        // var targetPosition = jumpTargetTransform.position;
        // targetPosition = new Vector3(targetPosition.x, targetPosition.y + (jumpParabolaSize - interpolation * jumpParabolaSize), targetPosition.z);

        currentJumpInterpolation += Time.deltaTime / timeToJumpToHead;
        transform.position = Vector3.Lerp(jumpStartPosition, jumpTargetTransform.position, currentJumpInterpolation);
        transform.LookAt(jumpTargetTransform.position);

        if (currentJumpInterpolation >= 1.0f)
        {
            maskJumpingOn = false;
            SetAnimatorState(AnimatorBooleans.Idle);
         
            MaskOnNewMorko();
        }

        // animator.Play("Attack", 0, currentJumpInterpolation);

        /*
        navMeshAgent.enabled = false;

        if (maskJumpingOn == false)
        {
            startJumpingPosition = transform.position;
        }
        
        Vector3 toMorkoHeadPosition = morkoHeadJoint.position;
        
        float distanceToTarget        = Vector3.Distance(startJumpingPosition, toMorkoHeadPosition);
        float currentDistanceToTarget = Vector3.Distance(transform.position, toMorkoHeadPosition);
        float interpolation         = (distanceToTarget - currentDistanceToTarget) / distanceToTarget;
        interpolation               = Mathf.Clamp01(interpolation);

        toMorkoHeadPosition = new Vector3(toMorkoHeadPosition.x, toMorkoHeadPosition.y + (jumpParabolaSize - interpolation * jumpParabolaSize), toMorkoHeadPosition.z);

        transform.position = Vector3.MoveTowards(transform.position, toMorkoHeadPosition, Time.deltaTime * jumpSpeed);
        transform.forward = toMorkoHeadPosition - transform.position;
        animator.Play("Attack", 0, interpolation);
        
        maskJumpingOn = true;

        if (interpolation >= jumpInterpolationCutOff)
        {
            maskJumpingOn = false;
            maskIsBeingPutOn = true;
            Instantiate(jumpSmokeEffectOnCollision, transform.position, Quaternion.identity);
            animator.Play("Idle");
            MaskOnNewMorko();
        }
        */
    }

    private void JumpOffHead(Vector3 targetLocation)
    {
        navMeshAgent.enabled = false;

        if (!maskJumpingOff)
        {
            MaskOffMorko();
            animator.applyRootMotion = true;
            
            startJumpingPosition = transform.localPosition;

            // Note(Leo): Test if should immediately jump on next morko
            float distanceToTarget = Vector2.Distance(new Vector2(startJumpingPosition.x, startJumpingPosition.z), new Vector2(targetLocation.x, targetLocation.z));
            if (distanceToTarget <= jumpMinDistanceFromCharacter)
            // if (distanceToTarget <= 2 * jumpMinDistanceFromCharacter)
            {
                JumpToHead(nextMorko);
                return;
            }
            
            Vector3 direction = Vector3.Normalize(targetLocation - startJumpingPosition);
            targetJumpingPosition = startJumpingPosition + direction * jumpMinDistanceFromCharacter;
            targetJumpingPosition.y = -5f;
            // targetJumpingPosition = new Vector3(targetJumpingPosition.x, -5f, targetJumpingPosition.z);
        }
        
        // Just a scope....
        {
            float distanceToTarget          = Vector3.Distance(startJumpingPosition, targetJumpingPosition);
            float currentDistanceToTarget   = Vector3.Distance(transform.position, targetJumpingPosition);
            float interpolation             = (distanceToTarget - currentDistanceToTarget) / distanceToTarget;
            interpolation                   = Mathf.Clamp01(interpolation);

            transform.position = Vector3.MoveTowards(transform.position, targetJumpingPosition, Time.deltaTime * jumpSpeed);
            animator.Play("Roar", 0, interpolation);
            
            maskJumpingOff = true;

            if (interpolation >= jumpInterpolationCutOff)
            {
                maskJumpingOff = false;
                SetAnimatorState(AnimatorBooleans.Idle);
                // animator.Play("Idle");
                this.InvokeAfter(() => MoveMaskToTarget(nextMorko, changingMovementSpeed), secondsBeforeMaskMovesToNewTarget);
            }
        }
    }
    
    public void MaskOffMorko()
    {
        transform.SetParent(null);
        var direction = (nextMorko.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        morkoHeadJoint.localScale = Vector3.one;
        
        morkoNeckJoint = nextMorko.transform.GetChild(1).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1);
        morkoHeadJoint = morkoNeckJoint.GetChild(0);
    }

    private void MaskOnNewMorko()
    {
        navMeshAgent.enabled = false;

        lookingForStartingMorko = false;
        maskMovingToNewMorko = false;
        maskJumpingOn = false;

        animator.applyRootMotion = false;
        morkoHeadJoint.localScale = Vector3.zero;
        transform.forward = nextMorko.forward;
        transform.SetParent(morkoNeckJoint);
        transform.localPosition = maskOnToNeckOffset;
        transform.forward = nextMorko.forward;
        transform.localRotation = Quaternion.Euler(maskOnToNeckRotation);

        SetAnimatorState(AnimatorBooleans.Idle);
        
        currentMorko = nextMorko;
        currenMorkoController = currentMorko.GetComponent<PlayerController>();
        currenMorkoController.isMorko = true;

        Instantiate(jumpSmokeEffectOnCollision, transform.position, Quaternion.identity);


        nextMorko.GetComponent<Character>().FreezeForSeconds(afterCollisionFreezeTime);

        IsTransferingToOtherCharacter = false;

        var newMorkoCharacter = currentMorko.GetComponent<Character>();
        if (newMorkoCharacter == null)
        {
            Debug.LogError("Bad newMorko, implement this class/function using Character instead of transform");
        }
        photonView.TransferOwnership(newMorkoCharacter.photonView.Owner);
        GameManager.SetCharacterMorko(newMorkoCharacter);
    }
    
    private void ResetAnimatorTriggers()
    {
        animator.ResetTrigger("Roar");
        animator.ResetTrigger("JumpToHead");
        animator.ResetTrigger("JumpOffHead");
    }

    public void SetAnimatorState(AnimatorBooleans state = default)
    {
        ResetAnimatorTriggers();
        animator.SetBool("Idle", false);
        animator.SetBool("Breathing", false);
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        animator.SetBool("Move", false);
        animator.SetBool("WalkSidewaysLeft", false);
        animator.SetBool("WalkSidewaysRight", false);
        animator.SetBool("WalkBackwards", false);

        if (currenMorkoController)
        {
            var morkoAnimation = currenMorkoController.currentAnimation;
            switch (morkoAnimation)
            {
                case PlayerController.AnimatorState.Idle:
                    animator.speed = breathingSpeed;
                    animator.SetBool("Idle", true);
                    break;
                case PlayerController.AnimatorState.Walk:
                    animator.speed = walkSpeed;
                    animator.SetBool("Walk", true);
                    break;
                case PlayerController.AnimatorState.WalkSidewaysLeft:
                    animator.speed = runSpeed;
                    animator.SetBool("WalkSidewaysLeft", true);
                    break;
                case PlayerController.AnimatorState.WalkSidewaysRight:
                    animator.speed = runSpeed;
                    animator.SetBool("WalkSidewaysRight", true);
                    break;
                case PlayerController.AnimatorState.WalkBackwards:
                    animator.speed = runSpeed;
                    animator.SetBool("WalkBackwards", true);
                    break;
                case PlayerController.AnimatorState.Run:
                    animator.speed = runSpeed;
                    animator.SetBool("Run", true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            switch (state)
            {
                case AnimatorBooleans.Breathing:
                    animator.speed = breathingSpeed;
                    animator.SetBool("Breathing", true);

                    break;
                case AnimatorBooleans.Move:
                    animator.speed = moveSpeed;
                    animator.SetBool("Move", true);

                    break;
            } 
        }
    }
}
