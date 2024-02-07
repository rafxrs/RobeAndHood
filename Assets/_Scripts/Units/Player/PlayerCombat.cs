using _Scripts.Managers;
using _Scripts.Scriptables;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Units.Player
{
    /// <summary>
    /// This class is the main class for any attacking related code for the player. It deals with input and sets the correct weapons on the animator
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {

// -------------------------------------------------------------------------------------------------------------------------- //
// PUBLIC VARIABLES
// -------------------------------------------------------------------------------------------------------------------------- //

        public ScriptableWeapon[] weapons;
        public ScriptableWeapon weapon;
        public RuntimeAnimatorController defaultController;
        public Transform[] attackPoints;
        public LayerMask enemyLayer, interactableLayer;

// -------------------------------------------------------------------------------------------------------------------------- //
        Player _player;
        Animator _animator;
        [FormerlySerializedAs("_weaponName")] [SerializeField] string weaponName;
        [FormerlySerializedAs("_attackRange")] [SerializeField]
        private float attackRange;
        [FormerlySerializedAs("_attackRate")] [SerializeField] float attackRate;
        [FormerlySerializedAs("_nextAttackTime")] [SerializeField] float nextAttackTime;
        [FormerlySerializedAs("_attackDamage")] [SerializeField] int attackDamage;
        float _comboTime;
        readonly float _throwSpearManaCost = 50f;
        int _combo;
        bool _isCombo;
        private static readonly int IsJumping = Animator.StringToHash("isJumping");
        private static readonly int Attack1 = Animator.StringToHash("Attack1");
        private static readonly int Attack3 = Animator.StringToHash("Attack3");

        // -------------------------------------------------------------------------------------------------------------------------- //
        void Start() 
        {
            _animator = GetComponent<Animator>(); NullCheck.CheckNull(_animator);
            _player = GetComponent<Player>(); 
            _animator.runtimeAnimatorController = defaultController;
        }

        // Update is called once per frame
        void Update()
        {
        
            if (weapon != null && GameManager.playerControl && Time.time>nextAttackTime && !_player.roll)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    if (_player.currentMana >= 10f)
                    {
                        AttackAnimation();
                        _player.IsAttacking();
                        nextAttackTime = Time.time + attackRate;
                    }
                    else
                    {
                        transform.Find("MissingMana").gameObject.SetActive(true);
                        Invoke(nameof(ResetMissingMana), 1f);
                    }
                    
                }
                else if ((_player.currentMana>=_throwSpearManaCost) && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1)) && weaponName=="Spear")
                {
                    if (!_player.isGrounded())
                    {
                        _animator.SetBool(IsJumping, false);
                    }
                    ThrowSpearAnimation();
                    nextAttackTime = Time.time + attackRate;
                    _player.IsAttacking();
                }
                else if ((_player.currentMana<_throwSpearManaCost) && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1)) && weaponName=="Spear")
                {
                    transform.Find("MissingMana").gameObject.SetActive(true);
                    Invoke(nameof(ResetMissingMana), 1f);
                
                }
                if (weaponName=="Bow")
                {
                    if (Input.GetKeyDown(KeyCode.Space)|| Input.GetMouseButtonDown(0))
                    {
                        if (_player.currentMana >= 20f)
                        {
                            AudioManager.instance.StopPlaying("Bow Load");
                            AudioManager.instance.Play("Bow Load");
                            _animator.SetTrigger(Attack1);
                            _player.IsAttacking();
                            nextAttackTime = Time.time + attackRate;
                        }
                        else
                        {
                            transform.Find("MissingMana").gameObject.SetActive(true);
                            Invoke(nameof(ResetMissingMana), 1f); 
                        }
                        
                        

                    }
                }
            }
        }
