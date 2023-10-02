using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum WallType {
    Normal,
    Damaging,
    Breakable
}

public class WallTile : Tile
{
    public WallType Type;

    // from https://docs.unity3d.com/Manual/Tilemap-ScriptableTiles-Example.html
#if UNITY_EDITOR
    [MenuItem("Assets/Create/WallTile")]
    public static void CreateWallTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Wall Tile", "New Wall Tile", "Asset", "Save Wall Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<WallTile>(), path);
    }
#endif
}
