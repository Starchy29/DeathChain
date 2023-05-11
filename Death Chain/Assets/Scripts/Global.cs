using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a class for utility code that belongs nowhere else
public static class Global
{
// Extension Methods
    // returns true if the other rectangle is entirely within this one
    public static bool Contains(this Rect self, Rect other)
    {
        return other.xMin >= self.xMin && other.yMin >= self.yMin && other.xMax <= self.xMax && other.yMax <= self.yMax;
    }
}
