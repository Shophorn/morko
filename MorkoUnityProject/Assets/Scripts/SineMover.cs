using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMover : MonoBehaviour
{
    public bool zAxis = false;
    Vector3 newPos;
    Vector3 startPos;
    float tick = 0;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (tick > 100f)
            tick = 0f;
        else
            tick += 1f * Time.deltaTime;
        if(zAxis)
        {
            newPos = new Vector3(transform.position.x, transform.position.y, startPos.z + Mathf.Sin(tick)*5);
            transform.position = Vector3.Lerp(transform.position, newPos,0.1f);
        }
        else
        {
            newPos = new Vector3(startPos.x + Mathf.Sin(tick) * 5, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, newPos, 0.1f);
        }
    }
}
