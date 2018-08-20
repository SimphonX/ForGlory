using Assets.Scripts.Player;
using Assets.Scripts.Player.Player;
using Assets.Scripts.ServerClient;
using Assets.Scripts.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Units
{
    public class Soldier : MonoBehaviour
    {

        public int hp;
        public int hpLeft = 1;
        public int damage;
        public int def;

        public bool idle, attack, move;

        public Vector3 targetPosition;
        private Transform deltaPos;
        public float speed;
        private float x, z;
        [Range(1.0f, 10.0f)] public float intervalAttack = 5.0f;
        [Range(1.0f, 10.0f)] public float idleTime = 5.0f;

        public float timeLeft = 0;
        public NavMeshAgent mNavMeshAgent;
        public GameObject enemyGroup;
        public GameObject enemy;

        private Vector3 direction;
        private Vector3 lastPos;

        public Vector3 GetDirection()
        {
            return direction;
        }

        private bool isYours;

        void OnDestroy()
        {
            transform.parent.GetChild(0).GetComponent<ControllUnit>().RemoveSoldier(this);
        }

        public Transform DeltaPos
        {
            get
            {
                return deltaPos;
            }
            set
            {
                deltaPos = value;
            }
        }
        public Vector3 TargetPosition
        {
            get
            {
                return targetPosition;
            }
            set
            {
                targetPosition = value;
            }
        }
        public int Hp
        {
            get
            {
                return hp;
            }
        }
        public int HpLeft
        {
            get
            {
                return hpLeft > 0? hpLeft:0;
            }

            set
            {
                hpLeft = value;
            }
        }
        public int Damage
        {
            get
            {
                return damage;
            }
        }
        public int Def
        {
            get
            {
                return def;
            }
        }

        public bool IsYours
        {
            get
            {
                return isYours;
            }

            set
            {
                isYours = value;
            }
        }

        // Use this for initialization
        void Start()
        {
            lastPos = transform.position;
            idle = attack = move = false;
            targetPosition = Vector3.zero;
            mNavMeshAgent.speed = speed + 0.2F;
        }
        public void SetStartPos(Vector3 pos)
        {
            move = false;
            attack = false;
            idle = true;
            targetPosition = pos;
            mNavMeshAgent.Warp(pos);
            mNavMeshAgent.updateRotation = true;
        }
        public void SetPosition(float x, float z)
        {
            this.x = x; this.z = z;
        }
        // Update is called once per frame
        void Update()
        {
            direction = transform.position - lastPos;
            lastPos = transform.position;
            if (hpLeft <= 0)
            {
                Destroy(gameObject);
                return;
            }
            if (enemyGroup != null)
            {
                if(Vector3.Distance(enemyGroup.transform.GetChild(0).position, transform.parent.GetChild(0).position) > transform.parent.GetComponent<UnitGroup>().MINDISTANCE)
                {
                    enemy = null;
                    SetRelUnitPosition();
                }
                if (enemy == null && Vector3.Distance(enemyGroup.transform.GetChild(0).position, transform.parent.GetChild(0).position) < transform.parent.GetComponent<UnitGroup>().MINDISTANCE)
                {
                    timeLeft = 0;
                    FindUnit();
                }
                if (enemy != null)
                {
                    if (Vector3.Distance(transform.position, enemy.transform.position) >= 0.7f)
                        targetPosition = enemy.transform.position;
                    if(Vector3.Distance(transform.position, enemy.transform.position) <= 1)
                    {
                        targetPosition = transform.position;
                        timeLeft += Time.deltaTime;
                        if (timeLeft >= intervalAttack)
                        {
                            timeLeft = 0;
                            AttackUnit();
                        }
                    }
                    
                }

            }
            else
            {
                attack = false;
            }
            if (!attack)
                SetRelUnitPosition();
            if (mNavMeshAgent.destination != targetPosition)
            {
                mNavMeshAgent.destination = targetPosition;
                if (enemy != null)
                    transform.LookAt(enemy.transform);
                move = true;
                attack = false;
                idle = false;
            }
            if (mNavMeshAgent.remainingDistance == 0 && !attack)
            {
                move = false;
                attack = false;
                idle = true;
            }
        }

        private void SetRelUnitPosition()
        {
            
            targetPosition.x = x;
            targetPosition.y = 0;
            targetPosition.z = z;
            targetPosition = deltaPos.TransformPoint(TargetPosition);

            if (Vector3.Distance(targetPosition, transform.position) != Mathf.Infinity) mNavMeshAgent.speed = speed + Vector3.Distance(targetPosition, transform.position);
        }
        public void AttackUnit(GameObject enemyGroup, GameObject enemy)
        {
            this.enemyGroup = enemyGroup;
            this.enemy = enemy;
            attack = true;
            move = false;
            idle = false;
            targetPosition = enemy.transform.position;
        }
        private void AttackUnit()
        {
            if (enemy.GetComponent<Soldier>() && isYours)
            {
                if (enemy.transform.parent.name == "NotEnemy")
                    return;
                DamageTo(enemy.GetComponent<Soldier>());
            }
            if (enemy.GetComponent<PlayerController>() && isYours)
            {
                if (enemy.name == "NotEnemy")
                    return;
                DamageTo(enemy.GetComponent<PlayerController>());
            }
        }

        private void DamageTo(Soldier sol)
        {
            sol.TakeDamage(damage);
        }
        private void DamageTo(PlayerController sol)
        {
            sol.TakeDamage(damage);
        }
        public void MoveUnit()
        {
            enemyGroup = null;
            attack = false;
        }

        public void AttackUnit(GameObject enemyGroup, GameObject enemy, GameObject arrow)
        {
            this.enemyGroup = enemyGroup;
            this.enemy = enemy;
            var arr = Instantiate(arrow, transform.position, Quaternion.identity).GetComponent<Arrow>();
            arr.Launch(enemy.transform, transform.parent.name == "Enemy"?"NotEnemy": "Enemy", damage, isYours);
            attack = true;
        }
        private void FindUnit()
        {
            if (enemyGroup.transform.childCount <= 2)
            {
                enemyGroup = null;
                return;
            }
                
            enemy = enemyGroup.transform.GetChild(2).gameObject;
            for (int i = 2; i < enemyGroup.transform.childCount; i++)
            {
                if (Vector3.Distance(enemyGroup.transform.GetChild(i).position, transform.position) < Vector3.Distance(enemy.transform.position, transform.position))
                    enemy = enemyGroup.transform.GetChild(i).gameObject;
            }
            targetPosition = enemy.transform.position;
        }
        internal void SetStats(int damage, int hp, int def)
        {
            hpLeft = hp;
            this.hp = hp;
            this.damage = damage;
            this.def = def;
        }
        internal void TakeDamage(int damage)
        {
            var damageDone = damage - (int)(def * Random.Range(0, 0.8f));
            var hpLeft = damageDone >20?damageDone: Random.Range(1,20);
            GameObject client = GameObject.Find("Client");
            if (client != null && client.GetComponent<Client>().connected)
                client.GetComponent<Client>().UpdateUnitHP(hpLeft, transform.parent.gameObject, gameObject);
        }
    }
}
