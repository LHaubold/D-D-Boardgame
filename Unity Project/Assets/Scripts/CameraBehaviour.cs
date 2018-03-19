using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//moves camera and provides function to move camera to dice thrower pos and back to previous pos
public class CameraBehaviour : MonoBehaviour
{
    //Camera movementspd
    public float spd = 2.0f;

    public float maxX; //max and min values used to restrict moveable area, set in inspector
    public float minX; //
    public float maxZ; //
    public float minZ; //

    void Start ()
    {
		
	}
		
	void Update ()
    {
        Movement();
	}

    void Movement()
    {
        //is moving Camera valid?
        if(StaticValues.playerCanAct == true )
        {
            float moveSpd;
            //increase speed with shift
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                moveSpd = spd * 2;
            }
            else
            {
                moveSpd = spd;
            }
            //move camera
            var z = Input.GetAxis("Vertical") * Time.deltaTime * moveSpd;
            var x = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpd;
            transform.position += new Vector3(x, 0, z);
            //restrict area camera can move in
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, minX, maxX), transform.position.y, Mathf.Clamp(transform.position.z, minZ, maxZ));
        }
    }
}
