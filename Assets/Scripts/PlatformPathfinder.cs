using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

public class PlatformPathfinder : MonoBehaviour
{
    [SerializeField] private GameObject solid;
    [SerializeField] private GameObject ladders;
    [SerializeField] private GameObject pointGraph;
    [SerializeField] private GameObject ladderGraph;
    [SerializeField] private GameObject nodeObject;
    
    private LayerMask groundMask;
    private Tilemap solidTilemap;
    private Tilemap laddersTilemap;
    private List<Vector2> solidTilePositions;
    private List<GraphNode> twoWayConnectionPoints;
    //private List<GraphNode> oneWayConnectionPoints;

    private readonly int jumpHeight = 1000;
    private readonly int jumpDistance = 2000;
    

    private void Awake()
    {
        solidTilePositions = new List<Vector2>();
        solidTilemap = solid.GetComponent<Tilemap>();
        laddersTilemap = ladders.GetComponent<Tilemap>();
        groundMask = LayerMask.GetMask("Ground");
        BoundsInt solidBounds = solidTilemap.cellBounds;
        TileBase[] solidTiles = solidTilemap.GetTilesBlock(solidBounds);
        BoundsInt laddersBounds = laddersTilemap.cellBounds;
        TileBase[] laddersTiles = laddersTilemap.GetTilesBlock(laddersBounds);
        for (int y = 0; y < solidBounds.yMax - solidBounds.y; y++)
        {
            for (int x = 0; x < solidBounds.xMax - solidBounds.x; x++)
            {
                TileBase tile = solidTiles[x + y * (solidBounds.xMax - solidBounds.x)];
                if (tile != null)
                    solidTilePositions.Add(new Vector2(x + solidBounds.x, y + solidBounds.y));
            }
        }
        
        for (int y = 0; y < laddersBounds.yMax - laddersBounds.y; y++)
        {
            for (int x = 0; x < laddersBounds.xMax - laddersBounds.x; x++)
            {
                TileBase tile = laddersTiles[x + y * (laddersBounds.xMax - laddersBounds.x)];
                if (tile != null)
                {
                    if (tile.name == "LadderClassic_0" || tile.name == "LadderClassic_4")
                    {
                        Vector3 tilePosition = new Vector3(x + laddersBounds.x, y + laddersBounds.y, 0);
                        CreatePoint(tilePosition, true, true);
                    }
                }
            }
        }
        
        foreach (var solidTilePosition in solidTilePositions)
        {
            Vector2 type = GetCellType(solidTilePosition);
            if (type != new Vector2(2, 2))
            {
                CreatePoint(solidTilePosition);
                if (type.x == -1)
                {
                    Vector2 position = new Vector2(solidTilePosition.x - 1, solidTilePosition.y);
                    Vector2 rayPosition = new Vector2(solidTilePosition.x - 0.5f, solidTilePosition.y + 0.5f);
                    Vector2 direction = Vector2.down;
                    RaycastHit2D result = Physics2D.Raycast(rayPosition, direction, 100, groundMask);
                    if (result)
                    {
                        CreatePoint(new Vector2(position.x, result.point.y), true);
                    }
                }
        
                if (type.y == -1)
                {
                    Vector2 position = new Vector2(solidTilePosition.x + 1, solidTilePosition.y);
                    Vector2 rayPosition = new Vector2(solidTilePosition.x + 1.5f, solidTilePosition.y + 0.5f);
                    Vector2 direction = Vector2.down;
                    RaycastHit2D result = Physics2D.Raycast(rayPosition, direction, 100, groundMask);
                    if (result)
                        CreatePoint(new Vector2(position.x, result.point.y), true);
                }
            }
        }
        AstarPath.active.Scan();
    }

