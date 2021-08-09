using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MixingPlane))]
public class MixingPlaneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!Application.isPlaying)
        {
            MixingPlane mixingPlane = target as MixingPlane;
            mixingPlane.currSettingIndex = EditorGUILayout.IntSlider("Index", mixingPlane.currSettingIndex, 0, mixingPlane.settings.Length - 1);
            mixingPlane.UpdateSetting();
        }
    }
}