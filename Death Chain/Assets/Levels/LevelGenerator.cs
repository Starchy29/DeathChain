using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] AllOpenZones;
    [SerializeField] private GameObject[] LeftRightOpenZones; // straight halls
    [SerializeField] private GameObject[] DownRightOpenZones; // L bends
    [SerializeField] private GameObject[] DownLeftRightOpenZones; // T junctions

    private LevelManager managerInstance;
    private ZoneType[] zoneTypes;
    private ZoneType[,] zoneGrid;

    private const int LENGTH = 10;
    private const int WIDTH = 5;

    void Start() {
        managerInstance = LevelManager.Instance;
        SetupZoneTypes();
        GenerateLayout();
        SpawnZones();
    }

    private void GenerateLayout() {
        zoneGrid = new ZoneType[LENGTH, WIDTH];
        int emptySpots = LENGTH * WIDTH;

        // construct main path
        int row = LENGTH - 1;
        int col = Random.Range(0, WIDTH);
        while(row > -1) {
            // choose a zone that can go on the current spot
            List<ZoneType> options = DetermineOptions(row, col);
            zoneGrid[row, col] = options[Random.Range(0, options.Count)];
            emptySpots--;

            // choose a direction to move
            List<Vector2Int> directions = new List<Vector2Int>();
            if(zoneGrid[row, col].left && col > 0 && !zoneGrid[row, col - 1].walkable) {
                directions.Add(Vector2Int.left);
            }
            if(zoneGrid[row, col].right && col < WIDTH - 1 && !zoneGrid[row, col + 1].walkable) {
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
        }

        // use wave function collapse to grow off of the main path
        while(emptySpots > 0) {
            List<Vector2Int> candidiates = new List<Vector2Int>();
            int leastOptions = zoneTypes.Length;

            // find which unplaced tiles next to an existing tile have the least possible options
            for(row = 0; row < LENGTH; row++) {
                for(col = 0; col < WIDTH; col++) {
                    if(zoneGrid[row, col].walkable || !HasAdjacentZone(row, col)) {
                        continue;
                    }

                    List<ZoneType> options = DetermineOptions(row, col);
                    if(options.Count == 0) {
                        continue;
                    }

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

    private void SpawnZones() {
        int tempWidth = 2;

        for(int row = 0; row < LENGTH; row++) {
            for(int col = 0; col < WIDTH; col++) {
                if(!zoneGrid[row, col].walkable) {
                    continue;
                }

                // TEMP PRINT INFO
                Debug.Log($"({col * tempWidth},{(LENGTH - 1 - row) * tempWidth}), up: {zoneGrid[row, col].up}, down: {zoneGrid[row, col].down}, left: {zoneGrid[row, col].left}, right: {zoneGrid[row, col].right}");

                GameObject[] prefabList = AllOpenZones;
                switch(zoneGrid[row, col].DetermineShape()) {
                    case ZoneShape.Plus:
                        prefabList = AllOpenZones;
                        break;

                    case ZoneShape.L_Bend:
                        prefabList = DownRightOpenZones;
                        break;

                    case ZoneShape.StraightHall:
                        prefabList = LeftRightOpenZones;
                        break;

                    case ZoneShape.T_Junction:
                        prefabList = DownLeftRightOpenZones;
                        break;
                }

                GameObject addedZone = Instantiate(prefabList[Random.Range(0, prefabList.Length)]);
                addedZone.transform.position = new Vector3(col * tempWidth, (LENGTH-1 - row) * tempWidth);
                addedZone.transform.rotation = Quaternion.Euler(0, 0, zoneGrid[row, col].DetermineRotation());
            }
        }
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
        bool leftValid = col - 1 < 0 || !zoneGrid[row, col - 1].walkable || (zone.left == zoneGrid[row, col - 1].right);
        bool rightValid = col + 1 > WIDTH - 1 || !zoneGrid[row, col + 1].walkable || (zone.right == zoneGrid[row, col + 1].left);
        bool upValid = row - 1 < 0 || !zoneGrid[row - 1, col].walkable || (zone.up == zoneGrid[row - 1, col].down);
        bool downValid = row + 1 > LENGTH - 1 || !zoneGrid[row + 1, col].walkable || (zone.down == zoneGrid[row + 1, col].up);
        return upValid && downValid && leftValid && rightValid;
    }

    private bool HasAdjacentZone(int row, int col) {
        return row - 1 >= 0 && zoneGrid[row - 1, col].walkable ||
            row + 1 <= LENGTH - 1 && zoneGrid[row + 1, col].walkable ||
            col - 1 >= 0 && zoneGrid[row, col - 1].walkable ||
            col + 1 <= WIDTH - 1 && zoneGrid[row, col + 1].walkable;
    }

    private void SetupZoneTypes() {
        zoneTypes = new ZoneType[11] {
            new ZoneType { walkable = true, up = true, down = true, left = true, right = true }, // + cross

            // straight halls
            new ZoneType { walkable = true, up = true, down = true, left = false, right = false },
            new ZoneType { walkable = true, up = false, down = false, left = true, right = true },

            // L bends
            new ZoneType { walkable = true, up = true, down = false, left = true, right = false },
            new ZoneType { walkable = true, up = false, down = true, left = true, right = false },
            new ZoneType { walkable = true, up = false, down = true, left = false, right = true },
            new ZoneType { walkable = true, up = true, down = false, left = false, right = true },

            // T junctions
            new ZoneType { walkable = true, up = false, down = true, left = true, right = true },
            new ZoneType { walkable = true, up = true, down = false, left = true, right = true },
            new ZoneType { walkable = true, up = true, down = true, left = false, right = true },
            new ZoneType { walkable = true, up = true, down = true, left = true, right = false }
        };
    }
}

struct ZoneType {
    public bool walkable; // whether or not it is an open area for gameplay

    // define which directions are open for a connection
    public bool up;
    public bool down;
    public bool left;
    public bool right;

    public int OpeningCount { get { return (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0); } }

    public ZoneShape DetermineShape() {
        int openings = OpeningCount;

        if(openings == 4) {
            return ZoneShape.Plus;
        }

        if(openings == 3) {
            return ZoneShape.T_Junction;
        }

        if(up && down || left && right) {
            return ZoneShape.StraightHall;
        }

        return ZoneShape.L_Bend;
    }
    
    public float DetermineRotation() {
        switch(DetermineShape()) {
            case ZoneShape.StraightHall:
                if(up && down) {
                    return 90f;
                }
                break;

            case ZoneShape.L_Bend:
                if(down && left) {
                    return -90;
                }
                if(right && up) {
                    return 90;
                }
                if(up && left) {
                    return 180;
                }
                break;

            case ZoneShape.T_Junction:
                if(!down) {
                    return 180f;
                }
                if(!left) {
                    return 90;
                }
                if(!right) {
                    return -90;
                }
                break;
        }

        return 0;
    }
}

enum ZoneShape {
    Plus,
    L_Bend,
    StraightHall,
    T_Junction
}