    // Start is called before the first frame update
    private void Start()
    {
        for (int i = 0; i < pointGraph.transform.childCount; i++)
        {
            Transform closestRightNodeTransform = null;
            Transform closestLeftDropNodeTransform = null;
            Transform closestRightDropNodeTransform = null;
            Transform nodeTransform = pointGraph.transform.GetChild(i);

            //oneWayConnectionPoints = new List<GraphNode>();
            twoWayConnectionPoints = new List<GraphNode>();
            Vector2 nodeType = GetCellType(nodeTransform.position, true, true);

            for (int j = 0; j < pointGraph.transform.childCount; j++)
            {
                Transform nodeTransform2 = pointGraph.transform.GetChild(j);

                if (nodeType.y == 0 && nodeTransform2.position.y == nodeTransform.position.y &&
                    nodeTransform2.position.x > nodeTransform.position.x)
                    if (closestRightNodeTransform == null ||
                        nodeTransform2.position.x < closestRightNodeTransform.position.x)
                        closestRightNodeTransform = nodeTransform2;

                if (nodeType.x == -1)
                {
                    if (nodeTransform2.position.x == nodeTransform.position.x - 1 &&
                        nodeTransform2.position.y < nodeTransform.position.y)
                        if (closestLeftDropNodeTransform == null ||
                            nodeTransform2.position.y > closestLeftDropNodeTransform.position.y)
                            closestLeftDropNodeTransform = nodeTransform2;

                    if (nodeTransform2.position.y >= nodeTransform.position.y - jumpHeight / 1000 &&
                        nodeTransform2.position.y <= nodeTransform.position.y &&
                        nodeTransform2.position.x > nodeTransform.position.x - (jumpDistance / 1000 + 2) &&
                        nodeTransform2.position.x < nodeTransform.position.x &&
                        GetCellType(nodeTransform2.position, true, true).y == -1)
                    {
                        GraphNode node2 = AstarPath.active.GetNearest(nodeTransform2.position).node;
                        twoWayConnectionPoints.Add(node2);
                    }
                }

                if (nodeType.y == -1)
                    if (nodeTransform2.position.x == nodeTransform.position.x + 1 &&
                        nodeTransform2.position.y < nodeTransform.position.y)
                        if (closestRightDropNodeTransform == null ||
                            nodeTransform2.position.y > closestRightDropNodeTransform.position.y)
                            closestRightDropNodeTransform = nodeTransform2;
            }

            for (int j = 0; j < ladderGraph.transform.childCount; j++)
            {
                Transform nodeTransform2 = ladderGraph.transform.GetChild(j);
            
                if (nodeType.y == 0 && nodeTransform2.position.y == nodeTransform.position.y &&
                    nodeTransform2.position.x > nodeTransform.position.x)
                    if (closestRightNodeTransform == null ||
                        nodeTransform2.position.x < closestRightNodeTransform.position.x)
                        closestRightNodeTransform = nodeTransform2;
            }

            if (closestRightNodeTransform != null)
            {
                GraphNode node2 = AstarPath.active.GetNearest(closestRightNodeTransform.position).node;
                twoWayConnectionPoints.Add(node2);
            }

            GraphNode node = AstarPath.active.GetNearest(nodeTransform.position).node;

            if (closestLeftDropNodeTransform != null)
            {
                GraphNode node2 = AstarPath.active.GetNearest(closestLeftDropNodeTransform.position).node;
                if (node.position.y <= node2.position.y + jumpHeight)
                    twoWayConnectionPoints.Add(node2);
                //else
                //    oneWayConnectionPoints.Add(node2);
            }

            if (closestRightDropNodeTransform != null)
            {
                GraphNode node2 = AstarPath.active.GetNearest(closestRightDropNodeTransform.position).node;
                if (node.position.y <= node2.position.y + jumpHeight)
                    twoWayConnectionPoints.Add(node2);
                //else
                //    oneWayConnectionPoints.Add(node2);
            }

            foreach (GraphNode point in twoWayConnectionPoints)
            {
                AstarPath.active.AddWorkItem(new AstarWorkItem(ctx =>
                {
                    uint cost = (uint)(point.position - node.position).costMagnitude;
                    node.AddConnection(point, cost);
                    point.AddConnection(node, cost);
                }));
            }

            //foreach (GraphNode point in oneWayConnectionPoints)
            //    AstarPath.active.AddWorkItem(new AstarWorkItem(ctx => { node.AddConnection(point, 1); }));
        }

        for (int i = 0; i < ladderGraph.transform.childCount; i++)
        {
            Transform closestRightNodeTransform = null;
            Transform nodeTransform = ladderGraph.transform.GetChild(i);
            GraphNode node = AstarPath.active.GetNearest(nodeTransform.position).node;
            Transform topNodeTransform = null;
            
            twoWayConnectionPoints = new List<GraphNode>();
            RaycastHit2D hit = Physics2D.Raycast(nodeTransform.position, Vector2.right, 500f, groundMask);
        
            for (int j = 0; j < pointGraph.transform.childCount; j++)
            {
                Transform nodeTransform2 = pointGraph.transform.GetChild(j);
        
                if (nodeTransform2.position.y == nodeTransform.position.y &&
                    nodeTransform2.position.x > nodeTransform.position.x &&
                    hit.distance > nodeTransform2.position.x - nodeTransform.position.x)
                    if (closestRightNodeTransform == null ||
                        nodeTransform2.position.x < closestRightNodeTransform.position.x)
                        closestRightNodeTransform = nodeTransform2;
            }
        
            if (closestRightNodeTransform != null)
            {
                GraphNode node2 = AstarPath.active.GetNearest(closestRightNodeTransform.position).node;
                twoWayConnectionPoints.Add(node2);
                RaycastHit2D hit2 = Physics2D.Raycast(nodeTransform.position, Vector2.up, 500f, groundMask);

                for (int j = 0; j < ladderGraph.transform.childCount; j++)
                {
                    Transform nodeTransform2 = ladderGraph.transform.GetChild(j);

                    if (hit2)
                    {
                        if (nodeTransform2.position.x != nodeTransform.position.x ||
                            !(nodeTransform2.position.y > nodeTransform.position.y) ||
                            !(nodeTransform2.position.y - nodeTransform.position.y < hit2.distance)) continue;
                        topNodeTransform = nodeTransform2;
                        break;
                    }
                    else
                    {
                        if (nodeTransform2.position.x != nodeTransform.position.x ||
                            !(nodeTransform2.position.y > nodeTransform.position.y)) continue;
                        topNodeTransform = nodeTransform2;
                        break;
                    }
                }
                GraphNode node3 = AstarPath.active.GetNearest(topNodeTransform.position).node;
                twoWayConnectionPoints.Add(node3);

                for (int j = 0; j < pointGraph.transform.childCount; j++)
                {
                    Transform nodeTransform2 = pointGraph.transform.GetChild(j);
                    if (nodeTransform2.position == new Vector3(node3.position.x / 1000f - 1.5f, node3.position.y / 1000f + 1) || nodeTransform2.position == new Vector3(node3.position.x / 1000f + 1.5f, node3.position.y / 1000f + 1))
                    {
                        GraphNode node4 = AstarPath.active.GetNearest(nodeTransform2.position).node;
                        AstarPath.active.AddWorkItem(new AstarWorkItem(ctx =>
                        {
                            uint cost = (uint)(node4.position - node3.position).costMagnitude;
                            node3.AddConnection(node4, cost);
                            node4.AddConnection(node3, cost);
                        }));
                    }
                }
            }

            foreach (GraphNode point in twoWayConnectionPoints)
            {
                AstarPath.active.AddWorkItem(new AstarWorkItem(ctx =>
                {
                    uint cost = (uint) (point.position - node.position).costMagnitude;
                    node.AddConnection(point, cost);
                    point.AddConnection(node, cost);
                }));
            }
        }
    }

