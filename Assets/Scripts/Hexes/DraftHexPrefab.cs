using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraftHexPrefab : MonoBehaviour
{
    public HexInfo InfoScript;

    public Text Height;
    public Text Humidity;
    public Text Temperature;
    public Image BGImage;

    [Space]
    public GameObject Hill;
    public GameObject Forest;
    public GameObject Spring;
    public GameObject DraftRiver;
    public GameObject DraftRiverEnd;
}
