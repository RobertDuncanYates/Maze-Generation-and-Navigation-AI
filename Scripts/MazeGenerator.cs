using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    private GameStorage storage; //reference to data from Menu
    public int[] SizeOfMaze; //stores size of maze
    public GameObject NodePrefab; //Prefab of default Maze Node
    public List<List<Node>> Maze; //stores all maze elements
    public GameObject AI; //reference to Navigation AI
    public GameObject Winn;//reference to goal object
    public GameObject MazeObjects; //reference to object which all node become children of. (As to not clutter the Hierary in editor

    private int PlaceObjectInGameCorner( int IgnoreCorner, GameObject TheObject) //spawns object randomly in one of 4 sections of the maze. Ignore corner will make the code ignore a specific corner, this is used to reduce the chance of the Nav Ai and Goal from spawning next to each other
    {
        //id number to corner layout
        // 2 3
        // 0 1

        //Picks a Corner
        int GameCorner = 0;//game corner picked
        while (true)//keep randomly picking untill a valid corner is chosen
        {
            GameCorner = Random.Range(0, 3);
            if(GameCorner != IgnoreCorner) { break; }
        }
        int[] Coord = new int[2] { 0, 0 }; //Coord Corner is between
        int[] Coord2 = new int[2] { 0, 0 }; 
        switch (GameCorner)//sets coord of corner depending on corder id
        {
            case 0://bottem left corner
                Coord = new int[2] { 0, 0 };
                Coord2 = new int[2] { SizeOfMaze[0]/2, SizeOfMaze[1] / 2 };
                break;
            case 1://bottem right corner
                Coord = new int[2] { SizeOfMaze[0] / 2, 0 };
                Coord2 = new int[2] { SizeOfMaze[0] / 2, SizeOfMaze[1]};
                break;
            case 2://top left corner
                Coord = new int[2] { 0, SizeOfMaze[1] / 2 };
                Coord2 = new int[2] { SizeOfMaze[0], SizeOfMaze[1] / 2 };
                break;
            case 3://top right corner
                Coord = new int[2] { SizeOfMaze[0] / 2, SizeOfMaze[1] / 2 };
                Coord2 = new int[2] { SizeOfMaze[0], SizeOfMaze[1]};
                break;
        }
        int[] SpawnCoord = new int[2] { Random.Range(Coord[0], Coord2[0]), Random.Range(Coord[1], Coord2[1]) };//pick a rand coord between the two corner coords
        TheObject.transform.position = new Vector3(SpawnCoord[0] * 3,1, SpawnCoord[1] * 3);//set the object coord to the chosen coord. (times 3 to translate from array coord to in game space coord)
        return GameCorner;//return corner Id used
    }


    private List<List<Node>> SetUpBlankMaze(int x, int z)//creates a default list of untouched maze nodes for maze generator to work with
    {
        List<List<Node>> maze = new List<List<Node>>() { };//creates empty list
        for (int i = 0; i < x; i++)//loops for size of x in maze
        {
            List<Node> zlist = new List<Node>() { }; //creates an empty row of Nodes
            for (int j = 0; j < z; j++) //loops for size of Z/Y in maze
            {
                zlist.Add(new Node());//add new node to list
            }
            maze.Add(zlist);//add list of nodes to Maze list
        }
        return(maze);//return default list of maze nodes
    }

    private List<List<Node>> CreatePhysicalMaze(List<List<Node>> TheMaze, Vector3 StartingPoint) //will spawn in objects a draw the Maze in the game world
    {
        for (int x = 0; x < TheMaze.Count; x++)//loop through each node in maze
        {
            for (int z = 0; z < TheMaze[x].Count; z++)
            {
                GameObject newnode = Instantiate(NodePrefab, new Vector3(x * 3, 0, z * 3), Quaternion.identity);//create new physical 
                newnode.transform.parent = MazeObjects.transform; //set parent to main Maze Object
                TheMaze[x][z].PhysicalNode = newnode.GetComponent<PhysicalNode>(); //save physical node to Maze Node List
                //set physical walls to the same as the wall setting in the Node in the List
                TheMaze[x][z].PhysicalNode.X[0].SetActive(TheMaze[x][z].X[0]); 
                TheMaze[x][z].PhysicalNode.X[1].SetActive(TheMaze[x][z].X[1]);
                TheMaze[x][z].PhysicalNode.Z[0].SetActive(TheMaze[x][z].Z[0]);
                TheMaze[x][z].PhysicalNode.Z[1].SetActive(TheMaze[x][z].Z[1]);
            }
        }
        return (TheMaze);//return all new Node Maze
    }

    private List<int[]>[] FindNeighbouringNodes(List<List<Node>> TheMaze, int[] CurrentPos) //Locates and returns a list of coords to neighbouring Nodes. List is split between ones which have been found before and ones unfound
    {
        List<int[]>[] NeighbouringNodes = new List<int[]>[2] {new List<int[]>(), new List<int[]>() }; //found nodes , unfound nodes
        try { //X + 1 //Try and catch used for when program goes out of bounce of array.
            if(TheMaze[CurrentPos[0] + 1][CurrentPos[1]].Found) //the node X+1 next the current node is found
            {
                NeighbouringNodes[0].Add(new int[2] { CurrentPos[0] + 1, CurrentPos[1] });//add it to found list
            }
            else
            {
                NeighbouringNodes[1].Add(new int[2] { CurrentPos[0] + 1, CurrentPos[1] });//add it to unfound list
            }
        }
        catch { }
        try//X - 1
        {
            if (TheMaze[CurrentPos[0] - 1][CurrentPos[1]].Found)//the node X-1 next the current node is found
            {
                NeighbouringNodes[0].Add(new int[2] { CurrentPos[0] - 1, CurrentPos[1] });//add it to found list
            }
            else
            {
                NeighbouringNodes[1].Add(new int[2] { CurrentPos[0] - 1, CurrentPos[1] });//add it to unfound list
            }
        }
        catch { }
        try//Z + 1
        {
            if (TheMaze[CurrentPos[0]][CurrentPos[1] + 1].Found)//the node Z+1 next the current node is found
            {
                NeighbouringNodes[0].Add(new int[2] { CurrentPos[0], CurrentPos[1] + 1 });//add it to found list
            }
            else
            {
                NeighbouringNodes[1].Add(new int[2] { CurrentPos[0], CurrentPos[1] + 1 });//add it to unfound list
            }
        }
        catch { }
        try//Z - 1
        {
            if (TheMaze[CurrentPos[0]][CurrentPos[1] - 1].Found)//the node Z-1 next the current node is found
            {
                NeighbouringNodes[0].Add(new int[2] { CurrentPos[0] , CurrentPos[1] - 1 });//add it to found list
            }
            else
            {
                NeighbouringNodes[1].Add(new int[2] { CurrentPos[0] , CurrentPos[1] - 1 });//add it to unfound list
            }
        }
        catch { }
        return NeighbouringNodes; //return list of neighbour nodes

    }

    private Node CutNode(int[] CurrentPos, int[] MovePos, Node TheNode) //cut path between maze nodes
    {
        if (CurrentPos[0] > MovePos[0])//If Node is to the left x - 1
        {
            TheNode.X[0] = false; //cut left wall
        }
        else if (CurrentPos[0] < MovePos[0])//If Node is to the right x + 1
        {
            TheNode.X[1] = false;//cut right wall
        }
        else if (CurrentPos[1] > MovePos[1])//If Node is under the current node Z - 1
        {
            TheNode.Z[0] = false;//cut bottom wall
        }
        else if (CurrentPos[1] < MovePos[1])//If Node is top the current node Z + 1
        {
            TheNode.Z[1] = false;//cut top wall
        }
        return TheNode;//return new cut wall
    }


    private int TheDebug(string text,int debug)
    {
        Debug.Log(debug.ToString() + " " + text);
        debug++;
        return debug;
    }

    private List<List<Node>> DrawRandomMaze(List<List<Node>> TheMaze)//uses Hunt and Kill algorithm, to draw out a maze
    {

        int Mode = 1; //Current behaviour the Ai is doing Id: 0 = hunt 1 = kill
        bool AllFound = false; //store
        int[] CurrentPos = new int[2] { 0, 0 }; //X,Z stores current position of pointer, starting at 0,0

        while(!AllFound)//loops untill all Maze Nodes are found, (Found means algorithm has already opened a wall into the node )
        {
            switch (Mode)
            {
                case 0://hunt mode will find the next unfound Node
                    int[] StartPos = new int[2] { CurrentPos[0], CurrentPos[1] };//stores the coord the Hunt mode started at
                    bool FirstLoop = true; //stores whether it the while loop is on its first iteration
                    while (true)//loops untill next found node with unfound neighbours is found or all nodes found
                    {
                        List<int[]>[] NeighbouringNodes2 = FindNeighbouringNodes(TheMaze, CurrentPos);//get list of all neighbouring nodes
                        if (!TheMaze[CurrentPos[0]][CurrentPos[1]].Found && NeighbouringNodes2[0].Count > 0)//if current node is found and has unfound neighbours
                        {
                            Mode = 1;//change to Kill bahaviour 
                            break;
                        }
                        else if (!FirstLoop && StartPos[0] == CurrentPos[0] && StartPos[1] == CurrentPos[1])//if not the first iteration and it has returned to the coord it started on
                        {
                            AllFound=true;//all nodes must of been found and the maze is finished
                            break;
                        }
                        else//else move to next coord
                        {
                            CurrentPos[1]++; //moves onto the next coord from the current coord
                            if (CurrentPos[1] >= TheMaze[0].Count)
                            {
                                CurrentPos[1] = 0;
                                CurrentPos[0]++;
                                if (CurrentPos[0] >= TheMaze.Count) { CurrentPos[0] = 0; }
                            }

                        }
                        
                        FirstLoop = false;
                    }
                    break;
                case 1://kill mode will cut out a path through unfound nodes untill there is no more unfound nodes to cut through
                    bool FirstLoop2 = true;//stores whether the while loop is on its first iteration
                    while (true)//loop untill there is no more unfound nodes to cut through
                    {
                        List<int[]>[] NeighbouringNodes = FindNeighbouringNodes(TheMaze, CurrentPos);//start by finding all neighbouring nodes from current position
                        TheMaze[CurrentPos[0]][CurrentPos[1]].Found= true;//set current node to found
                        if (FirstLoop2 && NeighbouringNodes[0].Count > 0) //if there is a found neighbouring current node and kill behaviour is on first iteration
                        {
                            int index = Random.Range(0, NeighbouringNodes[0].Count); //pick a random found node
                            int[] MoveTo2 = NeighbouringNodes[0][index];//store node coords
                            //cut path between nodes. This makes sure all paths are joined
                            TheMaze[CurrentPos[0]][CurrentPos[1]] = CutNode(CurrentPos, MoveTo2, TheMaze[CurrentPos[0]][CurrentPos[1]]);
                            TheMaze[MoveTo2[0]][MoveTo2[1]] = CutNode(MoveTo2, CurrentPos, TheMaze[MoveTo2[0]][MoveTo2[1]]);
                        }
                        else if (FirstLoop2) { Debug.Log("No Neighbours :("); }//if this is triggered then the AI is stuck
                        FirstLoop2 = false; //no longer first iteration 
                        if (NeighbouringNodes[1].Count > 0)//if there is unfound nodes
                        {
                            int index2 = Random.Range(0, NeighbouringNodes[1].Count);//pick a random unfound node to move to
                            int[] MoveTo = NeighbouringNodes[1][index2]; //store coord of new node
                            //cut path between current and new nodes.
                            TheMaze[CurrentPos[0]][CurrentPos[1]] = CutNode(CurrentPos, MoveTo, TheMaze[CurrentPos[0]][CurrentPos[1]]);
                            TheMaze[MoveTo[0]][MoveTo[1]] = CutNode(MoveTo, CurrentPos, TheMaze[MoveTo[0]][MoveTo[1]]);
                            CurrentPos = NeighbouringNodes[1][index2]; //set position to new node
                            TheMaze[CurrentPos[0]][CurrentPos[1]].Found = true;//set new node to found
                        }
                        else//if all surround nodes have been found
                        {
                            Mode = 0;//go back to hunt mode
                            break;
                        }
                    }
                    break;
            }
        }
        return TheMaze;//return completed maze

    }

    // Start is called before the first frame update
    void Start()
    {
        storage = FindObjectOfType<GameStorage>();//grab reference to gamestorage
        SizeOfMaze = new int[2] { storage.x, storage.y };//load maze size from gamestorage
        int corner = PlaceObjectInGameCorner(-1, AI); //spawn AI in random corner
        PlaceObjectInGameCorner(corner, Winn); //spawn goal in different corner to AI
        Maze = SetUpBlankMaze(SizeOfMaze[0], SizeOfMaze[1]); //set up empty maze of unfound nodes
        Maze = DrawRandomMaze(Maze);//draw out paths between nodes
        Maze = CreatePhysicalMaze(Maze, new Vector3(0, 0, 0));//make drawn maze in physical game space
        Debug.Log("done maze gen");
        AI.SetActive(true);//activate Nav AI when maze is done
    }

}
