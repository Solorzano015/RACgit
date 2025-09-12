#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimatorTester))]
public class AnimatorTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AnimatorTester tester = (AnimatorTester)target;

        if (GUILayout.Button("Agregar asignación"))
        {
            tester.keyParameters.Add(new AnimatorTester.KeyParameter());
        }
    }
}
#endif
