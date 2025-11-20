using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class FootstepController : MonoBehaviour
{
    [Header("Footstep Sounds / Wing Beats")]
    public AudioClip[] footstepClips;

    [Header("Settings")]
    public float stepInterval = 0.6f;
    public float minVelocity = 0.1f;

    [Header("Special")]
    public bool alwaysPlay = false;
    public float startDelay = 1f;

    private float timer = 0f;
    private float delayTimer = 0f;
    private AudioSource audioSource;
    private NavMeshAgent agent;
    private UnitStateManager unit;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        unit = GetComponent<UnitStateManager>();
        delayTimer = startDelay;
    }

    private void Update()
    {
        if (unit == null) return;
        if (unit.isDead || unit.isFrozen) return;
        if (alwaysPlay)
        {
            if (delayTimer > 0f)
            {
                delayTimer -= Time.deltaTime;
                return; 
            }

            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                PlayRandom();
                timer = stepInterval;
            }

            return;
        }

        if (agent == null) return;
        if (agent.velocity.magnitude <= minVelocity) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            PlayRandom();
            timer = stepInterval;
        }
    }

    private void PlayRandom()
    {
        if (footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(clip);
    }
}
