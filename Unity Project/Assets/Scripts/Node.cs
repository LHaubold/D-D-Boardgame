using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//

public class Node : MonoBehaviour
{
    GameObject Manager;
    public bool occupied = false; //returns true if this tile is occupied by a boardpiece, set in inspector when placing boardpieces
    public GameObject occupant; //boardpiece occupying this tile, set in inspector when placing boardpieces

    public List<GameObject> Neighbors = new List<GameObject>(); //list of all adjacent neighbors of this node
    public float maxDist = 1.2f; //maximum distance between 2 neighboring nodes

    [SerializeField]
    float HValue; //distance between current node and this node
    [SerializeField]
    float GValue; //distance between this node and target node

    public float SetHValue
    {
        set { HValue = value; }
    }
    public float SetGValue
    {
        set { GValue = value; }
    }
    public float GetF
    {
        get { return (GValue + HValue); }
    }

    void Awake()
    {
        //add this node into the list of all nodes
        Manager = GameObject.Find("Manager");
        Manager.GetComponent<Manager>().allNodes.Add(gameObject);
    }

    void Start()
    {
        //fill neighborlist
        SearchForNeighbors(Manager.GetComponent<Manager>().allNodes);
    }

    ///<summary>
    ///fills neighbor list with all adjacent neighbors
    ///</summary>
    public void SearchForNeighbors(List<GameObject> allNodes)
    {
        //clear neighbor list
        Neighbors.Clear();
        //loop all nodes
        for (int i = 0; i < allNodes.Count; i++)
        {
            if (allNodes[i] != gameObject)
            {
                float distance = Vector3.Distance(transform.position, allNodes[i].transform.position);
                //add node to list if adjacent
                if (distance <= maxDist)
                {                   
                    Neighbors.Add(allNodes[i]);                    
                }
            }
        }
    }
}
