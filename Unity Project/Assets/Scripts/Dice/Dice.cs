using UnityEngine;
using System.Collections;

//provides everything needed to roll dice

public class Dice : MonoBehaviour
{	
	// rollSpeed determines how many seconds pass between rolling the single dice
    public float rollSpeed = 0.25F;
	
	// rolling = true when there are dice still rolling, rolling is checked using rigidBody.velocity and rigidBody.angularVelocity
    public static bool rolling = true;

	// timer to check when next die needs to be rolled
    protected float rollTime = 0;

	// reference to the dice that have to be rolled
    private static ArrayList rollQueue = new ArrayList();
	// reference to all dice, created by Dice.Roll
	private static ArrayList allDice = new ArrayList();
	// reference to the dice that are rolling
    private static ArrayList rollingDice = new ArrayList();
    
	/// <summary>
	/// This method will create/instance a prefab at a specific position with a specific rotation and a specific scale and assign a material
	/// </summary>
	public static GameObject prefab(string name, Vector3 position, Vector3 rotation, Vector3 scale, string mat) 
	{		
		// load the prefab from Resources
        Object pf = Resources.Load("Prefabs/" + name);
		if (pf!=null)
		{
			// the prefab was found so create an instance for it.
			GameObject inst = (GameObject)Instantiate( pf , Vector3.zero, Quaternion.identity);
			if (inst!=null)
			{
				// the instance could be created so set material, position, rotation and scale.
				if (mat!="") inst.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/d6/" + mat);
				inst.transform.position = position;
				inst.transform.Rotate(rotation);
				inst.transform.localScale = scale;
				// return the created instance (GameObject)
				return inst;
			}
		}
		else
			Debug.Log("Prefab "+name+" not found!");
		return null;		
	}	
	
	/// <summary>
	/// Roll one or more dice with a specific material from a spawnPoint and give it a specific force.
	/// </summary>
	public static void Roll(string dice, string mat, Vector3 spawnPoint, Vector3 force)
	{
        rolling = true;
		// sorting dice to lowercase for comparing purposes
		dice = dice.ToLower();				
		int count = 1;
		string dieType = "d6";
		
		// 'd' must be present for a valid 'dice' specification
		int p = dice.IndexOf("d");
		if (p>=0)
		{
			// check if dice starts with d, if true a single die is rolled.
			// dice must have a count because dice does not start with 'd'
			if (p>0)
			{
				// extract count
				string[] a = dice.Split('d');
				count = System.Convert.ToInt32(a[0]);
			}
			else
				dieType = dice;
			
			// instantiate the dice
			for (int d=0; d<count; d++)
			{
				// randomize spawnPoint variation
				spawnPoint.x = spawnPoint.x - 1 + Random.value * 2;		
				spawnPoint.y = spawnPoint.y - 1 + Random.value * 2;
                spawnPoint.y = spawnPoint.y - 1 + Random.value * 2;
				// create the die prefab/gameObject
                GameObject die = prefab("d6", spawnPoint, Vector3.zero, Vector3.one, mat);
				// give it a random rotation
				die.transform.Rotate(new Vector3(Random.value * 360, Random.value * 360, Random.value * 360));
				// inactivate this gameObject because activating it will be handeled using the rollQueue and at the apropriate time
				die.SetActive(false);
				// create RollingDie class that will hold things like spawnpoint and force, to be used when activating the die at a later stage
                RollingDie rDie = new RollingDie(die, dieType, mat, spawnPoint, force);
				// add RollingDie to allDices
				allDice.Add(rDie);               
				// add RollingDie to the rolling queue
                rollQueue.Add(rDie);
			}
		}
	}

	/// <summary>
	/// Get value of all ( dieType = "" ) dice or dieType specific dice.
	/// </summary>
    public static int Value(string dieType)
    {
        int v = 0;
		// loop all dice
        for (int d = 0; d < allDice.Count; d++)
        {
            RollingDie rDie = (RollingDie) allDice[d];
			// check the type/color
            if (rDie.mat == dieType || dieType == "")
                v += rDie.die.value;
        }
        return v;
    }

