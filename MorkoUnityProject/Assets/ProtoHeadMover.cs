using UnityEngine;
using System.Collections;
public class ProtoHeadMover : MonoBehaviour
{
    public Animator animator;
    public Transform target;
    public float speed;
    public bool walking = false;
    public bool wakeUp = false;


    public ParticleSystem pSystem;
    public MeshRenderer[] meshRenderers;

    //JUMPSTUFF
    public float jumpHMax = 2f;
    public bool apexReached = false;
    public bool grounded = true;
    public float fallingSpeed = 9.81f;
    public float currentFallingSpeed = 0;
    float jumpHMaxFromCurrentAltitude;

    void Start()
    {
        jumpHMaxFromCurrentAltitude = jumpHMax + transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(wakeUp)
        {
            WakeUp();
        }
        else if (walking)
        {
            Walk();
        }
        
    }
    void WakeUp()
    {
        Debug.Log(jumpHMaxFromCurrentAltitude+" Above this I fall");
        if(apexReached==false)
        {
            animator.SetBool("WakeUp", true);
            grounded = false;
            float distanceToApex = (jumpHMaxFromCurrentAltitude - transform.position.y)/jumpHMax;
            Vector3 jumpDir = new Vector3(0, (20f * Time.deltaTime)* distanceToApex +0.01f, -4f * Time.deltaTime);
            transform.Translate(jumpDir);
            if (transform.position.y > jumpHMaxFromCurrentAltitude)
                apexReached = true;
        }
        if(apexReached)
        {
            currentFallingSpeed += (fallingSpeed * Time.deltaTime)/2f;
            float distanceFromApex = Mathf.Abs((jumpHMaxFromCurrentAltitude - transform.position.y) / jumpHMax);
            Vector3 fallDir = new Vector3(0, -currentFallingSpeed/2f, -4f * Time.deltaTime);
            transform.Translate(fallDir);

            if (transform.position.y < jumpHMaxFromCurrentAltitude - jumpHMax)
            {

                StartCoroutine(WakeUpDelay());
                wakeUp = false;
                transform.position = new Vector3(transform.position.x, jumpHMaxFromCurrentAltitude - jumpHMax, transform.position.z);
            }
        }

    }
    IEnumerator WakeUpDelay()
    {
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("OnGround", true);
        walking = true;
    }
    IEnumerator AttachDelay()
    {
        pSystem.Play();
        foreach(var mesh in meshRenderers)
        {
            mesh.enabled = false;
        }
        animator.SetBool("OnGround", false);
        animator.SetBool("WakeUp", false);
        yield return new WaitForSeconds(0.5f);
        foreach (var mesh in meshRenderers)
        {
            mesh.enabled = true;
        }
        transform.position = target.position;
        transform.rotation = target.rotation;
        pSystem.Play();
        
    }
    void Walk()
    {
        Vector3 twoDimensionalDir = target.position;
        twoDimensionalDir.y = transform.position.y;
        Quaternion lookLerped = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(transform.position - twoDimensionalDir), 0.1f);
        transform.rotation = lookLerped * Quaternion.Euler(0, Mathf.Sin(Time.time * 5f) * 2f, 0);

        transform.position += -transform.forward * speed * Time.deltaTime;
        if (Vector3.Distance(transform.position, target.position) < 6f)
        {
            walking = false;
            StartCoroutine(AttachDelay());

        }

    }
}
