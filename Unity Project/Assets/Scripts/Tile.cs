using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    GameObject Manager;
    public GameObject parentNode;

    void Start()
    {
        Manager = GameObject.Find("Manager");        
        parentNode = transform.parent.gameObject;
    }

    void OnMouseEnter()
    {
        if (StaticValues.selection == true && StaticValues.playerCanAct == true)//only do this if player is selected and allowed to act and can still move
        {
           if(Manager.GetComponent<Manager>().selectedPlayer.GetComponent<PlayerScript>().actions > 0)//character has actions left so he can still move
            {
                Manager.GetComponent<Manager>().StartNavigation(parentNode);
            }
           else if(Manager.GetComponent<Manager>().selectedPlayer.GetComponent<PlayerScript>().actions == 0 && 
                Manager.GetComponent<Manager>().selectedPlayer.GetComponent<PlayerScript>().moveRange > 0 &&
                Manager.GetComponent<Manager>().selectedPlayer.GetComponent<PlayerScript>().moveRange < Manager.GetComponent<Manager>().selectedPlayer.GetComponent<PlayerScript>().maxMove) 
                //character has no actions left but already started a moveaction which he didnt fully use yet so he can still use the moves he has left
            {
                Manager.GetComponent<Manager>().StartNavigation(parentNode);
            }
        }
    }

    void OnMouseDown()
    {
        if(StaticValues.selection == true)
        {
            //initialize movement

            Manager.GetComponent<Manager>().selectedPlayer.GetComponent<PlayerScript>().targetPos = parentNode;
            Manager.GetComponent<Manager>().selectedPlayer.GetComponent<PlayerScript>().startMove();
        }
    }
}
