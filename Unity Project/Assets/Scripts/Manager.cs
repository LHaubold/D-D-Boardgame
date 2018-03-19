using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//does the pathfinding, UI management, 

public class Manager : MonoBehaviour
{
    GameObject CurrentNode; //temporary position of the A-star algorithm
    public GameObject StartNode; //reference to the tile the selected player occupies
    public GameObject selectedPlayer = null; //reference to the selected Player
    
    GameObject Canvas; //reference to the ui elements
    
    public List<GameObject> playableHeroes;    //contains all playable characters set in inspector
    public List<GameObject> enemyList = new List<GameObject>(); //contains all enemys currently on the board
    List<GameObject> attackingEnemiesList = new List<GameObject>(); //contains all enemies that still need to attack
    public int enemiesLeft; //number of enemies left on the board, set in inspector
    public int heroesLeft = 4; //nuber of enemies left

    List<GameObject> openList = new List<GameObject>(); //list containing possible nodes for the next step of the path
    public List<GameObject> closedList = new List<GameObject>(); //list containing the finished path
    public List<GameObject> allNodes = new List<GameObject>(); //list containing all currently loaded nodes

    public GameObject Spawner1; //position the rolled dice spawns from
    public GameObject Spawner2; //a 2nd  position to avoid dice falling on top of each other
    public Camera MainCam; //the Main Camera
    Vector3 mainCamPos; //previous position of the main cam
    public Camera battleCam; //the camera used while rolling dice
    bool battleMode = false; //returns true while game is in battle mode
    int dmgBonus; //the bonus the attacking character has
    int defense; //the defense the defending character has
    int damageDone; //damage this attack did
    int manaRegained = 0; //amount of mana this attack drained
    bool specialMove = false; //returns true when character performs his special move
    int healAmount; //amount of HP this attack Healed

    bool enemyAttack = false;
    float timing = 0;

    void Start()
    {
        Canvas = GameObject.Find("Canvas");
    }

    void Update()
    {       
        //enables controls only viable while a player is selected
        if(StaticValues.selection == true)
        {
            PlayerControl();
        }        
        
        if(StaticValues.playerCanAct == true)
        {
            if(Canvas.transform.GetChild(4).gameObject.activeSelf == true)
            {
                Canvas.transform.GetChild(4).gameObject.SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StaticValues.playerCanAct = false; //player cant move while enemy turn
                attackingEnemiesList = new List<GameObject>(enemyList); //get a List of all enemys that could attack
                nextTurn();
            }
        }

        if(enemiesLeft <= 0)
        {
            Canvas.transform.GetChild(5).gameObject.SetActive(true);
            StaticValues.playerCanAct = false;
        }
        if(heroesLeft <= 0)
        {
            Canvas.transform.GetChild(5).gameObject.SetActive(true);
            Canvas.transform.GetChild(5).transform.GetChild(0).GetComponent<Text>().text = "OH NO! Looks like you fucked it up, well to bad, maybe next time.";
            StaticValues.playerCanAct = false;
        }

        doEnemyAttack();

        UIManaging();
    }

    #region //Actions and Combat

    private void doEnemyAttack()
    {
        if (enemyAttack)
        {
            if (timing <= 1.5)
            {
                timing += Time.deltaTime;
                //move camera to currently attacking enemy
                Vector3 targetPos = new Vector3(attackingEnemiesList[0].transform.position.x, 10, (attackingEnemiesList[0].transform.position.z - 3));
                MainCam.transform.position = Vector3.Lerp(mainCamPos, targetPos, timing);
            }
            else
            {
                timing = 0;
                enemyAttack = false;
                if(attackingEnemiesList[0].GetComponent<EnemyScript>().heroesInReach.Count > 0)
                {
                    //this enemy has a hero within reach so attack
                    ChangeToBattleMode();
                    Dice.Clear();
                    Dice.Roll("1d6", "d6-red-dots", Spawner1.transform.position, Vector3.down);
                }
                else
                {
                    //remove this enemy from attackingEnemiesList since he cant attack
                    attackingEnemiesList.RemoveAt(0);
                    //this enemy cant attack so continue working towards the next playerturn
                    nextTurn();
                }
            }
        }
    }

