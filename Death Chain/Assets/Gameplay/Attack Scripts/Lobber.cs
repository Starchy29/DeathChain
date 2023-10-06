using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a projectile that arcs, then does something when it lands on the ground
public class Lobber : Ability
{
    [SerializeField] private float speed;
    [SerializeField] private float gravity = 5.0f;
    [SerializeField] private float upVelocity;
    [SerializeField] private GameObject LandEffect; // the attack that occurs when this hits the ground
    private GameObject storedLandEffect;

    private Vector3 velocity;
    private Vector3 pos; // z represents height
    private bool falling; // true: falling in a pit as a visual effect
    private float startSize;
    private float shadowStartSize;

    public override Enemy User { 
        get => base.User; 
        set {
            base.User = value;

            // create the landing attack now in case the user dies before this lands
            if(storedLandEffect != null) {
                Destroy(storedLandEffect);
            }
            storedLandEffect = Instantiate(LandEffect);
            storedLandEffect.SetActive(false);
            storedLandEffect.GetComponent<Ability>().User = User;
        }
    }

    private void Start()
    {
        pos = transform.position;
        startSize = transform.localScale.x;
        if(transform.childCount > 0) {
            shadowStartSize = transform.GetChild(0).transform.localScale.x; // assumes scale is uniform
        }
    }

    // must be called whenever created. Direction should be a unit vector
    public override void SetDirection(Vector2 direction) {
        this.velocity = new Vector3(speed * direction.x, speed * direction.y, upVelocity);
    }

    // Update is called once per frame
    void Update()
    {
        if(falling) {
            const float DURATION_SECONDS = 1.0f;
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            float newAlpha = sprite.color.a - DURATION_SECONDS * Time.deltaTime;
            if(newAlpha <= 0) {
                // end fall
                Destroy(gameObject);
            } else {
                // shrink and fade out
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, newAlpha);
                float newScale = transform.localScale.x - startSize * (DURATION_SECONDS * 0.7f) * Time.deltaTime;
                transform.localScale = new Vector3(newScale, newScale, 1);
            }
            return;
        }

        velocity.z -= gravity * Time.deltaTime;
        pos += velocity * Time.deltaTime;

        if(pos.z < 0) {
            // don't do anything if landing in a pit
            FloorTile floorSpot = LevelManager.Instance.FloorGrid.GetTile<FloorTile>(LevelManager.Instance.FloorGrid.WorldToCell(transform.position));
            if(floorSpot != null && floorSpot.Type == FloorType.Pit) {
                falling = true;
                return;
            }

            // create effect when landing
            storedLandEffect.SetActive(true);
            storedLandEffect.transform.position = transform.position;
            Destroy(gameObject);
        } else {
            transform.position = new Vector3(pos.x, pos.y + pos.z, 0);
            
            // place shadow where it will land
            if(transform.childCount > 0) {
                Transform shadow = transform.GetChild(0);
                shadow.gameObject.transform.position = new Vector3(pos.x, pos.y, 0);
                float newScale = shadowStartSize - pos.z / 2;
                if(newScale < 0) {
                    newScale = 0;
                }
                shadow.localScale = new Vector3(newScale, newScale, 1);
            }
        }
    }
}
