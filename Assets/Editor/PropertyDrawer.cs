#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AnimatorTester.KeyParameter))]
public class KeyParameterDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        Rect rect = new Rect(position.x, position.y, position.width, lineHeight);

        SerializedProperty tecla1Prop = property.FindPropertyRelative("tecla1");
        SerializedProperty tecla2Prop = property.FindPropertyRelative("tecla2");
        SerializedProperty parametroProp = property.FindPropertyRelative("parametro");
        SerializedProperty tipoProp = property.FindPropertyRelative("tipo");
        SerializedProperty duracionProp = property.FindPropertyRelative("duracion");
        SerializedProperty bloquearProp = property.FindPropertyRelative("bloquearAmbas");
        SerializedProperty invertirProp = property.FindPropertyRelative("invertir");

        EditorGUI.PropertyField(rect, tecla1Prop, new GUIContent("Tecla 1"));
        rect.y += lineHeight + spacing;

        EditorGUI.PropertyField(rect, tecla2Prop, new GUIContent("Tecla 2 (opcional)"));
        rect.y += lineHeight + spacing;

        EditorGUI.PropertyField(rect, parametroProp);
        rect.y += lineHeight + spacing;

        EditorGUI.PropertyField(rect, tipoProp);
        rect.y += lineHeight + spacing;

        int tipo = tipoProp.enumValueIndex;
        bool showDuracion = (tipo == (int)AnimatorTester.ParameterType.Float || tipo == (int)AnimatorTester.ParameterType.Trigger);
        EditorGUI.BeginDisabledGroup(!showDuracion);
        EditorGUI.PropertyField(rect, duracionProp, new GUIContent("Duración"));
        EditorGUI.EndDisabledGroup();
        rect.y += lineHeight + spacing;

        EditorGUI.PropertyField(rect, bloquearProp, new GUIContent("Bloquear si ambas teclas están presionadas"));
        rect.y += lineHeight + spacing;

        EditorGUI.PropertyField(rect, invertirProp, new GUIContent("Invertir acción"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        return (lineHeight * 7) + (spacing * 6);
    }
}
#endif