    private void nextTurn()
    {
        //deselect the currently selected character
        StaticValues.selection = false;
        if(selectedPlayer != null)
        {
            selectedPlayer.GetComponent<PlayerScript>().selected = false;
            selectedPlayer = null;
        }

        //is there an enemy that did not attack yet
        if (attackingEnemiesList.Count > 0)
        {            
            mainCamPos = MainCam.transform.position;                
            enemyAttack = true;           
        }
        else
        {
            Debug.Log("Playerturn start");
            //no enemys so reset herostats
            for (int i = 0; i < playableHeroes.Count; i++)
            {
                playableHeroes[i].GetComponent<PlayerScript>().actions = 2; //restore the 2 actions per turn
                playableHeroes[i].GetComponent<PlayerScript>().moveRange = playableHeroes[i].GetComponent<PlayerScript>().maxMove; //if the hero had movement left when the turn ended this needs to reset
            }
            //return control to the player
            StaticValues.playerCanAct = true;
        }
    }

    //initiates normal attack
    void Attack()
    {
        Dice.Clear();
        //only attack if there actually is an enemy adjacent to the selected character and he has still an action left to attack it
        if (selectedPlayer.GetComponent<PlayerScript>().adjacentEnemy != null && selectedPlayer.GetComponent<PlayerScript>().actions > 0)
        {
            selectedPlayer.GetComponent<PlayerScript>().actions--;
            ChangeToBattleMode();
            defense = 3; //there is only one enemy type so his defense is set

            switch (selectedPlayer.name)
            {
                case ("Warrior"):
                    dmgBonus = 4;
                    Dice.Roll("1d6", "d6-black-dots", Spawner1.transform.position, Vector3.down);
                    break;
                case ("Rogue"):
                    dmgBonus = 2;
                    Dice.Roll("1d6", "d6-black-dots", Spawner1.transform.position, Vector3.down);
                    Dice.Roll("1d6", "d6-green-dots", Spawner2.transform.position, Vector3.down);
                    break;
                case ("Paladin"):
                    dmgBonus = 2;
                    Dice.Roll("1d6", "d6-black-dots", Spawner1.transform.position, Vector3.down);
                    Dice.Roll("1d6", "d6-blue-dots", Spawner2.transform.position, Vector3.down);
                    break;
                case ("Mage"):
                    dmgBonus = 0;
                    Dice.Roll("1d6", "d6-black-dots", Spawner1.transform.position, Vector3.down);
                    Dice.Roll("1d6", "d6-blue-dots", Spawner2.transform.position, Vector3.down);
                    break;
            }
        }       
    }

    //initiates special move
    void performSpecialMove()
    {
        Dice.Clear();
        if(selectedPlayer.name == "Mage") 
        {
            //only do this if the mage has a target, an action to perform the attack and enough mana
            if (selectedPlayer.GetComponent<PlayerScript>().adjacentEnemy != null && selectedPlayer.GetComponent<PlayerScript>().actions > 0 && selectedPlayer.GetComponent<PlayerScript>().currentMana >= 5)
            {
                selectedPlayer.GetComponent<PlayerScript>().actions--;
                selectedPlayer.GetComponent<PlayerScript>().currentMana -= 5;
                specialMove = true;
                dmgBonus = 5;
                Dice.Roll("1d6", "d6-black-dots", Spawner1.transform.position, Vector3.down);
                Dice.Roll("1d6", "d6-black-dots", Spawner2.transform.position, Vector3.down);
                ChangeToBattleMode();
            }
        }
        if(selectedPlayer.name == "Paladin")
        {
            //only do this if the paladin still has an action and enough mana
            if (selectedPlayer.GetComponent<PlayerScript>().actions > 0 && selectedPlayer.GetComponent<PlayerScript>().currentMana >= 2)
            {
                selectedPlayer.GetComponent<PlayerScript>().actions--;
                selectedPlayer.GetComponent<PlayerScript>().currentMana -= 2;
                specialMove = true;            
                Dice.Roll("1d6", "d6-black-dots", Spawner1.transform.position, Vector3.down);
                ChangeToBattleMode();
            }
        }
        
    }

