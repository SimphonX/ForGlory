using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Player.Items
{
    class Gloves : ItemsHandler
    {
        private int BASEDEF = 2;
        private float DEFINC = 0.7f;
        private float BASEATTACKSPEED = 0.01f;
        private float ATTACKSPEEDINC = 0.5f;

        void Start()
        {
            BASECOST = 2000;
            COSTINC = 0.3f;
        }

        public float speed;
        public int def;



        public float Speed { get { return speed; } }

        public int Def { get { return def; } }

        public override void SetLevel(int level)
        {
            this.level = level;
            speed = BASEATTACKSPEED * (Mathf.Pow(level, ATTACKSPEEDINC));
            def = (int)Mathf.Floor(BASEDEF * (Mathf.Pow(level, DEFINC)));
        }

        public override void GetUpgradedParams(out int[] stats)
        {
            stats = new int[2];
            stats[0] = (int)Mathf.Floor(BASEATTACKSPEED * (Mathf.Pow(level + 1, ATTACKSPEEDINC)*10000));
            stats[1] = (int)Mathf.Floor(BASEDEF * (Mathf.Pow(level + 1, DEFINC)));
        }

        public override void GetUpgradedCurentParams(out int[] stats, out string[] fields)
        {
            stats = new int[2];
            fields = new string[2] { "ASPEED", "DEF" };
            stats[0] = (int)Mathf.Floor(BASEATTACKSPEED * (Mathf.Pow(level, ATTACKSPEEDINC)*10000));
            stats[1] = (int)Mathf.Floor(BASEDEF * (Mathf.Pow(level, DEFINC)));
        }

        public override int GetDefence()
        {
            return def;
        }

        public override float GetAttackSpeed()
        {
            return -speed;
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
    }
}
