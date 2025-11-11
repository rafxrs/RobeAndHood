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

        private Vector3 _startPosition;
        public float currentDistance;
        private float _nextFlip;

        void Start()
        {
            // Store the initial position of the platform
            _startPosition = transform.position;
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
            currentDistance = Mathf.Abs(_startPosition.x - transform.position.x);
            // Check if the platform has moved the desired distance
            if (currentDistance >= distance && Time.time>_nextFlip)
            {
                _nextFlip = Time.time+1f;
                speed *= -1;
            }
            // Check if the platform has moved back to its starting position
            if (currentDistance <= 0 && speed < 0 && Time.time>_nextFlip)
            {
                _nextFlip = Time.time+1f;
                speed *= -1;
            }
        }

            void MoveVertically()
            {
                if (!move)
                    return;

                // Move either up or down depending on the flag
                float direction = movingDown ? -1f : 1f;
                transform.Translate(Vector3.up * direction * speed * Time.deltaTime);

                // Measure total distance moved (absolute difference)
                currentDistance = Mathf.Abs(transform.position.y - _startPosition.y);

                // Stop when reaching the target distance
                if (currentDistance >= distance && Time.time > _nextFlip)
                {
                    _nextFlip = Time.time + 0.5f;
                    move = false;
                    Debug.Log($"{name} stopped after moving {currentDistance:F2} units.");
                }
            }

        
        public void ActivateDownwardMotion()
        {   
            vertical = true;
            movingDown = true;
            move = true;
            _startPosition = transform.position; // reset start position if needed
            Debug.Log($"{name} now moving downward!");
        }
    }
}