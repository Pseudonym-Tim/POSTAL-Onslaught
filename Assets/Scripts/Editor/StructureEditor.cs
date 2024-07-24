using UnityEditor;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor for creating/modifying level generation structures...
/// </summary>
public class StructureEditor : EditorWindow
{
    private string jsonPath;
    private JObject structureDatabase;
    private string selectedStructure;
    private Vector2 scrollPos;
    private float zoomFactor = 10f;
    private float entityScale = 2.5f;
    private float levelObjectScale = 2.5f;

    [MenuItem("Window/Structure Editor")]
    public static void ShowWindow()
    {
        StructureEditor window = GetWindow<StructureEditor>("Structure Editor");
        window.minSize = new Vector2(800, 600); // Fixed size window
        window.maxSize = new Vector2(800, 600); // Fixed size window
    }

    private void OnEnable()
    {
        jsonPath = Path.Combine(Application.streamingAssetsPath, "structure_database.json");
        LoadJson();
        if(structureDatabase.Count > 0)
        {
            selectedStructure = structureDatabase.Properties().First().Name;
        }
    }

    private void LoadJson()
    {
        if(File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            structureDatabase = JObject.Parse(json);
        }
        else
        {
            structureDatabase = new JObject();
        }
    }

    private void SaveJson()
    {
        File.WriteAllText(jsonPath, structureDatabase.ToString());
        AssetDatabase.Refresh();
    }

