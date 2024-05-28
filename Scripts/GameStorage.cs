using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStorage : MonoBehaviour //passed data between  menu and samplescene
{
    private bool firstload = true;
    public static GameStorage instance;
    //storing varibles
    public int x; //size of maze
    public int y;
    void Awake()
    {
        DontDestroyOnLoad(gameObject); //makes it carry to the next scene
        if (firstload == true)//Only run at the fist scene loaded
        {
            if (instance == null)//if there is no DiffultyStorage
            {
                instance = this; //sets this as the main DiffultyStorage
                firstload = false; //no longer first load
            }
            else
            {
                Destroy(gameObject);//destroy if DiffultyStorage excists already
                return;
            }
        }
    }
    
}
