using System.Collections;
using System.Collections.Generic;
using _Scripts.Managers;
using _Scripts.Scriptables;
using _Scripts.Units.Enemy;
using _Scripts.Units.Player;
using UnityEngine;

/// <summary>
/// This class implements the spear behaviour
/// </summary>
public class Spear : MonoBehaviour
{
    public Side side;
    [SerializeField] private float rotationSpeed = 20f;
    Rigidbody2D _rb;
    public ScriptableWeapon w;
    bool _isInAir = true;
    Player _player;

    [System.Serializable]
    public enum Side
    {
        Player,
        Enemy,
    }

    void Start()
    {
        AudioManager.instance.Play("Throw Spear");
        Destroy(this.gameObject, 5f);
        _rb = GetComponent<Rigidbody2D>();
        _player= GameObject.Find("Player").GetComponent<Player>();
    }
    void FixedUpdate()
    {
        // float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x)* Mathf.Rad2Deg;
        // transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        if (_isInAir)
        {
            ChangeSpriteDirection();
        }
        
        
    }
    void ChangeSpriteDirection()
    {
        if (_rb.velocity != Vector2.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, _rb.velocity);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed*Time.deltaTime);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Ground" || other.tag == "Obstacle")
        {
            StopMotion();
            CircleCollider2D arrow = GetComponent<CircleCollider2D>();
            arrow.enabled = false;
            _isInAir =false;
            Invoke("StopMotion",0.05f);

        }
        if (side == Side.Player)
        {
            if (other.tag == "Enemy")
            {
                AudioManager.instance.Play("Spear");
                Enemy enemy= other.GetComponent<Enemy>();
                enemy.TakeDamage(w.attackDamage);
                // if (enemy.isDead)
                // {
                Destroy(this.gameObject);
                // }
                // else 
                // {
                //     Invoke("StopMotion",0.05f);
                // }
            }
            else if (other.tag == "Crate")
            {
                AudioManager.instance.Play("Spear");
                Destroy(this.gameObject);
                other.GetComponent<Crate>().TakeDamage(w.attackDamage);
            }
            else if (other.tag == "Lever")
            {
                AudioManager.instance.Play("Spear");
                other.GetComponent<Lever>().SwitchLeverState();
            }
        }
        else if (side == Side.Enemy)
        {
            if (other.tag == "PlayerHitbox")
            {
                
                
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