    private void OnGUI()
    {
        if(structureDatabase == null)
        {
            LoadJson();
        }

        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Add Structure"))
        {
            AddStructure();
        }
        if(GUILayout.Button("Save"))
        {
            SaveJson();
            Debug.Log("Saved structure!");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Dropdown list for structure selection
        List<string> structureNames = new List<string>();

        foreach(var structure in structureDatabase)
        {
            structureNames.Add(structure.Key);
        }

        int selectedIndex = structureNames.IndexOf(selectedStructure);
        selectedIndex = EditorGUILayout.Popup("Select Structure", selectedIndex, structureNames.ToArray());
        selectedStructure = selectedIndex >= 0 ? structureNames[selectedIndex] : null;

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(300));
        if(!string.IsNullOrEmpty(selectedStructure))
        {
            DrawStructureEditor(selectedStructure);
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        // Ensure the visualization has enough space
        if(!string.IsNullOrEmpty(selectedStructure))
        {
            GUILayout.BeginVertical(GUILayout.Width(460));
            GUILayout.Space(20);
            GUILayout.Label("Visualization", EditorStyles.boldLabel);

            // Zoom controls with slider
            zoomFactor = EditorGUILayout.Slider("Zoom", zoomFactor, 1f, 20f);
            entityScale = EditorGUILayout.Slider("Entity Scale", entityScale, 1f, 10f);
            levelObjectScale = EditorGUILayout.Slider("Level Object Scale", levelObjectScale, 1f, 10f);

            GUILayout.Space(10);

            Rect visualizationRect = GUILayoutUtility.GetRect(440, 400);
            DrawVisualization((JObject)structureDatabase[selectedStructure], visualizationRect);
            GUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void AddStructure()
    {
        string newStructureName = "NewStructure_" + structureDatabase.Count;
        JObject newStructure = new JObject
        {
            ["structureBounds"] = new JObject
            {
                ["center"] = new JObject { ["x"] = 0, ["y"] = 0 },
                ["boundsX"] = 8,
                ["boundsY"] = 8
            },
            ["entities"] = new JArray(),
            ["level_objects"] = new JArray()
        };
        structureDatabase[newStructureName] = newStructure;
        selectedStructure = newStructureName;
    }

    private void RemoveStructure(string structureID)
    {
        structureDatabase.Remove(structureID);

        if(structureDatabase.Count > 0)
        {
            selectedStructure = structureDatabase.Properties().First().Name;
        }
        else
        {
            selectedStructure = null;
        }
    }

    private void DrawStructureEditor(string structureID)
    {
        JObject structure = (JObject)structureDatabase[structureID];

        EditorGUILayout.LabelField("Structure: " + structureID, EditorStyles.boldLabel);

        if(GUILayout.Button("Remove Structure"))
        {
            RemoveStructure(structureID);
            selectedStructure = null;
            return;
        }

        EditorGUILayout.Space();

        Vector2 center = new Vector2(
            (int)structure["structureBounds"]["center"]["x"],
            (int)structure["structureBounds"]["center"]["y"]
        );
        center = EditorGUILayout.Vector2Field("Center", center);
        structure["structureBounds"]["center"]["x"] = center.x;
        structure["structureBounds"]["center"]["y"] = center.y;

        Vector2 bounds = new Vector2(
            (int)structure["structureBounds"]["boundsX"],
            (int)structure["structureBounds"]["boundsY"]
        );
        bounds = EditorGUILayout.Vector2Field("Bounds", bounds);
        structure["structureBounds"]["boundsX"] = bounds.x;
        structure["structureBounds"]["boundsY"] = bounds.y;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Entities", EditorStyles.boldLabel);

        JArray entities = (JArray)structure["entities"];

        for(int i = 0; i < entities.Count; i++)
        {
            JObject entity = (JObject)entities[i];

            foreach(KeyValuePair<string, JToken> item in entity)
            {
                Vector2 position = new Vector2(
                    (int)item.Value["position"]["x"],
                    (int)item.Value["position"]["y"]
                );
                position = EditorGUILayout.Vector2Field(item.Key, position);
                item.Value["position"]["x"] = position.x;
                item.Value["position"]["y"] = position.y;

                if(GUILayout.Button($"Remove {item.Key}"))
                {
                    entities.RemoveAt(i);
                    break;
                }
            }
        }

        if(GUILayout.Button("Add Entity"))
        {
            AddEntity(structureID);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Objects", EditorStyles.boldLabel);

        JArray levelObjects = (JArray)structure["level_objects"];

        for(int i = 0; i < levelObjects.Count; i++)
        {
            JObject levelObject = (JObject)levelObjects[i];

            foreach(KeyValuePair<string, JToken> item in levelObject)
            {
                Vector2 position = new Vector2(
                    (int)item.Value["position"]["x"],
                    (int)item.Value["position"]["y"]
                );
                position = EditorGUILayout.Vector2Field(item.Key, position);
                item.Value["position"]["x"] = position.x;
                item.Value["position"]["y"] = position.y;

                if(GUILayout.Button($"Remove {item.Key}"))
                {
                    levelObjects.RemoveAt(i);
                    break;
                }
            }
        }

        if(GUILayout.Button("Add Level Object"))
        {
            AddLevelObject(structureID);
        }
    }

    private void AddEntity(string structureID)
    {
        JObject structure = (JObject)structureDatabase[structureID];
        JArray entities = (JArray)structure["entities"];
        string newEntityName = "new_entity_" + entities.Count;
        JObject newEntity = new JObject
        {
            [newEntityName] = new JObject
            {
                ["position"] = new JObject { ["x"] = 0, ["y"] = 0 }
            }
        };
        entities.Add(newEntity);
    }

    private void AddLevelObject(string structureID)
    {
        JObject structure = (JObject)structureDatabase[structureID];
        JArray levelObjects = (JArray)structure["level_objects"];
        string newObjectName = "new_level_object_" + levelObjects.Count;
        JObject newObject = new JObject
        {
            [newObjectName] = new JObject
            {
                ["position"] = new JObject { ["x"] = 0, ["y"] = 0 }
            }
        };
        levelObjects.Add(newObject);
    }

    private void DrawVisualization(JObject structure, Rect visualizationRect)
    {
        // Calculate the center of the visualization area
        Vector2 visualizationCenter = new Vector2(
            visualizationRect.x + visualizationRect.width / 2,
            visualizationRect.y + visualizationRect.height / 2
        );

        // Draw the background
        EditorGUI.DrawRect(visualizationRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));

        Vector2 structureCenter = new Vector2(
            (float)structure["structureBounds"]["center"]["x"],
            (float)structure["structureBounds"]["center"]["y"]
        );
        Vector2 bounds = new Vector2(
            (float)structure["structureBounds"]["boundsX"],
            (float)structure["structureBounds"]["boundsY"]
        );

        // Adjust the structure origin to be in the center of the panel
        Rect structureRect = new Rect(
            visualizationCenter.x - ((bounds.x * zoomFactor) / 2),
            visualizationCenter.y - ((bounds.y * zoomFactor) / 2),
            bounds.x * zoomFactor,
            bounds.y * zoomFactor
        );
        EditorGUI.DrawRect(structureRect, new Color(0, 1, 0, 0.5f));

        JArray entities = (JArray)structure["entities"];
        foreach(JObject entity in entities)
        {
            foreach(var item in entity)
            {
                Vector2 position = new Vector2(
                    (float)item.Value["position"]["x"],
                    (float)item.Value["position"]["y"]
                );
                // Adjust position relative to the structure center
                position -= structureCenter;
                // Flip the y-coordinate for correct orientation
                position.y = -position.y;

                Rect entityRect = new Rect(
                    visualizationCenter.x + (position.x * zoomFactor) - (entityScale * zoomFactor / 2),
                    visualizationCenter.y + (position.y * zoomFactor) - (entityScale * zoomFactor / 2),
                    entityScale * zoomFactor,
                    entityScale * zoomFactor
                );
                EditorGUI.DrawRect(entityRect, new Color(0, 0, 1, 0.5f));

                // Draw the entity ID centered
                Vector2 entityLabelPos = new Vector2(entityRect.center.x, entityRect.center.y);
                Handles.Label(
                    entityLabelPos,
                    item.Key,
                    new GUIStyle()
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = new GUIStyleState() { textColor = Color.white },
                        fontSize = 12,
                    }
                );
            }
        }

        JArray levelObjects = (JArray)structure["level_objects"];
        foreach(JObject levelObject in levelObjects)
        {
            foreach(var item in levelObject)
            {
                Vector2 position = new Vector2(
                    (float)item.Value["position"]["x"],
                    (float)item.Value["position"]["y"]
                );
                // Adjust position relative to the structure center
                position -= structureCenter;
                // Flip the y-coordinate for correct orientation
                position.y = -position.y;

                Rect objectRect = new Rect(
                    visualizationCenter.x + (position.x * zoomFactor) - (levelObjectScale * zoomFactor / 2),
                    visualizationCenter.y + (position.y * zoomFactor) - (levelObjectScale * zoomFactor / 2),
                    levelObjectScale * zoomFactor,
                    levelObjectScale * zoomFactor
                );
                EditorGUI.DrawRect(objectRect, new Color(1, 0, 0, 0.5f));

                // Draw the level object ID centered
                Vector2 objectLabelPos = new Vector2(objectRect.center.x, objectRect.center.y);
                Handles.Label(
                    objectLabelPos,
                    item.Key,
                    new GUIStyle()
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = new GUIStyleState() { textColor = Color.white },
                        fontSize = 12,
                    }
                );
            }
        }
    }


}
