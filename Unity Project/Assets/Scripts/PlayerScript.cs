using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    GameObject Manager;
    public GameObject currentPos; //currently occupied node, needs to be initially set in the inspector
    public GameObject targetPos; //tile this player moves towards

    public int maxMove; //number of tiles this character can move in one action, set value in inspector
    public int moveRange; //number of tiles this character can still move
    public int actions = 2; //number of actions this character can still perform this turn
    List<GameObject> movePath = new List<GameObject>(); //List containing the path this character is supposed to move 
    List<GameObject> neighbors = new List<GameObject>(); //List of direct neighbors
    public GameObject adjacentDoor = null; //Door this character can currently open
    public GameObject adjacentEnemy = null; //Enemy this character can currently attack

    public int maxHP; //this characters max HP, set in inspector
    public int currentHP; //this characters current HP
    public int defense; //this characters defense, set in inspector
    public int maxMana; //this characters max Mana, set in inspector
    public int currentMana; //this characters current amount of Mana

    public bool selected = false; //returns true when this character is selected

    bool initialMove = true; //returns true if this is the first step this action
    bool moving = false; //returns true while the character moves
    float timing = 0;
    public float waitTime = 1.0f; //delay between steps while moving

    //damage this hero
    public void getDamaged(int damageDone)
    {
        int damageTaken = damageDone - defense;
        if(damageTaken < 0)
        {
            damageTaken = 0;
        }
        currentHP -= damageTaken;
    }

    void Start ()
    {
        Manager = GameObject.Find("Manager");
        currentHP = maxHP;
        moveRange = maxMove;
        currentMana = maxMana;
	}	    

	void Update ()
    {
        move();

        if(currentHP <= 0)
        {
            Manager.GetComponent<Manager>().heroesLeft--;
            Destroy(gameObject);
        }

        if(gameObject.name == "Mage")
        {
            if(currentMana == 10)
            {
                defense = 5;
            }
            else if(currentMana >= 5 && currentMana < 10)
            {
                defense = 3;
            }
            else
            {
                defense = 1;
            }
        }
    }
    #region //movement
    //moves the character to the next tile
    private void move()
    {
        if (moving)
        {
            if (timing <= waitTime)
            {
                timing += Time.deltaTime;
                gameObject.transform.position = Vector3.Lerp(currentPos.transform.position, movePath[0].transform.position, timing);
            }
            else
            {
                timing = 0;
                moving = false; //done moving
                currentPos.GetComponent<Node>().occupied = false; //previous position is no longer occupied
                currentPos.GetComponent<Node>().occupant = null;  //
                currentPos = movePath[0]; //update currentPos
                currentPos.GetComponent<Node>().occupied = true;       //set new position occupied
                currentPos.GetComponent<Node>().occupant = gameObject; //
                movePath.RemoveAt(0); //remove this characters position from path
                moveRange--; //character moved one step so reduce the amount of tiles he can still move
                //end movement if target tile is reached
                if (currentPos == targetPos)
                {
                    endMove();
                    return;
                }
                //check next tile if character can still move
                if (moveRange > 0)
                {
                    checkNextPos();
                }
                else
                {
                    endMove();
                }
            }
        }
    }

    private void endMove()
    {
        Manager.GetComponent<Manager>().ResetHighlight(); //reset any highlighted tiles that might be still highlighted
        //reset moveRange so player can use a second moveaction if this moveaction is done
        if (moveRange == 0)
        {
            moveRange = maxMove;
            initialMove = true;
        }
        neighbors = currentPos.GetComponent<Node>().Neighbors; //update this characters neighbors

        bool enemyFound = false; //returns true if neighbors contains a door
        bool doorFound = false; //returns true if neighbors contains an enemy

        //loop through neighbor list to search for interactable boardpieces
        for (int i = 0; i < neighbors.Count; i++)
        {
            if(neighbors[i].GetComponent<Node>().occupant != null)
            {
                //this neighbor is occupied, now check what type the neighbor is
                if(neighbors[i].GetComponent<Node>().occupant.tag == "Door")
                {
                    adjacentDoor = neighbors[i].GetComponent<Node>().occupant;
                    doorFound = true;
                }
                if(neighbors[i].GetComponent<Node>().occupant.tag == "Enemy")
                {
                    adjacentEnemy = neighbors[i].GetComponent<Node>().occupant;
                    enemyFound = true;
                }
            }
        }
        //if there is no adjacent door reset adjacentDoor
        if (doorFound == false)
        {
            adjacentDoor = null;
        }
        //if there is no adjacent enemy reset adjacentEnemy
        if(enemyFound == false)
        {
            adjacentEnemy = null;
        }
        //update startnode for pathfinding to new position
        Manager.GetComponent<Manager>().StartNode = currentPos;
        //return control to the player
        StaticValues.playerCanAct = true;
    }

    //checks if the next tile in the path is accessible
    private void checkNextPos()
    {
        //check if there is a next tile 
        if(movePath.Count > 0)
        {
            //check if next tile is occupied
            if (movePath[0].GetComponent<Node>().occupied)
            {
                // dont move if next tile is occupied
                Debug.Log("Path is blocked");
                endMove();
            }
            else
            {
                //use one action if this step starts a move action
                if(initialMove == true && moveRange == maxMove)
                {           
                    //dont move if the character has no actions left this turn 
                    if(actions == 0)
                    {
                        endMove();
                    }
                    actions--;
                    initialMove = false;
                }
                movePath[0].transform.GetChild(0).transform.gameObject.SetActive(false); //remove highlight from next tile
                moving = true; //start moving
            }
        }

    }

    public void startMove()
    {
        if(Manager.GetComponent<Manager>().closedList.Count > 0)
        {
            StaticValues.playerCanAct = false; //no input allowed while the character moves
            movePath = Manager.GetComponent<Manager>().closedList; //get current selected path as List
            movePath.RemoveAt(0); //remove this characters position from path
            checkNextPos(); //check if first step is accessible
        }
        else
        {
            //there is no path so dont move
            return;
        }
    }
    #endregion

    //selects this character if not selected and deselects it if already selected
    void OnMouseDown()
    {
        //only do this if player is allowed to act
        if(StaticValues.playerCanAct == true)
        {
            selected = !selected;

            if (selected)
            {
                if (StaticValues.selection != true)
                {
                    StaticValues.selection = true;
                }
                //if another character is already selected deselect him
                else
                {
                    Manager.GetComponent<Manager>().selectedPlayer.GetComponent<PlayerScript>().selected = false;
                    Manager.GetComponent<Manager>().ResetHighlight();
                }
                //set necessary information in the manager
                Manager.GetComponent<Manager>().StartNode = currentPos;
                Manager.GetComponent<Manager>().selectedPlayer = gameObject;

            }
            else
            {
                StaticValues.selection = false;
                Manager.GetComponent<Manager>().selectedPlayer = null;
                Manager.GetComponent<Manager>().ResetHighlight();
            }
        }   
    }
} 
