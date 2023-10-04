using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum FloorType {
    Pit,
    Sticky,
    Normal
}

public class FloorTile : Tile
{
    public FloorType Type;

    // from https://docs.unity3d.com/Manual/Tilemap-ScriptableTiles-Example.html
#if UNITY_EDITOR
    [MenuItem("Assets/Create/FloorTile")]
    public static void CreateFloorTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Floor Tile", "New Floor Tile", "Asset", "Save Floor Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<FloorTile>(), path);
    }
#endif
}
