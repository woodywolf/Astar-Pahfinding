using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public Transform seeker;
    public Transform target;
    
    private Grid grid;

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
            FindPath(seeker.position, target.position);
    }

    private void FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        
        Node startNode = grid.GetNodeFromWorldPoint(startPosition);
        Node targetNode = grid.GetNodeFromWorldPoint(targetPosition);

        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                sw.Stop();
                print("Path found " + sw.ElapsedMilliseconds + " ms");
                
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        void RetracePath(Node firstNode, Node endNode)
        {
            List<Node> path = new List<Node>();

            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            
            path.Reverse();

            grid.path = path;
        }

        int GetDistance(Node nodeA, Node nodeB)
        {
            int distanceX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int distanceY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

            if (distanceX > distanceY)
                return 14 * distanceY + 10 * (distanceX - distanceY);
            return 14 * distanceX + 10 * (distanceY - distanceX);
        }
    }
}
