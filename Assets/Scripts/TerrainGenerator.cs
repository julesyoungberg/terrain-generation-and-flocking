using System.Collections;
using UnityEngine;
using UnityEditor;

public class TerrainGenerator : TerrainCreator
{
    private GameObject plane;

    public Material terrainMaterial;

    // use OnEnable instead of Awake because OnEnable is called after recompiles
    private void OnEnable()
    {
        DestroyTerrain();
        GenerateTerrain();
    }

    // Use this for drawing in edit mode
    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            DestroyTerrain();
            GenerateTerrain();
        }
    }

    private void OnApplicationQuit ()
    {
        DestroyTerrain();
    }

    private void DestroyTerrain ()
    {
        if (plane == null) return;
        DestroyModels();
        DestroyImmediate(plane);
    }

    private void GenerateTerrain()
    {
        // Step 1. Generate a 250x250 square Perlin noise patch
        float[,] noisePatch = Noise.GenerateNoise(res + 1, noiseFreq, octaves, lacunarity, persistence, height, offset);

        // Step 2 & 3. Generate a Plane mesh divided into 250x250 grid using Perlin noise patch as a height map
        plane = Terrain.GeneratePlane(res, noisePatch, thresholds[0]);

        // Step 4. Assign the terrain material to the plane
        Renderer r = plane.GetComponent<Renderer>();
        r.material = terrainMaterial;

        // Add reflection probe for water reflections
        AddReflectionProbe();

        // Step 6. Scatter 3D models around the terrain
        if (createModels)
            models = Terrain.CreateModelsOnTerrain(prefabs, plane, thresholds, modelRadius, modelScale);
    }

    private void AddReflectionProbe()
    {
        ReflectionProbe probe = plane.AddComponent<ReflectionProbe>() as ReflectionProbe;
        // set texture resolution
        probe.resolution = 256;
        // Reflection will be used for objects in 10 units around the position of the probe 
        probe.size = new Vector3(10, 10, 10);
        // Set the position (or any transform property)
        probe.transform.position = new Vector3(0, 0, 0);
    }
}
