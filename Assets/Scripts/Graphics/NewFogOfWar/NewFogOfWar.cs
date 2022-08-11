using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NewFogOfWar : MonoBehaviour
{
    [SerializeField] private int visionRange;
    [SerializeField] private VisionGrid visionGrid;
    [SerializeField] private Tilemap solidTilemap;
    List<Vector3> visibleTiles = new List<Vector3>();
    private LayerMask groundMask;

    private int xc;
    private int yc;

    private int tempX = 0;

    private void Awake()
    {
        groundMask = LayerMask.GetMask("Ground");
    }

    private void Start()
    {
        CalculateVision();
    }

    private void Update()
    {
        CalculateVision();
    }

    private List<Vector3> CalculateVisionTiles(int x, int y)
    {
        List<Vector3> listOfTiles = new List<Vector3>();
        listOfTiles.AddRange(AddLineToVision(new Vector3(xc - x, yc + y), new Vector3(xc + x, yc + y)));
        if (y != 0)
        {
            listOfTiles.AddRange(AddLineToVision(new Vector3(xc - x, yc - y), new Vector3(xc + x, yc - y)));
        }
        if (tempX != x)
        {
            listOfTiles.AddRange(AddLineToVision(new Vector3(xc - (y - 1), yc + tempX), new Vector3(xc + (y - 1), 
                yc + tempX)));
            listOfTiles.AddRange(AddLineToVision(new Vector3(xc - (y - 1), yc - tempX),new Vector3(xc + (y - 1),
                yc - tempX)));
        }
        tempX = x;
        return listOfTiles;
    }

    private List<Vector3> AddLineToVision(Vector3 start, Vector3 end)
    {
        List<Vector3> visionLine = new List<Vector3>();
        Vector3 tilePosition;
        for (int i = (int)start.x; i <= end.x; i++)
        {
            tilePosition = new Vector3(i + 0.5f, start.y + 0.5f);
            Vector3 heading = transform.position - tilePosition;
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            Vector3 rayStart1 = new Vector3(tilePosition.x, tilePosition.y);
            Vector3 rayStart2 = new Vector3(tilePosition.x, tilePosition.y);
            if (direction.x > 0)
                rayStart1 += new Vector3(-0.49f, 0);
            if (direction.x < 0)
                rayStart1 += new Vector3(0.49f, 0);
            if (direction.y < 0)
                rayStart1 += new Vector3(0, -0.49f);
            if (direction.y > 0)
                rayStart1 += new Vector3(0, 0.49f);

            if (direction.x > 0)
                rayStart2 += new Vector3(0.49f, 0);
            if (direction.x < 0)
                rayStart2 += new Vector3(-0.49f, 0);
            if (direction.y < 0)
                rayStart2 += new Vector3(0, 0.49f);
            if (direction.y > 0)
                rayStart2 += new Vector3(0, -0.49f);
            


            RaycastHit2D hit1 = Physics2D.Raycast(rayStart1, direction, distance, groundMask);
            RaycastHit2D hit2 = Physics2D.Raycast(rayStart2, direction, distance, groundMask);
            if (hit1 && hit2)continue;

            tilePosition = new Vector3(tilePosition.x - 0.5f, tilePosition.y - 0.5f);
            visionLine.Add(tilePosition);
        }
        return visionLine;
    }

    private void CalculateVision()
    {
        visibleTiles.Clear();
        xc = (int)Mathf.Floor(transform.position.x);
        yc = (int)Mathf.Floor(transform.position.y + 1);
        int x = visionRange, y = 0;
        int d = 3 - 2 * visionRange;
        visibleTiles.AddRange(CalculateVisionTiles(x, y));
        while (x > y)
        {
            y++;

            if (d > 0)
            {
                x--;
                d = d + 4 * (y - x) + 10;
            }
            else
                d = d + 4 * y + 6;

            visibleTiles.AddRange(CalculateVisionTiles(x, y));
        }
        visionGrid.UpdateGrid(visibleTiles);
    }
}
