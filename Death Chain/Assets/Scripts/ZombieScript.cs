using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieScript : Enemy
{
    // Start is called before the first frame update
    protected override void ChildStart()
    {
        health = 20;
        controller = new AIController();
    }

    protected override void UpdateAbilities() {

    }
}
