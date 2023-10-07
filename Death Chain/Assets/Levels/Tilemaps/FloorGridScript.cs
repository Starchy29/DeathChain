using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorGridScript : MonoBehaviour {
    private List<Enemy> enemiesWithin;
    private Tilemap tiles;

    void Start()
    {
        enemiesWithin = new List<Enemy>();
        tiles = GetComponent<Tilemap>();
    }

    void FixedUpdate() {
        for(int i = 0; i < enemiesWithin.Count; i++) {
            Enemy enemy = enemiesWithin[i];

            // find which floor tiles this enemy is stepping on
            //float radius = enemy.GetComponent<CircleCollider2D>().bounds.extents.x;
            //Vector3Int centerTile = tiles.WorldToCell(enemy.transform.position);

            //List<FloorType> overlappedTiles = new List<FloorType>();
            //for(int x = -1; x <= 1; x++) {
            //    for(int y = -1; y <= 1; y++) {
            //        Vector3 colliderReach = enemy.transform.position + radius * new Vector3(x, y, 0).normalized;
            //        Vector3Int testPos = centerTile + new Vector3Int(x, y, 0);
            //        if(tiles.WorldToCell(colliderReach) == testPos) {
            //            FloorTile tile = tiles.GetTile<FloorTile>(testPos);
            //            if(tile == null) {
            //                overlappedTiles.Add(FloorType.Normal);
            //            } else {
            //                overlappedTiles.Add(tiles.GetTile<FloorTile>(testPos).Type);
            //            }
            //        }
            //    }
            //}



            // provide effects depending on which tiles are stepped on
            List<Vector3Int> overlappedFloors = LevelManager.Instance.GetOverlappedTiles(enemy.gameObject);
            bool onSticky = false;
            bool inPit = overlappedFloors.Count > 0;
            foreach(Vector3Int overlappedFloor in overlappedFloors) {
                FloorTile floor = LevelManager.Instance.FloorGrid.GetTile<FloorTile>(overlappedFloor);
                if(floor == null || floor.Type != FloorType.Pit) {
                    inPit = false;
                }
                if(floor != null && floor.Type == FloorType.Sticky) {
                    onSticky = true;
                }
            }

            if(onSticky) {
                enemy.ApplyStatus(Status.Slow, Time.deltaTime);
            }

            if(inPit) {
                enemiesWithin.RemoveAt(i);
                i--;
                enemy.FallInPit(tiles.GetCellCenterWorld(FindClosestLandTile(enemy.transform.position)));
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Enemy script = collision.gameObject.GetComponent<Enemy>();
        if(script != null && !script.Floating) {
            enemiesWithin.Add(script);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Enemy script = collision.gameObject.GetComponent<Enemy>();
        if(script != null) {
            enemiesWithin.Remove(script);
        }
    }

    private Vector3Int FindClosestLandTile(Vector2 enemyPosition) {
        Tilemap walls = LevelManager.Instance.WallGrid;
        Vector3Int startTile = tiles.WorldToCell(enemyPosition);
        Vector3Int[] directions = new Vector3Int[4] { Vector3Int.up, Vector3Int.down, Vector3Int.right, Vector3Int.left };

        // find open tiles as close to the current one as possible
        int nextRange = 0;
        List<Vector3Int> openSpotsThisRange = new List<Vector3Int>();
        List<Vector3Int> queuedChecks = new List<Vector3Int>();
        List<Vector3Int> currentChecks = new List<Vector3Int>() { startTile };
        bool extraCheck = false; // check one more range after finding open spots in case one has a closer tile
        while((openSpotsThisRange.Count <= 0 && currentChecks.Count > 0) || extraCheck) {
            nextRange++;
            foreach(Vector3Int checkSpot in currentChecks) {
                if(walls.GetTile(checkSpot) != null) {
                    continue;
                }

                FloorTile tile = tiles.GetTile<FloorTile>(checkSpot);
                if(tile == null || tile.Type != FloorType.Pit) {
                    openSpotsThisRange.Add(checkSpot);
                }

                // check adjacent tiles
                foreach(Vector3Int direction in directions) {
                    Vector3Int adjSpot = checkSpot + direction;
                    if(!queuedChecks.Contains(adjSpot) && Mathf.Abs(startTile.x - adjSpot.x) + Mathf.Abs(startTile.y - adjSpot.y) == nextRange) {
                        queuedChecks.Add(adjSpot);
                    }
                }
            }

            currentChecks = queuedChecks;
            queuedChecks = new List<Vector3Int>();

            if(!extraCheck && openSpotsThisRange.Count > 0) {
                extraCheck = true;
            }
            else if(extraCheck) {
                extraCheck = false;
            }
        }

        // determine which open tile to go to
        if(openSpotsThisRange.Count == 0) {
            Debug.Log("error: created a pit surrounded by walls on all sides");
            return startTile;
        }

        Vector3Int closestSpot = openSpotsThisRange[0];
        float closestDistance = Vector3.Distance(enemyPosition, tiles.GetCellCenterWorld(closestSpot));
        for(int i = 1; i < openSpotsThisRange.Count; i++) {
            float distance = Vector3.Distance(enemyPosition, tiles.GetCellCenterWorld(openSpotsThisRange[i]));
            if(distance < closestDistance) {
                closestDistance = distance;
                closestSpot = openSpotsThisRange[i];
            }
        }

        return closestSpot;
    }
}
