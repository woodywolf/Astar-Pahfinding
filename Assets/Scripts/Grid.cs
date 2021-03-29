using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool onlyDisplayPathGizmos;
    
    public Transform Player;
    public LayerMask UnwalkableMask;
    public Vector2 GridWorldSize;
    public float NodeRadius;
    public List<Node> path;
    
    private Node[,] grid;

    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    public int MaxSize => gridSizeX * gridSizeY;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(GridWorldSize.x, 1, GridWorldSize.y));

        if (onlyDisplayPathGizmos)
        {
            if (path != null)
            {
                foreach (Node node in path)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter-.1f));
                }
            }
        }
        else
        {
            if (grid != null)
            {
                Node playerNode = GetNodeFromWorldPoint(Player.position);
            
                foreach (var node in grid)
                {
                    Gizmos.color = (node.walkable) ? Color.white : Color.red;
                
                    if(playerNode == node)
                        Gizmos.color = Color.cyan;

                    if (path != null)
                    {
                        if (path.Contains(node))
                        {
                            Gizmos.color = Color.black;
                        }
                    }
                
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter-.1f));
                }
            }
        }
       
    }

    private void Start()
    {
        nodeDiameter = NodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(GridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(GridWorldSize.y / nodeDiameter);
        
        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];

        Vector3 worldBottomLeft = transform.position - Vector3.right * GridWorldSize.x / 
            2 - Vector3.forward * gridSizeY / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + NodeRadius)
                                                     + Vector3.forward * (y * nodeDiameter + NodeRadius);
                
                bool walkable = !(Physics.CheckSphere(worldPoint, NodeRadius, UnwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if(x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }
    

    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + GridWorldSize.x / 2) / GridWorldSize.x;
        float percentY = (worldPosition.z + GridWorldSize.y / 2) / GridWorldSize.y;
        
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        
        return grid[x,y];
    }
}
