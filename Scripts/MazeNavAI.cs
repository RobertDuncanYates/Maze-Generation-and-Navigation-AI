using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeNavAI : MonoBehaviour
{
    public int StageInt; //this was a varible thrown in for debugging. When = 0 AI behaves as normal but when -1 the AI will stop every move

    public bool stuck; //Will be true if AI is stuck and has no where to go
    public MoveAI TheMoveAI; //link to script moving the AI object
    public int State; //stores current behaviour 0 wait for move 1 calc move

    private List<List<Node>> MapMemory;//stores Ai memory / recreation of maze
    private List<int[]> MoveHistory;//stores where the AI came from

    public GameObject winobject;//stores reference to goal object
    //Note convert between array map and physical map is *3
    public int[] currentpos; //store current coords on array map. 
    //public int idnum = 0;// used

    public GameObject debugPrefab; //stores a prefab of red balls to spawn into scene to show what the ai thinks the maze looks like
    public GameObject DeadEndPrefab;//stores a dead end prefab of blue balls to spawn into scene to show what the ai thinks the maze looks like
    public GameObject allprefabs;//this gameobject will hold all spawned ball prefabs so the ai can easily refresh
    public GameObject WINTEXT; //stores prefab to text saying the ai found its goal

    // Start is called before the first frame update
    void Start()
    {
        State = 1; //set state to calc move to start with
        currentpos = new int[2] { (int)(gameObject.transform.position.x / 3), (int)(gameObject.transform.position.z / 3) }; //convert in world location to array location
        //Create empty map
        MapMemory = new List<List<Node>>() { };//create a fresh list to recreat the maze
        MoveHistory = new List<int[]>() { currentpos };//add current position to the move history to stop backtracking
        
        for (int x = 0; x < (int)(gameObject.transform.position.x/3) + 1; x++) //The AI knows the maze always starts at position 0,0 so the AI can make a guess on how the big the maze is based on its current position
        {
            MapMemory.Add(CreateEmptyNodeList(currentpos[1] + 1));//adds rows of empty nodes to the maze recreation
        }

    }

    private List<Node> CreateEmptyNodeList(int size)//creates  row of empty nodes
    {
        List<Node> newlistnode = new List<Node>() { };//row of empty nodes
        for(int i = 0;i < size; i++)//loops through for specified size 
        {
            newlistnode.Add(new Node());//adds node
        }
        return newlistnode;//returns row of nodes
    }

    private void ResizeMapMemory(int[] coordfound)//resizes MapMemory size to fit any new found coords outside the bounds of the array
    {
        if (coordfound[0] > MapMemory.Count - 1) //if the X needs resizing
        {

            for (int x = MapMemory.Count; x < coordfound[0] + 1; x++) { MapMemory.Add(CreateEmptyNodeList(MapMemory[0].Count)); }//add new rows
        }
        if (coordfound[1] > MapMemory[0].Count - 1) //if the z needs resizing
        {
            int OgZCount = MapMemory[0].Count;
            for (int x = 0; x < MapMemory.Count; x++)//add nodes to each row
            {
                for (int z = OgZCount; z < coordfound[1] + 1; z++)
                {
                    MapMemory[x].Add(new Node());
                }
            }
        }
    }

    private int[] ConvertWallToCoord(GameObject wall)//converts a world Wall coord to coord in MapMemory
    {
        int[] wallcoord = new int[2] { 0, 0 };
        int modx = (int)(wall.transform.position.x) % 3; //if mod of pos x is equal to 1 then the wall belongs to the center behind it. If not tobelongs to the center in front
        int modz = (int)(wall.transform.position.z) % 3;

        if (modx == 1) { wallcoord[0] = (int)(wall.transform.position.x) - 1; } //find correct position coords
        else { wallcoord[0] = (int)(wall.transform.position.x) + 1; }
        if (modz == 1) { wallcoord[1] = (int)(wall.transform.position.z) - 1; }
        else { wallcoord[1] = (int)(wall.transform.position.z) + 1; }

        wallcoord = new int[2] { wallcoord[0] / 3, wallcoord[1] / 3 }; //divide by 3 to convert in game coords to list coords
        return wallcoord;

    }

    private void CutMapMemory(int[] StartPoint, int[] EndPoint) //create pathway between nodes
    {
        if (StartPoint[0] != EndPoint[0])// If cutting through X 
        {
            int movedir = 1; //set direction were cutting path through
            if (EndPoint[0] < StartPoint[0]) { 
                movedir = -1;
                MapMemory[EndPoint[0]][EndPoint[1]].X[0] = true;//cut in direction
            }
            else { MapMemory[EndPoint[0]][EndPoint[1]].X[1] = true; }

            for (int i = StartPoint[0]; i != EndPoint[0]; i += movedir)// move between Start pos and End pos
            {
                if (movedir == 1) //move forward
                {
                    MapMemory[i][StartPoint[1]].X[1] = false;//cut wall between nodes
                    try
                    {
                        MapMemory[i + 1][StartPoint[1]].X[0] = false;
                    }
                    catch { }
                }
                else //move backward
                {
                    MapMemory[i][StartPoint[1]].X[0] = false;//cut walls between nodes
                    try
                    {
                        MapMemory[i - 1][StartPoint[1]].X[1] = false;
                    }
                    catch { }
                }
            }

        }
        else if (StartPoint[1] != EndPoint[1])//If cutting through Z
        {
            int movedir = 1; //set move direction
            if (EndPoint[1] < StartPoint[1]) { 
                movedir = -1;
                MapMemory[EndPoint[0]][EndPoint[1]].Z[0] = true;
            }
            else
            {
                MapMemory[EndPoint[0]][EndPoint[1]].Z[1] = true;
            }
            for (int i = StartPoint[1]; i != EndPoint[1]; i += movedir)// move between Start pos and End pos
            {
                if (movedir == 1) //move forward
                {
                    MapMemory[StartPoint[0]][i].Z[1] = false;//cut walls between nodes
                    try
                    {
                        MapMemory[StartPoint[0]][i + 1].Z[0] = false;
                    }
                    catch { }
                }
                else //move backward
                {
                    MapMemory[StartPoint[0]][i].Z[0] = false;//cut walls between nodes
                    try
                    {
                        MapMemory[StartPoint[0]][i - 1].Z[1] = false;
                    }
                    catch { }
                }
            }
        }
    }

    private bool DrawOutMaze() //draw out new infomation in maze memory 
    {
        bool WinningObjectFound = false;

        //Shoot Ray Forward
        Ray Forwardray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
        RaycastHit hit;
        if (Physics.Raycast(Forwardray, out hit))
        {
            if (hit.collider.tag == "win") { //if it hits goal, save goal, let script know goal has been found
                winobject = hit.collider.gameObject;
                WinningObjectFound = true;
            } //If hits goal return the goal position
            else if (hit.collider.tag == "wall")//if hits hall, convert wall to a coord in map and resize map and cut map accordingly 
            {
                int[] wallcoord = ConvertWallToCoord(hit.collider.gameObject);

                if (wallcoord != currentpos)
                {
                    ResizeMapMemory(wallcoord);
                    CutMapMemory(currentpos, wallcoord);
                }

            }
        }
        //Shoot Ray Backward
        Ray Backwardray = new Ray(transform.position, transform.TransformDirection(Vector3.back));
        if (Physics.Raycast(Backwardray, out hit))
        {
            if (hit.collider.tag == "win")//if it hits goal, save goal, let script know goal has been found
            {
                winobject = hit.collider.gameObject;
                WinningObjectFound = true;
            } //If hits goal return the goal position
            else if (hit.collider.tag == "wall")//if hits hall, convert wall to a coord in map and resize map and cut map accordingly 
            {
                int[] wallcoord = ConvertWallToCoord(hit.collider.gameObject);

                if (wallcoord != currentpos)
                {
                    ResizeMapMemory(wallcoord);
                    CutMapMemory(currentpos, wallcoord);
                }

            }
        }

        //Shoot Ray Right
        Ray Rightray = new Ray(transform.position, transform.TransformDirection(Vector3.right));
        if (Physics.Raycast(Rightray, out hit))
        {
            if (hit.collider.tag == "win")//if it hits goal, save goal, let script know goal has been found
            {
                winobject = hit.collider.gameObject;
                WinningObjectFound = true;
            } //If hits goal return the goal position
            else if (hit.collider.tag == "wall")//if hits hall, convert wall to a coord in map and resize map and cut map accordingly 
            {
                int[] wallcoord = ConvertWallToCoord(hit.collider.gameObject);

                if (wallcoord != currentpos)
                {
                    ResizeMapMemory(wallcoord);
                    CutMapMemory(currentpos, wallcoord);
                }

            }
        }
        //Shoot Ray Left
        Ray Leftray = new Ray(transform.position, transform.TransformDirection(Vector3.left));
        if (Physics.Raycast(Leftray, out hit))
        {
            if (hit.collider.tag == "win")//if it hits goal, save goal, let script know goal has been found
            {
                winobject = hit.collider.gameObject;
                WinningObjectFound = true;
            } //If hits goal return the goal position
            else if (hit.collider.tag == "wall")//if hits hall, convert wall to a coord in map and resize map and cut map accordingly 
            {
                int[] wallcoord = ConvertWallToCoord(hit.collider.gameObject);

                if (wallcoord != currentpos)
                {
                    ResizeMapMemory(wallcoord);
                    CutMapMemory(currentpos, wallcoord);
                }

            }
        }
        PrintMap();//print balls to show user what the ai sees
        WINTEXT.SetActive(WinningObjectFound);//set winning text to true if ai finds goal
        return WinningObjectFound;//let code know if goal found
    }

    private List<int[]> ReturnValidLocations(int[] pastloc)//returns a list of valid locations for AI to move to
    {
        List<int[]> ReturnData = new List<int[]>() { };
        int[][] loc = new int[][] //makes list of all move locations
        {
            new int[2] { currentpos[0] + 1, currentpos[1] },
            new int[2] { currentpos[0] - 1, currentpos[1] },
            new int[2] { currentpos[0], currentpos[1] + 1 },
            new int[2] { currentpos[0], currentpos[1] - 1 },
        };
        bool[] validloc = new bool[4]//makes a list on whether locations can be moved to or if there is a wall blocking them
        {
            !MapMemory[currentpos[0]][currentpos[1]].X[1],
            !MapMemory[currentpos[0]][currentpos[1]].X[0],
            !MapMemory[currentpos[0]][currentpos[1]].Z[1],
            !MapMemory[currentpos[0]][currentpos[1]].Z[0],

        };
        for(int i = 0; i < 4; i++)//loop through all moves
        {
            try
            {
                if (!MapMemory[loc[i][0]][loc[i][1]].DeadEnd && !(loc[i][0] == pastloc[0] && loc[i][1] == pastloc[1]) && validloc[i])//if not a dead end and not the previous move and a valid move
                {
                    ReturnData.Add(loc[i]);//add to list of valid coords
                }
            }
            catch { }
        }
        return ReturnData;//return list of moves
    }

   

    private void PrintMap() //spawns coloured balls on walls to show user what the ai thinks the maze looks like
    {
        try
        {
            Destroy(allprefabs);//destroy all previous spawned balls
        }
        catch { }
        allprefabs = new GameObject();//create new list of balls
        for (int x = 0; x < MapMemory.Count; x++)//loop through all nodes 
        {
            for (int z = 0; z < MapMemory[x].Count; z++)
            {
                GameObject mazeprefrab = debugPrefab;//use red ball
                if (MapMemory[x][z].DeadEnd)//if dead end though use blue ball
                {
                    mazeprefrab = DeadEndPrefab;
                }
                if (!(MapMemory[x][z].X[0] && MapMemory[x][z].X[1] && MapMemory[x][z].Z[0] && MapMemory[x][z].Z[1]))//if the node is visited
                {
                    GameObject newnode;//create ball and spawn them on active walls
                    if (MapMemory[x][z].X[0])
                    {
                        newnode = Instantiate(mazeprefrab, new Vector3((x * 3) - 1, 3, (z * 3)), Quaternion.identity);
                        newnode.transform.parent = allprefabs.transform;

                        //corner
                        
                    }
                    if (MapMemory[x][z].X[1])
                    {
                        newnode = Instantiate(mazeprefrab, new Vector3((x * 3) + 1, 3, (z * 3)), Quaternion.identity);
                        newnode.transform.parent = allprefabs.transform;

                        //corner
                    }
                    if (MapMemory[x][z].Z[0])
                    {
                        newnode = Instantiate(mazeprefrab, new Vector3((x * 3), 3, (z * 3) - 1), Quaternion.identity);
                        newnode.transform.parent = allprefabs.transform;

                    }
                    if (MapMemory[x][z].Z[1])
                    {
                        newnode = Instantiate(mazeprefrab, new Vector3((x * 3), 3, (z * 3) + 1), Quaternion.identity);
                        newnode.transform.parent = allprefabs.transform;

                    };//spawn them on corners
                    newnode = Instantiate(mazeprefrab, new Vector3((x * 3) - 1, 3, (z * 3) + 1), Quaternion.identity);
                    newnode.transform.parent = allprefabs.transform;
                    newnode = Instantiate(mazeprefrab, new Vector3((x * 3) - 1, 3, (z * 3) - 1), Quaternion.identity);
                    newnode.transform.parent = allprefabs.transform;
                    newnode = Instantiate(mazeprefrab, new Vector3((x * 3) + 1, 3, (z * 3) + 1), Quaternion.identity);
                    newnode.transform.parent = allprefabs.transform;
                    newnode = Instantiate(mazeprefrab, new Vector3((x * 3) + 1, 3, (z * 3) - 1), Quaternion.identity);
                    newnode.transform.parent = allprefabs.transform;

                }

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        switch (State)
        {
            case 0: //wait for move to end
                if (!TheMoveAI.moving) { State = 1; }//if the ai stops moving go to state 1
                break;
            case 1://calc next move
                //SendValidLoc("Move History: ", MoveHistory);
                stuck = false;
                //Debug.Log("[" + currentpos[0] + " , " + currentpos[1] +"]");
                bool winobjectfound = DrawOutMaze();//refresh understanding of maze
                //PrintMap();
                if (!winobjectfound)//if goal not found
                {
                    List<int[]> PossibleMove;//stores list of possible moves
                    if (MoveHistory.Count < 2)//if little move history other than current location then dont give move history 
                    {
                        PossibleMove = ReturnValidLocations(new int[2] { -1, -1 });
                    }
                    else
                    {
                        PossibleMove = ReturnValidLocations(MoveHistory[MoveHistory.Count - 2]);//get list of valid moves
                    }
                    //SendValidLoc("Move To: ", PossibleMove);

                    if (PossibleMove.Count == 0)//if no possible moves
                    {
                        MapMemory[currentpos[0]][currentpos[1]].DeadEnd = true;//mark current node as a dead end
                        if (MoveHistory.Count >= 2)//if there is a move history then move to where the AI was last 
                        {
                            TheMoveAI.GoToCoord(MoveHistory[MoveHistory.Count - 2][0], MoveHistory[MoveHistory.Count - 2][1]);
                            currentpos = new int[2] { MoveHistory[MoveHistory.Count - 2][0], MoveHistory[MoveHistory.Count - 2][1] };
                            MoveHistory.RemoveAt(MoveHistory.Count - 1);//remove the deadend from move history
                        }
                        else
                        {
                            TheMoveAI.GoToCoord(currentpos[0], currentpos[1]);//the ai is stuck

                            stuck = true;
                        }
                    }
                    else// if there is a possible move 
                    {
                        int MoveNum = Random.Range(0, PossibleMove.Count);//pick random movie out of the list of possible moves
                        TheMoveAI.GoToCoord(PossibleMove[MoveNum][0], PossibleMove[MoveNum][1]);//go to coord
                        currentpos = new int[2] { PossibleMove[MoveNum][0], PossibleMove[MoveNum][1] };
                        MoveHistory.Add(PossibleMove[MoveNum]); //add to move history
                    }

                    State = StageInt;//make ai wait for ai to reach new position
                }
                else //if goal is found
                {
                    TheMoveAI.moveto = new float[2] { winobject.transform.position.x, winobject.transform.position.z }; //AI moves straight to goal
                    currentpos = new int[2] { (int)winobject.transform.position.x / 3, (int)winobject.transform.position.z / 3 };
                    State = 2;//stop ai
                }
                break;
        }
    }
}
