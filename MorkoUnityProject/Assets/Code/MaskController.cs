using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MaskController : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    // Note(Sampo): Leo character lista saadaan varmaan GameManagerista
    public Transform[] characters;
    public float secondsBeforeMaskMovesAtStart;
    public float secondsBeforeMaskMovesToNewTarget;
    public float afterCollisionFreezeTime;
    public float maskStartingSpeed;
    public float maskChangingSpeed;
    public float minDistanceFromCharacter;
    public float jumpSpeed;

    private Animator animator;
    
    [SerializeField]
    public Transform currentMorko;
    [SerializeField]
    public Transform nextMorko;
    private Transform normal;

    private bool waitAfterCollision = true;
    private bool startWaitDurationWaited = false;
    private bool lookingForStartingMorko = true;
    private bool maskMovingToNewMorko = false;
    private bool maskJumpingOn = false;
    private bool maskJumpingOff = false;
    private bool maskIsBeingPutOn = false;

    private Vector3 startJumpingPosition;
    private Vector3 targetJumpingPosition;
    
    [Space]
    [Header("DEBUG")]
    public Transform p1;
    public Transform p2;
    public bool startMaskControllerInStartMethod = true;

    private void Start()
    {
        navMeshAgent = transform.GetComponent<NavMeshAgent>();
        navMeshAgent.Warp(transform.position);
        animator = transform.GetComponent<Animator>();

        if (startMaskControllerInStartMethod)
            this.InvokeAfter (FindStartingCharacter, secondsBeforeMaskMovesAtStart);
        
        currentMorko = p2;
        normal = p1;
    }

    private void Update()
    {
        if (lookingForStartingMorko && startWaitDurationWaited && !maskJumpingOn && !maskIsBeingPutOn)
            FindStartingCharacter();
        
        else if (maskMovingToNewMorko && !maskJumpingOn && !maskJumpingOff && !maskIsBeingPutOn)
            CheckMaskDistanceFromCharacter(nextMorko);

        else if (maskJumpingOn)
            TransitionMaskToHead(nextMorko);
        
        else if (maskJumpingOff)
            TransitionMaskOffHead(Vector3.zero);
        
        // DEBUG PURPOSES
        // SwitchMorko() is called when morko and normal characters collide
        if (!maskMovingToNewMorko && Input.GetKeyDown(KeyCode.Space))
        {
            SwitchMorko(currentMorko, normal);
            var temp = normal;
            normal = currentMorko;
            currentMorko = temp;
        }
    }
    
    public void StartMaskController()
    {
        this.InvokeAfter (FindStartingCharacter, secondsBeforeMaskMovesAtStart);
    }

    private void FindStartingCharacter()
    {
        startWaitDurationWaited = true;
        lookingForStartingMorko = true;
        nextMorko = FindClosestCharacter(characters);
        MoveMaskToTarget(nextMorko, maskStartingSpeed);
        CheckMaskDistanceFromCharacter(nextMorko);
    }

    private Transform FindClosestCharacter(Transform[] characters)
    {
        float distance = float.MaxValue;
        Transform closestCharacter = characters[Random.Range(0, characters.Length - 1)];
        
        foreach (var c in characters)
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
    
    private void MoveMaskToTarget(Transform target, float speed)
    {
        maskMovingToNewMorko = true;
        
        animator.applyRootMotion = true;
        animator.ResetTrigger("MaskJumpOffHead");
        animator.ResetTrigger("MaskOn");
        animator.SetBool("Move", true);
        
        navMeshAgent.enabled = true;
        navMeshAgent.speed = speed;
        navMeshAgent.destination = target.position;
    }

    private void CheckMaskDistanceFromCharacter(Transform target)
    {
        var distance = Vector3.Distance(transform.position, target.position);
        if (distance <= minDistanceFromCharacter)
            TransitionMaskToHead(target);
    }

    public void SwitchMorko(Transform oldMorko, Transform newMorko)
    {
        nextMorko = newMorko;
        maskJumpingOn = false;
        maskIsBeingPutOn = false;
        
        //animator.SetTrigger("MaskOff");
        nextMorko.GetComponent<Character>().FreezeForSeconds(afterCollisionFreezeTime);
        TransitionMaskOffHead(nextMorko.position);
        //this.InvokeAfter(()=> MoveMaskToTarget(newMorko, maskChangingSpeed), secondsBeforeMaskMovesToNewTarget);
    }
    
    public void MaskOffHead(Transform fromMorko, Transform toMorko)
    {
        animator.SetTrigger("MaskJumpOffHead");
        transform.parent = null;
        
        var direction = (toMorko.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void TransitionMaskToHead(Transform toMorko)
    {
        navMeshAgent.enabled = false;

        if (!maskJumpingOn)
        {
            startJumpingPosition = transform.localPosition;
            animator.ResetTrigger("Move");
        }
        
        Vector3 toMorkoHeadPosition = toMorko.transform.GetChild(0).position;
        
        float lengthToTarget = Vector3.Distance(startJumpingPosition, toMorkoHeadPosition);
        float currentLengthToTarget = Vector3.Distance(transform.position, toMorkoHeadPosition);
        float interpolation = (lengthToTarget - currentLengthToTarget) / lengthToTarget;

        transform.position = Vector3.MoveTowards(transform.position, toMorkoHeadPosition, Time.deltaTime * jumpSpeed);
        animator.Play("MaskJumpToHeadProto", 0, interpolation);
        
        maskJumpingOn = true;

        if (interpolation == 1f)
        {
            maskJumpingOn = false;
            maskIsBeingPutOn = true;
            animator.Play("MaskOn");
        }
    }
    public void TransitionMaskOffHead(Vector3 targetLocation)
    {
        navMeshAgent.enabled = false;

        if (!maskJumpingOff)
        {
            MaskOffMorko();
            animator.applyRootMotion = true;
            
            startJumpingPosition = transform.localPosition;

            float distanceToTarget = Vector2.Distance(new Vector2(startJumpingPosition.x, startJumpingPosition.z), new Vector2(targetLocation.x, targetLocation.z));
            if (distanceToTarget <= minDistanceFromCharacter)
            {
                TransitionMaskToHead(nextMorko);
                return;
            }
            else
            {
                Vector3 direction = Vector3.Normalize(targetLocation - startJumpingPosition);
                targetJumpingPosition = startJumpingPosition + direction * minDistanceFromCharacter;
                targetJumpingPosition = new Vector3(targetJumpingPosition.x, -4.694f, targetJumpingPosition.z);
            }
        }
        
        float lengthToTarget = Vector3.Distance(startJumpingPosition, targetJumpingPosition);
        float currentLengthToTarget = Vector3.Distance(transform.position, targetJumpingPosition);
        float interpolation = (lengthToTarget - currentLengthToTarget) / lengthToTarget;

        transform.position = Vector3.MoveTowards(transform.position, targetJumpingPosition, Time.deltaTime * jumpSpeed);
        animator.Play("MaskJumpOffHeadProto", 0, interpolation);
        
        maskJumpingOff = true;

        if (interpolation == 1f)
        {
            maskJumpingOff = false;
            MoveMaskToTarget(nextMorko, maskChangingSpeed);
        }
    }
    
    public void MaskOffMorko()
    {
        transform.parent = null;
        var direction = (nextMorko.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        
    }
    public void MaskOnNewMorko()
    {
        navMeshAgent.enabled = false;

        lookingForStartingMorko = false;
        maskMovingToNewMorko = false;
        maskJumpingOn = false;

        animator.applyRootMotion = false;

        var maskHolder = nextMorko.transform.GetChild(0);
        transform.parent = maskHolder.transform;
        transform.localPosition = Vector3.zero;
        transform.forward = nextMorko.forward;

        currentMorko = nextMorko;
    }
}
