using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BounceProjectile : Projectile
{
    [SerializeField] private int bounces;

    protected override void OnEnemyCollision(Enemy hitEnemy) {
        bounces--;
        if(bounces <= 0) {
            EndAttack();
        } else {
            Vector3 fromTarget = transform.position - hitEnemy.gameObject.transform.position;
            physicsBody.velocity = Vector2.Reflect(physicsBody.velocity, fromTarget.normalized);
        }
    }

    protected override void OnWallCollision(List<Vector3Int> hitTiles) {
        bounces--;

        if(bounces <= 0) {
            EndAttack();
        } else {
            Vector3Int closestTile = hitTiles[0];
            float closestDistance = Vector3.Distance(transform.position, LevelManager.Instance.WallGrid.GetCellCenterWorld(closestTile));
            for(int i = 1; i < hitTiles.Count; i++) {
                float distance = Vector3.Distance(transform.position, LevelManager.Instance.WallGrid.GetCellCenterWorld(hitTiles[i]));
                if(distance < closestDistance) {
                    closestDistance = distance;
                    closestTile = hitTiles[i];
                }
            }

            // check if hitting a corner
            foreach(Vector3Int hitTile in hitTiles) {
                if(hitTile.x != closestTile.x && hitTile.y != closestTile.y) {
                    physicsBody.velocity = Vector2.Reflect(physicsBody.velocity, Vector2.up);
                    physicsBody.velocity = Vector2.Reflect(physicsBody.velocity, Vector2.right);
                    return;
                }
            }

            Vector3 fromTileCenter = transform.position - LevelManager.Instance.WallGrid.GetCellCenterWorld(closestTile);
            if(Mathf.Abs(fromTileCenter.x) > Mathf.Abs(fromTileCenter.y)) {
                fromTileCenter.y = 0;
            } else {
                fromTileCenter.x = 0;
            }
            physicsBody.velocity = Vector2.Reflect(physicsBody.velocity, fromTileCenter.normalized);
        }
    }
}
