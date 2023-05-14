using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// should be attached to an empty game object whose children are all squares. The squares together define the area of the pit. Empty must have scale (1, 1, 1)
public class PitScript : MonoBehaviour
{
    private List<Rect> zones;
    private const float EDGE_BUFFER = 0.5f; // edges might not line up exactly. As long as they are this distance from each other, they are considered lining up 

    private void Start()
    {
        // these should be the values in the empty parent game object. There will be visible differences if it is set up incorrectly in the inspector
        if(transform.position != Vector3.zero || transform.localScale != Vector3.one) {
            transform.position = new Vector3(0, 0, 0);
            transform.localScale = new Vector3(1, 1, 1);
            Debug.Log("improperly changed the transform of the empty parent of a pit");
        }

        zones = new List<Rect>();

        // create rectangles from child squares
        for(int i = 0; i < transform.childCount; i++) {
            Transform rectTransform = transform.GetChild(i);
            Vector2 center = rectTransform.position;
            Vector2 dims = rectTransform.localScale; // requires parent scale (1, 1, 1)
            zones.Add(new Rect(center - dims/2, dims));
        }

        // add rectangles that overlap edges to prevent characters from walking across
        List<Rect> additionalZones = new List<Rect>();
        for(int i = 0; i < zones.Count; i++) {
            for(int j = i + 1; j < zones.Count; j++) {
                // check if rects have a shared edge
                if(Mathf.Abs(zones[i].xMin - zones[j].xMax) <= EDGE_BUFFER || Mathf.Abs(zones[i].xMax - zones[j].xMin) <= EDGE_BUFFER) {
                    // add overlapping rectangle
                    float leftEdge = Mathf.Min(zones[i].xMin, zones[j].xMin);
                    float rightEdge = Mathf.Max(zones[i].xMax, zones[j].xMax);
                    float topEdge = Mathf.Min(zones[i].yMax, zones[j].yMax);
                    float bottomEdge = Mathf.Max(zones[i].yMin, zones[j].yMin);
                    additionalZones.Add(new Rect(leftEdge, bottomEdge, rightEdge - leftEdge, topEdge - bottomEdge));
                }
                else if(Mathf.Abs(zones[i].yMin - zones[j].yMax) <= EDGE_BUFFER || Mathf.Abs(zones[i].yMax - zones[j].yMin) <= EDGE_BUFFER) {
                    // add overlapping rectangle
                    float topEdge = Mathf.Max(zones[i].yMax, zones[j].yMax);
                    float bottomEdge = Mathf.Min(zones[i].yMin, zones[j].yMin);
                    float leftEdge = Mathf.Max(zones[i].xMin, zones[j].xMin);
                    float rightEdge = Mathf.Min(zones[i].xMax, zones[j].xMax);
                    additionalZones.Add(new Rect(leftEdge, bottomEdge, rightEdge - leftEdge, topEdge - bottomEdge));
                }
            }
        }
        zones.AddRange(additionalZones);

        // remove any rectangles that are wholly contained by another
        for(int i = zones.Count - 1; i >= 0; i--) {
            for(int j = i - 1; j >= 0; j--) {
                if(zones[j].Contains(zones[i])) {
                    zones.RemoveAt(i);
                    break;
                }
                if(zones[i].Contains(zones[j])) {
                    zones.RemoveAt(j);
                    i--;
                }
            }
        }
    }

