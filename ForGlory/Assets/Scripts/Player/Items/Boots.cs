using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Player.Items
{
    public class Boots : ItemsHandler
    {
        private int BASEDHP = 5;
        private float HPINC = 0.3f;
        private float BASEMOVESPEED = 0.5f;
        private float MOVESPEEDINC = 0.5f;

        void Start()
        {
            BASECOST = 2000;
            COSTINC = 0.3f;
        }

        public float speed;
        public int hp;

        public override int GetHP()
        {
            return hp;
        }
        public override float GetMoveSpeed()
        {
            return speed;
        }

        public float Speed { get { return speed; } }

        public int Hp { get { return hp; } }

        public override void SetLevel(int level)
        {
            this.level = level;
            speed = BASEMOVESPEED * (Mathf.Pow(level, MOVESPEEDINC));
            hp = (int)Mathf.Floor(BASEDHP * (Mathf.Pow(level, HPINC)));
        }

        public override void GetUpgradedParams(out int[] stats)
        {
            stats = new int[2];
            stats[0] = (int)Mathf.Floor(BASEMOVESPEED * (Mathf.Pow(level + 1, MOVESPEEDINC)*10000));
            stats[1] = (int)Mathf.Floor(BASEDHP * (Mathf.Pow(level + 1, HPINC)));
        }

        public override void GetUpgradedCurentParams(out int[] stats, out string[] fields)
        {
            stats = new int[2];
            fields = new string[2] { "MSPEED", "HP" };
            stats[0] = (int)Mathf.Floor(BASEMOVESPEED * (Mathf.Pow(level, MOVESPEEDINC)*10000));
            stats[1] = (int)Mathf.Floor(BASEDHP * (Mathf.Pow(level, HPINC)));
        }

        public override int GetDamage()
        {
            return 0;
        }

        public override int GetDefence()
        {
            return 0;
        }

        public override float GetAttackSpeed()
        {
            return 0;
        }
    }
}
