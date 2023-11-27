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
    private List<GameObject> backstageEnemies; // enemies that are inactive until they become on screen

    public Tilemap WallGrid { get { return wallGrid; } }
    public Tilemap FloorGrid { get { return floorGrid; } }
    public GridInformation GridData { get { return gridData; } }
    public float TileWidth { get { return GetComponent<Grid>().cellSize.x * transform.localScale.x; } }
    public static LevelManager Instance { get; private set; }

    void Awake() {
        Instance = this;
        wallGrid = transform.GetChild(0).gameObject.GetComponent<Tilemap>();
        floorGrid = transform.GetChild(1).gameObject.GetComponent<Tilemap>();
        gridData = GetComponent<GridInformation>();

        Timer.ClearTimers();

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

        // start enemies offscreen as inactive
        backstageEnemies = new List<GameObject>();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject enemy in enemies) {
            if(enemy.GetComponent<PlayerGhost>() != null) {
                continue;
            }

            enemy.SetActive(false);
            backstageEnemies.Add(enemy);
        }
    }

    void Update() {
        Timer.UpdateAll(Time.deltaTime);

        // check for inactive enemies coming on screen
        if(CameraScript.Instance == null) {
            return;
        }

        Rect cameraArea = CameraScript.Instance.VisibleArea.MakeExpanded(2);
        for(int i = 0; i < backstageEnemies.Count; i++) {
            if(cameraArea.Contains(backstageEnemies[i].transform.position)) {
                backstageEnemies[i].SetActive(true);
                backstageEnemies.RemoveAt(i);
                i--;
            }
        }
    }

    // handles individual tile health changes
    public void DamageWall(Vector3Int position, int damage) {
        WallTile attackedWall = wallGrid.GetTile<WallTile>(position);

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

    // returns all tile positions that the input circle overlaps, regardless of if there is a tile there or not
    public List<Vector3Int> GetOverlappedTiles(Circle circle) {
        // determine which tiles could possibly be overlapped based on the position and radius
        List<Vector3Int> overlappedTiles = new List<Vector3Int>();
        Vector3Int centerTile = wallGrid.WorldToCell(circle.Center);
        int cellRange = (int)Mathf.Ceil(circle.Radius / wallGrid.cellSize.x);

        // check which tiles are within range
        for (int x = -cellRange; x <= cellRange; x++) {
            for(int y = -cellRange; y <= cellRange; y++) {
                Vector3Int testPos = centerTile + new Vector3Int(x, y, 0);
                Vector3 tileCenter = wallGrid.GetCellCenterWorld(testPos);
                Vector3 toTile = tileCenter - circle.Center;
                Vector3 closestPoint = circle.Center + circle.Radius * toTile.normalized;
                if(toTile.magnitude < circle.Radius || wallGrid.WorldToCell(closestPoint) == testPos) {
                    overlappedTiles.Add(testPos);
                }
            }
        }

        return overlappedTiles;
    }

    // overload for entites that have a circle collider, uses the collider as the test circle 
    public List<Vector3Int> GetOverlappedTiles(GameObject circularEntity) {
        Circle circle = new Circle(circularEntity.transform.position, circularEntity.GetComponent<CircleCollider2D>().radius * circularEntity.transform.localScale.x + 0.1f); // add a little to the radius for small objects
        return GetOverlappedTiles(circle);
    }
}
