using UnityEngine;

public class DestoyAfterParticleEffect : MonoBehaviour
{
    public ParticleSystem[] particles;
    void Start()
    {
        if (particles.Length == 0)
            particles = GetComponentsInChildren<ParticleSystem>();
    }

    private void CheckIfParticlesAreStillPlaying()
    {
        foreach (var p in particles)
        {
            if (p.IsAlive())
            {
                return;
            }
        }
        
        Destroy(gameObject);
    }
    void Update()
    {
        CheckIfParticlesAreStillPlaying();
    }
}
