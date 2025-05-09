using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Butterfly))]
public class ButterflyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Butterfly butterfly = (Butterfly)target;

        if (GUILayout.Button("Generate Path"))
        {
            butterfly.GeneratePath();
        }
    }
}