// -------------------------------------------------------------------------------------------------------------------------- //
        void AttackAnimation()
        {
        
            int randomAttack = Random.Range(1,3);
            string attackNumber= "Attack"+randomAttack;
            if (weaponName=="Sword")
            {
                AudioManager.instance.Play("SwordAttack");
                if (_comboTime!=0f)
                {
                    //Debug.Log("Check");
                    if (((Time.time - _comboTime < 1f))&& _combo==1)
                    {
                        Debug.Log("Combo pt2");
                        _combo = 2;
                        _comboTime = Time.time;
                        _isCombo = false;
                        attackNumber= "Attack"+_combo;
                        _animator.SetTrigger(attackNumber);
                        _animator.SetBool(IsJumping, false);
                    }
                    else if (((Time.time - _comboTime < 1f))&& _combo==2&& _player.currentMana<=_throwSpearManaCost)
                    {
                        _combo = 3;
                        attackNumber= "Attack"+_combo;
                        _animator.SetBool(attackNumber, true);
                        _animator.SetBool(IsJumping, false);
                        Invoke(nameof(FinishAttack3), 0.5f);
                        _combo = 0;
                        _comboTime = 0f;
                    }
                    else 
                    {
                        _isCombo = false;
                        _comboTime = 0f;
                        _animator.SetTrigger(attackNumber);
                        _animator.SetBool(IsJumping, false);
                        _combo = 1;
                    }
                }
                else
                {
                    _isCombo = false;
                    Debug.Log("Reset combo");
                    _animator.SetTrigger(attackNumber);
                    _animator.SetBool(IsJumping, false);
                    _comboTime = Time.time;
                    _combo = 1;
                }
            

            }
            else {
                _animator.SetTrigger(attackNumber);
                _animator.SetBool(IsJumping, false);
            }
        
        }
// -------------------------------------------------------------------------------------------------------------------------- //
        void Attack()
        {
            // Debug.Log("Attacking");
            Attack(weaponName, transform, _isCombo);
        }
        public void Attack(string name, Transform player, bool combo)
        {
            switch (name)
            {
                case "Spear":
                    _player.currentMana -= 10f;
                    _player.manaBar.SetMana(_player.currentMana);
                    
                    AudioManager.instance.Play("Spear");
                    //Debug.Log("Spear Attack");
                    Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoints[0].position, new Vector3(weapon.attackRange, 1, 1), 0f, enemyLayer);
                    Collider2D[] hitObjects = Physics2D.OverlapBoxAll(attackPoints[0].position, new Vector3(weapon.attackRange, 1, 1), 0f, interactableLayer);

                    foreach(Collider2D enemy in hitEnemies)
                    {
                        Debug.Log("Spear hit "+ enemy.name);
                        enemy.GetComponent<Enemy.Enemy>().TakeDamage(weapon.attackDamage);
                    }
                    foreach(Collider2D obj in hitObjects)
                    {
                        Debug.Log("Spear hit "+ obj.name);
                        switch(obj.tag)
                        {
                            case "Crate":
                                obj.GetComponent<Crate>().TakeDamage(weapon.attackDamage);
                                break;
                            case "Lever":
                                obj.GetComponent<Lever>().SwitchLeverState();
                                break;
                        }

                    }
                    break;
                case "Bow":
                    break;
                case "Sword":
                    _player.currentMana -= 10f;
                    _player.manaBar.SetMana(_player.currentMana);
                    // Debug.Log("Sword Attack");
                    hitEnemies = Physics2D.OverlapCircleAll(attackPoints[1].position, weapon.attackRange, enemyLayer);
                    hitObjects = Physics2D.OverlapCircleAll(attackPoints[1].position, weapon.attackRange, interactableLayer);
                
                    foreach(Collider2D enemy in hitEnemies)
                    {
                        Debug.Log("Sword hit "+ enemy.name);
                        if (!combo)
                        {
                            enemy.GetComponent<Enemy.Enemy>().TakeDamage(weapon.attackDamage);
                        }
                        else 
                        {
                            Debug.Log("COMBO!");
                            enemy.GetComponent<Enemy.Enemy>().TakeDamage((weapon.attackDamage)*3);
                        }
                    }
                    foreach(Collider2D obj in hitObjects)
                    {
                        Debug.Log("Sword hit "+ obj.name);
                        switch(obj.tag)
                        {
                            case "Crate":
                                obj.GetComponent<Crate>().TakeDamage(weapon.attackDamage);
                                break;
                            case "Lever":
                                obj.GetComponent<Lever>().SwitchLeverState();
                                break;
                        }

                    }
                    break;
            }
        }

