using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using System.Collections;


public class HeartScript : MonoBehaviour
{

    private void Start()
    {
        this.agent = base.GetComponent<NavMeshAgent>();
        this.gc = FindObjectOfType<GameControllerScript>();
        this.player = FindObjectOfType<PlayerScript>().transform;
        this.wanderer = FindObjectOfType<AILocationSelectorScript>();

        if (Sound)
        {
            audiosource = base.GetComponent<AudioSource>();
        }

        this.Wander();
    }

    private void Update()
    {
        if (this.coolDown > 0f)
        {
            this.coolDown -= 1f * Time.deltaTime;
        }
    }


    private void FixedUpdate()
    {
        if (!ChasePlayer)
        {
            this.db = false;
            if (this.agent.velocity.magnitude <= 1f & this.coolDown <= 0f)
            {
                this.Wander();
            }
        }
        else
        {
            Vector3 direction = this.player.position - base.transform.position;
            RaycastHit raycastHit;
            if (Physics.Raycast(base.transform.position, direction, out raycastHit, float.PositiveInfinity, 769, QueryTriggerInteraction.Ignore) & raycastHit.transform.tag == "Player" & (base.transform.position - this.player.position).magnitude <= 80f & this.IsAvailable)
            {
                this.playerSeen = true;
                this.TargetPlayer();
            }
            else if (this.playerSeen & this.coolDown <= 0f)
            {
                this.playerSeen = false;
                this.Wander();
            }
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsAvailable == false)
        {
            return;
        }

        if (other.transform.tag == "Player") // If touching the player
        {
            if (Sound == true)
            {
                audiosource.PlayOneShot(SoundPlayed);
            }

            int id = Mathf.RoundToInt(UnityEngine.Random.Range(1f, (Items - 1)));
            if (Items == -1)
            {
                id = 0;
            }
            ItemManager.Instance.CollectItem(itemID);
            StartCoroutine(StartCooldown());
        }
    }

    private void Wander()
    {
        this.wanderer.SetNewTargetForAgent(this.agent, AILocationSelectorScript.LocationType.Hall);
        this.agent.SetDestination(this.AI_LocationSelector.position); //Set its destination to position of the wanderTarget
        this.coolDown = 1f;
    }

    private void TargetPlayer()
    {
        this.agent.SetDestination(this.player.position); //Set it's destination to the player
        this.coolDown = 1f;
    }

    public IEnumerator StartCooldown()
    {
        IsAvailable = false;
        yield return new WaitForSeconds(CooldownDuration);
        IsAvailable = true;
    }

    [Header("Set this please!")]

    [SerializeField] private int itemID = 1;

    public Transform AI_LocationSelector;

    [Header("Customisable Parts!")]

    [Tooltip("How long the cooldown is between times it can give items")]
    public float CooldownDuration = 40;

    [Tooltip("Whether there is a sound when you recieve an item")]
    public bool Sound;

    [Tooltip("(REQUIRES SOUND TO BE TOGGLED) The sound played when recieving an item")]
    public AudioClip SoundPlayed;

    [Tooltip("Follows the player once they can recieve an item")]
    public bool ChasePlayer = false;

    [Tooltip("How many items are in the game?")]
    public float Items = 11;

    [Header("Other Stuff (Not recommended to interfere)")]

    public bool db;

    public bool IsAvailable = true;

    private Transform player;

    private AILocationSelectorScript wanderer;

    private NavMeshAgent agent;

    private GameControllerScript gc;

    private AudioSource audiosource;

    private float coolDown;

    private bool playerSeen;


}