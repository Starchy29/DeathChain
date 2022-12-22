using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// the blast from the blight that poisons instead of dealing damage
public class PoisonBlast : BlastZone
{
    protected override void OnEnemyCollision(Enemy hitEnemy) {
        hitEnemy.ApplyStatus(Status.Poison, 3);
    }
}
