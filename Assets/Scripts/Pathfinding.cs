using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private PathRequestManager requestManager;
    
    private Grid grid;

    private void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
    }

    public void StartFindPath(Vector3 pathStart, Vector3 pathEnd)
    {
        StartCoroutine(FindPath(pathStart, pathEnd));
    }
    private IEnumerator FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;
        
        Node startNode = grid.GetNodeFromWorldPoint(startPosition);
        Node targetNode = grid.GetNodeFromWorldPoint(targetPosition);

        if (startNode.walkable && targetNode.walkable)
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
        
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
            
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
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
            
            yield return null;
            if(pathSuccess)
                waypoints = RetracePath(startNode, targetNode);
            
            requestManager.FinishedProcessingPath(waypoints, pathSuccess);
        }

        Vector3[] RetracePath(Node firstNode, Node endNode)
        {
            List<Node> path = new List<Node>();

            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            Vector3[] simplifiedPath = SimplifyPath(path);
            
            Array.Reverse(simplifiedPath);
            
            return simplifiedPath;
        }

        Vector3[] SimplifyPath(List<Node> path)
        {
            List<Vector3> points = new List<Vector3>();
            
            Vector2 directionOld = Vector2.zero;

            for (int i = 1; i < path.Count; i++)
            {
                Vector2 directionNew = new Vector2(
                    path[i - 1].gridX - path[i].gridX,
                    path[i - 1].gridY - path[i].gridY);

                if (directionNew != directionOld)
                {
                    points.Add(path[i].worldPosition);
                }

                directionOld = directionNew;
            }

            return points.ToArray();
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
