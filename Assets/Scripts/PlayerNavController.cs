using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class PlayerNavController : MonoBehaviour {

    // Reference to this script to access public methods
    public static PlayerNavController Static;
    
    // NavMesh info
    public Vector3 initialPosition;
    private Vector3 destination;
    private NavMeshAgent nmAgent;
    private Animator animAgent;

    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    private float nextJump;
    private bool isJumping = false;


    // Use this for initialization
    void Start () {
	    nmAgent = GetComponent<NavMeshAgent>();
        animAgent = GetComponent<Animator>();
        Static = this;
        destination = initialPosition;
        nextJump = Time.time;
    }

    // Update is called once per frame
    void Update() {
        if (transform.position != destination)
        {
            nmAgent.SetDestination(destination);

            // Update animation parameters
            AnimateLogic.Static.AnimateNavMesh(Vector3.Distance(transform.position, destination) > 0.5f, false);
            if (nmAgent.isOnOffMeshLink && !isJumping)
            {
                isJumping = true;
            }
            else
            {
                nmAgent.CompleteOffMeshLink();
                nmAgent.Resume();
                isJumping = false;
            }
            /*
            if (nmAgent.isOnOffMeshLink && nextJump > Time.time && !isJumping)
            {
                nextJump = Time.time + 1.0f;
                isJumping = true;
                AnimateLogic.Static.AnimateNavMesh(Vector3.Distance(transform.position, destination) > 0.5f, true);
            }
            else
            {
                AnimateLogic.Static.AnimateNavMesh(Vector3.Distance(transform.position, destination) > 0.5f, false);

                if (Time.time > nextJump && isJumping)
                {
                    nmAgent.CompleteOffMeshLink();
                    nmAgent.Resume();
                    isJumping = false;
                }
            }*/
        }
    }


    /* Utility Functions
     */
    public void SetDestination(Vector3 target)
    {
        destination = target;
    }
}
