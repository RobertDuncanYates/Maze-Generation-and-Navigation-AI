using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Node 
{
    public bool[] X = new bool[2] { true, true };// -1 1 Whether walls on X are present
    public bool[] Z = new bool[2] { true, true };//Whether walls on Y are present
    public bool Found;//whether ai generation has found this node
    public bool DeadEnd;//whether navigator has deemed this a dead end
    public PhysicalNode PhysicalNode;//store reference to physical node in game world
}
