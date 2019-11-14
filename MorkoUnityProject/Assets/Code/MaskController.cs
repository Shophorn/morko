using System.Collections;
using UnityEngine;

public class MaskController : MonoBehaviour
{

    public Transform p1;
    public Transform p2;
    public float rotateTime;
    public float maskToTargetDuration;

    [HideInInspector]
    public Transform morko;
    private Transform normal;

    private void Start()
    {
        morko = p1;
        normal = p2;
        
        MaskToHead(morko);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchMorko(morko, normal);
            
            var temp = normal;
            normal = morko;
            morko = temp;
        }
    }

    public void SwitchMorko(Transform fromMorko, Transform toMorko)
    {
        MaskOffHead(fromMorko);
        MaskFliesOffHead(fromMorko);
        StartCoroutine(RotateMaskTowardsInSeconds(toMorko.position, rotateTime));
        MaskToHead(toMorko);
    }
    
    public void MaskOffHead(Transform fromMorko)
    {
        transform.parent = null;
    }

    public void MaskToHead(Transform toMorko)
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
