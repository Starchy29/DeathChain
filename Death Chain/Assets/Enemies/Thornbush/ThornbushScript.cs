using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornbushScript : Enemy
{
    private bool countering;

    protected override void ChildStart() {
        controller = new AIController(gameObject, AIMode.Wander, AIMode.Flee, 5.5f);
    }

    protected override void UpdateAbilities() {
        if(UseAbility(0)) {
            // triple thorn shot
        }
        else if(UseAbility(1)) {
            // trap counter
        }
    }

    public override void TakeDamage(int amount, bool ignoreStatus = false) {
        base.TakeDamage(amount, ignoreStatus);
    }

    public override void AIUpdate(AIController controller) {
        
    }
}
