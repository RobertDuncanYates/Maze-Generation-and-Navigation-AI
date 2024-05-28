using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraMovement : MonoBehaviour
{
    public float speed; //Speed of camera
    
    void Update()
    {
        float x = Input.GetAxis("Horizontal");//Inputs
        float z = Input.GetAxis("Vertical");
        float y = (0 - Input.mouseScrollDelta.y)*100; //reverses input so moving scroll wheel back will increase Y and vise versa
        //Moves Camera
        gameObject.transform.position = new Vector3 (gameObject.transform.position.x + ((x * speed) * Time.deltaTime), gameObject.transform.position.y + ((y * speed) * Time.deltaTime), gameObject.transform.position.z + ((z * speed) * Time.deltaTime));
        if(gameObject.transform.position.y < 10)//if camera is below Y level 10
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, 10, gameObject.transform.position.z );//bring the camera back to Y level 10

        }
        if (Input.GetKeyDown(KeyCode.Escape))//if esc key is pressed
        {
            SceneManager.LoadScene("Menu");//return to main menu
        }
    }
}
