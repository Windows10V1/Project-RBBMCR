using System;
using UnityEngine;
using UnityEngine.AI;


public class SuperintendentScript : MonoBehaviour
{
    private void Start()
    {
        agent = base.GetComponent<NavMeshAgent>(); //Get the agent
        audioDevice = gameObject.AddComponent<AudioSource>(); //Creates the AudioSource
        baldiScript = FindObjectOfType<BaldiScript>(); //Finds the BaldiScript
        wanderer = FindObjectOfType<AILocationSelectorScript>(); //Finds the Script which selects the location for the AI
        wanderTarget = GameObject.Find("/Environment/AIWanderPoints/AI_LocationSelector").transform; //Finds the transform of the AI  Location Selector so that he can actually go there
        player = GameObject.Find("Player").transform; //Finds the players transform
    }

    private void Update()
    {
        if (this.seesPlayer)
        {
            CallBaldi(); // Call Baldi to his location
        }
        if (this.coolDown > 0f)
        {
            this.coolDown -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (coolDown <= 0f) // If the Superintendent is not angry   
        {
            if (this.agent.velocity.magnitude <= 1f)
            {
                this.Wander(); // Start wandering again
            }

            this.aim = this.player.position - base.transform.position; // If he sees the player and his cooldown is low
            if (Physics.Raycast(base.transform.position, this.aim, out this.hit, float.PositiveInfinity, 769, QueryTriggerInteraction.Ignore) & this.hit.transform.tag == "Player")
            {
                if (activateOnDistance)
                {
                    if ((base.transform.position - this.player.position).magnitude <= 60f) // Change this if you want Superintendent range to be higher/lower
                    {
                        this.seesPlayer = true;
                    }
                }
                else
                {
                    this.seesPlayer = true;
                }
            }
        }
        if (this.agent.velocity.magnitude <= 1f)
        {
            this.Wander(); // Start wandering again
        }
    }

    private void Wander()
    {
        this.wanderer.SetNewTargetForAgent(this.agent, AILocationSelectorScript.LocationType.Default);
        this.agent.SetDestination(this.wanderTarget.position);
    }

    private void CallBaldi()
    {
        Debug.Log("CallBaldi Worked");
        if (randomCoolDown)
        {
            coolDown = UnityEngine.Random.Range(30f, 150f);
        }
        else
        {
            coolDown = 90f; // Change this if you want the Superintendent to wait longer/shorter
        }
        seesPlayer = false;
        audioDevice.PlayOneShot(aud_CallBaldi);
        baldiScript.Hear(player.position, 2f);
        Debug.Log("Baldi heard the Superintendent");
    }

    private bool seesPlayer;

    public bool activateOnDistance; //When activated. It will make the superintendent have a short vision. Only calling baldi when the player is close to him

    public bool randomCoolDown;  //When activated. It will make the superintendent wait for a random amount of time until he activates again

    private Transform player;

    private BaldiScript baldiScript;

    private Transform wanderTarget;

    private AILocationSelectorScript wanderer;

    public float coolDown;

    public AudioClip aud_CallBaldi; // The audio that plays when he is calling for baldi

    private NavMeshAgent agent;

    private AudioQueueScript audioQueue;

    private AudioSource audioDevice;

    private RaycastHit hit;

    private Vector3 aim;
}
