using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainComponent : TerrainCreator
{
    // use OnEnable instead of Awake because OnEnable is called after recompiles
    private void OnEnable()
    {
        DestroyModels();
        GenerateTerrain();
    }

    // Use this for drawing in edit mode
    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            DestroyModels();
            GenerateTerrain();
        }
    }

    private void OnApplicationQuit ()
    {
        DestroyModels();
    }

    private void GenerateTerrain()
    {
        // Step 1. Generate a 250x250 square Perlin noise patch
        float[,] noisePatch = Noise.GenerateNoise(res + 1, noiseFreq, octaves, lacunarity, persistence, height, offset);

        // Step 2 & 3. Generate a Plane mesh divided into 250x250 grid using Perlin noise patch as a height map
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        Terrain.GenerateMesh(mesh, res, noisePatch, thresholds[0]);

        // Step 6. Scatter 3D models around the terrain
        if (createModels) 
            models = Terrain.CreateModelsOnTerrain(prefabs, gameObject, thresholds, modelRadius, modelScale);
    }

}