    //called by the ok button after a player attacked
    public void EndPlayerAttack()
    {
        ChangeToNormalMode();//change back to the playerview
        if(selectedPlayer.GetComponent<PlayerScript>().adjacentEnemy != null) //this could be null after paladin heal, which does no dmg anyway
        {
            if(healAmount == 0)//only deal dmg when this attack is not a heal
            {
                selectedPlayer.GetComponent<PlayerScript>().adjacentEnemy.GetComponent<EnemyScript>().currentHP -= damageDone; //actually do the dmg
            }
        }
        selectedPlayer.GetComponent<PlayerScript>().currentMana += manaRegained; //add regained mana
        if(selectedPlayer.GetComponent<PlayerScript>().currentMana > selectedPlayer.GetComponent<PlayerScript>().maxMana)
        {
            selectedPlayer.GetComponent<PlayerScript>().currentMana = selectedPlayer.GetComponent<PlayerScript>().maxMana; //dont add more mana than this character can store
        }
        if(healAmount > 0)
        {
            //loop through all heroes for the group heal
            for(int i = 0; i < playableHeroes.Count; i++)
            {
                if(playableHeroes[i] != null)
                {
                    playableHeroes[i].GetComponent<PlayerScript>().currentHP += healAmount;
                    if(playableHeroes[i].GetComponent<PlayerScript>().currentHP > playableHeroes[i].GetComponent<PlayerScript>().maxHP)
                    {
                        playableHeroes[i].GetComponent<PlayerScript>().currentHP = playableHeroes[i].GetComponent<PlayerScript>().maxHP;
                    }
                }
            }
        }
        //reset for next attack
        manaRegained = 0; 
        specialMove = false;
        healAmount = 0;
    }

    //called by the ok button after an enemy attacked
    public void EndEnemyAttack()
    {
        ChangeToNormalMode();
        //loops through all players in attacking range and damages them
        for(int i = 0; i < attackingEnemiesList[i].GetComponent<EnemyScript>().heroesInReach.Count; i++)
        {
            attackingEnemiesList[i].GetComponent<EnemyScript>().heroesInReach[i].GetComponent<PlayerScript>().getDamaged(damageDone);
        }
        //remove this enemy from attackingEnemyList because he now attacked
        attackingEnemiesList.RemoveAt(0);
        //continue working towards next playerturn
        nextTurn();
    }

    private void ChangeToNormalMode()
    {
        //change used Cameras
        MainCam.gameObject.SetActive(true);
        battleCam.gameObject.SetActive(false);

        //activate normal UI
        Canvas.transform.GetChild(0).gameObject.SetActive(true);
        Canvas.transform.GetChild(1).gameObject.SetActive(true);
        Canvas.transform.GetChild(2).gameObject.SetActive(true);

        //deactivate battle UI
        Canvas.transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(false);
        Canvas.transform.GetChild(3).transform.GetChild(2).gameObject.SetActive(false);
        Canvas.transform.GetChild(3).gameObject.SetActive(false);

        StaticValues.playerCanAct = true;

        battleMode = false;
    }

    private void ChangeToBattleMode()
    {
        //change used camera
        MainCam.gameObject.SetActive(false);
        battleCam.gameObject.SetActive(true);

        //deactivate normal ui
        Canvas.transform.GetChild(0).gameObject.SetActive(false);
        Canvas.transform.GetChild(1).gameObject.SetActive(false);
        Canvas.transform.GetChild(2).gameObject.SetActive(false);

        //activate battle UI
        Canvas.transform.GetChild(3).gameObject.SetActive(true);

        StaticValues.playerCanAct = false;

        battleMode = true;
    }
    
    private void doorOpener()
    {
        //only open a door if there actually is a door adjacent to the selected character and he has still an action left to open it
        if (selectedPlayer.GetComponent<PlayerScript>().adjacentDoor != null && selectedPlayer.GetComponent<PlayerScript>().actions > 0)
        {
            selectedPlayer.GetComponent<PlayerScript>().actions--;
            selectedPlayer.GetComponent<PlayerScript>().adjacentDoor.GetComponent<Door>().openDoor();
        }
    }

    #endregion

