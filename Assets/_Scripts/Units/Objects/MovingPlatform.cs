using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 5f; // Speed of the platform
    public float distance = 10f; // Distance the platform should move

    private Vector3 startPosition;
    private float currentDistance;
    public bool movingUp;

    void Start()
    {
        // Store the initial position of the platform
        startPosition = transform.position;
        currentDistance = 0f;
    }

    void Update()
    {
        if (!movingUp)
        {
            // Move the platform horizontally
            transform.Translate(Vector3.right * (speed * Time.deltaTime));
            // Update the current distance moved
            currentDistance = Mathf.Abs(startPosition.x-transform.position.x);
            // Check if the platform has moved the desired distance
            if (currentDistance >= distance)
            {
                // Reverse the direction of movement
                speed *= -1;
            }
            // Check if the platform has moved back to its starting position
            if (currentDistance <= 0 && speed < 0)
            {
                // Reverse the direction of movement
                speed *= -1;
            }
        }
        else
        {
            // Move the platform vertically
            transform.Translate(Vector3.up * (speed * Time.deltaTime));
            // Update the current distance moved
            currentDistance = Mathf.Abs(startPosition.y-transform.position.y);
            // Check if the platform has moved the desired distance
            if (currentDistance >= distance)
            {
                // Reverse the direction of movement
                speed *= -1;
            }
            // Check if the platform has moved back to its starting position
            if (currentDistance <= 0 && speed < 0)
            {
                // Reverse the direction of movement
                speed *= -1;
            }
        }
        

        

        

        
    }
}