    void Update()
    {
        // check each enemy to see if it should fall in this pit
        List<GameObject> enemies = EntityTracker.Instance.Enemies;
        foreach(GameObject enemy in enemies) {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if(enemyScript.Floating || enemyScript.CurrentState != Enemy.State.Normal) {
                continue;
            }

            // determine if the enemy is inside this pit
            bool inPit = false;
            Vector3 pos = enemy.transform.position;
            float radius = enemy.GetComponent<Enemy>().CollisionRadius;
            Rect hitbox = new Rect(pos.x - radius, pos.y - radius, 2*radius, 2*radius);
            foreach(Rect zone in zones) {
                if(zone.Contains(hitbox)) {
                    inPit = true;
                    break;
                }
            }

            if(!inPit) {
                continue;
            }

            // determine where to place the enemy back on land
            foreach(Rect zone in zones) {
                if(!zone.Overlaps(hitbox)) {
                    continue;
                }

                // try shifting the character in a cardinal direction
                Vector2 left = new Vector2(zone.xMin - radius, hitbox.center.y);
                Vector2 right = new Vector2(zone.xMax + radius, hitbox.center.y);
                Vector2 top = new Vector2(hitbox.center.x, zone.yMax + radius);
                Vector2 bottom = new Vector2(hitbox.center.x, zone.yMin - radius);
                List<Vector2> standardSpots = new List<Vector2>() { left, right, top, bottom };

                // ignore directions that are adjacent to a border wall
                for(int i = standardSpots.Count - 1; i >= 0; i--) {
                    Vector2 edgeSpot = standardSpots[i];
                    Rect testBox = new Rect(edgeSpot.x - radius, edgeSpot.y - radius, 2 * radius, 2 * radius);
                    foreach(GameObject wall in EntityTracker.Instance.Walls) {
                        if(wall.layer == LayerMask.NameToLayer("Border") && wall.GetComponent<WallScript>().Area.Overlaps(testBox)) {
                            standardSpots.RemoveAt(i);
                        }
                    }
                }

                // if an edge spot would place the character in a wall, find a spot next to the wall to place it
                List<Vector2> potentialSpots = new List<Vector2>();
                foreach(Vector2 edgeSpot in standardSpots) {
                    Rect testBox = new Rect(edgeSpot.x - radius, edgeSpot.y - radius, 2*radius, 2*radius);
                    bool horizontalSpot = (edgeSpot - hitbox.center).x != 0; // false: above or below

                    Rect lesserRect = testBox;
                    bool intersectsWall = true;
                    while(intersectsWall) {
                        intersectsWall = false;
                        Rect? intersectingWall = FindIntersectingWall(lesserRect);
                        if(intersectingWall.HasValue) {
                            intersectsWall = true;
                            if(horizontalSpot) {
                                lesserRect.center = new Vector2(lesserRect.center.x, intersectingWall.Value.yMin - radius - 0.001f);
                            } else {
                                lesserRect.center = new Vector2(intersectingWall.Value.xMin - radius - 0.001f, lesserRect.center.y);
                            }
                        }
                    }

                    Rect greaterRect = testBox;
                    intersectsWall = true;
                    while(intersectsWall) {
                        intersectsWall = false;
                        Rect? intersectingWall = FindIntersectingWall(greaterRect);
                        if(intersectingWall.HasValue) {
                            intersectsWall = true;
                            if(horizontalSpot) {
                                greaterRect.center = new Vector2(greaterRect.center.x, intersectingWall.Value.yMax + radius + 0.001f);
                            } else {
                                greaterRect.center = new Vector2(intersectingWall.Value.xMax + radius + 0.001f, greaterRect.center.y);
                            }
                        }
                    }

                    // don't use spots over this pit
                    bool lesserOverPit = false;
                    bool greaterOverPit = false;
                    foreach(Rect pitArea in zones) {
                        if(pitArea.Contains(lesserRect.center)) {
                            lesserOverPit = true;
                        }
                        if(pitArea.Contains(greaterRect.center)) {
                            greaterOverPit = true;
                        }
                    }
                    if(!lesserOverPit) {
                        potentialSpots.Add(lesserRect.center);
                    }
                    if(!greaterOverPit && lesserRect.center != greaterRect.center) {
                        potentialSpots.Add(greaterRect.center);
                    }
                }

                // find the closest spot of all valid locations
                potentialSpots.Sort((Vector2 first, Vector2 last) => {
                    float distanceComparison = Vector2.Distance(first, hitbox.center) - Vector2.Distance(last, hitbox.center);
                    return (int)(distanceComparison * 100); // multiply by 100 for accurate comparison even with int cast
                });
                Vector2 bestSpot = potentialSpots[0];
                hitbox = new Rect(bestSpot.x - radius, bestSpot.y - radius, 2*radius, 2*radius);
            }

            enemyScript.FallInPit(hitbox.center);
        }
    }

    private Rect? FindIntersectingWall(Rect testBox) {
        foreach(GameObject wall in EntityTracker.Instance.Walls) {
            Rect wallArea = wall.GetComponent<WallScript>().Area;
            if(wallArea.Overlaps(testBox)) {
                return wallArea;
            }
        }

        return null;
    }
}
