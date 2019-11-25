using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MaskController : MonoBehaviour
{
    public Transform mask;
    public NavMeshAgent navMeshAgent;
    public Transform[] characters;
    public float waitForSecondsBeforeMovingMask;
    public float maskStartingSpeed;
    public float maskChangingSpeed;
    public float minDistanceFromCharacter;
    public float landingRadius;

    public Transform p1;
    public Transform p2;

    private Animator animator;
    
    [SerializeField]
    public Transform morko;
    [SerializeField]
    public Transform toMorko;
    private Transform normal;

    private bool collisionDurationWait = true;
    private bool startingDurationWait = true;
    private bool lookingForStartingMorko = true;
    private bool maskMovingToNewMorko = false;

    private void Start()
    {
        navMeshAgent = mask.GetComponent<NavMeshAgent>();
        navMeshAgent.Warp(transform.position);

        animator = mask.GetComponent<Animator>();
        
        var closestCharacter = FindClosestCharacter(characters);
        StartCoroutine(MoveMaskToTarget(closestCharacter, maskStartingSpeed, waitForSecondsBeforeMovingMask, startingDurationWait));
        
        morko = p2;
        normal = p1;
    }

    private void Update()
    {
        if (lookingForStartingMorko)
        {
            toMorko = FindClosestCharacter(characters);
            StartCoroutine(MoveMaskToTarget(toMorko, maskStartingSpeed, waitForSecondsBeforeMovingMask, startingDurationWait));
            CheckMaskDistanceFromCharacter(toMorko);
        }
        
        if (!maskMovingToNewMorko && Input.GetKeyDown(KeyCode.Space))
        {
            SwitchMorko(morko, normal);
            var temp = normal;
            normal = morko;
            morko = temp;
        }

        if (maskMovingToNewMorko)
            CheckMaskDistanceFromCharacter(toMorko);
    }

    public Transform FindClosestCharacter(Transform[] characters)
    {
        float distance = 100000000000f;
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
    
    IEnumerator MoveMaskToTarget(Transform target, float speed, float waitForSeonds, bool wait)
    {
        if (wait)
            yield return new WaitForSeconds(waitForSeonds);
        
        animator.SetBool("Snake", true);
        startingDurationWait = false;
        
        navMeshAgent.speed = speed;
        navMeshAgent.destination = target.position;

        maskMovingToNewMorko = true;
    }

    private void CheckMaskDistanceFromCharacter(Transform target)
    {
        var distance = Vector3.Distance(mask.transform.position, target.position);
        if (distance <= minDistanceFromCharacter)
            MaskToHead(target);
    }

    public void SwitchMorko(Transform oldMorko, Transform newMorko)
    {
        toMorko = newMorko;
        maskMovingToNewMorko = true;
        
        MaskOffHead(oldMorko, toMorko);
        //disable toMorko movement
        StartCoroutine(MoveMaskToTarget(newMorko, maskChangingSpeed, waitForSecondsBeforeMovingMask, true));
    }
    
    public void MaskOffHead(Transform fromMorko, Transform toMorko)
    {
        animator.ResetTrigger("MaskOn");
        animator.SetTrigger("MaskOff");
        navMeshAgent.baseOffset = 0;
        mask.transform.parent = null;
        var direction = (toMorko.position - mask.position).normalized;
        mask.rotation = Quaternion.LookRotation(direction);
        //mask.position = GetMaskLandingPosition(fromMorko.position);
    }

    public void MaskToHead(Transform toMorko)
    {
        animator.SetTrigger("MaskOn");
        animator.SetBool("Snake", false);

        lookingForStartingMorko = false;
        maskMovingToNewMorko = false;
        
        var maskHolder = toMorko.transform.GetChild(0);
        mask.transform.parent = maskHolder.transform;
        mask.transform.localPosition = Vector3.zero;
        mask.transform.forward = toMorko.forward;
        navMeshAgent.baseOffset = 0.82f;

        morko = toMorko;
    }

    private Vector3 GetMaskLandingPosition(Vector3 currentPosition)
    {
        Vector3 position = Random.insideUnitCircle * landingRadius;
        Vector3 newPosition = new Vector3(currentPosition.x + position.x, currentPosition.y, currentPosition.z + position.y);
        return newPosition;
    }

    IEnumerator RotateMaskTowardsInSeconds(Vector3 target, float duration)
    {
        float timer = 0f;
        var startRotation = mask.transform.rotation;
        
        while (timer <= duration)
        {
            timer += Time.deltaTime;

            var direction = (target - mask.transform.position).normalized;
            var lookRotation = Quaternion.LookRotation(direction);
 
            mask.transform.rotation = Quaternion.Slerp(startRotation, lookRotation, timer / duration);
            yield return null;
        }
    }
    
    
    
    private IEnumerator WaitForAnimation ( Animation animation )
    {
        do
        {
            yield return null;
        }
        while ( animation.isPlaying );
    }
}
