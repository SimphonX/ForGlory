using Assets.Scripts.Player.Player;
using Assets.Scripts.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Player.Items
{
    class TwoHandsSword: Weapon
    {
        private const float DAMAGEINC = 0.1f;

        public override void Awake()
        {
            input = GameObject.Find("ClickControler").GetComponent<InputController>();
            BASECOST = 3000;
            COSTINC = 0.5f;
            BASEDAMAGE = 120;
            bASESPEED = 0.6f;
            attackInterval += BASESPEED;
            rotateAngle = 120;
        }

        void Update()
        {
            if (level != 0 && dmg == 0)
                dmg = (int)Mathf.Floor(BASEDAMAGE * (Mathf.Pow(level, DAMAGEINC)));
        }

        public override void OnTriggerEnter(Collider other)
        {

            Debug.Log(other.name);
            var player = GetComponentInParent<PlayerController>();
            if(other.GetComponent<Soldier>())
            {
                if (other.transform.parent.name == "NotEnemy")
                    return;
                DamageTo(other.GetComponent<Soldier>(), player);
            }
            if(other.GetComponent<PlayerController>())
            {
                if (other.name == "NotEnemy")
                    return;
                DamageTo(other.GetComponent<PlayerController>(), player);
            }
        }
        private void DamageTo(Soldier sol, PlayerController player)
        {
            sol.TakeDamage(dmg + player.GetDamage());
        }
        private void DamageTo(PlayerController sol, PlayerController player)
        {
            sol.TakeDamage(dmg + player.GetDamage());
        }

        public override void GetUpgradedParams(out int[] stats)
        {
            stats = new int[1];
            stats[0] = (int)Mathf.Floor(BASEDAMAGE * (Mathf.Pow(level + 1, DAMAGEINC)));
        }

        public override void GetUpgradedCurentParams(out int[] stats, out string[] fields)
        {
            stats = new int[1];
            fields = new string[1] { "DMG" };
            stats[0] = (int)Mathf.Floor(BASEDAMAGE * (Mathf.Pow(level, DAMAGEINC)));
        }
        public override void SetLevel(int level)
        {
            Debug.Log(level);
            this.level = level;
            dmg = 0;
        }
    }
}
