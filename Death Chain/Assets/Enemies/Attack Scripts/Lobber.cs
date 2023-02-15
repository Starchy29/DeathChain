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

    private void Start()
    {
        pos = transform.position;
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
        velocity.z -= gravity * Time.deltaTime;
        pos += velocity * Time.deltaTime;

        if(pos.z < 0) {
            storedLandEffect.SetActive(true);
            storedLandEffect.transform.position = transform.position;
            Destroy(gameObject);
        } else {
            transform.position = new Vector3(pos.x, pos.y + pos.z, 0);
            
            // place shadow where it will land
            if(transform.childCount > 0) {
                transform.GetChild(0).gameObject.transform.position = new Vector3(pos.x, pos.y, 0);
            }
        }
    }
}
