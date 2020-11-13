using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Edited by Tymon Versmoren
public class Astar
{
    /// <summary>
    /// TODO: Implement this function so that it returns a list of Vector2Int positions which describes a path
    /// Note that you will probably need to add some helper functions
    /// from the startPos to the endPos
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="grid">Grid starts bottom left</param>
    /// <returns></returns>
    public List<Vector2Int> FindPathToTarget(Vector2Int startPos, Vector2Int endPos, Cell[,] grid)
    {
        // Set start and end nodes
        Node startNode = new Node(startPos, null, (int)Vector2.Distance(startPos, startPos), (int)Vector2.Distance(startPos, endPos));        
        Node targetNode = new Node(endPos, null, (int)Vector2.Distance(startPos, endPos), (int)Vector2.Distance(endPos, endPos));        

        // Lists open and close
        List<Node> openNodes = new List<Node>();
        List<Node> closedNodes = new List<Node>(); // Hashet since it is faster than list and used to prevent duplicate elements

        // Add the start node to the openNodes
        openNodes.Add(startNode);
        Node currentNode = null;

        // Loop trough all openNodes
        int antiInfinitLoopCounter = 0;
        while(openNodes.Count > 0)
        {
            currentNode = openNodes[0];

            // Get the lowest f cost
            for(int i = 1; i < openNodes.Count; i++)
            {
                // Compare f cost, if equal see which one is closest to the endPos by comparing the h cost
                if(openNodes[i].FScore < currentNode.FScore || openNodes[i].FScore == currentNode.FScore)
                {
                    if(openNodes[i].HScore < currentNode.HScore)
                        currentNode = openNodes[i];
                }
            }

            // Remove current node from open and add to closed
            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            // Check if position is equal to target node
            if(currentNode.position == targetNode.position)
            {
                // Found endPos
                break;
            }

            // Loop trough neighbour nodes and compare
            foreach(Node neighbour in GetNeighbourNodes(currentNode, grid))
            {
                if(ListContainsNode(neighbour, closedNodes))
                {
                    continue;
                }

                // Calculate the new cost to move to the neighbour
                int newMovementCostToNeighbour = (int)currentNode.GScore + GetDistanceBetweenNodes(currentNode, neighbour);
                if(newMovementCostToNeighbour < neighbour.GScore || !ListContainsNode(neighbour, openNodes))
                {
                    neighbour.GScore = newMovementCostToNeighbour;
                    neighbour.HScore = GetDistanceBetweenNodes(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    // Check if it is in the openNodes
                    if(!ListContainsNode(neighbour, openNodes))
                    {
                        Debug.Log("yo");
                        openNodes.Add(neighbour);
                    }
                }
            }

            antiInfinitLoopCounter++;
            if(antiInfinitLoopCounter > 6000) // If i mess up the code i dont want to restart unity
            {
                Debug.LogWarning("Path takes too long ");
                break;
            }
        }

        // Return the path
        List<Vector2Int> path = new List<Vector2Int>();

        while(currentNode.position != startNode.position)
        {
            path.Add(currentNode.position);
            Debug.Log("Path: " + currentNode.position);
            currentNode = currentNode.parent;
            if(currentNode == null) break;
        }
        // Set path right way
        path.Reverse();
        return path;
    }

    /// <summary>
    /// Get surrounding neighbours from a node
    /// </summary>
    /// <param name="n"></param>
    /// <param name="grid"></param>
    /// <returns>List of neighbour nodes</returns>
    private List<Node> GetNeighbourNodes(Node n, Cell[,] grid)
    {
        Debug.Log("Check on position: " + n.position.x + " " + n.position.y);
        List<Node> neighbours = new List<Node>();

        // Search in 3x3 block
        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                // Skip orgin node && diagonal nodes since this is a maze with squares
                if(x == 0 && y == 0 || x == -1 && y == 1 || x == 1 && y == 1 || x == -1 && y == -1 || x == 1 && y == -1)
                {
                    Debug.Log("c");
                    continue;
                }

                int checkX = n.position.x + x;
                int checkY = n.position.y + y;

                // Check if position is inside the grid
                if(checkX >= 0 && checkX < grid.GetLength(0) && checkY >= 0 && checkY < grid.GetLength(1))
                {
                    // Node is in grid
                    // Now check if there isnt a wall blocking it
                    bool blocked = false;
                    //Debug.Log(checkX + " " + checkY); // grid size is wrong
                    if(x == 0)
                    {
                        if(y == 1)
                        {
                            if((grid[checkX, checkY].walls & Wall.DOWN) != 0) blocked = true;
                        }
                        else if(y == -1)
                        {
                            if((grid[checkX, checkY].walls & Wall.UP) != 0) blocked = true;
                        }
                    }
                    else if(y == 0)
                    {
                        if(x == -1)
                        {
                            if((grid[checkX, checkY].walls & Wall.RIGHT) != 0) blocked = true;
                        }
                        else if(x == 1)
                        {
                            if((grid[checkX, checkY].walls & Wall.LEFT) != 0) blocked = true;
                        }
                    }

                    if(!blocked)
                    {
                        // Add it
                        neighbours.Add(new Node(new Vector2Int(checkX, checkY), n, 0, 0));
                    }                    
                }
            }
        }
        return neighbours;
    }

    /// <summary>
    /// Get the distance (yea you can do Vector2.Distance but i like this one better)
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int GetDistanceBetweenNodes(Node a, Node b)
    {
        int dX = Mathf.Abs(a.position.x - b.position.x);
        int dY = Mathf.Abs(a.position.y - b.position.y);

        // Return cost
        if(dX > dY)
        {
            return 14 * dY + 10 * (dX - dY); // first int is moveCost
        }
        return 14 * dX + 10 * (dY - dX);
    }

    /// <summary>
    /// Apperantly cannot use Contains on List<> ????, not working for me
    /// </summary>
    /// <param name="n"></param>
    /// <param name="l"></param>
    /// <returns></returns>
    private bool ListContainsNode(Node n, List<Node> l)
    {
        for(int i = 0; i < l.Count; i++)
        {
            if(n.position == l[i].position) return true;
        }

        return false;
    }

    /// <summary>
    /// This is the Node class you can use this class to store calculated FScores for the cells of the grid, you can leave this as it is
    /// </summary>
    public class Node
    {
        public Vector2Int position; //Position on the grid
        public Node parent; //Parent Node of this node

        public float FScore { //GScore + HScore
            get { return GScore + HScore; }
        }
        public float GScore; //Current Travelled Distance, or distance from starting node
        public float HScore; //Distance estimated based on Heuristic, or opposite of g cost, meaning how far away from endPos

        public Node() { }
        public Node(Vector2Int position, Node parent, int GScore, int HScore)
        {
            this.position = position;
            this.parent = parent;
            this.GScore = GScore;
            this.HScore = HScore;
        }
        // Added myself
        public Node(Node n)
        {
            this.position = n.position;
            this.parent = n.parent;
            this.GScore = n.GScore;
            this.HScore = n.HScore;
        }
    }
}
