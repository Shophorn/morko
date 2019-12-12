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
    private PlayerController currentMorkoController
    {
        get {   if (currentMorko == null)
                    return null;
                return currentMorko.GetComponent<PlayerController>();

            }
    }

    public Transform nextMorko;

    private Animator animator;
    private bool startWaitDurationWaited = false;
    private bool lookingForStartingMorko = false;
    private bool maskMovingToNewMorko = false;
    private bool maskJumpingOn = false;
    private bool maskJumpingOff = false;
    private bool maskIsBeingPutOn = false;

    private Vector3 startJumpingPosition;
    private Vector3 targetJumpOffPosition;

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
        // currentMorko = GameManager.GetCharacterByActorNumber(currentMorkoActorNumber).transform;
        // currentMorkoController = currentMorko.GetComponent<PlayerController>();
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // if(stream.IsWriting)
        // {
        //     int state = (int)morkoState;
        //     stream.SendNext(state);

        //     // stream.SendNext(currentMorkoActorNumber);
        // }
        // else if (stream.IsReading)
        // {
        //     int state = (int)stream.ReceiveNext();
        //     morkoState = (MorkoState)state;

        //     // currentMorkoActorNumber = (int)stream.ReceiveNext();
        //     // currentMorko = GameManager.GetCharacterByActorNumber(currentMorkoActorNumber).transform;
        // }
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
                JumpToHead();
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
            StartJumpToHead();
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
        

        var direction               = (nextMorko.position - transform.position).normalized;
        transform.rotation          = Quaternion.LookRotation(direction);
        Vector3 targetLocation      = nextMorko.position;
        

        animator.applyRootMotion    = true;
        
        startJumpingPosition        = transform.localPosition;

        // Note(Leo): Test if should immediately jump on next morko
        float distanceToTarget      = Vector2.Distance( new Vector2(startJumpingPosition.x, startJumpingPosition.z),
                                                        new Vector2(targetLocation.x, targetLocation.z));
        // Note(Leo): Only uncomment these when all else works
        // if (distanceToTarget <= jumpMinDistanceFromCharacter)
        // {
        //     StartJumpToHead();
        //     return;
        // }
        
        // TODO(Leo): REUSE 'direction' please fix
        direction               = Vector3.Normalize(targetLocation - startJumpingPosition);
        targetJumpOffPosition   = startJumpingPosition + direction * jumpMinDistanceFromCharacter;

        currentJumpOffInterpolation = 0.0f;
        jumpOffStartPosition = transform.position;
        jumpOffTargetPosition = Vector3.Lerp(transform.position, nextMorko.position, 0.5f);//direction * Mathf.Min(distanceToTarget / 2f, jumpMinDistanceFromCharacter);
        jumpOffTargetPosition.y = -5f;


        animator.SetTrigger("Roar");
    }

    private void JumpOffHead()
    {
        navMeshAgent.enabled = false;

        // float distanceToTarget          = Vector3.Distance(startJumpingPosition, targetJumpOffPosition);
        // float currentDistanceToTarget   = Vector3.Distance(transform.position, targetJumpOffPosition);
        // float interpolation             = (distanceToTarget - currentDistanceToTarget) / distanceToTarget;
        // interpolation                   = Mathf.Clamp01(interpolation);

        // transform.position = Vector3.MoveTowards(transform.position, targetJumpOffPosition, Time.deltaTime * jumpSpeed);
        // animator.Play("Roar", 0, interpolation);
        
        // maskJumpingOff = true;

        // if (interpolation >= jumpInterpolationCutOff)
        // {
        //     maskJumpingOff = false;
        //     StartJumpToHead();
        // }


        currentJumpOffInterpolation += Time.deltaTime / jumpOffHeadTime;
        transform.position = Vector3.Lerp(  transform.position,
                                            jumpOffTargetPosition,
                                            currentJumpOffInterpolation);
        transform.LookAt(jumpOffTargetPosition);

        if (currentJumpOffInterpolation >= 1.0f)
        {
            StartJumpToHead();            
        }
    }

    // Hack(Leo): HACKHACKHACK
    // Note(Leo): timeToJumpToHead must be the length of attack animation. It is't because animation triggering does not work...
    float timeToJumpToHead = 0.8f;
    float currentJumpInterpolation = 0.0f;
    Vector3 jumpToStartPosition;
    Transform jumpToTargetTransform;

    private void StartJumpToHead()
    {
        SetCurrentMorko(nextMorko);
        nextMorko = null;
      
        photonView.TransferOwnership(currentMorkoCharacter.photonView.Owner);
        photonView.RPC(nameof(StartJumpToHeadRPC), RpcTarget.Others, currentMorkoActorNumber);
    }

    [PunRPC]
    private void StartJumpToHeadRPC(int actorNumber)
    {
        if (actorNumber != photonView.Owner.ActorNumber)
            return;

        morkoState = MorkoState.SwitchingOn;
        jumpToStartPosition       = transform.position;
        currentJumpInterpolation  = 0.0f;
        jumpToTargetTransform     = morkoHeadJoint;

        animator.SetTrigger("JumpToHead");          
    }

    private void JumpToHead()
    {
        navMeshAgent.enabled = false;

        // Todo(Leo): Add parabola, but this is not how. Also wont bother now, because it is not seen in game anyway        
        // var targetPosition = jumpToTargetTransform.position;
        // targetPosition = new Vector3(targetPosition.x, targetPosition.y + (jumpParabolaSize - interpolation * jumpParabolaSize), targetPosition.z);

        currentJumpInterpolation += Time.deltaTime / timeToJumpToHead;
        transform.position = Vector3.Lerp(  jumpToStartPosition,
                                            jumpToTargetTransform.position,
                                            currentJumpInterpolation);
        transform.LookAt(jumpToTargetTransform.position);

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
