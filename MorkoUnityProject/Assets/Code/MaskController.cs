using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MaskController : MonoBehaviourPun, IPunObservable, IPunOwnershipCallbacks
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
    private PlayerController currentMorkoController;
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

    private Transform morkoHeadJoint => currentMorkoCharacter.Head;
    private Transform morkoNeckJoint => currentMorkoCharacter.MaskTarget;


    public bool startMaskControllerInStartMethod = true;
    public Vector3 maskOnToNeckOffset;
    public Vector3 maskOnToNeckRotation;

    public int currentMorkoActorNumber;

    // Todo(Leo): HACKHACKACK
    public bool IsTransferingToOtherCharacter { get; private set; }

    // HACKHACKHACK
    private Character currentMorkoCharacter => currentMorko.GetComponent<Character>();

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

    public enum MorkoState
    {
        IdleInBeginning,
        ChasingFirstTime,
        OnHead,
        SwitchingOff,
        SwitchingOn,
    }

    public MorkoState morkoState;// {get; private set;}

    // Note(Leo): Not used
    void IPunOwnershipCallbacks.OnOwnershipRequest (PhotonView targetView, Player requestingPlayer) {}
 
    void IPunOwnershipCallbacks.OnOwnershipTransfered (PhotonView targetView, Player previousOwner)
    {
        currentMorko = GameManager.GetCharacterByActorNumber(currentMorkoActorNumber).transform;
        currentMorkoController = currentMorko.GetComponent<PlayerController>();
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            int state = (int)morkoState;
            stream.SendNext(state);
        }
        else if (stream.IsReading)
        {
            morkoState = (MorkoState)stream.ReceiveNext();
        }
    }

    private void Awake()
    {
        GameManager.RegisterMask(this);
    }

    public Transform TEST;

    private void Start()
    {
        navMeshAgent = transform.GetComponent<NavMeshAgent>();
        navMeshAgent.Warp(transform.position);
        animator = transform.GetComponent<Animator>();

        if (startMaskControllerInStartMethod)
            this.InvokeAfter (  () => morkoState = MorkoState.ChasingFirstTime,
                                secondsBeforeMaskMovesAtStart);

        // enabled = false;
    }

    private void SetCurrentMorko(Transform newCurrentMorko)
    {
        currentMorkoController.isMorko = false;

        currentMorko = newCurrentMorko;

        if (currentMorko == null)
            return;

        currentMorkoActorNumber = currentMorko.GetComponent<Character>().photonView.Owner.ActorNumber;
        currentMorkoController = currentMorko.GetComponent<PlayerController>();
        currentMorkoController.isMorko = true;
    }

    private void Update()
    {
        if (photonView.IsMine == false)
            return;

        switch(morkoState)
        {
            case MorkoState.IdleInBeginning:
                break;

            case MorkoState.ChasingFirstTime:
                FindStartingCharacter();
                break;

            case MorkoState.OnHead:
                SetAnimatorState();
                transform.position = currentMorkoCharacter.MaskTarget.position + maskOnToNeckOffset;
                transform.rotation = currentMorkoCharacter.MaskTarget.rotation * Quaternion.Euler(maskOnToNeckRotation);
                break;

            case MorkoState.SwitchingOff:
                JumpOffHead(nextMorko.position);
                break;

            case MorkoState.SwitchingOn:
                JumpToHead();
                break;

        }

        
        // if (lookingForStartingMorko && startWaitDurationWaited && !maskJumpingOn && !maskIsBeingPutOn)
        //     FindStartingCharacter();
        
        // else if (maskMovingToNewMorko && !maskJumpingOn && !maskJumpingOff && !maskIsBeingPutOn)
        //     CheckMaskDistanceFromCharacter(nextMorko);

        // else if (maskJumpingOn)
        //     JumpToHead();
        
        // else if (maskJumpingOff)
        //     JumpOffHead(Vector3.zero);

        // if (!lookingForStartingMorko)
        // {
        // }
    }

    // public void StartMaskController()
    // {
    //     this.InvokeAfter (() => morkoState = MorkoState.ChasingFirstTime, secondsBeforeMaskMovesAtStart);
    // }

    private void FindStartingCharacter()
    {
        enabled = true;

        var previousNextMorko = nextMorko;
        nextMorko = FindClosestCharacter();
        
        if (previousNextMorko != nextMorko)
        {
            photonView.TransferOwnership(nextMorko.GetComponent<Character>().photonView.Owner);
        }

        // morkoNeckJoint = nextMorko.transform.GetChild(1).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1);
        // morkoHeadJoint = morkoNeckJoint.GetChild(0);
        
        // MoveMaskToTarget(nextMorko, startMovementSpeed);
        animator.applyRootMotion    = true;
        SetAnimatorState(AnimatorBooleans.Move);
        
        navMeshAgent.enabled        = true;
        navMeshAgent.speed          = startMovementSpeed;
        navMeshAgent.acceleration   = acceleration;
        navMeshAgent.destination    = nextMorko.position;
        CheckMaskDistanceFromCharacter(nextMorko);


        // enabled = true;

        // startWaitDurationWaited = true;
        // lookingForStartingMorko = true;
        // nextMorko = FindClosestCharacter();
        
        // morkoNeckJoint = nextMorko.transform.GetChild(1).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1);
        // morkoHeadJoint = morkoNeckJoint.GetChild(0);
        
        // MoveMaskToTarget(nextMorko, startMovementSpeed);
        // CheckMaskDistanceFromCharacter(nextMorko);
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
        MorkoSoundController msc = GetComponent<MorkoSoundController>();
        msc.PlayAttack();

        IsTransferingToOtherCharacter = true;

        nextMorko = newMorko;
        
        StartJumpOffHead();
        // JumpOffHead(nextMorko.position);
    }
    
    private void MoveMaskToTarget(Transform target, float speed)
    {
        // maskMovingToNewMorko        = true;
        // animator.applyRootMotion    = true;
        // SetAnimatorState(AnimatorBooleans.Move);
        
        // navMeshAgent.enabled        = true;
        // navMeshAgent.speed          = speed;
        // navMeshAgent.acceleration   = acceleration;
        // navMeshAgent.destination    = target.position;
    }
    
    private void CheckMaskDistanceFromCharacter(Transform target)
    {
        var distance = Vector3.Distance(transform.position, target.position);
        if (distance <= jumpMinDistanceFromCharacter)
        {
            // JumpToHead();
            StartJumpToHead();
            // morkoState = MorkoState.SwitchingOn;
        }

    }
    
    // Hack(Leo): HACKHACKHACK
    // Note(Leo): timeToJumpToHead must be the length of attack animation. It is't because animation triggering does not work...
    float timeToJumpToHead = 0.4f;
    float currentJumpInterpolation = 0.0f;
    Vector3 jumpStartPosition;
    Transform jumpTargetTransform;

    private void StartJumpToHead()
    {
        morkoState = MorkoState.SwitchingOn;

        jumpStartPosition           = transform.position;
        currentJumpInterpolation    = 0.0f;
        jumpTargetTransform         = nextMorko.GetComponent<Character>().Head;

        animator.SetTrigger("JumpToHead");        
    }

    private void JumpToHead()
    {
        navMeshAgent.enabled = false;

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
    }

    private void StartJumpOffHead()
    {
        // MaskOffMorko();

    // transform.SetParent(null);
        var direction = (nextMorko.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        morkoHeadJoint.localScale = Vector3.one;
        

        SetCurrentMorko(null);
        // currentMorko = null;
        // currentMorkoController.isMorko = false;
        currentMorkoController = null;


        Vector3 targetLocation = nextMorko.position;
        morkoState = MorkoState.SwitchingOff;

        animator.applyRootMotion = true;
        
        startJumpingPosition = transform.localPosition;

        // Note(Leo): Test if should immediately jump on next morko
        float distanceToTarget = Vector2.Distance(new Vector2(startJumpingPosition.x, startJumpingPosition.z), new Vector2(targetLocation.x, targetLocation.z));
        if (distanceToTarget <= jumpMinDistanceFromCharacter)
        // if (distanceToTarget <= 2 * jumpMinDistanceFromCharacter)
        {
            JumpToHead();
            return;
        }
        
        // TODO(Leo): REUSE 'direction' please fix
        direction = Vector3.Normalize(targetLocation - startJumpingPosition);
        targetJumpingPosition = startJumpingPosition + direction * jumpMinDistanceFromCharacter;
        targetJumpingPosition.y = -5f;
        // targetJumpingPosition = new Vector3(targetJumpingPosition.x, -5f, targetJumpingPosition.z);
    }

    private void JumpOffHead(Vector3 targetLocation)
    {
        navMeshAgent.enabled = false;

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
            JumpToHead();
            // SetAnimatorState(AnimatorBooleans.Idle);
            // // animator.Play("Idle");
            // this.InvokeAfter(() => MoveMaskToTarget(nextMorko, changingMovementSpeed), secondsBeforeMaskMovesToNewTarget);
        }
    }
    
    // public void MaskOffMorko()
    // {
        
    //     // morkoNeckJoint = nextMorko.transform.GetChild(1).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1);
    //     // morkoHeadJoint = morkoNeckJoint.GetChild(0);
    // }

    private void MaskOnNewMorko()
    {
        SetCurrentMorko(nextMorko);
        // currentMorko                    = nextMorko;
        // currentMorkoController           = currentMorko.GetComponent<PlayerController>();
        // currentMorkoController.isMorko   = true;

        navMeshAgent.enabled = false;
        morkoState = MorkoState.OnHead;
        // lookingForStartingMorko = false;
        // maskMovingToNewMorko    = false;
        // maskJumpingOn           = false;

        animator.applyRootMotion    = false;
        morkoHeadJoint.localScale   = Vector3.zero;

        // transform.SetParent(morkoNeckJoint);
        // transform.localPosition     = maskOnToNeckOffset;
        // transform.localRotation     = Quaternion.Euler(maskOnToNeckRotation);

        SetAnimatorState(AnimatorBooleans.Idle);
        

        Instantiate(jumpSmokeEffectOnCollision, transform.position, Quaternion.identity);
        MorkoSoundController msc = GetComponent<MorkoSoundController>();
        msc.PlayAttach();

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

        if (currentMorkoController)
        {
            var morkoAnimation = currentMorkoController.currentAnimation;
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
