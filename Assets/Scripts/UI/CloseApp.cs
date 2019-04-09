using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CloseApp", menuName = "Scriptables"/*, order = 1*/)]
public class CloseApp : ScriptableObject
{
    public void Close()
    {
        Application.Quit();
    }
}
