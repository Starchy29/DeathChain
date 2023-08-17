using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagingWallScript : WallScript
{
    private const int DAMAGE = 1;
    private const float PUSH_FORCE = 8.0f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
        if(enemyScript == null) {
            return;
        }

        Vector2 pos = enemyScript.gameObject.transform.position;
        Vector2 direction;
        direction.x = pos.x > area.center.x ? 1 : -1;
        direction.y = pos.y > area.center.y ? 1 : -1;
        if(pos.y > area.yMin && pos.y < area.yMax) {
            direction.y = 0;
        }
        if(pos.x > area.xMin && pos.x < area.xMax) {
            direction.x = 0;
        }

        enemyScript.Push(direction.normalized * PUSH_FORCE);
        enemyScript.TakeDamage(DAMAGE);
    }
}
