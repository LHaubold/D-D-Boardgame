using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    //set these in inspector when placing the door
    public GameObject room;//second room
    public GameObject doorPosition; //Node this Door was placed on, should not belong to any room
    
    //opens door and activates the room behind this door 
    public void openDoor()
    {
        //this doesnt work with rooms that have multiple entrances, since the door is part of a room and wont be active if we enter from the wrong entrance in the current setup
        //this is why the leveldesign needs to be linear with only single-entrance rooms until this issue is taken care of
        //using a door-manager that activates doors when one of their adjacent rooms is active would be my go-to solution right now
        room.SetActive(true);

        //unblock this doors position so characters can move through it
        doorPosition.GetComponent<Node>().occupied = false;
        doorPosition.GetComponent<Node>().occupant = null;

        //get rid of this door
        Destroy(gameObject);
    }
}
