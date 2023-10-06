using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for abilities that affect enemies, tracks the user
public abstract class Ability : MonoBehaviour
{
    private bool isAlly;
    protected Enemy user;

    public virtual Enemy User { 
        get { return user; } // might be null if the user dies
        set { // must be set by the ability user on creation
            user = value;
            isAlly = user.IsAlly;
            gameObject.layer = LayerMask.NameToLayer((isAlly ? "AllyAttack" : "EnemyAttack"));
        }
    }

    public bool IsAlly {
        get { return isAlly; }
    }

    public virtual void SetDirection(Vector2 direction) { }
}