    #region //UI stuff
    //everything UI related is done here
    private void UIManaging()
    {
        //handles activation of UI Elements according to current selection
        if (selectedPlayer == null)
        {
            Canvas.transform.GetChild(0).gameObject.SetActive(false); //deactivate infopanels for selected player and performable actions
        }
        else
        {
            switch (selectedPlayer.name)
            {
                case ("Warrior"):
                    Canvas.transform.GetChild(0).gameObject.SetActive(true); //activate mainpanel
                    deactivateInfoPanels(); //deactivate all infopanels
                    Canvas.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(true); //activate infopanel for warrior
                    break;
                case ("Rogue"):
                    Canvas.transform.GetChild(0).gameObject.SetActive(true); //activate mainpanel
                    deactivateInfoPanels(); //deactivate all infopanels
                    Canvas.transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(true); //activate infopanel for Rogue
                    break;
                case ("Paladin"):
                    Canvas.transform.GetChild(0).gameObject.SetActive(true); //activate mainpanel
                    deactivateInfoPanels(); //deactivate all infopanels
                    Canvas.transform.GetChild(0).transform.GetChild(3).gameObject.SetActive(true); //activate infopanel for Paladin
                    break;
                case ("Mage"):
                    Canvas.transform.GetChild(0).gameObject.SetActive(true); //activate mainpanel
                    deactivateInfoPanels(); //deactivate all infopanels
                    Canvas.transform.GetChild(0).transform.GetChild(4).gameObject.SetActive(true); //activate infopanel for Mage
                    break;
            }
        }
        
        if(battleMode == true)
        {
            manageBattleUI();
        }

        updateMiniInfoPanels();
    }

