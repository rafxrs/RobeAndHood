using UnityEngine;

namespace _Scripts.Units.Enemy
{
    public class DrawAttackGizmos : MonoBehaviour
    {
        void OnDrawGizmosSelected()
        {
        
            if (this.CompareTag("Player"))
            {
                if (this.name == "SwordAttackPoint"){
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(transform.position,0.75f);
                }
                else if (this.name == "SpearAttackPoint")
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(transform.position, new Vector3(2,1,1));
                }
            
            }
            else if (this.CompareTag("Enemy") && this.GetComponentInParent<global::_Scripts.Units.Enemy.Enemy>().enemyScriptable.avancedStats.hasAttacks)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, GetComponentInParent<global::_Scripts.Units.Enemy.Enemy>().enemyScriptable.avancedStats.weaponAttackRange);
            }
        
        }
    }
}
