using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
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
    private PlayerController currentMorkoController
    {
        get {   if (currentMorko == null)
                    return null;
                return currentMorko.GetComponent<PlayerController>();

            }
    }

    public Transform nextMorko;
    private Animator animator;
    
    private Transform morkoHeadJoint => currentMorkoCharacter.Head;
    private Transform morkoNeckJoint => currentMorkoCharacter.MaskTarget;


    public bool startMaskControllerInStartMethod = true;
    public Vector3 maskOnToNeckOffset;
    public Vector3 maskOnToNeckRotation;

    public int currentMorkoActorNumber;

    public AnimationCurve jumpOffCurve;
    public AnimationCurve jumpOnCurve;

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
            this.InvokeAfter (  () => morkoState = MorkoState.ChasingFirstTime,
                                secondsBeforeMaskMovesAtStart);

    }

    private void SetCurrentMorko(Transform newCurrentMorko)
    {   
        if (currentMorko != null)
            currentMorkoController.isMorko = false;

        currentMorko = newCurrentMorko;
        currentMorkoActorNumber = currentMorko.GetComponent<Character>().photonView.Owner.ActorNumber;
        currentMorkoController.isMorko = true;

        photonView.RPC(nameof(SetCurrentMorkoRPC), RpcTarget.Others, currentMorkoActorNumber);
    }

    [PunRPC]
    private void SetCurrentMorkoRPC(int actorNumber)
    {
        if (currentMorko != null)
            currentMorkoController.isMorko = false;
 
        currentMorkoActorNumber = actorNumber;
        currentMorko = GameManager.GetCharacterByActorNumber(actorNumber).transform;
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
                JumpOffHead();
                break;

            case MorkoState.SwitchingOn:
                JumpOnHead();
                break;

        }
    }

    private void FindStartingCharacter()
    {
        enabled = true;

        nextMorko = FindClosestCharacter();
        
        animator.applyRootMotion    = true;
        SetAnimatorState(AnimatorBooleans.Move);
        
        navMeshAgent.enabled        = true;
        navMeshAgent.speed          = startMovementSpeed;
        navMeshAgent.acceleration   = acceleration;
        navMeshAgent.destination    = nextMorko.position;
        CheckMaskDistanceFromCharacter(nextMorko);
    }

    private void CheckMaskDistanceFromCharacter(Transform target)
    {
        var distance = Vector3.Distance(transform.position, target.position);
        if (distance <= jumpMinDistanceFromCharacter)
        {
            StartJumpOnHead();
        }

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
        if (newMorko == null)
            return;

        MorkoSoundController msc = GetComponent<MorkoSoundController>();
        msc.PlayAttack();

        IsTransferingToOtherCharacter = true;

        nextMorko = newMorko;

        GameManager.UnsetCharacterMorko();
        
        StartJumpOffHead();
    }
    
    float jumpOffHeadTime = 0.8f;
    float currentJumpOffInterpolation = 0.0f;
    Vector3 jumpOffStartPosition;
    Vector3 jumpOffTargetPosition;
 
    private void StartJumpOffHead()
    {       
        morkoState                  = MorkoState.SwitchingOff;
        morkoHeadJoint.localScale   = Vector3.one;
        
        Vector3 targetLocation      = nextMorko.position;
        // animator.applyRootMotion    = true;
        var direction               = Vector3.Normalize(targetLocation - transform.position);

        currentJumpOffInterpolation = 0.0f;
        jumpOffStartPosition = transform.position;
        jumpOffTargetPosition = Vector3.Lerp(transform.position, nextMorko.position, 0.5f);//direction * Mathf.Min(distanceToTarget / 2f, jumpMinDistanceFromCharacter);

        const float BAD_HAXOR_HARD_CODED_REMOVE_groundLevel = -5f;
        const float maskClearance = 0.3f;
        jumpOffTargetPosition.y = BAD_HAXOR_HARD_CODED_REMOVE_groundLevel + maskClearance;


        animator.SetTrigger("Roar");
    }

    private void JumpOffHead()
    {
        navMeshAgent.enabled = false;

        currentJumpOffInterpolation += Time.deltaTime / jumpOffHeadTime;
        var currentPosition = Vector3.Lerp( jumpOffStartPosition,
                                            jumpOffTargetPosition,
                                            currentJumpOffInterpolation);

        float currentYInterpolation = jumpOffCurve.Evaluate(currentJumpOffInterpolation);
        currentPosition.y = Mathf.LerpUnclamped(jumpOffStartPosition.y, jumpOffTargetPosition.y, currentYInterpolation);


        transform.position = currentPosition;
        transform.LookAt(jumpOffTargetPosition);

        if (currentJumpOffInterpolation >= 1.0f)
        {
            StartJumpOnHead();            
        }
    }

    // Hack(Leo): HACKHACKHACK
    // Note(Leo): timeToJumpOnHead must be the length of attack animation. It is't because animation triggering does not work...
    float timeToJumpOnHead = 0.8f;
    float currentJumpInterpolation = 0.0f;
    Vector3 jumpOnStartPosition;
    Transform jumpOnTargetTransform;

    private void StartJumpOnHead()
    {
        SetCurrentMorko(nextMorko);
        nextMorko = null;

        SetAnimatorState(AnimatorBooleans.Idle);
      
        photonView.TransferOwnership(currentMorkoCharacter.photonView.Owner);
        photonView.RPC(nameof(StartJumpOnHeadRPC), RpcTarget.Others, currentMorkoActorNumber);
    }

    [PunRPC]
    private void StartJumpOnHeadRPC(int actorNumber)
    {
        if (actorNumber != photonView.Owner.ActorNumber)
            return;

        morkoState = MorkoState.SwitchingOn;
        jumpOnStartPosition       = transform.position;
        currentJumpInterpolation  = 0.0f;
        jumpOnTargetTransform     = morkoHeadJoint;

        animator.SetTrigger("JumpOnHead");          
    }

    private void JumpOnHead()
    {
        navMeshAgent.enabled = false;

        // Todo(Leo): Add parabola, but this is not how. Also wont bother now, because it is not seen in game anyway        
        // var targetPosition = jumpOnTargetTransform.position;
        // targetPosition = new Vector3(targetPosition.x, targetPosition.y + (jumpParabolaSize - interpolation * jumpParabolaSize), targetPosition.z);

        currentJumpInterpolation += Time.deltaTime / timeToJumpOnHead;
        var currentPosition = Vector3.Lerp( jumpOnStartPosition,
                                            jumpOnTargetTransform.position,
                                            currentJumpInterpolation);

        float currentYInterpolation = jumpOnCurve.Evaluate(currentJumpInterpolation);
        currentPosition.y = Mathf.LerpUnclamped(jumpOnStartPosition.y, jumpOnTargetTransform.position.y, currentYInterpolation);

        transform.position = currentPosition;
        transform.LookAt(jumpOnTargetTransform.position);

        if (currentJumpInterpolation >= 1.0f)
        {
            MaskOnNewMorko();
        }
    }

    
    private void MaskOnNewMorko()
    {
        navMeshAgent.enabled = false;
        morkoState = MorkoState.OnHead;

        animator.applyRootMotion    = false;
        morkoHeadJoint.localScale   = Vector3.zero;

        SetAnimatorState(AnimatorBooleans.Idle);
        

        Vector3 effectPosition = currentMorkoCharacter.Head.position;
        Instantiate(jumpSmokeEffectOnCollision, effectPosition, Quaternion.identity);
        MorkoSoundController msc = GetComponent<MorkoSoundController>();
        msc.PlayAttach();

        currentMorkoCharacter.FreezeForSeconds(afterCollisionFreezeTime);

        IsTransferingToOtherCharacter = false;

        var newMorkoCharacter = currentMorko.GetComponent<Character>();
        if (newMorkoCharacter == null)
        {
            Debug.LogError("Bad newMorko, implement this class/function using Character instead of transform");
        }
        // photonView.TransferOwnership(newMorkoCharacter.photonView.Owner);
        GameManager.SetCharacterMorko(newMorkoCharacter);
    }
    
    private void ResetAnimatorTriggers()
    {
        animator.ResetTrigger("Roar");
        animator.ResetTrigger("JumpOnHead");
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