// -------------------------------------------------------------------------------------------------------------------------- //
        void ThrowSpearAnimation()
        {
            _animator.SetBool(Attack3, true);
            Invoke(nameof(FinishAttack3), 0.5f);
        }
        void ThrowSpear()
        {
            _player.currentMana -= _throwSpearManaCost;
            _player.manaBar.SetMana(_player.currentMana);
            Rigidbody2D spearInstance = Instantiate(weapon.weaponPrefab[0], transform.position, transform.rotation);
            Vector2 throwDirection = transform.right;

            // Calculate the required initial velocity for the desired arc
            float horizontalDistance = weapon.launchForce * weapon.launchDuration;
            float verticalDistance = weapon.launchArcHeight;
            Vector2 initialVelocity = CalculateInitialVelocity(throwDirection, horizontalDistance, verticalDistance);

            // Apply the initial velocity to the spear
            spearInstance.velocity = initialVelocity;
        }
// -------------------------------------------------------------------------------------------------------------------------- //
        void ShootArrow()
        {
            _player.currentMana -= 20f;
            _player.manaBar.SetMana(_player.currentMana);
            ChooseWeaponNoSound(1);
            // Debug.Log("ShootingArrow");
            // Fire arrow deals twice the damage and has 20% chance of happening
            int arrow = 0;
            int temp = Random.Range(0, 5);
            if (temp == 2)
            {
                ChooseWeaponNoSound(3);
            }
            Rigidbody2D arrowInstance = Instantiate(weapon.weaponPrefab[arrow], transform.position, Quaternion.identity);
            Vector2 throwDirection = transform.right;

            // Calculate the required initial velocity for the desired arc
            float horizontalDistance = weapon.launchForce * weapon.launchDuration;
            float verticalDistance = weapon.launchArcHeight;
            Vector2 initialVelocity = CalculateInitialVelocity(throwDirection, horizontalDistance, verticalDistance);

            // Apply the initial velocity to the spear
            arrowInstance.velocity = initialVelocity;
        }
// -------------------------------------------------------------------------------------------------------------------------- //
        private Vector2 CalculateInitialVelocity(Vector2 direction, float horizontalDistance, float verticalDistance)
        {
            // Calculate the initial velocity components for the desired arc
            float horizontalVelocity = horizontalDistance / weapon.launchDuration;
            float verticalVelocity = (verticalDistance - (0.5f * Physics2D.gravity.y * Mathf.Pow(weapon.launchDuration, 2f))) / weapon.launchDuration;

            // Combine the components with the direction vector
            Vector2 initialVelocity = direction * horizontalVelocity;
            initialVelocity.y = verticalVelocity;

            return initialVelocity;
        }   
// -------------------------------------------------------------------------------------------------------------------------- //
        void SwordComboAttack()
        {
            _player.currentMana -= _throwSpearManaCost;
            _player.manaBar.SetMana(_player.currentMana);
            Debug.Log("combo");
            _isCombo = true;
            Attack();
            Invoke(nameof(FinishCombo), 1f);
        }
        void FinishCombo()
        {
            _isCombo = false;
        }
        private void FinishAttack3()
        {
            _animator.SetBool(Attack3, false);
        }
// -------------------------------------------------------------------------------------------------------------------------- //
        private void SetWeapon(ScriptableWeapon w)
        {
            weaponName = w.name;
            weapon = w;
            attackDamage = w.attackDamage;
            attackRate = w.attackRate;
            attackRange = w.attackRange;
        }
        public void ChooseWeapon(int number) //0 is spear, 1 is bow, 2 is sword
        {
            AudioManager.instance.Play("Equip");
            weapon = weapons[number];
            ChangeController(weapon);
            SetWeapon(weapon);
        }
        
        public void ChooseWeaponNoSound(int number) //0 is spear, 1 is bow, 2 is sword
        {
            weapon = weapons[number];
            ChangeController(weapon);
            SetWeapon(weapon);
        }
        private void ChangeController(ScriptableWeapon scriptableWeapon)
        {
            _animator.runtimeAnimatorController = scriptableWeapon.controller;
        }
// -------------------------------------------------------------------------------------------------------------------------- //

        void ResetMissingMana()
        {
            transform.Find("MissingMana").gameObject.SetActive(false);
        }

    
    }
}
