#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string[] nameList = AllSceneNames();
        if (nameList.Length == 0)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        if (property.propertyType == SerializedPropertyType.String)
        {
            int selectedIndex = Mathf.Max(0, Array.IndexOf(nameList, property.stringValue));
            int index = EditorGUI.Popup(position, property.displayName, selectedIndex, nameList);
            property.stringValue = nameList[index];
        }
        else if (property.propertyType == SerializedPropertyType.Integer)
        {
            property.intValue = EditorGUI.Popup(position, property.displayName, property.intValue, nameList);
        }
        else
        {
            base.OnGUI(position, property, label);
        }
    }

    private static string[] AllSceneNames()
    {
        List<string> sceneNames = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
            {
                continue;
            }

            string fileName = scene.path.Substring(scene.path.LastIndexOf('/') + 1);
            string sceneName = fileName.Substring(0, fileName.Length - 6);
            sceneNames.Add(sceneName);
        }

        return sceneNames.ToArray();
    }
}

#endif