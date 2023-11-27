using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum Direction {
    None,
    Up,
    Down,
    Left,
    Right
}

public struct Circle {
    public Vector3 Center;
    public float Radius;

    public Circle(Vector3 center, float radius) {
        Center = center;
        Radius = radius;
    }
}

delegate void VoidFunc();

// a class for utility code that belongs nowhere else
public static class Global
{
    public static Dictionary<Direction, Vector2> DirectionToVector = new Dictionary<Direction, Vector2>() {
        { Direction.Up, Vector2.up },
        { Direction.Down, Vector2.down },
        { Direction.Left, Vector2.left },
        { Direction.Right, Vector2.right }
    };

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

    // determines if this point lies on the line segment between the other vectors
    public static bool IsBetween(this Vector2 test, Vector2 start, Vector2 end) {
        if(test == start || test == end) {
            return true;
        }

        bool onLine = Vector2.Dot((test - start).normalized, (end - start).normalized) == 1;
        bool between = Vector2.Dot(start - test, end - test) < 0;
        return onLine && between;
    }
}
