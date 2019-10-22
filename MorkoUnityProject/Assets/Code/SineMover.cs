using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMover : MonoBehaviour
{
    public bool zAxis = false;
    Vector3 newPos;
    Vector3 startPos;
    float tick = 0;
    public bool manual = false;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(manual == false)
        {
            if (tick > 100f)
                tick = 0f;
            else
                tick += 1f * Time.deltaTime;
            if (zAxis)
            {
                newPos = new Vector3(transform.position.x, transform.position.y, startPos.z + Mathf.Sin(tick) * 5);
                transform.position = Vector3.Lerp(transform.position, newPos, 0.1f);
            }
            else
            {
                newPos = new Vector3(startPos.x + Mathf.Sin(tick) * 5, transform.position.y, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, newPos, 0.1f);
            }
        }
        else
        {
            transform.Translate(new Vector3(
                Input.GetAxis("Horizontal") * Time.deltaTime * 5f,
                0,
                Input.GetAxis("Vertical") * Time.deltaTime * 5f));
            transform.Rotate(0, Input.GetAxis("Mouse X") * Time.deltaTime * 80f, 0);
        }
        
    }
}
