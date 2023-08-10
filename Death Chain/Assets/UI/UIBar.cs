using UnityEngine;

// controls the length of a UI bar such as health. Must be attached to an object with a rectangle sprite
public class UIBar : MonoBehaviour
{
    private float height;

    void Start()
    {
        height = transform.localScale.y;
    }

    public void SetValue(int amount) {
        // keep the left side of the bar in the same position
        float barLeft = transform.position.x - transform.localScale.x / 2;

        if(amount < 0) {
            amount = 0;
        }

        transform.localScale = new Vector3(amount * height, height, 1);
        transform.position = new Vector3(barLeft + transform.localScale.x / 2, transform.position.y, 0);
    }
}
