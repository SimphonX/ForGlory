using Assets.Scripts.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Units
{
    public class Soldier : MonoBehaviour {

        private int hp;
        private int hpLeft;
        private int damage;
        private int def;

        Vector3 targetPosition;
        private Transform deltaPos;
        public float speed;
        private float x, z;
        [Range(1.0f, 10.0f)] public float intervalAttack = 5.0f;
        [Range(1.0f, 10.0f)] public float idleTime = 5.0f;
        public float timeLeft = 0;
        public NavMeshAgent mNavMeshAgent;
        public GameObject enemyGroup;
        public GameObject enemy;
        public bool main;
        public bool attack = false;

        public Transform DeltaPos { get; set; }
        public Vector3 TargetPosition { get; set; }
        public int Hp { get; }
        public int HpLeft { get; }
        public int Damage { get; }
        public int Def { get; }

        // Use this for initialization
        void Start() {
            targetPosition = Vector3.zero;
            mNavMeshAgent.Warp(transform.position);
            mNavMeshAgent.speed = speed + 0.2F;
            main = false;
        }
        public void SetPosition(float x, float z)
        {
            this.x = x; this.z = z;
        }
        // Update is called once per frame
        void Update() {
            if (enemyGroup != null)
            {

                if (enemy == null && Vector3.Distance(transform.GetChild(0).position, enemyGroup.transform.GetChild(0).position) <= 9)
                {
                    timeLeft = 0;
                    FindUnit();
                }
                if (enemy != null && Vector3.Distance(transform.position, enemy.transform.position) <= 0.4f)
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
            if (!attack)
                SetRelUnitPosition();
            if (mNavMeshAgent.destination != targetPosition) mNavMeshAgent.destination = targetPosition;
        }

        private void SetRelUnitPosition()
        {
            targetPosition.x = x;
            targetPosition.y = transform.position.y;
            targetPosition.z = z;
            targetPosition = deltaPos.TransformPoint(TargetPosition);
            if (mNavMeshAgent.remainingDistance != Mathf.Infinity) mNavMeshAgent.speed = speed + mNavMeshAgent.remainingDistance;
        }
        public void AttackUnit(GameObject enemyGroup, GameObject enemy)
        {
            this.enemyGroup = enemyGroup.transform.parent.gameObject;
            this.enemy = enemy;
            attack = true;
            targetPosition = enemy.transform.position;
        }
        private void AttackUnit()
        {
            if (Random.Range(0, 5) == 1)
                Destroy(enemy);
        }
        public void MoveUnit()
        {
            attack = false;
        }

        public void AttackUnit(GameObject enemyGroup, GameObject enemy, GameObject arrow)
        {
            this.enemyGroup = enemyGroup.transform.parent.gameObject;
            this.enemy = enemy;
            var arr = Instantiate(arrow, transform.position, Quaternion.identity).GetComponent<Arrow>();
            arr.Launch(enemy.transform);
            attack = true;
        }
        private void FindUnit()
        {
            enemy = enemyGroup.transform.GetChild(2).gameObject;
            for (int i = 3; i < enemyGroup.transform.childCount; i++)
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
    }
}