    private Vector2 GetCellType(Vector2 position, bool global = false, bool isAbove = false)
    {
        if (global)
            position = new Vector2(position.x - 0.5f, position.y - 0.5f);
        if (isAbove)
            position = new Vector2(position.x, position.y - 1);

        Vector2 result = new Vector2(0, 0);
        if (solidTilePositions.Contains(new Vector2(position.x, position.y + 1))) 
            return new Vector2(2, 2);

        if (solidTilePositions.Contains(new Vector2(position.x - 1, position.y + 1)))
            result += new Vector2(1, 0);
        else if (!solidTilePositions.Contains(new Vector2(position.x - 1, position.y)))
            result += new Vector2(-1, 0);

        if (solidTilePositions.Contains(new Vector2(position.x + 1, position.y + 1)))
            result += new Vector2(0, 1);
        else if (!solidTilePositions.Contains(new Vector2(position.x + 1, position.y)))
            result += new Vector2(0, -1);
        
        return result;
    }

    private void CreatePoint(Vector2 tilePosition, bool isAbove = false, bool isLadder = false)
    {
        if (isAbove)
            tilePosition.y -= 1;

        tilePosition.y = Mathf.RoundToInt(tilePosition.y);
        Vector3 above = new Vector3(tilePosition.x + 0.5f, tilePosition.y + 1.5f, 0);
        if (isLadder) above.x += 0.5f;

        for (int i = 0; i < pointGraph.transform.childCount; i++)
            if (pointGraph.transform.GetChild(i).position == above)
                return;

        Instantiate(nodeObject, above, Quaternion.identity, !isLadder ? pointGraph.transform : ladderGraph.transform);
    }
}