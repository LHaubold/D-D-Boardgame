using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyScript : MonoBehaviour
{
    GameObject Manager;
    GameObject Canvas;

    //stats of this enemy, set in inspector
    public int maxHP;
    public int currentHP;
    public int defense;

    List<GameObject> neighbors = new List<GameObject>(); //contains all neighbors
    public List<GameObject> heroesInReach = new List<GameObject>(); //contains all attackable Heroes
    public GameObject currentPos; //tile this enemy stands on, set in inspector
    public bool didAttack = false;

	void Start ()
    {
        Manager = GameObject.Find("Manager");
        Canvas = GameObject.Find("Canvas");
        Manager.GetComponent<Manager>().enemyList.Add(gameObject);	//add this enemy to the enemyList
        currentHP = maxHP;//spawn with full HP
	}
	
	void Update ()
    {
        //kill this enemy when he has no HP left
	    if(currentHP <= 0)
        {
            Manager.GetComponent<Manager>().enemyList.Remove(gameObject); //remove this enemy from the enemyList
            Manager.GetComponent<Manager>().enemiesLeft--; //remove one from the remaining enemys counter
            Canvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(false);//just in case this enemys infopanel was activated
            currentPos.GetComponent<Node>().occupant = null;
            currentPos.GetComponent<Node>().occupied = false;
            Destroy(gameObject);
        }

        checkForAttackableNeighbors();
	}

    //fills heroesInReach 
    private void checkForAttackableNeighbors()
    {
        heroesInReach.Clear();

        neighbors = currentPos.GetComponent<Node>().Neighbors; //update this characters neighbors        

        //loop through neighbor list to search for interactable boardpieces
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (neighbors[i].GetComponent<Node>().occupant != null)
            {
                //this neighbor is occupied, now check what type the neighbor is
                if (neighbors[i].GetComponent<Node>().occupant.tag == "Player")
                {
                    heroesInReach.Add(neighbors[i].GetComponent<Node>().occupant);                    
                }                
            }
        }     
    }

    void OnMouseEnter()
    {
        //activate this enemys Infopanel and display its current HP(there is currently only one enemytype, otherwise this would get more complicated)
        Canvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(true);
        Canvas.transform.GetChild(2).transform.GetChild(0).transform.GetChild(4).GetComponent<Text>().text = "Health: " + currentHP.ToString() + "/5";
    }

    void OnMouseExit()
    {
        //disable this enemys infopanel
        Canvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(false);
    }
}