    private void manageBattleUI()
    {
        if (Canvas.transform.GetChild(0).gameObject.activeSelf == true)
        {
            Canvas.transform.GetChild(0).gameObject.SetActive(false);
        }

        if (selectedPlayer != null)
        {            
            //updates battleinfo
            switch (selectedPlayer.name)
            {             
                case ("Warrior"):
                    if(Dice.rolling == true)
                    {
                        //dice is still rolling so just put some "?" down
                        Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = "? + 4 bonus - " + defense + " defense = ? damage";
                    }
                    else
                    {
                        int diceValue = Dice.Value("");
                        damageDone = diceValue + dmgBonus - defense;
                        Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = 
                            diceValue + " + " + dmgBonus + " bonus - " + defense + " defense = " + damageDone + " damage";
                        Canvas.transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(true);
                    }
                    break;
                case ("Rogue"):
                    if(Dice.rolling == true)
                    {
                        //dice are still rolling so put some "?" down
                        Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = "? + 2 bonus  + ? + 2 bonus - " + defense + " defense = ? damage";
                    }
                    else
                    {
                        float diceValue = Dice.Value("d6-black-dots");//first dice value
                        float secondDiceValue = Dice.Value("d6-green-dots");//2nd dice value
                        int halvedDiceValue = Mathf.CeilToInt(diceValue/2);//first dicevalue halfed and rounded up   
                        int halvedSecondDiceValue = Mathf.CeilToInt(secondDiceValue/2);//second dicevalue halfed and rounded up
                        damageDone = halvedDiceValue + dmgBonus + halvedSecondDiceValue + dmgBonus - defense;
                        Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = 
                            halvedDiceValue + " + " + dmgBonus + " bonus + " + halvedSecondDiceValue + " + " + dmgBonus + " bonus - " + defense + " defense = " + damageDone + " damage";
                        Canvas.transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(true);
                    }
                    break;
                case ("Paladin"):
                    if(specialMove == false)
                    {
                        //do the normal attack
                        if (Dice.rolling == true)
                        {
                            //dice are still rolling so put some "?" down
                            Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = "? + 2 bonus  - " + defense + " defense = ? damage;  ? - 4 = ? Mana regained";
                        }
                        else
                        {
                            int diceValue = Dice.Value("d6-black-dots");//first dice value
                            int secondDiceValue = Dice.Value("d6-blue-dots");//2nd dice value
                            damageDone = diceValue + dmgBonus - defense;
                            manaRegained = secondDiceValue - 4;
                            //clamp regained mana to 0
                            if (manaRegained < 0)
                            {
                                manaRegained = 0;
                            }
                            Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text =
                                diceValue + " + " + dmgBonus + " bonus - " + defense + " defense = " + damageDone + " damage; " + secondDiceValue + " - 4 = " + manaRegained + " Mana regained";
                            Canvas.transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (Dice.rolling == true)
                        {
                            //dice are still rolling so put some "?" down
                            Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = "? / 3 = ? healed";
                        }
                        else
                        {
                            float diceValue = Dice.Value("");                            
                            healAmount = Mathf.CeilToInt(diceValue / 3);
                            Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = diceValue + " / 3 = " + healAmount + " healed";
                            Canvas.transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(true);
                        }
                    }
                    break;
                case ("Mage"):
                    if(specialMove == false)
                    {
                        //do the normal attack
                        if (Dice.rolling == true)
                        {
                            //dice are still rolling so put some "?" down
                            Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = "? - " + defense + " defense = ? damage;  ? / 2 = ? Mana regained";
                        }
                        else
                        {
                            int diceValue = Dice.Value("d6-black-dots");//first dice value
                            float secondDiceValue = Dice.Value("d6-blue-dots");//2nd dice value
                            damageDone = diceValue - defense;
                            //clamp damage done to 0
                            if (damageDone < 0)
                            {
                                damageDone = 0;
                            }
                            manaRegained = Mathf.CeilToInt(secondDiceValue / 2);
                            Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text =
                                diceValue + " - " + defense + " defense = " + damageDone + " damage; " + secondDiceValue + " / 2 = " + manaRegained + " Mana regained";
                            Canvas.transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (Dice.rolling == true)
                        {
                            //dice are still rolling so put some "?" down
                            Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = "? + " + dmgBonus + " bonus = ? damage";
                        }
                        else
                        {
                            int diceValue = Dice.Value("");
                            damageDone = diceValue + dmgBonus;
                            Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = diceValue + " + " + dmgBonus + " bonus = " + damageDone + " damage";
                            Canvas.transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(true);
                        }
                    }
                    break;
            }
        }
        else //no player selected = enemy attacks
        {
            if (Dice.rolling == true)
            {
                //dice are still rolling so put some "?" down
                Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = "? + 2 bonus = ? damage (-hero defense)";
            }
            else
            {
                int diceValue = Dice.Value("");
                damageDone = diceValue + 2;
                Canvas.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = diceValue + " + 2 bonus = " + damageDone + " damage (-hero defense)";
                Canvas.transform.GetChild(3).transform.GetChild(2).gameObject.SetActive(true);
            }
        }                      
    }

    //loops through all playable characters and updates the displayed info according to their current stats
    private void updateMiniInfoPanels()
    {
        for(int i = 0; i < playableHeroes.Count; i++)
        {
            if (playableHeroes[i] != null) //is character alive?
            {
                Canvas.transform.GetChild(1).transform.GetChild(i).transform.GetChild(2).transform.GetComponent<Text>().text =
                    playableHeroes[i].GetComponent<PlayerScript>().moveRange.ToString() + "/" + playableHeroes[i].GetComponent<PlayerScript>().maxMove.ToString(); //movement text
                Canvas.transform.GetChild(1).transform.GetChild(i).transform.GetChild(4).transform.GetComponent<Text>().text =
                    playableHeroes[i].GetComponent<PlayerScript>().currentHP.ToString() + "/" + playableHeroes[i].GetComponent<PlayerScript>().maxHP.ToString(); //HP text
                Canvas.transform.GetChild(1).transform.GetChild(i).transform.GetChild(6).transform.GetComponent<Text>().text =
                    playableHeroes[i].GetComponent<PlayerScript>().defense.ToString(); //defense text
                if (playableHeroes[i].GetComponent<PlayerScript>().maxMana == 0)
                {
                    Canvas.transform.GetChild(1).transform.GetChild(i).transform.GetChild(8).transform.GetComponent<Text>().text = "-/-"; //no magic
                }
                else
                {
                    Canvas.transform.GetChild(1).transform.GetChild(i).transform.GetChild(8).transform.GetComponent<Text>().text =
                        playableHeroes[i].GetComponent<PlayerScript>().currentMana.ToString() + "/" + playableHeroes[i].GetComponent<PlayerScript>().maxMana.ToString(); //mana text                    
                }
                Canvas.transform.GetChild(1).transform.GetChild(i).transform.GetChild(9).transform.GetComponent<Text>().text =
                        "Actions: " + playableHeroes[i].GetComponent<PlayerScript>().actions.ToString() + "/2"; //actions text
            }
            else
            {
                Canvas.transform.GetChild(1).transform.GetChild(i).transform.GetChild(10).transform.gameObject.SetActive(true); //display is dead message
            }
        }    
    }



    //loops through all infopanels and deactivates them ignoring the actions panel
    private void deactivateInfoPanels()
    {
        for(int i = 1; i < Canvas.transform.GetChild(0).childCount; i++)
        {
            Canvas.transform.GetChild(0).transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    #endregion

    //manages Input while player is selected
    private void PlayerControl()
    {
        //deselect with right-click or Esc and resets highlighted path
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            StaticValues.selection = false;
            selectedPlayer.GetComponent<PlayerScript>().selected = false;
            selectedPlayer = null;
            //reset highlighted tiles
            ResetHighlight();
        }

        //attack with Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Attack();
        }

        //open door with E
        if (Input.GetKeyDown(KeyCode.E))
        {
            doorOpener();
        }

        //use special move with R
        if (Input.GetKeyDown(KeyCode.R))
        {
            performSpecialMove();
        }
    }

    #region //pathfinding via A-star

    //resets all highlighted tiles
    public void ResetHighlight()
    {
        if (closedList.Count > 1)
        {
            for (int i = 1; i < closedList.Count; i++)
            {
                if (closedList[i].transform.GetChild(0).gameObject.activeSelf)
                {
                    closedList[i].transform.GetChild(0).gameObject.SetActive(false);
                }
            }
        }
    }

    public void StartNavigation(GameObject TargetNode)
    {
        //reset highlighted path
        ResetHighlight();
        //reset closed list
        closedList.Clear();
        //add startposition to closed list
        closedList.Add(StartNode);
        //reset neighborlists
        CalculateNeighbors();
        //create new path
        createPath(TargetNode);
    }

    private void createPath(GameObject TargetNode)
    {
        //switch currentnode to last node in closed list
        CurrentNode = closedList[closedList.Count - 1];
        //reset open list
        if (openList.Count != 0)
        {
            openList.Clear();
        }
        //check if the current node is the target node and the pathfinding is therefore finished
        if (CurrentNode != TargetNode)
        {
            //loop neighbors of current node and remove neighbors that are already in the closed list
            for (int t = 0; t < CurrentNode.GetComponent<Node>().Neighbors.Count; t++)
            {
                for (int r = 0; r < closedList.Count; r++)
                {
                    if (closedList[r] == CurrentNode.GetComponent<Node>().Neighbors[t])
                    {
                        CurrentNode.GetComponent<Node>().Neighbors.RemoveAt(t);
                        t = 0;
                    }
                }
            }
            //add remaining neighbors into open list
            for (int i = 0; i < CurrentNode.GetComponent<Node>().Neighbors.Count; i++)
            {
                openList.Add(CurrentNode.GetComponent<Node>().Neighbors[i]);
            }
            //set H and G for open list members
            for (int m = 0; m < openList.Count; m++)
            {
                openList[m].transform.GetComponent<Node>().SetGValue = Vector3.Distance(CurrentNode.transform.position, openList[m].transform.position);

                openList[m].transform.GetComponent<Node>().SetHValue = Vector3.Distance(openList[m].transform.position, TargetNode.transform.position);
            }
            //find neighbor closest to the target (=smallest F value) 
            float FValue = 10000;
            int nextNode = 0;
            for (int v = 0; v < openList.Count; v++)
            {
                float newFValue = openList[v].GetComponent<Node>().GetF;
                //add enormous value to found F value if this neighbor is occupied in order to avoid it 
                //(this ignores completely blocked paths so that this method always returns a path, blocked paths will be dealt with while moving)
                if (openList[v].GetComponent<Node>().occupied)
                {
                    newFValue += 5000;
                }
                if (newFValue < FValue)
                {
                    FValue = openList[v].GetComponent<Node>().GetF;
                    nextNode = v;
                }
            }
            closedList.Add(openList[nextNode]);

            //restart this method to find next tile along the path
            createPath(TargetNode);

        }
        else
        {
            //target was found so now new path needs to be highlighted and neighborlists need to be reset
            highlightPath();
            CalculateNeighbors();
        }
    }

    //higlights tiles up to the selected players movement Range
    private void highlightPath()
    {
        int movesLeft = selectedPlayer.GetComponent<PlayerScript>().moveRange; //number of tiles the player can still move
       //highlight as many tiles as the player can still move
        if(closedList.Count > movesLeft)
        {
            for (int i = 1; i <= movesLeft; i++)
            {
                closedList[i].transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        //the path is shorter then the players movement range so this is needed to avoid looping unnecessarily
        else
        {
            for (int n = 1; n < closedList.Count; n++)
            {
                closedList[n].transform.GetChild(0).gameObject.SetActive(true);
            }
        }    
    }

    //resets all neighbor lists
    private void CalculateNeighbors()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            allNodes[i].GetComponent<Node>().SearchForNeighbors(allNodes);
        }
    }
    #endregion  

    public void StartPlaying()
    {
        StaticValues.playerCanAct = true;
        Canvas.transform.GetChild(1).gameObject.SetActive(true);
    }

    public void EndGame()
    {
        Application.Quit();
    }
}
