using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexInfo : MonoBehaviour
{
    public int X = -1;
    public int Y = -1;

    [Space]
    public float Height = 88;
    public float Humidity = 88;
    public float Temperature = 88;

    [Space]
    public string Biome;
    public bool IsHill = false;
    public bool IsForest = false;
    public bool IsSpring = false;
    public bool IsRiver = false;
    public string LandType; //island or mainland

    [Space]
    //[HideInInspector]
    public List<DraftHexPrefab> Neighbors = new List<DraftHexPrefab>();


    public void SwitchNeighbors()
    {
        foreach (var neighbor in Neighbors)
        {
            bool state = neighbor.gameObject.activeSelf;
            neighbor.gameObject.SetActive(!state);
        }
    }
}
