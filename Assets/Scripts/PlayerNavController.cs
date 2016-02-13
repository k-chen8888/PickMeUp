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

    // Use this for initialization
    void Start () {
	    nmAgent = GetComponent<NavMeshAgent>();
        animAgent = GetComponent<Animator>();
        Static = this;
        destination = initialPosition;
    }
	
	// Update is called once per frame
	void Update () {
        nmAgent.SetDestination(destination);

        // Update animation parameters
        AnimateLogic.Static.AnimateNavMesh(Vector3.Distance(transform.position, destination) > 0.5f);
    }


    /* Utility Functions
     */
    public void SetDestination(Vector3 target)
    {
        destination = target;
    }
}
