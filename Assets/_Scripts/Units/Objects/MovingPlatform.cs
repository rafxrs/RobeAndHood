using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Units.Objects
{
    public class MovingPlatform : MonoBehaviour
    {
        public float speed = 5f; // Speed of the platform
        public float distance = 10f; // Distance the platform should move
        [FormerlySerializedAs("movingUp")] public bool vertical;
        public bool movingDown;
        public bool move = true; // Control whether the platform can move vertically

        private Vector3 startPosition;
        public float currentDistance;

        void Start()
        {
            // Store the initial position of the platform
            startPosition = transform.position;
            currentDistance = 0f;
            if (vertical)
            {
                transform.Find("ActivateVertical").gameObject.SetActive(true);
                move = false;
            }
        }

        void Update()
        {
            if (!vertical)
            {
                MoveHorizontally();
            }
            else
            {
                if (move) // Check if the platform is allowed to move vertically
                    MoveVertically();
            }
        }

        void MoveHorizontally()
        {
            // Move the platform horizontally
            transform.Translate(Vector3.right * (speed * Time.deltaTime));
            // Update the current distance moved
            currentDistance = Mathf.Abs(startPosition.x - transform.position.x);
            // Check if the platform has moved the desired distance
            if (currentDistance >= distance)
            {
                
                speed *= -1;
            }
            // Check if the platform has moved back to its starting position
            if (currentDistance <= 0 && speed < 0)
            {
                speed *= -1;
            }
        }

        void MoveVertically()
        {
            // If the platform is paused or not allowed to move, do nothing
            if (!move)
                return;

            if (!movingDown) transform.Translate(Vector3.up * (speed * Time.deltaTime));
            else transform.Translate(Vector3.down * (speed * Time.deltaTime));
            // Update the current distance moved
            currentDistance = transform.position.y - startPosition.y;
            // Check if the platform has moved the desired distance
            if (currentDistance >= distance)
            {
                // Reverse the direction of movement
                speed *= -1;
                move = false;
            }
            // Check if the platform has moved back to its starting position
            if (currentDistance <= 0 && speed < 0)
            {
                // Reverse the direction of movement
                speed *= -1;
                move = false;
            }
        }
    }
}