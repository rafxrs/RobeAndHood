using System.Collections;
using _Scripts.Managers;
using _Scripts.Scriptables;
using _Scripts.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Units.Player
{
    /// <summary>
    /// Main class for the player
    /// This class deals with health, movement, mana, coins and player related UI
    /// </summary>

//-------------------------------------------------------------------------------------------//
    public class Player : MonoBehaviour
    {
        //-------------------------------------------------------------------------------------------//
        // GLOBAL VARIABLES
        //-------------------------------------------------------------------------------------------//
        // PUBLIC
        [FormerlySerializedAs("_platformLayerMask")] public LayerMask platformLayerMask;
        public CharacterController2D controller;
        public HealthBar healthBar;
        public ManaBar manaBar;
        //-------------------------------------------------------------------------------------------//
        public float currentMana;
        public int coins;
        public bool isClimbing;
        public bool roll;
        public bool tookDamage;
        //-------------------------------------------------------------------------------------------//
        // PRIVATE VARIABLES
        //-------------------------------------------------------------------------------------------//
        ScriptablePlayer _playerScriptable;
        private Animator _animator; 
        Rigidbody2D _rb;
        UIManager _uiManager;
        SpriteRenderer _spriteRenderer;
        Color _hurtColor;
        CapsuleCollider2D _hitbox;
        GameObject[] _impactPrefabs;

        //-------------------------------------------------------------------------------------------//
        // PRIMITIVES
        float _horizontalInput;
        float _nextClimb=-1f;
        private float _nextStepsTime = -1f;
        int _currentHealth;
        bool _jump;
        bool _isDead;
        private bool _isAttacking;
        bool _hasKey;
        //-------------------------------------------------------------------------------------------//


        //-------------------------------------------------------------------------------------------//
        // START
        void Start()
        {
            _playerScriptable = Resources.Load<ScriptablePlayer>("ScriptableObjects/Player");
            if (_playerScriptable==null)
            {
                Debug.LogError("PlayerScriptable is null");
            }

            _currentHealth = _playerScriptable.baseStats.maxHealth;
            currentMana = _playerScriptable.advancedStatistics.maxMana;
            _hurtColor = Color.red;

            manaBar.SetMaxMana(currentMana);
            manaBar.SetMana(currentMana);
            healthBar.SetMax(_playerScriptable.baseStats.maxHealth);
            healthBar.Set(_playerScriptable.baseStats.maxHealth);

            _uiManager = GameObject.Find("Main Canvas").GetComponent<UIManager>();
            if (_playerScriptable==null)
            {
                Debug.LogError("UI Manager is null");
            }
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _hitbox = GameObject.Find("PlayerHitbox").GetComponent<CapsuleCollider2D>();
            if (_hitbox == null)
            {
                Debug.LogError("Hitbox is null");
            }

            InvokeRepeating("RegainMana",0f,0.01f);
        }

        //-------------------------------------------------------------------------------------------//
        // UPDATE
        void Update()
        {
            if (GameManager.playerControl && !_isDead)
            {
                _horizontalInput = Input.GetAxisRaw("Horizontal") * _playerScriptable.advancedStatistics.speed;
                if (Mathf.Abs(_horizontalInput) > 0 && Time.time>_nextStepsTime && isGrounded())
                {
                    _nextStepsTime += Time.time + 2f;
                    Debug.Log("Playing Steps sound");
                    AudioManager.instance.Play("Steps");
                }
                else if (Mathf.Abs(_horizontalInput) == 0)
                {
                    AudioManager.instance.StopPlaying("Steps");
                }
                
                _animator.SetFloat("Speed", Mathf.Abs(_horizontalInput));
                if (isGrounded())
                {
                    _animator.SetBool("isClimbing",false);
                }

                if (isClimbing && (Time.time>_nextClimb) && (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) ) {
                
                    Climb();
                }
                else if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && !roll && !isClimbing)
                {
                
                    Jump();
                }
                if (Input.GetButtonDown("Roll") && !roll && (currentMana>=_playerScriptable.advancedStatistics.rollManaCost) && isGrounded() && !isClimbing && Mathf.Abs(_horizontalInput) > 0.01)  
                {
                    Debug.Log("Rolling");
                    Roll();
                
                } 
                else if (Input.GetButtonDown("Roll") && (currentMana<_playerScriptable.advancedStatistics.rollManaCost) && isGrounded() && !isClimbing && Mathf.Abs(_horizontalInput) > 0.01)
                {
                    transform.Find("MissingMana").gameObject.SetActive(true);
                    Invoke("ResetMissingMana",0.75f);
                }
            }
            else 
            {
                _horizontalInput =0f;
            }
        
        
        }

        void FixedUpdate()
        {
            if (GameManager.playerControl)
            {
                controller.Move(_horizontalInput* Time.fixedDeltaTime, roll, _jump);
                _jump = false;
            }
        

        }

        //-------------------------------------------------------------------------------------------//

        /*
    MOVEMENT FUNCTIONS
    */
        public void OnLanding()
        {
            _animator.SetBool("isJumping", false);
        }
        public void OnRoll()
        {
            _animator.SetBool("isRolling", false);
        }

        public bool isGrounded()
        {
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            bool grounded = collider.IsTouchingLayers(platformLayerMask);
            return grounded;
        }
        public void Jump()
        {
            _jump = true;
            if (!_isAttacking)
            {
                _animator.SetBool("isJumping", true);
            }
        
        }
        public void Roll()
        {
            roll = true;
            AudioManager.instance.Play("Roll");
            _animator.SetBool("isRolling", true);
            currentMana -= _playerScriptable.advancedStatistics.rollManaCost;
            manaBar.SetMana(currentMana);
            StartCoroutine(RollDownRoutine());
        }
        public void Climb()
        {
            _animator.SetBool("isClimbing", true);
            float verticalInput = Input.GetAxis("Vertical");
            Vector2 climbVelocity = new Vector2(_rb.velocity.x, verticalInput * _playerScriptable.advancedStatistics.climbSpeed);
            _rb.velocity = climbVelocity;
        
        
        }
        public void Bounce()
        {
            AudioManager.instance.Play("SlimeDeath");
            Debug.Log("Boucing off");
            _hitbox.enabled = false;
            Invoke("EnableHitbox", 0.2f);
            _rb.velocity = Vector2.zero;
            _rb.AddForce(new Vector2(0f, _playerScriptable.advancedStatistics.bounceForce));
            // animator.SetBool("isJumping", true);
        }

        void EnableHitbox()
        {
            _hitbox.enabled = true;
        }

        public void StopMotion()
        {
            _animator.SetFloat("Speed",0f);
            _rb.velocity = Vector2.zero;
        }
        //-------------------------------------------------------------------------------------------//

        /*
    COLLIDER FUNCTIONS
    */
        void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("DeathFloor"))
            {
            
                TakeDamage(_playerScriptable.baseStats.maxHealth);
            }
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag=="Climbable")
            {
                Debug.Log("We can climb");
                isClimbing = true;
            }
        }
        void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag=="Climbable")
            {
                _nextClimb = Time.time +0.5f;
                isClimbing = false;
                _animator.SetBool("isClimbing", false);
            }
        }

        //-------------------------------------------------------------------------------------------//
        // ROUTINES
        //-------------------------------------------------------------------------------------------//

        IEnumerator RollDownRoutine() 
        {
            yield return new WaitForSeconds(0.5f);
            roll = false;
        }

        //-------------------------------------------------------------------------------------------//
        //HEALTH & COMBAT FUNCTIONS
        //-------------------------------------------------------------------------------------------//

        public void TakeDamage(int damage)
        {
            tookDamage = true;
            int randomImpact = Random.Range(0,2);
            Instantiate(_playerScriptable.impactPrefabs[randomImpact], transform.position, Quaternion.identity);
            _spriteRenderer.color = _hurtColor;
            Invoke("ResetColor",0.25f);
            _currentHealth -= damage;
            healthBar.Set(_currentHealth);
            if (_currentHealth <= 0)
            {
                Die();
            }
        }
        void ResetColor()
        {
            _spriteRenderer.color = new Color(255,255,255,255);
        }
        public void Knockback(Collider2D other)
        {
            _rb.velocity = Vector2.zero;
            GameManager.playerControl = false;
            // animator.SetTrigger("Stun");
            Invoke("EnablePlayerControl", .5f);
            Vector2 knockbackDirection = (transform.position - other.transform.position).normalized;

            // Apply the knockback force to the player
        
            _rb.AddForce(knockbackDirection * 100f, ForceMode2D.Impulse);
        }
        public void IsAttacking()
        {
            _isAttacking = true;
            Invoke("ResetIsAttacking", 0.8f);
        }
        void ResetIsAttacking()
        {
            _isAttacking= false;
        }
        void RegainMana()
        {
            if (currentMana >= _playerScriptable.advancedStatistics.maxMana)
            {
                currentMana =100f;
                manaBar.SetMana(currentMana);
            }
            else
            {
                currentMana+=0.1f;
                manaBar.SetMana(currentMana);
            }
        }
        //-------------------------------------------------------------------------------------------//
        void Die()
        {
            Invoke("GameOverSequence",1.5f);
            _isDead = true;
            _animator.SetBool("isJumping", false);
            _animator.SetBool("isClimbing", false);
            _animator.SetTrigger("Death");
            GetComponent<PlayerCombat>().enabled = false;
            GetComponent<CharacterController2D>().enabled = false;
            // GetComponent<BoxCollider2D>().enabled = false;
            // GetComponent<CircleCollider2D>().enabled = false;
            Destroy(this.gameObject, 4f);
        }
        //-------------------------------------------------------------------------------------------//
        // UI AND SCORE FUNCTIONS
        //-------------------------------------------------------------------------------------------//
        void GameOverSequence()
        {
            _uiManager.GameOverSequence();
        }
        public void LevelComplete()
        {
            _uiManager.LevelComplete();
        }
        public void ResetMissingMana()
        {
            transform.Find("MissingMana").gameObject.SetActive(false);
        }
        //-------------------------------------------------------------------------------------------//
        // INTERACTION FUNTIONS
        //
        public void AddCoin(int coin)
        {
            coins += coin;
            _uiManager.UpdateCoin(coins);
        }
        public void CollectKey()
        {
            Debug.Log("Collected a key");
            _hasKey =true;
        }
        public bool UnlockChest()
        {
            if (_hasKey)
            {
                Debug.Log("Unlocking chest");
                // unlock
                _hasKey = false;
                return true;
            
            }
            else 
            {
                Debug.Log("No key");
                // show missing key on top of player
                return false;
            }
        }

        public void HealthPotion()
        {
            _currentHealth += 50;
            if (_currentHealth>= _playerScriptable.baseStats.maxHealth)
            {
                _currentHealth = _playerScriptable.baseStats.maxHealth;
            }
            healthBar.Set(_currentHealth);

        }

    }
}
