using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction {
    None,
    Up,
    Down,
    Left,
    Right
}

// a class for utility code that belongs nowhere else
public static class Global
{
// Extension Methods
    // returns true if the other rectangle is entirely within this one
    public static bool Contains(this Rect self, Rect other)
    {
        return other.xMin >= self.xMin && other.yMin >= self.yMin && other.xMax <= self.xMax && other.yMax <= self.yMax;
    }

    // keeps the center the same, but moves each edge outward equal to the input amount. Shrinks from a negative input
    public static Rect MakeExpanded(this Rect rect, float amount) {
        return new Rect(rect.x - amount, rect.y - amount, rect.width + 2*amount, rect.height + 2*amount);
    }
}
