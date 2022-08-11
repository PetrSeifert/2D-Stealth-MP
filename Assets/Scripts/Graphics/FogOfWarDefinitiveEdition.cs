using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class FogOfWarDefinitiveEdition : MonoBehaviour
{

    [SerializeField] private Tilemap solidTilemap;
    [SerializeField] private Texture2D fogTexture;
    [SerializeField] private GridLayout gridLayout;

    private int width, height;
    private int camWidth = Mathf.CeilToInt(480f / 16f + 2);
    private int camHeight = Mathf.CeilToInt(270f / 16f + 2);
    private LayerMask groundMask;
    private bool[,] tileSeen;
    [SerializeField] Transform camTransform;
    [SerializeField] Transform playerTransform;
    [SerializeField] Shader fogShader;
    Material fogMaterial;
    RenderTexture renderTexture;
    Texture2D mapTexture;
    Vector2 mapTexturePosition;
    RawImage fogOfWar;
    Vector2[] directions = new Vector2[] { new Vector2(0.24f, 0.24f), new Vector2(0.24f, -0.24f), new Vector2(-0.24f, 0.24f), new Vector2(-0.24f, -0.24f) };
    List<Vector2> limitsOfExcludedRaycasts1;
    List<Vector2> limitsOfExcludedRaycasts2;
    List<float> distanceOfExcludedRaycasts;

    private bool shouldRaycast = true;

    private void Awake()
    {
        fogOfWar = GetComponent<RawImage>();
        groundMask = LayerMask.GetMask("Ground");
        solidTilemap.CompressBounds();
        width = solidTilemap.size.x;
        height = solidTilemap.size.y;

        tileSeen = new bool[width * 2, height * 2];

        fogMaterial = new Material(fogShader);
    }

    void Update()
    {
        if (mapTexture != null)
        {
            /* Zavolat shader */
            Destroy(renderTexture);
            renderTexture = new RenderTexture(480, 270, 24);
            renderTexture.filterMode = FilterMode.Point;

            BetterGraphics.ShaderData shaderData = new BetterGraphics.ShaderData();
            shaderData.Add("_MainTex", fogTexture);
            shaderData.Add("_DataTex", mapTexture);
            shaderData.Add("_Deviation", (Vector2)camTransform.position - new Vector2(Mathf.Floor(mapTexturePosition.x), Mathf.Floor(mapTexturePosition.y)));
            //shaderData.Add("_DataUV", new Vector2(1, 1));
            shaderData.Add("_CamPosition", camTransform.position);
            //shaderData.Add("_CamPosition", camTransform.position);
            //shaderData.Add("_Quality", (int)edgeFiltering);
            BetterGraphics.Blit(renderTexture, fogMaterial, shaderData, new Vector2(480, 270));
            fogOfWar.texture = renderTexture;
            shouldRaycast = true;
        }
    }

    void FixedUpdate()
    {
        if (!shouldRaycast) return;
        Texture2D texture = new Texture2D(camWidth * 2 + 1, camHeight * 2 + 3);
        texture.filterMode = FilterMode.Point;

        for (int i = ((-solidTilemap.cellBounds.xMin + Mathf.FloorToInt(camTransform.position.x) - camWidth / 2) * 2); i <= ((-solidTilemap.cellBounds.xMin + Mathf.FloorToInt(camTransform.position.x) + camWidth / 2) * 2 + 1); i++)
        {
            for (int j = ((-solidTilemap.cellBounds.yMin + Mathf.FloorToInt(camTransform.position.y) - camHeight / 2) * 2); j <= ((-solidTilemap.cellBounds.yMin + Mathf.FloorToInt(camTransform.position.y) + camHeight / 2) * 2 + 3); j++)
            {
                Vector2 tile = new Vector2(i / 2f + solidTilemap.cellBounds.xMin, j / 2f + solidTilemap.cellBounds.yMin) - new Vector2(0.25f, 0.25f);

                Vector2 start = (Vector2)playerTransform.position + new Vector2(0, 1f);

                bool visible = false;

                int x = i - ((-solidTilemap.cellBounds.xMin + Mathf.FloorToInt(camTransform.position.x) - camWidth / 2) * 2);
                int y = j - ((-solidTilemap.cellBounds.yMin + Mathf.FloorToInt(camTransform.position.y) - camHeight / 2) * 2);

                Vector2 target;
                
                foreach (var dir in directions)
                {
                    target = tile - start + dir;
                    if (target.magnitude <= new Vector2(camWidth * 0.8f, camHeight * 0.67777f).magnitude)
                    {
                        RaycastHit2D hit = Physics2D.Raycast(start, target, target.magnitude - 0.125f, groundMask);
                        if (!hit)
                        {
                            visible = true;
                            break;
                        }
                    }
                }

                if (j < 0 || j >= tileSeen.GetLength(1) || i < 0 || i >= tileSeen.GetLength(0))   //mimo tilemapu
                {                    
                    if (visible)texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                    else texture.SetPixel(x, y, Color.black);
                }
                else if (visible)
                {
                    tileSeen[i, j] = true;
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
                else if (tileSeen[i, j]) texture.SetPixel(x, y, new Color(0, 0, 0, 0.75f));
                else texture.SetPixel(x, y, Color.black);
            }
        }
        texture.Apply();

        Destroy(mapTexture);
        mapTexture = new Texture2D(camWidth * 2 + 1, camHeight * 2 + 3);
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.SetPixels(texture.GetPixels());
        mapTexture.Apply();
        mapTexturePosition = camTransform.position;
        Destroy(texture);
        shouldRaycast = false;
    }
}

public enum TileType
{
    UnSeen, Seen, Visible
}