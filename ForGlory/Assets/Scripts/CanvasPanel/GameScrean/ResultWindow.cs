using Assets.Scripts.Player.Player;
using Assets.Scripts.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasPanel.GameScrean
{
    class ResultWindow:MonoBehaviour
    {
        public Text gold;
        public Text xp;
        public GameObject player;
        public List<GameObject> units = new List<GameObject>();
        public InputController controller;
        private void Start()
        {
            gameObject.SetActive(false);
        }
        private void Update()
        {
            if (controller.player == null)
                return;
            SetPlayerInfo(controller.player.GetComponent<PlayerController>());
            int i = 0;
            foreach (GameObject unit in controller.Units)
            {

                if (unit != null)
                    SetUnitInfo(unit.GetComponent<UnitGroup>(), i);
                else
                    units[i].SetActive(false);
                i++;
            }
                
        }

        private void SetUnitInfo(UnitGroup unitGroup, int num)
        {
            units[num].SetActive(true);
            units[num].transform.GetChild(0).GetComponent<Text>().text = unitGroup.UnitName;
            units[num].transform.GetChild(1).GetComponent<Text>().text = unitGroup.Progress.ToString();
            units[num].transform.GetChild(2).GetComponent<Text>().text = unitGroup.NextLevel().ToString();
            units[num].transform.GetChild(3).GetComponent<Slider>().value = unitGroup.Progress * 100 / unitGroup.NextLevel();
        }

        private void SetPlayerInfo(PlayerController playerInfo)
        {
            player.transform.GetChild(0).GetComponent<Text>().text = playerInfo.PlayerName;
            player.transform.GetChild(1).GetComponent<Text>().text = playerInfo.Progress.ToString();
            player.transform.GetChild(2).GetComponent<Text>().text = playerInfo.NextLevel().ToString();
            player.transform.GetChild(3).GetComponent<Slider>().value = playerInfo.Progress*100/playerInfo.NextLevel();
        }

        public void SetGoldAndXP(int gold, int xp)
        {
            gameObject.SetActive(true);
            this.gold.text = gold.ToString();
            this.xp.text = xp.ToString();
        }
    }
}
