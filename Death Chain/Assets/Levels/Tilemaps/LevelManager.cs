using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private WallTile lightCracks;
    [SerializeField] private WallTile heavyCracks;

    private Tilemap wallGrid;
    private Tilemap floorGrid;
    private GridInformation gridData;

    public Tilemap WallGrid { get { return wallGrid; } }
    public Tilemap FloorGrid { get { return floorGrid; } }
    public float TileWidth { get { return GetComponent<Grid>().cellSize.x; } }
    public static LevelManager Instance { get; private set; }

    void Awake() {
        Instance = this;
        wallGrid = transform.GetChild(0).gameObject.GetComponent<Tilemap>();
        floorGrid = transform.GetChild(1).gameObject.GetComponent<Tilemap>();
        gridData = GetComponent<GridInformation>();

        // set up the correct data in each tile
        BoundsInt tiledArea = wallGrid.cellBounds;
        for(int x = tiledArea.xMin; x <= tiledArea.xMax; x++) {
            for(int y = tiledArea.yMin; y <= tiledArea.yMax; y++) {
                Vector3Int position = new Vector3Int(x, y, 0);
                WallTile wall = wallGrid.GetTile<WallTile>(position);
                if(wall != null && wall.Type == WallType.Breakable) {
                    gridData.SetPositionProperty(position, "health", WallGridScript.BREAKABLE_START_HEALTH);
                }
            }
        }
    }

    // handles individual tile health changes
    public void DamageWall(Vector3Int position, int damage) {
        WallTile attackedWall = wallGrid.GetTile<WallTile>(position);
        if(attackedWall == null || attackedWall.Type != WallType.Breakable) {
            return;
        }

        int health = gridData.GetPositionProperty(position, "health", 0);
        health -= damage;
        gridData.SetPositionProperty(position, "health", health);
        if(health <= 0) {
            wallGrid.SetTile(position, null);
        }
        else if(health <= WallGridScript.BREAKABLE_START_HEALTH / 3) {
            if(attackedWall.sprite != heavyCracks.sprite) {
                wallGrid.SetTile(position, heavyCracks);
            }
        }
        else if(health <= WallGridScript.BREAKABLE_START_HEALTH * 2/3) {
            if(attackedWall.sprite != lightCracks.sprite) {
                wallGrid.SetTile(position, lightCracks);
            }
        }
    }
}
