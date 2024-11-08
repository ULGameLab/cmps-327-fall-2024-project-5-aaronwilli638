using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MapGen;

public class Node
{
    public Node cameFrom = null; //parent node
    public double priority = 0; // F value
    public double costSoFar = 0; // G Value
    public Tile tile;

    public Node(Tile _tile, double _priority, Node _cameFrom, double _costSoFar)
    {
        cameFrom = _cameFrom;
        priority = _priority; 
        costSoFar = _costSoFar;
        tile = _tile;
    }
}

public class PathFinder
{
    List<Node> TODOList = new List<Node>();
    List<Node> DoneList = new List<Node>();
    Tile goalTile; 


    // This is the constructor
    public PathFinder()
    {
        goalTile = null;
    }

    // TODO: Find the path based on A-Star Algorithm
    public Queue<Tile> FindPathAStar(Tile start, Tile goal)
    {
        TODOList = new List<Node>();
        DoneList = new List<Node>();

        TODOList.Add(new Node(start, 0, null, 0));
        goalTile = goal;

        while (TODOList.Count > 0)
        {
            TODOList.Sort((x, y) => (x.priority.CompareTo(y.priority))); // This will keep the TODO List sorted based on the F cost
            Node current = TODOList[0];
            DoneList.Add(current);
            TODOList.RemoveAt(0);

            if (current.tile == goal)
            {
                return RetracePath(current);  // Returns the Path if goal is reached
            }

            // for each neighboring tile calculate the costs
            foreach (Tile nextTile in current.tile.Adjacents)
            {
                // If the neighboring tile is already processed, continue
                if (DoneList.Exists(node => node.tile == nextTile))
                {
                    continue;
                }

                double newCost = current.costSoFar + GetDistance(current.tile, nextTile);
                Node nextNode = TODOList.Find(node => node.tile == nextTile);

                if (nextNode == null)
                {
                    // If the neighbor is not in the TODO list, add it with the calculated cost
                    double priority = newCost + GetDistance(nextTile, goal);
                    TODOList.Add(new Node(nextTile, priority, current, newCost));
                }
                else if (newCost < nextNode.costSoFar)
                {
                    // If the new path to neighbor is shorter, update the costs and path
                    nextNode.cameFrom = current;
                    nextNode.costSoFar = newCost;
                    nextNode.priority = newCost + GetDistance(nextTile, goal);
                }
            }
        }
        return new Queue<Tile>(); // Returns an empty Path if no path is found
    }

    // Helper method to calculate the distance between two tiles (using Manhattan Distance or Euclidean Distance)
    private double GetDistance(Tile a, Tile b)
    {
        // Assuming there is a way to get the position from the Tile, like a Vector3 or similar
        Vector3 posA = a.transform.position; // Update this line if Tile has a different property to access its position
        Vector3 posB = b.transform.position; // Update accordingly

        // Calculate the Manhattan distance or Euclidean distance
        return Math.Abs(posA.x - posB.x) + Math.Abs(posA.z - posB.z); // Using x and z for 2D distance
    }




    // Helper method to retrace the path from goal to start
    private Queue<Tile> RetracePath(Node node)
    {
        List<Tile> tileList = new List<Tile>();
        Node nodeIterator = node;
        while (nodeIterator.cameFrom != null)
        {
            tileList.Insert(0, nodeIterator.tile);
            nodeIterator = nodeIterator.cameFrom;
        }
        return new Queue<Tile>(tileList);
    }



    // TODO: Find the path based on A-Star Algorithm
    // In this case avoid a path passing near an enemy tile
    // BONUS TASK (Required the for Honors Contract Students)
    public Queue<Tile> FindPathAStarEvadeEnemy(Tile start, Tile goal)
    {
        TODOList = new List<Node>();
        DoneList = new List<Node>();

        TODOList.Add(new Node(start, 0, null, 0));
        goalTile = goal;

        while (TODOList.Count > 0)
        {
            TODOList.Sort((x, y) => (x.priority.CompareTo(y.priority))); // This will keep the TODO List sorted
            Node current = TODOList[0];
            DoneList.Add(current);
            TODOList.RemoveAt(0);

            if (current.tile == goal)
            {
                return RetracePath(current);  // Returns the Path if goal is reached
            }

            foreach (Tile nextTile in current.tile.Adjacents)
            {
                // If the neighboring tile is already processed, continue
                if (DoneList.Exists(node => node.tile == nextTile))
                {
                    continue;
                }

                double newCost = current.costSoFar + GetDistance(current.tile, nextTile);
                Node nextNode = TODOList.Find(node => node.tile == nextTile);

                if (nextNode == null)
                {
                    // If the neighbor is not in the TODO list, add it with the calculated cost
                    double priority = newCost + GetDistance(nextTile, goal);
                    TODOList.Add(new Node(nextTile, priority, current, newCost));
                }
                else if (newCost < nextNode.costSoFar)
                {
                    // If the new path to neighbor is shorter, update the costs and path
                    nextNode.cameFrom = current;
                    nextNode.costSoFar = newCost;
                    nextNode.priority = newCost + GetDistance(nextTile, goal);
                }
            }
        }
        return new Queue<Tile>(); // Returns an empty Path
    }

    // Manhattan Distance with horizontal/vertical cost of 10
    double HeuristicsDistance(Tile currentTile, Tile goalTile)
    {
        int xdist = Math.Abs(goalTile.indexX - currentTile.indexX);
        int ydist = Math.Abs(goalTile.indexY - currentTile.indexY);
        // Assuming cost to move horizontally and vertically is 10
        //return manhattan distance
        return (xdist * 10 + ydist * 10);
    }

    // Retrace path from a given Node back to the start Node
   


    // Generate a Random Path. Used for enemies
    public Queue<Tile> RandomPath(Tile start, int stepNumber)
    {
        List<Tile> tileList = new List<Tile>();
        Tile currentTile = start;
        for (int i = 0; i < stepNumber; i++)
        {
            Tile nextTile;
            //find random adjacent tile different from last one if there's more than one choice
            if (currentTile.Adjacents.Count < 0)
            {
                break;
            }
            else if (currentTile.Adjacents.Count == 1)
            {
                nextTile = currentTile.Adjacents[0];
            }
            else
            {
                nextTile = null;
                List<Tile> adjacentList = new List<Tile>(currentTile.Adjacents);
                ShuffleTiles<Tile>(adjacentList);
                if (tileList.Count <= 0) nextTile = adjacentList[0];
                else
                {
                    foreach (Tile tile in adjacentList)
                    {
                        if (tile != tileList[tileList.Count - 1])
                        {
                            nextTile = tile;
                            break;
                        }
                    }
                }
            }
            tileList.Add(currentTile);
            currentTile = nextTile;
        }
        return new Queue<Tile>(tileList);
    }

    private void ShuffleTiles<T>(List<T> list)
    {
        // Knuth shuffle algorithm :: 
        // courtesy of Wikipedia :) -> https://forum.unity.com/threads/randomize-array-in-c.86871/
        for (int t = 0; t < list.Count; t++)
        {
            T tmp = list[t];
            int r = UnityEngine.Random.Range(t, list.Count);
            list[t] = list[r];
            list[r] = tmp;
        }
    }
}
