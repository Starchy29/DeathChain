using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    private LevelManager managerInstance;
    private ZoneType[] zoneTypes;
    private ZoneType[,] zoneGrid;

    private const int LENGTH = 10;
    private const int WIDTH = 5;

    void Start() {
        managerInstance = LevelManager.Instance;
        SetupZoneTypes();
        GenerateLayout();
    }

    private void GenerateLayout() {
        zoneGrid = new ZoneType[LENGTH, WIDTH];
        int emptySpots = LENGTH * WIDTH;

        // construct main path
        int row = LENGTH - 1;
        int col = WIDTH / 2;
        while(row > 0) {
            // choose a direction
            List<Vector2Int> directions = new List<Vector2Int>();
            if(zoneGrid[row, col].left && row > 0 && !zoneGrid[row - 1, col].playable) {
                directions.Add(Vector2Int.left);
            }
            if(zoneGrid[row, col].right && row < WIDTH - 1 && !zoneGrid[row + 1, col].playable) {
                directions.Add(Vector2Int.right);
            }
            if(zoneGrid[row, col].up) {
                directions.Add(new Vector2Int(0, -1));
            }

            if(directions.Count == 0) {
                // readjust this zone to be able to go up
                zoneGrid[row, col].up = true;
                directions.Add(new Vector2Int(0, -1));
            }

            Vector2Int direction = directions[Random.Range(0, directions.Count)];
            row += direction.y;
            col += direction.x;

            // choose a zone that can go on that spot
            List<ZoneType> options = DetermineOptions(row, col);
            zoneGrid[row, col] = options[Random.Range(0, options.Count)];
            emptySpots--;
        }

        // use wave function collapse to grow off of the main path
        while(emptySpots > 0) {
            List<Vector2Int> candidiates = new List<Vector2Int>();
            int leastOptions = zoneTypes.Length;

            // find which unplaced tiles next to an existing tile have the least possible options
            for(row = 0; row < LENGTH; row++) {
                for(col = 0; col < WIDTH; col++) {
                    if(zoneGrid[row, col].playable || !HasAdjacentZone(row, col)) {
                        continue;
                    }

                    List<ZoneType> options = DetermineOptions(row, col);
                    if(options.Count < leastOptions) {
                        candidiates.Clear();
                        leastOptions = options.Count;
                    }
                    if(options.Count == leastOptions) {
                        candidiates.Add(new Vector2Int(row, col));
                    }
                }
            }

            if(candidiates.Count <= 0) {
                // end when no more tiles can be placed
                break;
            }

            // place a random tile in the chosen spot
            Vector2Int chosenSpot = candidiates[Random.Range(0, candidiates.Count)];
            List<ZoneType> chosenOptions = DetermineOptions(chosenSpot.x, chosenSpot.y);
            zoneGrid[chosenSpot.x, chosenSpot.y] = chosenOptions[Random.Range(0, chosenOptions.Count)];
        }
    }

    private void SetTiles() {

    }

    private List<ZoneType> DetermineOptions(int row, int col) {
        List<ZoneType> options = new List<ZoneType>();
        foreach(ZoneType zone in zoneTypes) {
            if(IsValidZone(zone, row, col)) {
                options.Add(zone);
            }
        }

        return options;
    }

    private bool IsValidZone(ZoneType zone, int row, int col) {
        bool upValid = col - 1 >= 0 || !zoneGrid[row, col - 1].playable || (zoneGrid[row, col].up == zoneGrid[row, col - 1].down);
        bool downValid = col + 1 <= WIDTH - 1 || !zoneGrid[row, col + 1].playable || (zoneGrid[row, col].down == zoneGrid[row, col + 1].up);
        bool leftValid = row - 1 >= 0 || !zoneGrid[row - 1, col].playable || (zoneGrid[row, col].left == zoneGrid[row - 1, col].right);
        bool rightValid = row + 1 <= LENGTH - 1 || !zoneGrid[row + 1, col].playable || (zoneGrid[row, col].right == zoneGrid[row + 1, col].left);
        return upValid && downValid && leftValid && rightValid;
    }

    private bool HasAdjacentZone(int row, int col) {
        return row - 1 >= 0 && zoneGrid[row - 1, col].playable ||
            row + 1 <= LENGTH - 1 && zoneGrid[row + 1, col].playable ||
            col - 1 >= 0 && zoneGrid[row, col - 1].playable ||
            col + 1 <= WIDTH - 1 && zoneGrid[row, col + 1].playable;
    }

    private void SetupZoneTypes() {
        zoneTypes = new ZoneType[11] {
            new ZoneType {playable = true, up = false, down = false, left = false, right = false }, // + cross

            // straight halls
            new ZoneType {playable = true, up = true, down = true, left = false, right = false },
            new ZoneType {playable = true, up = false, down = false, left = true, right = true },

            // L bends
            new ZoneType {playable = true, up = true, down = false, left = true, right = false },
            new ZoneType {playable = true, up = false, down = true, left = true, right = false },
            new ZoneType {playable = true, up = false, down = true, left = false, right = true },
            new ZoneType {playable = true, up = true, down = false, left = false, right = true },

            // T junctions
            new ZoneType {playable = true, up = false, down = true, left = true, right = true },
            new ZoneType {playable = true, up = true, down = false, left = true, right = true },
            new ZoneType {playable = true, up = true, down = true, left = false, right = true },
            new ZoneType {playable = true, up = true, down = true, left = true, right = false }
        };
    }
}

struct ZoneType {
    public bool playable; // whether or not it is an open area for gameplay

    // define which directions are open for a connection
    public bool up;
    public bool down;
    public bool left;
    public bool right;
}
