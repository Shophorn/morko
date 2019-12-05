using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MaskController : MonoBehaviour
{
    public Transform mask;
    public NavMeshAgent navMeshAgent;
    // Note(Sampo): Leo character lista saadaan varmaan GameManagerista
    public Transform[] characters;
    public float secondsBeforeMaskMovesAtStart;
    public float secondsBeforeMaskMovesToNewTarget;
    public float afterCollisionFreezeTime;
    public float maskStartingSpeed;
    public float maskChangingSpeed;
    public float minDistanceFromCharacter;

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
    private bool maskJumping = false;
    
    [Space]
    [Header("DEBUG")]
    public Transform p1;
    public Transform p2;
    public bool startMaskControllerInStartMethod = true;

    private void Start()
    {
        navMeshAgent = mask.GetComponent<NavMeshAgent>();
        navMeshAgent.Warp(transform.position);
        animator = mask.GetComponent<Animator>();

        if (startMaskControllerInStartMethod)
            this.InvokeAfter (FindStartingCharacter, secondsBeforeMaskMovesAtStart);
        
        currentMorko = p2;
        normal = p1;
    }

    private void Update()
    {
        if (lookingForStartingMorko && startWaitDurationWaited && !maskJumping)
            FindStartingCharacter();
        
        if (maskMovingToNewMorko && !maskJumping)
            CheckMaskDistanceFromCharacter(nextMorko);
        
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
            var characterDistance = Vector3.Distance(mask.transform.position, c.position);
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
        animator.SetBool("Move", true);
        
        navMeshAgent.speed = speed;
        navMeshAgent.destination = target.position;

        maskMovingToNewMorko = true;
    }

    private void CheckMaskDistanceFromCharacter(Transform target)
    {
        var distance = Vector3.Distance(mask.transform.position, target.position);
        if (distance <= minDistanceFromCharacter)
            TransitionMaskToHead(target);
    }

    public void SwitchMorko(Transform oldMorko, Transform newMorko)
    {
        nextMorko = newMorko;
        
        animator.SetTrigger("MaskOff");
        nextMorko.GetComponent<Character>().FreezeForSeconds(afterCollisionFreezeTime);
        this.InvokeAfter(()=> MoveMaskToTarget(newMorko, maskChangingSpeed), secondsBeforeMaskMovesToNewTarget);
    }
    
    public void MaskOffHead(Transform fromMorko, Transform toMorko)
    {
        animator.SetTrigger("MaskOff");
        navMeshAgent.baseOffset = 0;
        mask.transform.parent = null;
        
        var direction = (toMorko.position - mask.position).normalized;
        mask.rotation = Quaternion.LookRotation(direction);
    }

    public void TransitionMaskToHead(Transform toMorko)
    {
        animator.applyRootMotion = true;
        animator.SetTrigger("MaskJumpToHead");
        animator.SetBool("Move", false);
        maskJumping = true;
    }
    
    public void MaskOffMorko()
    {
        navMeshAgent.Resume();
        navMeshAgent.baseOffset = 0;
        mask.transform.parent = null;
        var direction = (nextMorko.position - mask.position).normalized;
        mask.rotation = Quaternion.LookRotation(direction);
    }
    public void MaskOnNewMorko()
    {
        lookingForStartingMorko = false;
        maskMovingToNewMorko = false;
        maskJumping = false;

        animator.applyRootMotion = false;
        
        var maskHolder = nextMorko.transform.GetChild(0);
        mask.transform.parent = maskHolder.transform;
        mask.transform.localPosition = Vector3.zero;
        mask.transform.forward = nextMorko.forward;
        navMeshAgent.baseOffset = 0.82f;

        currentMorko = nextMorko;
    }
}
