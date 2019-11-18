using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MaskController : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public Transform[] characters;
    public float maskStartingSpeed;
    public float maskChangingSpeed;
    public float waitForSeconds;
    public float minDistanceFromCharacter;

    public Transform p1;
    public Transform p2;
    
    public float rotateTime;
    public float maskToTargetDuration;

    [HideInInspector]
    public Transform morko;
    private Transform toMorko;
    private Transform normal;

    private bool collisionDurationWait = true;
    private bool startingDurationWait = true;
    private bool lookingForStartingMorko = true;
    private bool maskMovingToNewMorko = false;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.Warp(transform.position);
        
        var closestCharacter = FindClosestCharacter(characters);
        StartCoroutine(MoveMaskToTarget(closestCharacter, maskStartingSpeed, waitForSeconds, startingDurationWait));
        
        morko = p2;
        normal = p1;
    }

    private void Update()
    {
        if (lookingForStartingMorko)
        {
            toMorko = FindClosestCharacter(characters);
            StartCoroutine(MoveMaskToTarget(toMorko, maskStartingSpeed, waitForSeconds, startingDurationWait));
            CheckMaskDistanceFromFallenCharacter(toMorko);
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchMorko(morko, normal);
            var temp = normal;
            normal = morko;
            morko = temp;
        }

        if (maskMovingToNewMorko)
            CheckMaskDistanceFromFallenCharacter(toMorko);
    }

    public Transform FindClosestCharacter(Transform[] characters)
    {
        
        float distance = 100000000000f;
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
    
    IEnumerator MoveMaskToTarget(Transform target, float speed, float waitForSeonds, bool wait)
    {
        if (wait)
            yield return new WaitForSeconds(waitForSeonds);

        startingDurationWait = false;
        
        navMeshAgent.speed = speed;
        navMeshAgent.destination = target.position;

        maskMovingToNewMorko = true;
    }

    private void CheckMaskDistanceFromFallenCharacter(Transform target)
    {
        if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            SetMaskToHead(target);
    }

    public void SwitchMorko(Transform oldMorko, Transform newMorko)
    {
        toMorko = newMorko;
        maskMovingToNewMorko = true;
        
        MaskOffHead(oldMorko);
        MaskFliesOffHead(oldMorko);
        //play falling animation
        //disable toMorko movement
        StartCoroutine(MoveMaskToTarget(newMorko, maskChangingSpeed, waitForSeconds, true));
    }
    
    public void MaskOffHead(Transform fromMorko)
    {
        navMeshAgent.baseOffset = 0;
        transform.parent = null;
    }

    public void SetMaskToHead(Transform toMorko)
    {
        lookingForStartingMorko = false;
        maskMovingToNewMorko = false;
        
        var maskHolder = toMorko.transform.GetChild(0);
        transform.parent = maskHolder.transform;
        transform.localPosition = Vector3.zero;
        transform.forward = toMorko.forward;
        navMeshAgent.baseOffset = 0.82f;

        morko = toMorko;
    }

    public void MaskFliesOffHead(Transform fromMorko)
    {
        transform.position = new Vector3(transform.position.x, fromMorko.position.y, transform.position.z);
    }

    IEnumerator RotateMaskTowardsInSeconds(Vector3 target, float duration)
    {
        float timer = 0f;
        var startRotation = transform.rotation;
        
        while (timer <= duration)
        {
            timer += Time.deltaTime;

            var direction = (target - transform.position).normalized;
            var lookRotation = Quaternion.LookRotation(direction);
 
            transform.rotation = Quaternion.Slerp(startRotation, lookRotation, timer / duration);
            yield return null;
        }
        
        //StartCoroutine(MoveMaskToTarget(target, maskToTargetDuration));
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
