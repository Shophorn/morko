using UnityEngine;

public class MaskEyeBlink : MonoBehaviour
{
    private Renderer renderer;
    private Material material;
    private Color color;
    public float blinkTimer = 3f;
    public float blindDuration = 0.4f;
    public float offset;
    
    void Start()
    {
        renderer = GetComponent<Renderer>();
        material = renderer.material;
        color = material.GetColor("_EmissionColor");
    }

    void Update()
    {
        float time = Mathf.PingPong(Time.time, blinkTimer);
        if (time > blindDuration + offset)
            material.SetColor("_EmissionColor", color);
        else
            material.SetColor("_EmissionColor", color * 0f);
    }
}
