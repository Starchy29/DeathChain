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

public struct Corner {
    private static List<Corner> loop = new List<Corner>() {
        new Corner(Direction.Left, Direction.Up),
        new Corner(Direction.Right, Direction.Up),
        new Corner(Direction.Right, Direction.Down),
        new Corner(Direction.Left, Direction.Down)
    };

    public Direction Horizontal;
    public Direction Vertical;

    public Corner(Direction horizontal, Direction vertical) {
        Horizontal = horizontal;
        Vertical = vertical;

        if(horizontal == Direction.Up || horizontal == Direction.Down || vertical == Direction.Left || vertical == Direction.Right) {
            throw new ArgumentException("Input a vertical value as the horizontal or vice versa.");
        }
    }

    public Corner GetClockwise() {
        int index = loop.IndexOf(this) + 1;
        if(index >= loop.Count) {
            index = 0;
        }

        return loop[index];
    }

    public Corner GetCounterClockwise() {
        int index = loop.IndexOf(this) - 1;
        if(index < 0) {
            index = loop.Count - 1;
        }

        return loop[index];
    }
}

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
}
