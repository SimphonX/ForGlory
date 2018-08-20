using Assets.Scripts.ServerClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Player.Items
{
    public abstract class ItemsHandler:MonoBehaviour
    {
        protected int BASECOST;
        public int level;
        protected float COSTINC;
        public int code;
        public int Level { get { return level; } }

        public void SetLevel(int level, int code)
        {
            this.code = code;
            SetLevel(level);
        }

        public void IncLevel(int code)
        {
            if(code == this.code)
                SetLevel(level+1);
        }
        public abstract void SetLevel(int level);

        public void LevelUp(string playerName)
        {
            GameObject.Find("MainMenu").GetComponent<MainMenu>().UpgradeItem(code, playerName, GetUpgradeCost());
        }

        public abstract int GetDamage();
        public abstract int GetDefence();
        public abstract int GetHP();
        public abstract float GetMoveSpeed();
        public abstract float GetAttackSpeed();

        public int GetUpgradeCost()
        {
            return (int)Mathf.Floor(BASECOST * (Mathf.Pow(level + 1, COSTINC)));
        }

        public abstract void GetUpgradedParams(out int[] stats);

        public abstract void GetUpgradedCurentParams(out int[] stats, out string[] fields);
    }
}
