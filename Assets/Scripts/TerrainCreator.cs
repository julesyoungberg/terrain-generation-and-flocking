using UnityEngine;
using System.Collections;

public class TerrainCreator : MonoBehaviour
{
    [Range(10, 500)]
    public int res = 250;

    [Range(1, 8)]
    public int octaves = 5;

    [Range(1f, 4f)]
    public float lacunarity = 2f;

    [Range(0f, 1f)]
    public float persistence = 0.5f;

    [Range(10, 500)]
    public int height = 20;

    [Range(1, 20)]
    public int noiseFreq = 10;

    public Vector3 offset = new Vector3(0, 0, 0);

    public string[] prefabs = {
        "Trees/tree_a", "Trees/tree_b", "Trees/tree_c", "Trees/tree_d", "Trees/tree_e", "Trees/tree_f",
        "Trees/tree_g", "Trees/tree_h", "Trees/tree_i", "Trees/tree_j", "Trees/tree_k", "Trees/tree_l"
    };

    public Vector3 thresholds = new Vector3(30, 33, 50);

    protected GameObject[] models;

    public int modelScale = 4;

    public int modelRadius = 10;

    public bool createModels = true;

    public bool drawGizmos = false;

    protected void DestroyModels()
    {
        foreach (GameObject model in models)
            if (model != null) DestroyImmediate(model);
    }
}
