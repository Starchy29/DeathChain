using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// easily shows information for debugging
public class DebugDisplay : MonoBehaviour
{
    public GameObject DebugRect;
    public GameObject DebugDot;

    private static DebugDisplay instance;
    public static DebugDisplay Instance { get { return instance; } }

    private Dictionary<string, GameObject> dots = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public void PlaceDot(Vector2 location, string identifier) {
        if(!dots.ContainsKey(identifier)) {
            dots[identifier] = Instantiate(DebugDot);
        }
        dots[identifier].transform.position = location;
    }

    public void DisplayRect(Rect area) {
        GameObject newRect = Instantiate(DebugRect);
        newRect.transform.position = area.center;
        newRect.transform.localScale = area.size;
    }
}
