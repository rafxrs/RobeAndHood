using System;
using _Scripts.Scriptables;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Units.Enemy
{
    public class EnemyAI : MonoBehaviour
    {

//-------------------------------------------------------------------------------------------//
        // PUBLIC GLOBAL VARIABLES
        public Transform leftBoundary;
        public Transform rightBoundary;
        public Transform bottomBoundary;
        public Transform upBoundary;
        public bool isChasing;
        [FormerlySerializedAs("TOLERANCE")] public float tolerance = 0.01f;
//-------------------------------------------------------------------------------------------//
        // PRIVATE PRIMITIVES
//-------------------------------------------------------------------------------------------//
        int _currentWaypoint;
        float _speed;
        float _distToPlayer;
        float _chaseSpeed;
        float _patrolSpeed;
        float _nextAttackTime = -1f;
        float _dir= -1f; // direction of patrolling
        float _nextWayPointDist;
        bool _hasAttacks;
        bool _isStatic;
        [FormerlySerializedAs("_isInBounds")] [SerializeField] bool isInBounds;
        bool _wasInBounds;

//-------------------------------------------------------------------------------------------//
        // PRIVATE NON PRIMITIVES
//-------------------------------------------------------------------------------------------//
        Path _path;
        Enemy _enemy;
        Seeker _seeker;
        Rigidbody2D _rb;
        ScriptableEnemy _enemyScriptable;
        Transform _playerTransform;
        SpriteRenderer _spriteRenderer;

//-------------------------------------------------------------------------------------------//
// START AND UPDATE
//-------------------------------------------------------------------------------------------//
        // Start is called before the first frame update
        void Start()
        {
            _speed = _chaseSpeed;
            _seeker = GetComponent<Seeker>();
            _rb = GetComponent<Rigidbody2D>();
            _enemy = GetComponent<Enemy>();
            GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _playerTransform = GameObject.Find("Player").GetComponent<Transform>();

            _enemyScriptable = _enemy.enemyScriptable;
            _chaseSpeed = _enemyScriptable.advancedStats.chaseSpeed;
            _patrolSpeed = _enemyScriptable.advancedStats.patrolSpeed;
            _nextWayPointDist = _enemyScriptable.advancedStats.nextWayPointDist;
            _hasAttacks = _enemyScriptable.advancedStats.hasAttacks;
            _isStatic = _enemyScriptable.advancedStats.isStatic;

            if (!_isStatic)
            {
                InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
            }
        }
        void Update()
        {
            _distToPlayer = Mathf.Abs(transform.position.x - _playerTransform.position.x);
            if (Math.Abs(_playerTransform.position.x - transform.position.x) < tolerance)
            {
                _rb.velocity = Vector2.zero;
            }
            if ((_playerTransform.position.x > leftBoundary.position.x) && (_playerTransform.position.x < rightBoundary.position.x)
                                                                        && (_playerTransform.position.y > bottomBoundary.position.y) && (_playerTransform.position.y < upBoundary.position.y))
            {
                isInBounds = true;
            }
            else
            {
                isInBounds = false;
            }

            if (_isStatic && _hasAttacks && Time.timeSinceLevelLoad>2f)
            {
                if(_playerTransform.position.x > transform.position.x)
                {
                    transform.localScale = new Vector3(-1f, 1f,1f);
                }
                else if (_playerTransform.position.x < transform.position.x)
                {
                    transform.localScale = new Vector3(1f, 1f,1f);
                }

                if (_distToPlayer<_enemyScriptable.advancedStats.attackDistance && Time.time > _nextAttackTime && isInBounds)
                {
                    // Debug.Log("Static attack");
                    _nextAttackTime = Time.time + _enemyScriptable.advancedStats.attackRate;
                    _enemy.AttackAnimation();
                }  
            }
            else if (_hasAttacks)
            {
                if (!_isStatic)
                {
                    if (!isInBounds) 
                    {
                        if (_wasInBounds && _enemyScriptable.advancedStats.canFly)
                        {
                            ChaseAndAttack();
                        }
                        else 
                        {
                            Patrol();
                        }
                    
                    }
                    else 
                    {   
                        ChaseAndAttack();
                    }
                }
            }
            else 
            {
                if (!_isStatic)
                {
                    Patrol();
                }
            
            }
        

        
        }
//-------------------------------------------------------------------------------------------//
// PATH FUNCTIONS
//-------------------------------------------------------------------------------------------//
        void UpdatePath()
        {
            if (_seeker.IsDone())
            {
                // if (_isInBounds)
                // {
                _seeker.StartPath(_rb.position, _playerTransform.position, OnPathComplete);
                // }
                // else if (!_isInBounds && _enemyScriptable.advancedStats.canFly)
                // {
                //      _seeker.StartPath(_rb.position, _spawnPosition, OnPathComplete);
                // }
            
            }
        
        }
        void OnPathComplete(Path p)
        {
            if (!p.error)
            {
                _path = p;
                _currentWaypoint =0;
            }
        }
//-------------------------------------------------------------------------------------------//
// PATROL FUNCTIONS
//-------------------------------------------------------------------------------------------//
        private void Patrol()
        {
            _speed=_patrolSpeed;
            isChasing = false;
            Vector2 direction = new Vector2(_dir,0f);
            Vector2 force = direction * (_speed * Time.deltaTime);
            _rb.AddForce(force);
            if (_enemyScriptable.enemyType == ScriptableEnemy.EnemyType.Spider)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
                _spriteRenderer.flipY = !_wasInBounds;

                if(force.x >= 0.01)
                {
                    transform.localScale = new Vector3(-1f, 1f,1f);
                }
                else if (force.x <= 0.01)
                {
                    transform.localScale = new Vector3(1f, 1f,1f);
                }
            }
            else 
            {
                if(force.x >= 0.01)
                {
                    transform.localScale = new Vector3(-1f, 1f,1f);
                }
                else if (force.x <= 0.01)
                {
                    transform.localScale = new Vector3(1f, 1f,1f);
                }
            }
        
        }
        public void FlipPatrol()
        {
            if (_enemyScriptable.advancedStats.hasAttacks)
            {
                _rb.velocity = Vector2.zero;
            }
            _dir*=-1f;
        }

//-------------------------------------------------------------------------------------------//

private void Chase()
        {
            if (_enemyScriptable.enemyType == ScriptableEnemy.EnemyType.Spider)
            {
                _spriteRenderer.flipY = false;
                _rb.gravityScale = 2;
            }
            _speed=_chaseSpeed;
            isChasing = true;
            if (_path ==null)
            {
                return;
            }
        
            if (_currentWaypoint >= _path.vectorPath.Count)
            {
                return;
            }

            Vector2 direction = ((Vector2)_path.vectorPath[_currentWaypoint] - _rb.position).normalized;
            Vector2 force = direction * (_speed * Time.deltaTime);
            _rb.AddForce(force);
            float distance = Vector2.Distance(_rb.position, _path.vectorPath[_currentWaypoint]);

            if (distance<_nextWayPointDist)
            {
                _currentWaypoint++;
            }

            if(force.x >= 0.01)
            {
                transform.localScale = new Vector3(-1f, 1f,1f);
            }
            else if (force.x <= 0.01)
            {
                transform.localScale = new Vector3(1f, 1f,1f);
            }
        }

        void ChaseAndAttack()
        {
            _wasInBounds = true;
            if (_distToPlayer<_enemyScriptable.advancedStats.attackDistance && Time.time < _nextAttackTime)
            {
                // Debug.Log("Cant attack");
                Chase();
            }  
            else if (_distToPlayer<_enemyScriptable.advancedStats.attackDistance && Time.time > _nextAttackTime)
            {
                _nextAttackTime = Time.time + _enemyScriptable.advancedStats.attackRate;
                _enemy.AttackAnimation();
            }  
        
            else
            {
                Chase();
            }
        }
//-------------------------------------------------------------------------------------------//
        public void Jump()
        {
            _rb.AddForce(new Vector2(0f, _enemyScriptable.advancedStats.jumpForce));
        }

    
    }
}
