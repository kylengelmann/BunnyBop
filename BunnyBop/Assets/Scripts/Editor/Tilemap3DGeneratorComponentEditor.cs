using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tilemap3DGeneratorComponent))]
[CanEditMultipleObjects]
public class Tilemap3DGeneratorComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Generate 3D Geometry"))
        {
            Tilemap3DGeneratorComponent tilemap3DGenerator = (Tilemap3DGeneratorComponent) serializedObject.targetObject;
            if (tilemap3DGenerator)
            {
                tilemap3DGenerator.Generate3DGeometry();
            }
        }
    }
}
