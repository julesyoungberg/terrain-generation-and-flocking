using System.Collections;
using UnityEngine;
using UnityEditor;

public static class Terrain
{
    // generates a mesh grid of size res and applies the given heightmap
    public static Mesh GenerateMesh(Mesh mesh, int res, float[,] heightMap, float waterThreshold)
    {
        mesh.name = "Procedural Grid";

        // calculate the vertices and uv coords for the mesh
        Vector3[] vertices = new Vector3[(res + 1) * (res + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0, y = 0; y <= res; y++)
            for (int x = 0; x <= res; x++, i++)
            {
                float height = heightMap[x, y];
                if (height < waterThreshold) height = waterThreshold - 0.1f;
                vertices[i] = new Vector3(x, height, y);
                uv[i] = new Vector2((float)x / res, (float)y / res);
            }

        // calculate the triangles of the mesh
        int[] triangles = new int[res * res * 6];
        for (int ti = 0, vi = 0, y = 0; y < res; y++, vi++)
            for (int x = 0; x < res; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + res + 1;
                triangles[ti + 5] = vi + res + 2;
            }

        // update the mesh
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }

    // creates a new plane game object s
    public static GameObject GeneratePlane(int res, float[,] heightMap, float waterThreshold)
    {
        // Create empty game object with mesh filter and mesh renderer
        GameObject p = new GameObject("Terrain");
        MeshFilter mf = p.AddComponent<MeshFilter>();
        MeshRenderer mr = p.AddComponent<MeshRenderer>();
        // generate vertices and triangles
        mf.sharedMesh = GenerateMesh(new Mesh(), res, heightMap, waterThreshold);
        return p;
    }

    // assigns a model to a location on the given terrain
    // rotates the model to fit and checks that no other models are within 10pxs
    public static void OnTerrain(GameObject model, GameObject terrain, GameObject[] others, Vector3 thresholds, int radius)
    {
        Mesh mesh = terrain.GetComponent<MeshFilter>().sharedMesh;

        int index = 0;
        bool isUnique = false;
        Vector3 vertex = mesh.vertices[0];
        // continually randomly choose vertices till 
        while (!isUnique)
        {
            index = Random.Range(0, mesh.vertexCount);
            vertex = mesh.vertices[index];
;
            // check if abover beach
            if (vertex.y > thresholds.y)
            {
                // check distance between others
                bool found = false;
                for (int i = 0; i < others.Length; i++)
                {
                    GameObject other = others[i];
                    if (other != null && Vector3.Distance(vertex, other.transform.position) < radius)
                    {
                        found = true;
                        break;
                    }
                }

                // if not close to any others move ons
                isUnique = !found;
            }
        }

        // move model to vertex's position
        model.transform.position = vertex;

        // rotate model to match normal
        Vector3 normal = mesh.normals[index];
        Vector3 modelNormal = new Vector3(0, 1, 0);
        Vector3 axis = Vector3.Cross(modelNormal, normal);
        float angle = Vector3.Angle(modelNormal, normal);
        model.transform.Rotate(axis, angle, Space.Self);
    }

    // simpler version of OnTerrain that ignores the other models
    public static void OnTerrain(GameObject model, GameObject terrain, Vector3 thresholds)
    {
        OnTerrain(model, terrain, new GameObject[0], thresholds, 0);
    }

    // instantiates a list of prefabs from the resources prefabbs folder and randomly places them on the given terrain
    public static GameObject[] CreateModelsOnTerrain(string[] prefabs, GameObject terrain, Vector3 thresholds, int radius, int scale)
    {
        GameObject[] m = new GameObject[prefabs.Length];
        for (int i = 0; i < m.Length; i++)
        {
            GameObject model = Object.Instantiate(Resources.Load("Prefabs/" + prefabs[i], typeof(GameObject))) as GameObject;
            if (model.CompareTag("Structure"))
            {
                model.transform.localScale = new Vector3(scale * 5, scale * 5, scale * 5);
            }
            else
            {
                model.transform.localScale = new Vector3(scale, scale, scale);
            }
            Terrain.OnTerrain(model, terrain, radius == 0 ? new GameObject[0] : m, thresholds, radius);
            m[i] = model;
        }
        return m;
    }

    // simpler version of CreateModelsOnTerrain that doesn't worry about the other models
    public static GameObject[] CreateModelsOnTerrain(string[] prefabs, GameObject terrain, Vector3 thresholds, int scale)
    {
        return CreateModelsOnTerrain(prefabs, terrain, thresholds, 0, scale);
    }
}