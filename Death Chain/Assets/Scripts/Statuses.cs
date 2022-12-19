using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// a data structure that tracks every different status effect for an enemy
public enum Status {
    Poison,
    Slow,
    Freeze,
    Weakness,
    Vulnerability,
    Strength,
    Speed,
    Resistance
}

public class Statuses
{
    private float[] durations; // index matches enum value

    public Statuses() {
        durations = new float[Enum.GetNames(typeof(Status)).Length];
    }

    // ticks down any active statuses. Must be called every frame by its enemy
    public void Update() {
        for(int i = 0; i < durations.Length; i++) {
            durations[i] -= Time.deltaTime;
            if(durations[i] < 0) {
                durations[i] = 0;
            }
        }
    }

    // apply a status effect for some time
    public void Add(Status effect, float duration) {
        durations[(int)effect] += duration;
    }

    // determine if the input status is currently in effect
    public bool HasStatus(Status effect) {
        return durations[(int)effect] > 0;
    }
}
