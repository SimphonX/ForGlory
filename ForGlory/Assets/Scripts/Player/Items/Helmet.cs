using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Player.Items
{
    class Helmet: ItemsHandler
    {
        private int BASEDEFENCE = 2;
        private float DEFINC = 0.5f;
        private int BASEDHP = 3;
        private float HPINC = 0.8f;

        void Start()
        {
            BASECOST = 3000;
            COSTINC = 0.5f;
        }

        public int def;
        public int hp;

        public int Def { get { return def; } }

        public int Hp { get { return hp; } }

        public override void SetLevel(int level)
        {
            this.level = level;
            def = (int)Mathf.Floor(BASEDEFENCE * (Mathf.Pow(level, DEFINC)));
            hp = (int)Mathf.Floor(BASEDHP * (Mathf.Pow(level, HPINC)));
        }

        public override void GetUpgradedParams(out int[] stats)
        {
            stats = new int[2];
            stats[0] = (int)Mathf.Floor(BASEDEFENCE * (Mathf.Pow(level + 1, DEFINC)));
            stats[1] = (int)Mathf.Floor(BASEDHP * (Mathf.Pow(level + 1, HPINC)));
        }

        public override void GetUpgradedCurentParams(out int[] stats, out string[] fields)
        {
            stats = new int[2];
            fields = new string[2] { "DEF", "HP" };
            stats[0] = (int)Mathf.Floor(BASEDEFENCE * (Mathf.Pow(level, DEFINC)));
            stats[1] = (int)Mathf.Floor(BASEDHP * (Mathf.Pow(level, HPINC)));
        }

        public override int GetDefence()
        {
            return def;
        }

        public override int GetHP()
        {
            return hp;
        }

        public override int GetDamage()
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
