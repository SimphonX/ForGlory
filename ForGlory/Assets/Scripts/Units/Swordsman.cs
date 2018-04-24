using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Units
{
    class Swordsman:UnitGroup
    {
        private const int BASEDAMAGE = 15;
        private const int BASEHP = 120;
        private const int BASEDEFENCE = 6;
        private const float DMGINC = 0.3f;
        private const float HPINC = 0.4f;
        private const float DEFINC = 0.3f;

        private const int MAXDISTANCE = 40;
        private const int MINDISTANCE = 9;
        protected override void Awake()
        {
            type = Resources.Load<Sprite>("generic_swordsman");
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
                    (enemy == null || Vector3.Distance(ene.transform.position, transform.GetChild(0).position) < Vector3.Distance(enemy.transform.position, transform.GetChild(0).position)) ? ene.transform.GetChild(0).gameObject : enemy;
            }
            if (enemy != null)
            {
                controll.transform.LookAt(enemy.transform.GetChild(0).position);
                move = true;
            }
        }
        private void AttackClose()
        {
            controll.AttackUnits(enemy);
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
    }
}
