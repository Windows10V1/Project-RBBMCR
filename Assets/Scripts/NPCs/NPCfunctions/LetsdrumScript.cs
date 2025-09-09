using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Credits to the person who made BBRMS DJ Yellow character script. He a real one

public class LetsDrumScript : MonoBehaviour
{
    // Customizable Bools so that your character can behave differently without any serious effort
    [Header("Customizable Bools")]

    [Tooltip("When enabled. The character will walk to the player when they see them (Default = Off)")]
    public bool walkToPlayer = false;

    [Tooltip("When enabled. The character will stop moving upon being touched (Default = Off)")]
    public bool stopUponTouched = false;

    [Tooltip("When enabled. The character will mute all the other sounds when activated (Default = On)")]
    public bool muteOtherSounds = true;

    [Tooltip("When enabled. Will give a random cool down to the character when they finish activating (Default = Off)")]
    public bool randomCoolDown = false;

    // Audio Related Stuff
    [Space(10)]
    [Header("Audio")]

    [Tooltip("The idle song for the character (Not required to run)")]
    public AudioClip idle;

    [Tooltip("The audio played when the character spots you (Not required to run)")]
    public AudioClip foundPlayer;

    [Tooltip("The audio played when the character activates (Required to run)")]
    public AudioClip antiHearing;

    // Miscellaneous stuff
    [Space(10)]
    [Header("Miscellaneous")]

    [Tooltip("How many times the Anti Hearing sound plays (Default = 4)")]
    public int repeatAntiHearing = 4;

    [Tooltip("Length between each random cool down (Won't work when Random Cool Down is disabled)")]
    public Vector2 randomCoolDownValues = new Vector2(30f, 90f);



    // Private stuff to not hog space in the inspector
    private Transform player;
    private Transform wanderTarget;
    private AILocationSelectorScript wanderer;
    private NavMeshAgent agent;
    private float coolDown;
    private bool playerSeen;
    private float characterCooldown;
    private AudioSource idleSource;
    private AudioSource audioDevice;
    private BaldiScript baldiScript;

    private void Start()
    {
        baldiScript = FindObjectOfType<BaldiScript>();
        agent = GetComponent<NavMeshAgent>();

        // If you are in v1.3.2. There is no Navmesh on Placeface. This if statement detects if this is true. And will create a brand new navmesh
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
            agent.baseOffset = 1f;
            agent.speed = 20f;
            agent.angularSpeed = 360f;
            agent.acceleration = 100f;

            agent.radius = 1f;
            agent.height = 2f;
        }

        wanderTarget = GameObject.Find("/AIWanderPoints/AI_LocationSelector").transform;
        wanderer = FindObjectOfType<AILocationSelectorScript>();
        player = GameObject.Find("Player").transform;
        audioDevice = gameObject.AddComponent<AudioSource>();


        // Creates the Idle source
        if (idle != null) // If the idle sound is nil. Then it won't even bother creating the idle source
        {
            idleSource = gameObject.AddComponent<AudioSource>();
            idleSource.clip = idle;
            idleSource.loop = true;
            idleSource.spatialBlend = 1;

            idleSource.rolloffMode = AudioRolloffMode.Linear;

            // You can change these so that you can hear the idle song alot closer/farther
            idleSource.minDistance = 10f;
            idleSource.maxDistance = 70f;

            idleSource.Play();
        }

        if (antiHearing == null)
        {
            Debug.LogWarning("Anti Hearing Audio hasn't been referenced yet! Don't forget to do that now or else you may experience a buggy character");
        }

        Wander();
    }

    private void Update()
    {
        if (coolDown > 0f)
        {
            coolDown -= 1f * Time.deltaTime;
        }

        if (characterCooldown > 0f)
        {
            characterCooldown -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        Vector3 direction = player.position - base.transform.position;
        RaycastHit hitInfo;
        if (Physics.Raycast(base.transform.position, direction, out hitInfo, float.PositiveInfinity, 769, QueryTriggerInteraction.Ignore) & (hitInfo.transform.tag == "Player"))
        {
            if (!playerSeen & characterCooldown <= 0f)
            {
                PlayerFound();
            }
            if (walkToPlayer & characterCooldown <= 0f)
            {
                TargetPlayer();
                return;
            }
        }
        if ((agent.velocity.magnitude <= 1f) & (coolDown <= 0f))
        {
            playerSeen = false;
            Wander();
        }
    }

    private void Wander()
    {
        wanderer.SetNewTargetForAgent(agent, AILocationSelectorScript.LocationType.Hall);
        agent.SetDestination(wanderTarget.position);
        coolDown = 1f;
    }

    private void TargetPlayer()
    {
        agent.SetDestination(player.position);
        coolDown = 1f;
    }

    // Anti Hearing Code
    private IEnumerator AntiHearing()
    {
        baldiScript.ActivateAntiHearing(antiHearing.length * repeatAntiHearing);
        MuteEveryone(muteOtherSounds);
        if (stopUponTouched)
        {
            agent.isStopped = true;
        }

        if (randomCoolDown)
        {
            characterCooldown = UnityEngine.Random.Range(randomCoolDownValues.x, randomCoolDownValues.y);
        }
        else
        {
            characterCooldown = 60f;
        }


        audioDevice.clip = antiHearing;
        for (int i = 0; i < repeatAntiHearing; i++)
        {
            audioDevice.Play();
            yield return new WaitForSeconds(antiHearing.length);
        }

        agent.isStopped = false;

        MuteEveryone(false);
        yield break;
    }

    // Part of the DJ Yellow Code
    private void MuteEveryone(bool mute)
    {
        if (mute)
        {
            foreach (AudioSource device in UnityEngine.Object.FindObjectsOfType<AudioSource>())
            {
                device.volume = 0f;
                audioDevice.volume = 1f;
            }
        }
        else
        {
            foreach (AudioSource device2 in UnityEngine.Object.FindObjectsOfType<AudioSource>())
            {
                device2.volume = 1f;
            }
        }
    }

    // When the character touches the player
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.name == "Player" & characterCooldown <= 0)
        {
            StartCoroutine(AntiHearing());
        }
    }

    // When the player has first been found
    private void PlayerFound()
    {
        playerSeen = true;
        if (foundPlayer != null)
        {
            audioDevice.clip = foundPlayer;
            audioDevice.Play();
        }
    }

}