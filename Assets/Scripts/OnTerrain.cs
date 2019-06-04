using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTerrain : MonoBehaviour
{
    public Vector3 thresholds = new Vector3(30, 33, 50);

    private void Start()
    {
        Run();
    }

    private void Run ()
    {
        // get terrain mesh
        GameObject terrain = GameObject.Find("Terrain");
        Terrain.OnTerrain(gameObject, terrain, thresholds);
    }
}
