using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(HexInfo), true)]
public class HexInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var hexInfo = target as HexInfo;
        if (hexInfo == null)
            return;

        EditorGUILayout.Space();
        if (GUILayout.Button("SwitchNeighbors"))
            hexInfo.SwitchNeighbors();
    }
}

