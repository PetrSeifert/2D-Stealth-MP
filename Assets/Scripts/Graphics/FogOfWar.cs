using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWar : MonoBehaviour
{
    /* Mapy, Layery */
    [SerializeField] Tilemap solidTilemap;
    [SerializeField] GameObject doors;
    int raycastMask;
    List<Vector2>[,] mapSegments;   //Pole segmentů tilemapy (10x10), počítáno od tilemap.cellbounds.min
    List<Vector2> solidCorners = new List<Vector2>();

    /* Pozice hráče a kamery */
    [SerializeField] Transform player;
    Vector2 playerCenter;
    Vector2 cameraCenter;
    float cameraHeight = 270f / 16f;
    float cameraWidth = 480f / 16f;
    float cameraDiagonal;

    /* Shadery, Materiály, Textury */
    [SerializeField] Shader alphaShader;    //Vytváří texturu z meshe
    [SerializeField] Shader mainShader;     //Aplikuje alfu na texturu
    [SerializeField] Shader averageShader;  //Zprůměruje textury do jedné
    Material alphaMat;
    Material mainMat;
    Material averageMat;
    [SerializeField] Texture2D mainTex;
    RenderTexture alphaTex;

    /* Nastavení */
    [SerializeField] Quality fogQuality = Quality.medium;
    [SerializeField] Quality edgeFiltering = Quality.medium;
    [SerializeField] float fogDensity = 0.5f;
    [SerializeField] float visibleOffset = 0.1875f;
    //[SerializeField] bool alphaTimeBlur = false;
    void Start()
    {
        cameraDiagonal = new Vector2(cameraWidth, cameraHeight).magnitude;
        cameraCenter = GetComponent<Transform>().position;
        alphaMat = new Material(alphaShader);
        mainMat = new Material(mainShader);
        averageMat = new Material(averageShader);
        raycastMask = LayerMask.GetMask("Ground");
        alphaTex = new RenderTexture(480, 240, 24);

        /* Načíst pevné rohy */
        mapSegments = new List<Vector2>[Mathf.CeilToInt((Mathf.Abs(solidTilemap.cellBounds.xMin) + Mathf.Abs(solidTilemap.cellBounds.xMax)) / 10) + 1, Mathf.CeilToInt((Mathf.Abs(solidTilemap.cellBounds.yMin) + Mathf.Abs(solidTilemap.cellBounds.yMax)) / 10) + 1];
        
        foreach (var position in solidTilemap.cellBounds.allPositionsWithin)
        {
            if (!solidTilemap.HasTile(position)) continue;
            

            foreach (var vertex in solidTilemap.GetSprite(new Vector3Int(position.x, position.y, 0)).vertices)   //Zápis rohů tilu do mapy
            {
                Vector2 corner = vertex + (Vector2Int)position + new Vector2(0.5f, 0.5f);
                Vector2Int segment = new Vector2Int(Mathf.FloorToInt((corner.x - solidTilemap.cellBounds.xMin) / 10), Mathf.FloorToInt((corner.y - solidTilemap.cellBounds.yMin) / 10));
                if (mapSegments[segment.x, segment.y] == null) mapSegments[segment.x, segment.y] = new List<Vector2>();

                if (!mapSegments[segment.x, segment.y].Contains(corner))
                {
                    mapSegments[segment.x, segment.y].Add(corner);
                }
            }
        }

        foreach (var doorCollider in doors.GetComponentsInChildren<BoxCollider2D>())    //Zápis rohů dveří do mapy
        {
            foreach (var corner in doorCollider.CreateMesh(true, true).vertices)
            {
                Vector2Int segment = new Vector2Int(Mathf.FloorToInt((corner.x - solidTilemap.cellBounds.xMin) / 10), Mathf.FloorToInt((corner.y - solidTilemap.cellBounds.yMin) / 10));
                if (mapSegments[segment.x, segment.y] == null) mapSegments[segment.x, segment.y] = new List<Vector2>();

                if (!mapSegments[segment.x, segment.y].Contains(corner))
                {
                    mapSegments[segment.x, segment.y].Add(corner);
                }
            }
        }

        UpdateVisibleCorners();
    }
    void OnRenderImage(RenderTexture src, RenderTexture dest)   //Místo Updatu
    {
        if (playerCenter != (Vector2)player.position || cameraCenter != (Vector2)GetComponent<Transform>().position) UpdateVisibleCorners(); //Přepočítání segmentů při změně pozice //šlo by optimalizovat na načítání jednotlivých segmentů


        /*Texture2DArray blurAlphaTexs = new Texture2DArray(480, 270, 2, TextureFormat.RGBA32, false);
        if (alphaTimeBlur) Graphics.CopyTexture(alphaTex, 0, 0, blurAlphaTexs, 0, 0);  //přepsat, místo kopie dělá referenci//záloha minulé alphy*/

        UpdateAlpha();  //Určuje, kde má být mlha   //lze optimalizovat stejnou podmínkou jako rohy, akorát podmínit i dveřmi   //možná vynechat hodně průhledné pixely?

        /*if (alphaTimeBlur)  //spojení s minulou alphou - působí spíš jako lagy
        {
            Graphics.CopyTexture(alphaTex, 0, 0, blurAlphaTexs, 1, 0);
            BetterGraphics.ShaderData sData = new BetterGraphics.ShaderData();
            sData.Add("_Textures", blurAlphaTexs);
            sData.Add("_TexturesCount", 2);
            BetterGraphics.Blit(alphaTex, averageMat, sData, new Vector2(cameraWidth, cameraHeight));
            Destroy(blurAlphaTexs);
        }*/

        /* Vykreslení mlhy do kamery */
        BetterGraphics.ShaderData shaderData = new BetterGraphics.ShaderData();
        shaderData.Add("_MainTex", mainTex);
        shaderData.Add("_AlphaTex", alphaTex);
        shaderData.Add("_OriginalTex", src);    //Bude se střídat až uděláme textury
        shaderData.Add("_CamPosition", cameraCenter);
        shaderData.Add("_Quality", (int)edgeFiltering);
        BetterGraphics.Blit(dest, mainMat, shaderData, new Vector2(cameraWidth, cameraHeight));
    }
    void UpdateVisibleCorners() //Vymění cíle raycastů za body v segmentech zasahujících do kamery
    {
        solidCorners = new List<Vector2>();

        Vector2 relativeCamCenter = cameraCenter - (Vector2Int)solidTilemap.cellBounds.min;
        for (int i = Mathf.FloorToInt((relativeCamCenter.x - cameraWidth / 2) / 10); i <= Mathf.FloorToInt((relativeCamCenter.x + cameraWidth / 2) / 10) + 1; i++)
        {
            if (i < 0 || i >= mapSegments.GetLength(0)) continue;
            for (int j = Mathf.FloorToInt((relativeCamCenter.y - cameraHeight / 2) / 10); j <= Mathf.FloorToInt((relativeCamCenter.y + cameraHeight / 2) / 10) + 1; j++)
            {
                if (j < 0 || j >= mapSegments.GetLength(1)) continue;

                if (mapSegments[i, j] == null) mapSegments[i, j] = new List<Vector2>();
                foreach (var point in mapSegments[i, j])
                {
                    solidCorners.Add(point);
                }
            }
        }
    }
    void UpdateAlpha()
    {
        playerCenter = player.position;
        cameraCenter = GetComponent<Transform>().position;

        List<Vector2> a = new List<Vector2>(solidCorners);
        a.Add(new Vector2(cameraCenter.x + cameraWidth / 2, cameraCenter.y + cameraHeight / 2));
        a.Add(new Vector2(cameraCenter.x + cameraWidth / 2, cameraCenter.y - cameraHeight / 2));
        a.Add(new Vector2(cameraCenter.x - cameraWidth / 2, cameraCenter.y + cameraHeight / 2));
        a.Add(new Vector2(cameraCenter.x - cameraWidth / 2, cameraCenter.y - cameraHeight / 2));


        /* Výběr středů meshů */
        List<Vector2> centers = new List<Vector2>();
        centers.Add(playerCenter);
        if (fogQuality > Quality.low)
        {
            centers.Add(playerCenter + Vector2.up * fogDensity);
            centers.Add(playerCenter + Vector2.down * fogDensity);
            centers.Add(playerCenter + Vector2.left * fogDensity);
            centers.Add(playerCenter + Vector2.right * fogDensity);
            if (fogQuality > Quality.medium)
            {
                centers.Add(playerCenter + new Vector2(1, 1).normalized * fogDensity);
                centers.Add(playerCenter + new Vector2(1, -1).normalized * fogDensity);
                centers.Add(playerCenter + new Vector2(-1, 1).normalized * fogDensity);
                centers.Add(playerCenter + new Vector2(-1, -1).normalized * fogDensity);
                if (fogQuality == Quality.ultra)
                {
                    centers.Add(playerCenter + new Vector2(2, 1).normalized * fogDensity);
                    centers.Add(playerCenter + new Vector2(2, -1).normalized * fogDensity);
                    centers.Add(playerCenter + new Vector2(-2, 1).normalized * fogDensity);
                    centers.Add(playerCenter + new Vector2(-2, -1).normalized * fogDensity);
                    centers.Add(playerCenter + new Vector2(1, 2).normalized * fogDensity);
                    centers.Add(playerCenter + new Vector2(1, -2).normalized * fogDensity);
                    centers.Add(playerCenter + new Vector2(-1, 2).normalized * fogDensity);
                    centers.Add(playerCenter + new Vector2(-1, -2).normalized * fogDensity);
                }
            }
        }


        /* Vytvoření jednotlivých alph z meshů */
        Texture2DArray alphaTexs = new Texture2DArray(480, 270, centers.Count, TextureFormat.RGBA32, false);
        float currentOffset = 0;
        for (int i = 0; i < centers.Count; i++)
        {
            switch (Mathf.Floor((i - 1) / 4))
            {
                case 0:
                    currentOffset = visibleOffset * 0.75f;
                    break;
                case 1:
                    currentOffset = visibleOffset * 0.5f;
                    break;
                case 2:
                case 3:
                    currentOffset = visibleOffset * 0.25f;
                    break;
                default:
                    currentOffset = visibleOffset * 1f;
                    break;
            }
            RenderTexture alpha = new RenderTexture(480, 270, 24);
            Mesh mesh = VisibleArea(a, centers[i], currentOffset);

            BetterGraphics.MeshBlit(alpha, alphaMat, mesh, null, cameraCenter, cameraWidth, cameraHeight);
            Graphics.CopyTexture(alpha, 0, 0, alphaTexs, i, 0);
            Destroy(alpha);
        }


        /* Spojení alph do jedné */
        Destroy(alphaTex);
        alphaTex = new RenderTexture(480, 270, 24);

        BetterGraphics.ShaderData shaderData = new BetterGraphics.ShaderData();
        shaderData.Add("_Textures", alphaTexs);
        shaderData.Add("_TexturesCount", centers.Count);
        BetterGraphics.Blit(alphaTex, averageMat, shaderData, new Vector2(cameraWidth, cameraHeight));
        Destroy(alphaTexs);
    }
    Vector2 Offset(Vector2 point, Vector2 direction, float offset) { return point + direction.normalized * offset; }
    Mesh VisibleArea(List<Vector2> corners, Vector2 center, float offset)   //Vytvoří mesh místa, kde není mlha
    {
        /* Raycasty na rohy */
        List<Vector2> hits = new List<Vector2>();
        foreach (var point in corners)
        {
            Vector2[] points = new Vector2[3];
            points[0] = (point - center);
            points[1] = (new Vector2(-points[0].y, points[0].x).normalized * 0.0001f + points[0]);
            points[2] = (new Vector2(points[0].y, -points[0].x).normalized * 0.0001f + points[0]);
            for (int i = 0; i < 3; i++)
            {
                Vector2 direction = points[i].normalized;
                float x = Mathf.Abs(center.x - cameraCenter.x - Mathf.Sign(direction.x) * cameraWidth / 2);  //Vzdálenost od kraje vlevo nebo vpravo
                float y = Mathf.Abs(center.y - cameraCenter.y - Mathf.Sign(direction.y) * cameraHeight / 2); //Nahoře a dole
                if (Physics2D.Raycast(center, points[i], cameraDiagonal * 1.5f, raycastMask))
                {
                    RaycastHit2D raycastHit2D = Physics2D.Raycast(center, points[i], cameraDiagonal * 1.5f, raycastMask);
                    Vector2 hit = raycastHit2D.point;
                    if (Mathf.Abs(hit.x - center.x) > x + 1 || Mathf.Abs(hit.y - center.y) > y + 1) //Raycast trefil bod mimo viditelnou oblast
                    {
                        if (x * Mathf.Abs(direction.y) < y * Mathf.Abs(direction.x)) hits.Add((center + direction / Mathf.Abs(direction.x) * x));
                        else hits.Add((center + direction / Mathf.Abs(direction.y) * y));
                    }
                    else if (hit.magnitude >= (points[i] + center).magnitude - 0.1f)  //Raycast trefil Bod     //tolerance zvyšuje počet zapsaných bodů   //možná brát toleranci podle úhlu na collider?
                    {
                        hits.Add(hit - raycastHit2D.normal.normalized * offset);   //Offset se neuplatní ve směru raycastu, ale kolmě na collider   //při trefení bodu se možná úhel collideru nedá zjistit?
                    }
                }
                else    //Raycast nic netrefil
                {
                    if (x * Mathf.Abs(direction.y) < y * Mathf.Abs(direction.x)) hits.Add((center + direction / Mathf.Abs(direction.x) * x));
                    else hits.Add((center + direction / Mathf.Abs(direction.y) * y));
                }
            }
        }


        /* Seřadit podle úhlů */
        List<float> radians = new List<float>();
        for (int i = 0; i < hits.Count; i++)
        {
            radians.Add(Mathf.Asin(Vector2.Dot(new Vector2(0, 1), (hits[i] - center).normalized)));
            if (hits[i].x - center.x < 0) radians[i] = Mathf.PI - radians[i];
        }
        bool sorted = false;
        while (!sorted) //Bubble sort podle radiánů  //lze optimalizovat rozdělením na čtvrtiny
        {
            sorted = true;
            for (int i = 0; i < hits.Count - 1; i++)
            {
                if (radians[i] > radians[i + 1])
                {
                    float tempRad = radians[i];
                    Vector2 tempVector = hits[i];
                    radians[i] = radians[i + 1];
                    radians[i + 1] = tempRad;
                    hits[i] = hits[i + 1];
                    hits[i + 1] = tempVector;
                    sorted = false;
                }
            }
        }


        /*Vytvořit mesh*/
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[hits.Count + 1];
        int[] triangles = new int[hits.Count * 3];
        for (int i = 0; i < hits.Count; i++)
        {
            vertices[i] = hits[i];
            triangles[3 * i] = hits.Count;
            triangles[3 * i + 1] = i;
            triangles[3 * i + 2] = i + 1;
        }
        triangles[3 * hits.Count - 1] = 0;
        vertices[hits.Count] = center;
        mesh.vertices = vertices;
        mesh.triangles = triangles;


        return mesh;
    }
}
public enum Quality : int { low = 0, medium = 1, high = 2, ultra = 3 }