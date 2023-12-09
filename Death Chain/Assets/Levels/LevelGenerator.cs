using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject EndGateZone;
    [SerializeField] private GameObject StartingZone;
    [SerializeField] private GameObject[] AllOpenZones;
    [SerializeField] private GameObject[] LeftRightOpenZones; // straight halls
    [SerializeField] private GameObject[] DownRightOpenZones; // L bends
    [SerializeField] private GameObject[] TopClosedZones; // T junctions
    [SerializeField] private GameObject[] WallZones;

    private LevelManager managerInstance;
    private ZoneType[] zoneTypes;
    private ZoneType[,] zoneGrid;
    private Vector2Int startZone;
    private Vector2Int endZone;

    private const int LENGTH = 14;
    private const int WIDTH = 8;

    void Start() {
        managerInstance = LevelManager.Instance;

        zoneTypes = new ZoneType[11] {
            new ZoneType { placed = true, up = true, down = true, left = true, right = true }, // + cross

            // straight halls
            new ZoneType { placed = true, up = true, down = true, left = false, right = false },
            new ZoneType { placed = true, up = false, down = false, left = true, right = true },

            // L bends
            new ZoneType { placed = true, up = true, down = false, left = true, right = false },
            new ZoneType { placed = true, up = false, down = true, left = true, right = false },
            new ZoneType { placed = true, up = false, down = true, left = false, right = true },
            new ZoneType { placed = true, up = true, down = false, left = false, right = true },

            // T junctions
            new ZoneType { placed = true, up = false, down = true, left = true, right = true },
            new ZoneType { placed = true, up = true, down = false, left = true, right = true },
            new ZoneType { placed = true, up = true, down = true, left = false, right = true },
            new ZoneType { placed = true, up = true, down = true, left = true, right = false }
        };

        GenerateLayout();
        SpawnZones();
    }

    private void GenerateLayout() {
        zoneGrid = new ZoneType[LENGTH, WIDTH];
        int emptySpots = LENGTH * WIDTH;

        // construct main path
        int row = LENGTH - 1;
        int col = Random.Range(0, WIDTH);
        startZone = new Vector2Int(row, col);
        while(row > -1) {
            // choose a zone that can go on the current spot
            zoneGrid[row, col] = ChooseZone(DetermineOptions(row, col));
            emptySpots--;

            // choose a direction to move
            List<Vector2Int> directions = new List<Vector2Int>();
            if(zoneGrid[row, col].left && col > 0 && !zoneGrid[row, col - 1].placed) {
                directions.Add(Vector2Int.left);
            }
            if(zoneGrid[row, col].right && col < WIDTH - 1 && !zoneGrid[row, col + 1].placed) {
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

        endZone = new Vector2Int(row, col);
        zoneGrid[startZone.x, startZone.y].down = true; // make sure starting zone can connect down
        zoneGrid[endZone.x + 1, endZone.y].up = true; // make sure ending zone can connect up

        // fill in walls in spots not adjacent to the main path
        ZoneType solidWall = new ZoneType { placed = true, up = false, down = false, left = false, right = false };
        for(row = 0; row < LENGTH; row++) {
            for(col = 0; col < WIDTH; col++) {
                if(zoneGrid[row, col].placed) {
                    col++; // skip the next tile because it can't be valid
                    continue;
                }

                if(!HasAdjacentPlayZone(row, col)) {
                    zoneGrid[row, col] = solidWall;
                    emptySpots--;
                }
            }
        }

        // use wave function collapse to grow off of the main path
        while(emptySpots > 0) {
            List<Vector2Int> candidiates = new List<Vector2Int>();
            int leastOptions = zoneTypes.Length;

            // find which unplaced tiles next to an existing tile have the least possible options
            for(row = 0; row < LENGTH; row++) {
                for(col = 0; col < WIDTH; col++) {
                    if(zoneGrid[row, col].placed) {
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
            List<ZoneType> zoneOptions = DetermineOptions(chosenSpot.x, chosenSpot.y);
            if(zoneOptions.Count > 0) {
                zoneGrid[chosenSpot.x, chosenSpot.y] = ChooseZone(zoneOptions);
            } else {
                zoneGrid[chosenSpot.x, chosenSpot.y] = solidWall;
            }
            emptySpots--;
        }
    }

    private void SpawnZones() {
        const int ZONE_WIDTH = 10; // number of tiles wide and tall
        const float TILE_WIDTH = 1.5f; // world space width of 1 tile
        Dictionary<ZoneShape, GameObject[]> shapeToPrefabList = new Dictionary<ZoneShape, GameObject[]> {
            { ZoneShape.Plus, AllOpenZones },
            { ZoneShape.L_Bend, DownRightOpenZones },
            { ZoneShape.StraightHall, LeftRightOpenZones },
            { ZoneShape.T_Junction, TopClosedZones },
            { ZoneShape.Wall, WallZones }
        };

        for(int row = -1; row <= LENGTH; row++) {
            for(int col = -1; col <= WIDTH; col++) {
                ZoneShape shape;
                float rotation;
                if(row == -1 || col == -1 || row == LENGTH || col == WIDTH) {
                    shape = ZoneShape.Wall;
                    rotation = 0;
                } else {
                    shape = zoneGrid[row, col].DetermineShape();
                    rotation = zoneGrid[row, col].DetermineRotation();
                }
  
                if(shape == ZoneShape.Wall && !HasAdjacentPlayZone(row, col)) {
                    continue;
                }

                GameObject[] prefabList = shapeToPrefabList[shape];
                GameObject addedZone = Instantiate(prefabList[Random.Range(0, prefabList.Length)]);

                // move to the correct position and orientation
                addedZone.transform.position = new Vector3(col * ZONE_WIDTH * TILE_WIDTH, (LENGTH - 1 - row) * ZONE_WIDTH * TILE_WIDTH, 0);

                // copy the tiles into the main tilemap
                // (test world positions from each tilemap)
                addedZone.transform.rotation = Quaternion.Euler(0, 0, rotation);

                // unpack child game objects and delete the container
            }
        }

        // place the start and end points
    }

    // determines if any orthogonally adjacent tiles are part of the level the player walks through
    private bool HasAdjacentPlayZone(int row, int col) {
        bool upZone = IsInGrid(row + 1, col) && zoneGrid[row + 1, col].placed && zoneGrid[row + 1, col].OpeningCount > 0;
        bool downZone = IsInGrid(row - 1, col) && zoneGrid[row - 1, col].placed && zoneGrid[row - 1, col].OpeningCount > 0;
        bool rightZone = IsInGrid(row, col + 1) && zoneGrid[row, col + 1].placed && zoneGrid[row, col + 1].OpeningCount > 0;
        bool leftZone = IsInGrid(row, col - 1) && zoneGrid[row, col - 1].placed && zoneGrid[row, col - 1].OpeningCount > 0;
        return upZone || downZone || leftZone || rightZone;
    }

    private bool IsInGrid(int row, int col) {
        return row >= 0 && row < LENGTH && col >= 0 && col < WIDTH;
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
        bool leftValid = col - 1 < 0 || !zoneGrid[row, col - 1].placed || (zone.left == zoneGrid[row, col - 1].right);
        bool rightValid = col + 1 > WIDTH - 1 || !zoneGrid[row, col + 1].placed || (zone.right == zoneGrid[row, col + 1].left);
        bool upValid = row - 1 < 0 || !zoneGrid[row - 1, col].placed || (zone.up == zoneGrid[row - 1, col].down);
        bool downValid = row + 1 > LENGTH - 1 || !zoneGrid[row + 1, col].placed || (zone.down == zoneGrid[row + 1, col].up);
        return upValid && downValid && leftValid && rightValid;
    }

    private ZoneType ChooseZone(List<ZoneType> options) {
        int[] weights = new int[options.Count];
        int totalWeight = 0;
        for(int i = 0; i < weights.Length; i++) {
            int weight = options[i].OpeningCount - 1;
            weights[i] = weight;
            totalWeight += weight;
        }

        int randomChosen = Random.Range(1, totalWeight);
        int currentIndex = -1;
        while(randomChosen > 0) {
            currentIndex++;
            randomChosen -= weights[currentIndex];
        }

        return options[currentIndex];
    }
}

struct ZoneType {
    public bool placed; // whether or not this spot in the grid has had a zone type chosen yet

    // define which directions are open for a connection
    public bool up;
    public bool down;
    public bool left;
    public bool right;

    public int OpeningCount { get { return (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0); } }

    public ZoneShape DetermineShape() {
        int openings = OpeningCount;

        if(openings == 0) {
            return ZoneShape.Wall;
        }

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
    T_Junction,
    Wall
}