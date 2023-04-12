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
    private GameObject[] particles; // active particle effects
    private GameObject user;

    public Statuses(GameObject user) {
        durations = new float[Enum.GetNames(typeof(Status)).Length];
        particles = new GameObject[durations.Length];
        this.user = user;
    }

    // ticks down any active statuses. Must be called every frame by its enemy
    public void Update() {
        for(int i = 0; i < durations.Length; i++) {
            durations[i] -= Time.deltaTime;
            if(durations[i] < 0) {
                durations[i] = 0;
                MonoBehaviour.Destroy(particles[i]);
                particles[i] = null;
            }
        }
    }

    // apply a status effect for some time
    public void Add(Status effect, float duration) {
        int index = (int)effect;
        durations[index] += duration;

        if(particles[index] == null) {
            particles[index] = MonoBehaviour.Instantiate(EntityTracker.Instance.statusParticlePrefabs[index]);
            particles[index].transform.SetParent(user.transform);
            particles[index].transform.localPosition = Vector3.zero;
        }
    }

    // determine if the input status is currently in effect
    public bool HasStatus(Status effect) {
        return durations[(int)effect] > 0;
    }

    public void ClearPoison() {
        durations[(int)Status.Poison] = 0;
        if(particles[(int)Status.Poison] != null) {
            MonoBehaviour.Destroy(particles[(int)Status.Poison]);
            particles[(int)Status.Poison] = null;
        }
    }
}
