using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Player.Items
{
    class Sheald: ItemsHandler
    {
        private int BASEDEFENCE = 7;
        private float DEFINC = 0.4f;

        void Start()
        {
            BASECOST = 3000;
            COSTINC = 0.5f;
        }

        public int def;

        public override int GetDefence()
        {
            return def;
        }

        public int Def { get { return def; } }

        public override void SetLevel(int level)
        {
            this.level = level;
            def = (int)Mathf.Floor(BASEDEFENCE * (Mathf.Pow(level, DEFINC)));
        }

        public override void GetUpgradedParams(out int[] stats)
        {
            stats = new int[1];
            stats[0] = (int)Mathf.Floor(BASEDEFENCE * (Mathf.Pow(level + 1, DEFINC)));
        }

        public override void GetUpgradedCurentParams(out int[] stats, out string[] fields)
        {
            stats = new int[1];
            fields = new string[] { "DEF" };
            stats[0] = (int)Mathf.Floor(BASEDEFENCE * (Mathf.Pow(level, DEFINC)));
        }

        public override int GetDamage()
        {
            return 0;
        }

        public override int GetHP()
        {
            return 0;
        }

        public override float GetMoveSpeed()
        {
            return 0;
        }

        public override float GetAttackSpeed()
        {
            return 0;
        }

    }
}
