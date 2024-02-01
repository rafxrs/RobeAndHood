using _Scripts.Scriptables;
using UnityEngine;

namespace _Scripts.Units.Weapons
{
    /// <summary>
    /// This class implements arrow behaviour
    /// </summary>
    public class Arrow : MonoBehaviour
    {
        public Side side;
        public ScriptableWeapon w;
        Rigidbody2D _rb;
        bool _isInAir = true;
        Player.Player _player;

        [System.Serializable]
        public enum Side
        {
            Player,
            Enemy,
        }


        // Start is called before the first frame update
        void Start()
        {
            Destroy(this.gameObject, 5f);
            _rb = GetComponent<Rigidbody2D>();
            _player = GameObject.Find("Player").GetComponent<Player.Player>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (_isInAir)
            {
                var velocity = _rb.velocity;
                float angle = Mathf.Atan2(velocity.y, velocity.x)* Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Ground") || other.CompareTag("Obstacle"))
            {
                StopMotion();
                PolygonCollider2D arrow = GetComponent<PolygonCollider2D>();
                arrow.enabled = false;
                _isInAir =false;
                Invoke(nameof(StopMotion),0.05f);

            }
            if (side == Side.Player)
            {
                if (other.CompareTag("Enemy"))
                {
                    Debug.Log("arrow hit "+other.name);
                    Enemy.Enemy enemy= other.GetComponent<Enemy.Enemy>();
                    if (enemy != null) enemy.TakeDamage(w.attackDamage);
                    Destroy(this.gameObject);
                }
                else if (other.CompareTag("Crate"))
                {
                
                    Destroy(this.gameObject);
                    other.GetComponent<Crate>().TakeDamage(w.attackDamage);
                }
                else if (other.CompareTag("Lever"))
                {
                    other.GetComponent<Lever>().SwitchLeverState();
                }
            }
            else if (side == Side.Enemy)
            {
                if (other.CompareTag("PlayerHitbox"))
                {
                    // Player player= other.GetComponent<Player>();
                    _player.TakeDamage(w.attackDamage);
                    Destroy(this.gameObject);
                }
            }
        
        
        }
        void StopMotion()
        {
            _rb.velocity = Vector2.zero;
            _rb.gravityScale =0;
        }
    }
}
