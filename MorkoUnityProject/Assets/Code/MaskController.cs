using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MaskController : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public Transform p1;
    public Transform p2;
    public Transform[] chracters;
    public float rotateTime;
    public float maskToTargetDuration;

    [HideInInspector]
    public Transform morko;
    private Transform normal;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.Warp(transform.position);
        morko = p1;
        normal = p2;
        
        //SetMaskToHead(morko);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            FindStartingMorko(chracters, 2);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchMorko(morko, normal);
            
            var temp = normal;
            normal = morko;
            morko = temp;
        }
    }

    public void FindStartingMorko(Transform[] characters, float timeBeforeFindingMorko)
    {
        
        // sleep?
        
        float distance = 100000000000f;
        Transform toMorko = chracters[Random.Range(0, chracters.Length - 1)];
        
        foreach (var c in characters)
        {
            var characterDistance = Vector3.Distance(transform.position, c.position);
            if (characterDistance < distance)
            {
                distance = characterDistance;
                toMorko = c;
            }
        }

        navMeshAgent.destination = toMorko.position;
    }

    public void SwitchMorko(Transform fromMorko, Transform toMorko)
    {
        MaskOffHead(fromMorko);
        MaskFliesOffHead(fromMorko);
        StartCoroutine(RotateMaskTowardsInSeconds(toMorko.position, rotateTime));
        SetMaskToHead(toMorko);
    }
    
    public void MaskOffHead(Transform fromMorko)
    {
        transform.parent = null;
    }

    public void SetMaskToHead(Transform toMorko)
    {
        var maskHolder = toMorko.transform.GetChild(0);
        transform.parent = maskHolder.transform;
        transform.localPosition = Vector3.zero;
        transform.forward = toMorko.forward;
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
        
        StartCoroutine(MoveMaskToTargetInSecods(target, maskToTargetDuration));
    }
    
    IEnumerator MoveMaskToTargetInSecods(Vector3 target, float duration)
    {
        float timer = 0f;
        var startPos = transform.position;
        while (timer <= duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, target, timer / duration);
            
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
