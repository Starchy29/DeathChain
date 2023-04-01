using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a helpful class for tracking durations and delayed events
public class Timer
{
    private static List<Timer> timers = new List<Timer>();
    public static void UpdateAll(float deltaTime) { // must be called once per frame and given the delta time, done in EntityTracker.cs
        for(int i = timers.Count - 1; i >= 0; i--) {
            if(timers[i].ended) {
                timers.RemoveAt(i);
                continue;
            }
            
            timers[i].Update(deltaTime);
            if(timers[i].secondsLeft <= 0) {
                timers.RemoveAt(i);
            }
        }
    }

    private readonly bool repeated; // false: one time use
    private readonly float durationSecs;
    private float secondsLeft;
    private bool ended; // tells the list to remove this

    public delegate void Effect();
    private readonly Effect TickEffect;

    public bool Active { get { return secondsLeft > 0; } }

    public Timer(float durationSecs, bool repeated, Effect tickEffect) {
        this.durationSecs = durationSecs;
        this.repeated = repeated;
        this.TickEffect = tickEffect;
        Restart();
    }

    private void Update(float deltaTime) {
        secondsLeft -= deltaTime;
        if(secondsLeft <= 0) {
            TickEffect();

            if(repeated) {
                secondsLeft += durationSecs;
            }
        }
    }

    public void Restart() {
        secondsLeft = durationSecs;
        if(!timers.Contains(this)) {
            timers.Add(this);
        }
    }

    public void End() {
        ended = true;
    }
}
