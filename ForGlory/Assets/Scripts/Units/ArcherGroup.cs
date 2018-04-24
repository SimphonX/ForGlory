using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Units
{
    class ArcherGroup : UnitGroup
    {
        private const int BASEDAMAGE = 20;
        private const int BASEHP = 100;
        private const int BASEDEFENCE = 5;
        private const float DMGINC = 0.5f;
        private const float HPINC = 0.3f;
        private const float DEFINC = 0.3f;

        [Range(1.0f, 10.0f)] public float intervalAttack = 5.0f;
        [Range(1.0f, 10.0f)] public float idleTime = 5.0f;
        private const int MAXDISTANCE = 70;
        private const int MINDISTANCE = 9;
        private const int MIDDISTANCE = 40;

        protected override void Awake()
        {
            type = Resources.Load<Sprite>("generic_archer");
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();
            if (name == "PlayerUnit")
            {
                if (enemy != null && Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) > MAXDISTANCE)
                    enemy = null;
                if (enemy == null && !move)
                    FindTarget();
                if (enemy != null)
                    AttackTarget();
                if (enemy != null && Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) < MINDISTANCE)
                    AttackClose();
            }
        }

        private void FindTarget()
        {
            var enemies = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Enemy");
            foreach (GameObject ene in enemies)
            {
                enemy = Vector3.Distance(ene.transform.position, transform.GetChild(0).position) <= MAXDISTANCE && 
                    (enemy == null || Vector3.Distance(ene.transform.position, transform.GetChild(0).position) < Vector3.Distance(enemy.transform.position, transform.GetChild(0).position)) ? ene.transform.GetChild(0).gameObject: enemy;
            }
            if(enemy != null)
                controll.transform.LookAt(enemy.transform.GetChild(0).position);
        }

        private void Attack()
        {
            controll.AttackUnits(arrow, enemy);
        }

        protected override void SetParams()
        {
            base.SetParams();
            controll.Size[0] = 6;
            controll.Size[1] = 5;
            int hp = (int)Mathf.Floor(BASEHP * (Mathf.Pow(Level, HPINC)));
            int dmg = (int)Mathf.Floor(BASEDAMAGE * (Mathf.Pow(Level, DMGINC)));
            int def = (int)Mathf.Floor(BASEDEFENCE * (Mathf.Pow(Level, DEFINC)));
            controll.Soldiers.ForEach(x => x.SetStats(dmg, hp, def));
        }

        private void AttackClose()
        {
            controll.AttackUnits(enemy);
        }
        private void AttackTarget()
        {
            
            if (!move && (Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) > MIDDISTANCE && Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) < MAXDISTANCE || Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) < MINDISTANCE))
            {
                controll.SetTargetLocation(enemy.transform.position);
                move = true;
                attack = true;
            }
            timeLeft += Time.deltaTime;
            if (attack && move && Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) < MIDDISTANCE && Vector3.Distance(enemy.transform.position, transform.GetChild(0).position) > MINDISTANCE)
            {
                controll.SetTargetLocation(transform.GetChild(0).position);
                controll.transform.LookAt(enemy.transform.GetChild(0).position);
                timeLeft = 0;
                move = false;
            }
            if (attack && !move && timeLeft >= intervalAttack)
            {
                Attack();
                timeLeft = 0;
                Debug.Log(timeLeft);
            }
            
        }
    }
}
