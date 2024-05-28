using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class Menu : MonoBehaviour //used to add funtionality to main menu
{
    private GameStorage storage; //reference to script that will pass values to the next scene
    public TMP_InputField XText; //reference to text box storing user input for X value
    private int Xtextsave = 10; //stores what text box used to equal in case an invalid input is entered
    public TMP_InputField YText;//reference to text box storing user input for Y value
    private int Ytextsave = 10;//stores what text box used to equal in case an invalid input is entered
    // Start is called before the first frame update
    void Start()
    {
        storage = FindObjectOfType<GameStorage>();//find game storage
    }
    public void changeininputX()//if input box is edited
    {
        try
        {
            int newnumber = int.Parse(XText.text);//try convert input to a number
            if(newnumber < 5)//if number less than 5
            {
                XText.text = Xtextsave.ToString();//reset textbox back
            }
        }
        catch//if input fails to convert to number
        {
            XText.text = Xtextsave.ToString();//reset textbox back
        }
        Xtextsave = int.Parse(XText.text);//save new input
    }
    public void changeininputY()//if input box is edited
    {
        try//try convert input to a number
        {
            int newnumber = int.Parse(YText.text);//if number less than 5
            if (newnumber < 5)
            {
                YText.text = Ytextsave.ToString();//reset textbox back
            }
        }
        catch//if input fails to convert to number
        {
            YText.text = Ytextsave.ToString();//reset textbox back
        }
        Ytextsave = int.Parse(YText.text);//save new input
    }

    public void GoToGame()//changes the scene
    {
        changeininputX();//make sure inputs are valid
        changeininputY();
        storage.y= int.Parse(YText.text);//saves maze size for next scene
        storage.x = int.Parse(XText.text);
        SceneManager.LoadScene("SampleScene");//load next scene
    }
    public void Quit()//Runs quit command
    {
        Application.Quit();
    }
}
