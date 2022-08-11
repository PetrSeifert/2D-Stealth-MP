using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BetterGraphics
{
    public class ShaderData
    {
        public List<string> Names = new List<string>();
        public List<dynamic> Data = new List<dynamic>();
        public List<ShaderDataType> Types = new List<ShaderDataType>();
        public void Add(string name, Texture data)
        {
            Names.Add(name);
            Data.Add(data);
            Types.Add(ShaderDataType.texture);
        }
        public void Add(string name, Texture[] data)
        {
            Names.Add(name);
            Data.Add(data);
            Types.Add(ShaderDataType.textures);
        }
        public void Add(string name, Vector4 data)
        {
            Names.Add(name);
            Data.Add(data);
            Types.Add(ShaderDataType.vector);
        }
        public void Add(string name, Vector4[] data)
        {
            Names.Add(name);
            Data.Add(data);
            Types.Add(ShaderDataType.vectors);
        }
        public void Add(string name, int data)
        {
            Names.Add(name);
            Data.Add(data);
            Types.Add(ShaderDataType.integer);
        }
        public void ApplyData(Material material)
        {
            for (int i = 0; i < Types.Count; i++)
            {
                switch (Types[i])
                {
                    case ShaderDataType.texture:
                        material.SetTexture(Names[i], Data[i]);
                        break;                        
                    case ShaderDataType.textures:
                        material.SetTexture(Names[i], Data[i]);
                        break;
                    case ShaderDataType.vector:
                        material.SetVector(Names[i], Data[i]);
                        break;
                    case ShaderDataType.vectors:
                        material.SetVectorArray(Names[i], Data[i]);
                        break;
                    case ShaderDataType.integer:
                        material.SetInt(Names[i], Data[i]);
                        break;
                    default:
                        break;
                }
            }
        }
    }
    public enum ShaderDataType
    {
        texture, textures, vector, vectors, integer
    }
    public static void MeshBlit(RenderTexture destination, Material material, Mesh mesh, ShaderData data = null, Vector2 center = new Vector2(), float width = 1f, float height = 1f)
    {
        var original = RenderTexture.active;
        RenderTexture.active = destination;

        if (data != null) data.ApplyData(material);
        
        GL.PushMatrix();
        GL.LoadOrtho();

        material.SetPass(0);

        GL.Begin(GL.TRIANGLES);
        foreach (var order in mesh.triangles)
        {
            Vector2 vec = (Vector2)mesh.vertices[order] - center + new Vector2(width / 2f, height / 2f); //pozice vůči rohu 0f,0f
            GL.Vertex3(vec.x / width, vec.y / height, 0f); //přepočítáno na 0f - 1f
        }

        GL.End();
        GL.PopMatrix();
        RenderTexture.active = original;
    }
    public static void Blit(RenderTexture destination, Material material, ShaderData data = null, Vector2 uv = new Vector2())
    {
        var original = RenderTexture.active;
        RenderTexture.active = destination;

        if (uv == null) uv = new Vector2(1, 1);
        if (data != null) data.ApplyData(material);

        GL.PushMatrix();
        GL.LoadOrtho();

        material.SetPass(0);

        GL.Begin(GL.QUADS);
        GL.TexCoord2(0f, 0f); GL.Vertex3(0f, 0f, 0f);
        GL.TexCoord2(0f, uv.y); GL.Vertex3(0f, 1f, 0f);
        GL.TexCoord2(uv.x, uv.y); GL.Vertex3(1f, 1f, 0f);
        GL.TexCoord2(uv.x, 0f); GL.Vertex3(1f, 0f, 0f);

        GL.End();
        GL.PopMatrix();
        RenderTexture.active = original;
    }
}
