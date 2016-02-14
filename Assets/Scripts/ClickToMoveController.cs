using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMoveController : MonoBehaviour {

    public Camera playerCamera;

    // Use this for initialization
    void Start () {
        
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                PlayerNavController.Static.SetDestination(hit.point);
            }
        }
    }
}
