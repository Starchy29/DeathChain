using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// attached to the tilemap that contains all of the level's walls
public class WallGridScript : MonoBehaviour
{
    private const int WALL_DAMAGE = 1;
    private const float PUSH_FORCE = 8.0f;
    public const int BREAKABLE_START_HEALTH = 6;

    private Tilemap tiles;

    private void Start()
    {
        tiles = GetComponent<Tilemap>();
    }

    // triggers when any wall is collided with
    private void OnCollisionEnter2D(Collision2D collision) {
        Vector3Int gridPos = tiles.WorldToCell(collision.GetContact(0).point);
        WallTile wall = tiles.GetTile<WallTile>(gridPos);
        if(wall == null) {
            return;
        }

        switch(wall.Type) {
            case WallType.Damaging:
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if(enemy == null) {
                    return;
                }

                Vector2 direction = (Vector2)collision.gameObject.transform.position - collision.GetContact(0).point;
                enemy.TakeDamage(WALL_DAMAGE);
                enemy.Push(PUSH_FORCE * direction.normalized);
                break;
        }
    }
}
