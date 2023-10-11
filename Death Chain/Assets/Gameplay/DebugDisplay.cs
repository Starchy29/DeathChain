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
    private List<GameObject> rects = new List<GameObject>();

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
        if(area.width == 0) {
            area = new Rect(area.xMin - 0.2f, area.yMin, 0.4f, area.height);
        }
        if(area.height == 0) {
            area = new Rect(area.xMin, area.yMin - 0.2f, area.width, 0.4f);
        }

        GameObject newRect = Instantiate(DebugRect);
        rects.Add(newRect);
        newRect.transform.position = area.center;
        newRect.transform.localScale = area.size;
    }

    public void ClearRects() {
        foreach(GameObject rect in rects) {
            Destroy(rect);
        }
        rects.Clear();
    }
}
