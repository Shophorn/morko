using System.Collections;
using UnityEngine;

public class DisableMovement : MonoBehaviour
{
    private CharacterController characterController;
    private Character character;
    private Vector3 positionToHold;
    private Quaternion rotationToHold;
    public float duration;
    private void Start()
    {
        positionToHold = transform.position;
        rotationToHold = transform.rotation;
        character = GetComponent<Character>();
        
        characterController = GetComponent<CharacterController>();
        StartCoroutine(HoldPosition(duration));
    }

    /*
    IEnumerator HoldPosition(float secondsToHold)
    {
        
        characterController.enabled = false;
        yield return new WaitForSeconds(secondsToHold);
        characterController.enabled = true;

        enabled = false;
    }
    */
    IEnumerator HoldPosition(float secondsToHold)
    {

        float timer = 0f;

        while (timer <= secondsToHold)
        {
            transform.position = positionToHold;
            transform.rotation = rotationToHold;
            timer += Time.deltaTime;
            yield return null;
        }

        character.DisableDisableMovementScript();
    }
}
