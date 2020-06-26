using UnityEngine;
using System;
using System.Security.Cryptography;

public class MeshGen : MonoBehaviour
{
    public int xSize = 20;
    public int zSize = 20;
    private bool running = false;
    GameObject walltop;
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    Texture2D heightmaptex;
    float[,] heightmap;
    Color[] pix;
    Renderer rendy;

    void Start()
    {
        walltop = GameObject.Find("WallTop");
        heightmaptex = new Texture2D(xSize,zSize, TextureFormat.ARGB32, false);
        heightmap = new float[xSize, zSize];
        calcmap(0.15f);
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        rendy = GetComponent<Renderer>();
        rendy.material.mainTexture = heightmaptex;
        rendy.material.EnableKeyword("_NORMALMAP");
        rendy.material.SetTexture("_BumpMap", heightmaptex);
        rendy.material.SetFloat("_BumpScale", 0.15f);
    }

    public static int getrand(int min, int max)
    {
        if (min >= max)
        {
            return min;
        }
        byte[] intBytes = new byte[4];
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetNonZeroBytes(intBytes);
        }
        return min + Math.Abs(BitConverter.ToInt32(intBytes, 0)) % (max - min + 1);
    }

    public void calcmap(float perlinscale)
    {
        pix = new Color[xSize * zSize];
        float seed = getrand(11, 999);                
        for (int x = 0; x < xSize; x++)            
        {            
            for (int z = 0; z < zSize; z++)
            {
                float xCoord = x * perlinscale;
                float yCoord = z * perlinscale;
                float sample = Mathf.PerlinNoise(xCoord + seed, yCoord + seed);
                heightmap[x, z] = sample;
                pix[(int)x * heightmaptex.width + (int)z] = new Color(sample,sample,sample);                
            }
        }
        heightmaptex.SetPixels(pix);
        heightmaptex.Apply();
    }

    private void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        float xchoop = getrand(9, 99);
        float zchoop = getrand(9, 99);
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y;
                if (x <= 2 || x >= xSize -2 || z <= 2 || z >= zSize -2)
                {
                    y = -1;
                }
                else
                {
                    y = heightmap[x, z];
                }
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];
        
        var vert = 0;
        var tris = 0;
        

        for (var z = 0; z < zSize; z++)
        {
            for (var x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;                
            }
            vert++;
        }
        uvs = new Vector2[vertices.Length];
        for (int i = 0, x = 0; x <= xSize; x++)
        {
            for (int z = 0; z <= zSize; z++)
            {
                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);
                i++;
            }
        }

    }

    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();               
    }
}
