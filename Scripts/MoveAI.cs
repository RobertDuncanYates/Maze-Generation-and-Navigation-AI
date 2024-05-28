using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAI : MonoBehaviour
{
    public bool moving; //stores if ai is moving
    public float[] moveto; //stores coord ai is moving to
    public float speed;//store speed of ai

    public void GoToCoord(int x,int z)//sends ai to coord from Array coord
    {
        moveto = new float[2] { (float)x * 3,(float)z * 3 };
    }
    // Start is called before the first frame update
    void Start()
    {
        moveto = new float[2] { gameObject.transform.position.x, gameObject.transform.position.z };//set move to to current position
    }

    // Update is called once per frame
    void Update()
    {

        if (moveto[0] == gameObject.transform.position.x && moveto[1] == gameObject.transform.position.z){moving = false;}//if at the location it needs to move to set moveing to false
        else//if it needs to move
        {
            moving = true;
            float[] movedir = new float[2] {0,0};//Calc the direction it needs to move to
            if (moveto[0] > gameObject.transform.position.x) { movedir[0] = 1; }
            else if (moveto[0] < gameObject.transform.position.x) { movedir[0] = -1; }
            if (moveto[1] > gameObject.transform.position.z) { movedir[1] = 1; }
            else if (moveto[1] < gameObject.transform.position.z) { movedir[1] = -1; }
           
            //move in that direction
            gameObject.transform.position = new Vector3(gameObject.transform.position.x + (movedir[0] * Time.deltaTime * speed), 1, gameObject.transform.position.z + (movedir[1] * Time.deltaTime * speed));
            
            //if the AI over shoots its target just jump to the target pos
            if(movedir[0] == 1 && moveto[0] < gameObject.transform.position.x) { gameObject.transform.position = new Vector3(moveto[0], 1, gameObject.transform.position.z); }
            else if (movedir[0] == -1 && moveto[0] > gameObject.transform.position.x) { gameObject.transform.position = new Vector3(moveto[0], 1, gameObject.transform.position.z); }
            if (movedir[1] == 1 && moveto[1] < gameObject.transform.position.z) { gameObject.transform.position = new Vector3(gameObject.transform.position.x, 1, moveto[1]); }
            else if (movedir[1] == -1 && moveto[1] > gameObject.transform.position.z) { gameObject.transform.position = new Vector3(gameObject.transform.position.x, 1, moveto[1]); }
        }
    }
}
