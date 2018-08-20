using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Units
{
    class Swordsman:UnitGroup
    {
        private const int BASEDAMAGE = 50;
        private const int BASEHP = 500;
        private const int BASEDEFENCE = 20;
        private const int BASEPRICE = 4500;
        private const float PRICEINC = 0.2f;
        private const float DMGINC = 0.3f;
        private const float HPINC = 0.4f;
        private const float DEFINC = 0.3f;
        
        protected override void Awake()
        {
            unitName = "Swashbucklers";
            MAXDISTANCE = 40;
            MINDISTANCE = 9;
            typeName = "generic_swordsman";
            type = Resources.Load<Sprite>("generic_swordsman");
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();
            if (Stay)
            {
                if (enemy != null && Vector3.Distance(player.transform.position, enemy.transform.position) > 5.0f)
                {
                    var temp = FindTarget(player, MINDISTANCE + 10);
                    if (temp != null)
                        enemy = temp;
                }
                if (Vector3.Distance(player.transform.position, transform.GetChild(0).position) > 5)
                {

                    move = true;
                    attack = false;
                    enemy = null;
                    MoveUnit(player.transform.position);
                }
                else
                {
                    if (!attack)
                    {
                        MoveUnit(transform.GetChild(0).position);
                        move = false;
                        enemy = FindTarget(player, MINDISTANCE + 10);
                        if (enemy == null)
                            enemy = FindTarget(gameObject, MINDISTANCE);
                    }
                }
            }
            else if ((enemy == null || Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) > MINDISTANCE) && !move)
            {
                enemy = FindTarget(gameObject, MINDISTANCE);
            }
            if (enemy != null)
                Enemy();
        }
        private void Enemy()
        {
            if (Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) > MAXDISTANCE && !move)
            {
                enemy = null;
                return;
            }
            attack = false;
            if (Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) < MINDISTANCE)
            {
                AttackClose();
                attack = true;
            }
        }
        private GameObject FindTarget(GameObject data, int distance)
        {
            GameObject enemy = null;
            var enemies = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == targetType && Vector3.Distance(obj.transform.GetChild(0).position, data.transform.GetChild(0).position) < distance);
            if (enemies == null)
                return null;
            foreach (GameObject ene in enemies)
            {
                enemy = enemy == null || Vector3.Distance(ene.transform.GetChild(0).position, data.transform.GetChild(0).position) < Vector3.Distance(enemy.transform.GetChild(0).position, data.transform.GetChild(0).position) ? ene.transform.GetChild(0).gameObject : enemy;
            }
            if (enemy != null)
                controll.transform.LookAt(enemy.transform.GetChild(0).position);
            return enemy;
        }
        private void AttackClose()
        {
            controll.AttackUnits(enemy.transform.parent.gameObject);
            controll.transform.LookAt(enemy.transform.GetChild(0).position);
        }

        public override void SetSoldierStatus()
        {
            hp = (int)Mathf.Floor(BASEHP * (Mathf.Pow(Level, HPINC)));
            dmg = (int)Mathf.Floor(BASEDAMAGE * (Mathf.Pow(Level, DMGINC)));
            def = (int)Mathf.Floor(BASEDEFENCE * (Mathf.Pow(Level, DEFINC)));
            price = (int)Mathf.Floor(BASEPRICE * (Mathf.Pow(Level, PRICEINC)));
            controll.Soldiers.ForEach(x => x.SetStats(dmg, hp, def));
        }

        protected override void SetParams()
        {
            controll.Size[0] = 5;
            controll.Size[1] = 4;
        }
    }
}
