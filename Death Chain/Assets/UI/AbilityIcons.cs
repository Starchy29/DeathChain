using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityIcons : MonoBehaviour
{
    [SerializeField] public Sprite PossessIcon;
    [SerializeField] private Sprite SlashIcon;
    [SerializeField] private Sprite ShootIcon;
    [SerializeField] private Sprite BlastIcon;
    [SerializeField] private Sprite ShroomTeleportIcon;
    [SerializeField] private Sprite PuddleIcon;
    [SerializeField] private Sprite WebIcon;

    private static AbilityIcons instance;
    public static AbilityIcons Instance { get { return instance; } }

    void Start()
    {
        instance = this;
    }

    public Sprite[] GetIcons(Enemy enemyScript) {
        if(enemyScript is PlayerGhost) {
            return new Sprite[] { SlashIcon, ShootIcon, null };
        }
        else if(enemyScript is BlightScript) {
            return new Sprite[] { BlastIcon, null, null };
        }
        else if(enemyScript is MushroomScript) {
            return new Sprite[] { ShootIcon, ShroomTeleportIcon, null };
        }
        else if(enemyScript is ShadowScript) {
            return new Sprite[] { SlashIcon, null, null };
        }
        else if(enemyScript is SlimeScript) {
            return new Sprite[] { ShootIcon, PuddleIcon, null };
        }
        else if(enemyScript is SpiderScript) {
            return new Sprite[] { ShootIcon, WebIcon, null };
        }

        return new Sprite[] { null, null, null };
    }
}
