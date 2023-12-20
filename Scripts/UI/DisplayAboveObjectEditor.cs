using UnityEditor;

[CustomEditor(typeof(DisplayAboveObject))]
public class DisplayAboveObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DisplayAboveObject script = (DisplayAboveObject)target;
        bool allowSceneObjects = !EditorUtility.IsPersistent(script);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginVertical("box");
        script.guiSKin = EditorGUILayout.ObjectField("GUI Skin", script.guiSKin, typeof(UnityEngine.GUISkin), allowSceneObjects) as UnityEngine.GUISkin;
        script.m_OnHideShow = (OnHide)EditorGUILayout.EnumPopup("On Hide Show", script.m_OnHideShow);
        script.positionDisplay = EditorGUILayout.ObjectField("Display Target", script.positionDisplay, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        script.playerName = EditorGUILayout.TextField("Player Name", script.playerName);
        script.Name_Color = EditorGUILayout.ColorField("Text Color", script.Name_Color);
        script.Bar_Color = EditorGUILayout.ColorField("Bar Color", script.Bar_Color);
        script.HideTexureColor = EditorGUILayout.ColorField("Hide Texture Color", script.HideTexureColor);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        script.Hide_Distance = EditorGUILayout.FloatField("Hide Distance", script.Hide_Distance);
        if (script.Hide_In_Distance)
        {
            script.HideTextureSize = EditorGUILayout.FloatField("Hide Texture Size", script.HideTextureSize);
        }
        if (script.Show_Bar)
        {
            script.Bar_Size = EditorGUILayout.Vector2Field("Bar Size", script.Bar_Size);
            script.BacgroundBarSize = EditorGUILayout.Vector2Field("Back Bar Size", script.BacgroundBarSize);

        }
        if (script.OutLine)
        {
            script.OutLineSize = EditorGUILayout.IntSlider("OutLine Size", script.OutLineSize, 0, 10);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        script.Show_Text = EditorGUILayout.ToggleLeft("Show Text", script.Show_Text);
        script.Show_Bar = EditorGUILayout.ToggleLeft("Show Bar", script.Show_Bar);
        script.Hide_In_Distance = EditorGUILayout.ToggleLeft("Hide", script.Hide_In_Distance);
        script.OutLine = EditorGUILayout.ToggleLeft("OutLine", script.OutLine);
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical("box");
        script.Bar_Texture = EditorGUILayout.ObjectField("Bar Texture", script.Bar_Texture, typeof(UnityEngine.Texture2D), allowSceneObjects) as UnityEngine.Texture2D;
        script.Hide_Texture = EditorGUILayout.ObjectField("Hide Texture", script.Hide_Texture, typeof(UnityEngine.Texture2D), allowSceneObjects) as UnityEngine.Texture2D;
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
    }
}
