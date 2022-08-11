using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;


public class VisionGrid : MonoBehaviour
{
    [SerializeField] private Tilemap solidTilemap;
    [SerializeField] private Texture2D fogTexture;

    private int width, height;
    private Texture2D fogOfWarTexture;
    private List<TileType> visionMap;
    private Color[] fogTexturePixels;
    private Color[] seenColor = new Color[256];
    private Color[] visibleColor = new Color[256];

    private void Awake()
    {
        solidTilemap.CompressBounds();
        width = solidTilemap.size.x;
        height = solidTilemap.size.y;
        fogOfWarTexture = new Texture2D(width * 16, height * 16, TextureFormat.RGBA32, false);
        fogOfWarTexture.filterMode = FilterMode.Point;
        GetComponent<RawImage>().texture = fogOfWarTexture;
        GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        GetComponent<RectTransform>().position = new Vector2(solidTilemap.cellBounds.x, solidTilemap.cellBounds.y);
        fogTexturePixels = fogTexture.GetPixels(0);
        seenColor = fogTexture.GetPixels(0);

        visionMap = new List<TileType>();
        for (int i = 0; i < 256; i++)
        {
            seenColor[i].a = 0.7f;
            visibleColor[i] = new Color(0, 0, 0, 0f);
        }

        for (int i = 0; i < width * height; i++)
        {
            visionMap.Add(TileType.UnSeen);
            Debug.Log(fogTexturePixels.Length);
            fogOfWarTexture.SetPixels(i % width * 16, i / width * 16, 16, 16, fogTexturePixels, 0);
        }

        fogOfWarTexture.Apply();
    }

    public void UpdateGrid(List<Vector3> visibleTiles)
    {
        for (int i = 0; i < visionMap.Count; i++)
            if (visionMap[i] == TileType.Visible)
            {
                visionMap[i] = TileType.Seen;
             
                fogOfWarTexture.SetPixels(i % width * 16, i / width * 16, 16, 16, seenColor, 0);
                //fogOfWarTexture.SetPixel(i % width, i / width, new Color(0, 0, 0, 0.7f));
            }
        for (int i = 0; i < visibleTiles.Count; i++)
        {
            int tileIndex = (int) (visibleTiles[i].x - solidTilemap.cellBounds.x + (visibleTiles[i].y - solidTilemap.cellBounds.y) * width);
            if (tileIndex < visionMap.Count)
            {
                visionMap[tileIndex] = TileType.Visible;
                fogOfWarTexture.SetPixels(((int)visibleTiles[i].x - solidTilemap.cellBounds.x) * 16, ((int)visibleTiles[i].y - solidTilemap.cellBounds.y) * 16, 16, 16, visibleColor, 0);
                //fogOfWarTexture.SetPixel((int)visibleTiles[i].x - solidTilemap.cellBounds.x, (int)visibleTiles[i].y - solidTilemap.cellBounds.y, new Color(0, 0, 0, 0f));
            }
        }

        fogOfWarTexture.Apply();
    }
}
