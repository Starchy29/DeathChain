using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a projectile that arcs, then does something when it lands on the ground
public class Lobber : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float gravity = 5.0f;
    [SerializeField] private float upVelocity;
    [SerializeField] private GameObject LandEffect; // spawned when hitting the ground
    private GameObject storedLandEffect;

    private Vector3 velocity;
    private Vector3 pos; // z represents height
    private bool falling; // true: falling in a pit as a visual effect
    private float startSize;

    private void Start()
    {
        pos = transform.position;
        startSize = transform.localScale.x;
    }

    // must be called whenever created. Direction should be a unit vector
    public void Setup(Vector2 direction, GameObject user) {
        this.velocity = new Vector3(speed * direction.x, speed * direction.y, upVelocity);

        // create the attack now in case the user dies before this lands
        storedLandEffect = Instantiate(LandEffect);
        storedLandEffect.GetComponent<Attack>().User = user;
        storedLandEffect.SetActive(false);
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
            foreach(PitScript pit in EntityTracker.Instance.Pits) {
                foreach(Rect area in pit.Zones) {
                    if(area.Contains(transform.position)) {
                        falling = true;
                        return;
                    }
                }
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
                shadow.localScale = new Vector3(transform.localScale.x / 2 + pos.z, transform.localScale.y / 2 + pos.z, 1);
            }
        }
    }
}
