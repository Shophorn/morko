using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTestCharacterController : MonoBehaviour
{
    public Transform cameraPos;
    public Vector3 targetCameraPos;
    public Camera cam;
    public CharacterController characterController;
    public float arbitraryAngleLimit = 0.55f;
    public float arbitraryRunLimit = 0.85f;
    public float speed = 2.5f;
    public float currentSpeed = 2.5f;
    public float speedGrowth = 0.1f;
    public float maxSpeed = 3.5f;
    public float distanceToCursor;

    


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Rotate();
       
    }
    private void LateUpdate()
    {
        targetCameraPos = Vector3.Lerp(transform.position, transform.position + (transform.forward * 1f), Mathf.Clamp(distanceToCursor,0,1));
        cameraPos.position = Vector3.Lerp(cameraPos.position, targetCameraPos, 0.2f);

    }

    private void Move()
    {
        Vector2 moveAxis2D = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        float grav = -1;
        Vector3 moveAxis = new Vector3(moveAxis2D.x, grav, moveAxis2D.y);
        float actualSpeed = Speed(new Vector3(moveAxis.x,0,moveAxis.z));
        characterController.Move(moveAxis * Time.deltaTime * actualSpeed);
    }

    float Speed(Vector3 moveDir)
    {
        if (Vector3.Dot(transform.forward, moveDir) > arbitraryAngleLimit)
        {
            if (currentSpeed < speed)
                currentSpeed += (speedGrowth * Time.deltaTime);

            if (Vector3.Dot(transform.forward, moveDir) > arbitraryRunLimit)
            {
                if (currentSpeed < speed)
                    currentSpeed = speed;

                if (currentSpeed < maxSpeed)
                    currentSpeed += (speedGrowth * Time.deltaTime);
                else
                    currentSpeed = maxSpeed;
            }
            
        }
        else
        {
            if (currentSpeed > speed / 2f)
                currentSpeed -= (speedGrowth * Time.deltaTime * 100f);
            else
                currentSpeed = speed / 2f;
        }
        return currentSpeed;
    }

    private void Rotate()
    {
        Vector3 direction = (CursorPosition() - transform.position).normalized;
        Debug.DrawRay(transform.position, direction);
        Quaternion lookDir = Quaternion.LookRotation(direction, Vector3.up);
        Vector3 euler = Quaternion.Lerp(transform.rotation, lookDir, 0.2f).eulerAngles;
        transform.rotation = Quaternion.Euler(0, euler.y, 0);
    }

    private Vector3 CursorPosition()
    {
        Vector3 target = Vector3.zero;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position);

        float distance;

        if (plane.Raycast(ray, out distance))
        {
            target = ray.GetPoint(distance);
            target = new Vector3(target.x, transform.position.y, target.z);
        }
        distanceToCursor = Vector3.Distance(transform.position, target) / 5;

        return target;
    }

}
