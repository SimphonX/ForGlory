using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Units
{
    class ArcherGroup : UnitGroup
    {
        private const int BASEDAMAGE = 60;
        private const int BASEHP = 400;
        private const int BASEDEFENCE = 20;
        private const int BASEPRICE = 5000;
        private const float PRICEINC = 0.2f;
        private const float DMGINC = 0.5f;
        private const float HPINC = 0.3f;
        private const float DEFINC = 0.3f;

        [Range(1.0f, 10.0f)] public float intervalAttack = 5.0f;
        [Range(1.0f, 10.0f)] public float idleTime = 5.0f;

        protected override void Awake()
        {
            unitName = "Archers";
            MAXDISTANCE = 70;
            MINDISTANCE = 9;
            MIDDISTANCE = 40;
            typeName = "generic_archer";
            type = Resources.Load<Sprite>("generic_archer");
            base.Awake();
        }

        

        protected override void Update()
        {
            base.Update();

            if(Stay)
            {
                if(enemy != null && Vector3.Distance(player.transform.position, enemy.transform.position) > 5.0f)
                {
                    var temp = FindTarget(player, MINDISTANCE + 10);
                    if(temp != null)
                        enemy = temp;
                }
                
                if (Vector3.Distance(player.transform.position, transform.GetChild(0).position) > MIDDISTANCE - 10)
                {
                    move = true;
                    attack = false;
                    enemy = null;
                    MoveUnit(player.transform.position);
                }
                else
                {
                    if(!attack)
                    {
                        MoveUnit(transform.GetChild(0).position);
                        move = false;
                        enemy = FindTarget(player, MINDISTANCE + 10);
                        if(enemy == null)
                            enemy = FindTarget(gameObject, MIDDISTANCE);
                    }
                }
                
            }
            else if ((enemy == null || Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) > MIDDISTANCE) && !move)
            {
                enemy = FindTarget(gameObject, MIDDISTANCE);
            }
            if (enemy != null)
                Enemy();

        }
        private void Enemy()
        {
            
            if (Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) > MAXDISTANCE && !move)
            {
                attack = false;
                enemy = null;
                return;
            }
            attack = true;
            if (Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) < MINDISTANCE)
                AttackClose();
            else
                AttackTarget();
        }
        private GameObject FindTarget(GameObject data, int distance)
        {
            GameObject enemy = null;
            var enemies = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == targetType && Vector3.Distance(obj.transform.GetChild(0).position, data.transform.GetChild(0).position) < distance);
            if (enemies == null)
                return null;
            foreach (GameObject ene in enemies)
            {
                enemy = enemy == null || Vector3.Distance(ene.transform.GetChild(0).position, data.transform.GetChild(0).position) < Vector3.Distance(enemy.transform.GetChild(0).position, data.transform.GetChild(0).position) ? ene.transform.GetChild(0).gameObject: enemy;
            }
            if(enemy != null)
                controll.transform.LookAt(enemy.transform.GetChild(0).position);
            return enemy;
        }

        private void Attack()
        {
            controll.AttackUnits(arrow, enemy);
            controll.transform.LookAt(enemy.transform.GetChild(0).position);
        }

        protected override void SetParams()
        {
            controll.Size[0] = 5;
            controll.Size[1] = 4;
        }

        private void AttackClose()
        {
            controll.AttackUnits(enemy.transform.parent.gameObject);
            controll.transform.LookAt(enemy.transform.GetChild(0).position);
        }
        private void AttackTarget()
        {
            /*if (!move && (Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) > MIDDISTANCE && Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) < MAXDISTANCE || Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) < MINDISTANCE))
            {
                controll.SetTargetLocation(enemy.transform.position);
                move = true;
                attack = true;
            }*/
            if (attack && move && Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) < MIDDISTANCE && Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) > MINDISTANCE)
            {
                controll.SetTargetLocation(transform.GetChild(0).position);
                controll.transform.LookAt(enemy.transform.GetChild(0).position);
                timeLeft = 0;
                move = false;
            }
            else
            {
                timeLeft += Time.deltaTime;
            }
            if (attack && !move && timeLeft >= intervalAttack)
            {
                Attack();
                timeLeft = 0;
            }
            
        }

        public override void SetSoldierStatus()
        {
            hp = (int)Mathf.Floor(BASEHP * (Mathf.Pow(Level, HPINC)));
            dmg = (int)Mathf.Floor(BASEDAMAGE * (Mathf.Pow(Level, DMGINC)));
            def = (int)Mathf.Floor(BASEDEFENCE * (Mathf.Pow(Level, DEFINC)));
            price = (int)Mathf.Floor(BASEPRICE * (Mathf.Pow(Level, PRICEINC)));
            controll.Soldiers.ForEach(x => x.SetStats(dmg, hp, def));
        }
    }
}