	/// <summary>
	/// Get number of all ( dieType = "" ) dice or dieType specific dice.
	/// </summary>
    public static int Count(string dieType)
    {
        int v = 0;
		// loop all dice
        for (int d = 0; d < allDice.Count; d++)
        {
            RollingDie rDie = (RollingDie)allDice[d];
			// check the type/color
            if (rDie.mat == dieType || dieType == "")
                v++;
        }
        return v;
    }

	/// <summary>
	/// Clears all currently rolling dice
	/// </summary>
    public static void Clear()
	{
		for (int d=0; d<allDice.Count; d++)
			GameObject.Destroy(((RollingDie)allDice[d]).gameObject);

        allDice.Clear();
        rollingDice.Clear();
        rollQueue.Clear();

        rolling = false;
	}
    
    void Update()
    {
        if (rolling)
        {
			// there are dice rolling so increment rolling time
            rollTime += Time.deltaTime;
			// check rollTime against rollSpeed to determine if a die should be activated ( if one available in the rolling  queue )
            if (rollQueue.Count > 0 && rollTime > rollSpeed)
            {
				// get die from rolling queue
                RollingDie rDie = (RollingDie)rollQueue[0];
                GameObject die = rDie.gameObject;
				// activate the gameObject
				die.SetActive(true);
				// apply the force impuls
                die.GetComponent<Rigidbody>().AddForce((Vector3) rDie.force, ForceMode.Impulse);
				// apply a random torque
                die.GetComponent<Rigidbody>().AddTorque(new Vector3(-50 * Random.value * die.transform.localScale.magnitude, -50 * Random.value * die.transform.localScale.magnitude, -50 * Random.value * die.transform.localScale.magnitude), ForceMode.Impulse);
				// add die to rollingDice
                rollingDice.Add(rDie);
				// remove the die from the queue
                rollQueue.RemoveAt(0);
				// reset rollTime so we can check when the next die has to be rolled
                rollTime = 0;
            }
            else
                if (rollQueue.Count == 0)
                {
					// roll queue is empty so if no dice are rolling we can set the rolling attribute to false
                    if (!IsRolling())
                        rolling = false;
                }
        }
    }

	/// <summary>
	/// Check if all dice have stopped rolling
	/// </summary>
    private bool IsRolling()
    {
        int d = 0;
		// loop rollingDice
        while (d < rollingDice.Count)
        {
			// if rolling die no longer rolling , remove it from rollingDice
            RollingDie rDie = (RollingDie)rollingDice[d];
            if (!rDie.rolling)
                rollingDice.Remove(rDie);
            else
                d++;
        }
		// return false if we have no rolling dice 
        return (rollingDice.Count > 0);
    }
}

/// <summary>
/// Supporting rolling die class to keep die information
/// </summary>
class RollingDie
{

    public GameObject gameObject;		// associated gameObject
    public Die die;								// associated Die (value calculation) script

    public string name = "";				// dieType
    public string mat;						// die material (asString)
    public Vector3 spawnPoint;			// die spawnPoiunt
    public Vector3 force;					// die initial force impuls

	// rolling attribute specifies if this die is still rolling
    public bool rolling
    {
        get
        {
            return die.rolling;
        }
    }

    public int value
    {
        get
        {
            return die.value;
        }
    }

	// constructor
    public RollingDie(GameObject gameObject, string name, string mat, Vector3 spawnPoint, Vector3 force)
    {
        this.gameObject = gameObject;
        this.name = name;
        this.mat = mat;
        this.spawnPoint = spawnPoint;
        this.force = force;
		// get Die script of current gameObject
        die = (Die)gameObject.GetComponent(typeof(Die));
    }

	// ReRoll this specific die
    public void ReRoll()
    {
        if (name != "")
        {
            GameObject.Destroy(gameObject);
            Dice.Roll(name, mat, spawnPoint, force);
        }
    }
}

