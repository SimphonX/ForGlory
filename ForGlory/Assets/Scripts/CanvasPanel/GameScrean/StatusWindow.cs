using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using Assets.Scripts.Units;
using UnityEngine.UI;
using Assets.Scripts.Player.Items;
using Assets.Scripts.Player.Player;
using Assets.Scripts.ServerClient;

namespace Assets.Scripts.CanvasPanel.GameScrean
{
    class StatusWindow:MonoBehaviour
    {
        public GameObject player;
        public GameObject[] items = new GameObject[5];
        public GameObject weapon;
        public GameObject UpgradeInfo;
        public GameObject statusWindow;
        public int type;
        void Start()
        {
            gameObject.SetActive(false);
        }
        internal void SetPlayer(GameObject player)
        {
            this.player = player;
            SetWeapon(player.transform.Find("Weapon").GetChild(0).gameObject);
            SetItem(player.transform.GetChild(0).Find("Armour").gameObject, 0);
            SetItem(player.transform.GetChild(0).Find("Sheald").gameObject, 1);
            SetItem(player.transform.GetChild(0).Find("Boots").gameObject, 2);
            SetItem(player.transform.GetChild(0).Find("Gloves").gameObject, 3);
            SetItem(player.transform.GetChild(0).Find("Helmet").gameObject, 4);
        }

        void Update()
        {
            if (player == null)
                return;
            UpgradeInfo.transform.GetChild(7).GetComponent<Text>().text = player.GetComponent<PlayerController>().Gold.ToString();
            statusWindow.transform.GetChild(0).GetComponent<Text>().text = player.GetComponent<PlayerController>().STR.ToString();
            statusWindow.transform.GetChild(1).GetComponent<Text>().text = player.GetComponent<PlayerController>().CONST.ToString();
            statusWindow.transform.GetChild(2).GetComponent<Text>().text = player.GetComponent<PlayerController>().DEF.ToString();
            statusWindow.transform.GetChild(3).GetComponent<Text>().text = player.GetComponent<PlayerController>().GetDamage().ToString();
            statusWindow.transform.GetChild(4).GetComponent<Text>().text = player.GetComponent<PlayerController>().HP.ToString();
            statusWindow.transform.GetChild(5).GetComponent<Text>().text = player.GetComponent<PlayerController>().GetDefence().ToString();
            statusWindow.transform.GetChild(6).GetComponent<Text>().text = String.Format("{0:0.00}", player.GetComponent<PlayerMovement>().Speed * 100 / 6);
            statusWindow.transform.GetChild(7).GetComponent<Text>().text = String.Format("{0:0.00}", player.GetComponent<PlayerController>().GetItemsAttSpeed() * 100 / player.GetComponent<PlayerController>().weapon.GetComponent<Weapon>().BASESPEED);
            statusWindow.transform.GetChild(8).GetComponent<Text>().text = player.GetComponent<PlayerController>().GetUpgradePoints().ToString();
            if(player.GetComponent<PlayerController>().GetUpgradePoints() == 0)
            {
                statusWindow.transform.GetChild(0).GetComponentInChildren<Button>(true).gameObject.SetActive(false);
                statusWindow.transform.GetChild(1).GetComponentInChildren<Button>(true).gameObject.SetActive(false);
                statusWindow.transform.GetChild(2).GetComponentInChildren<Button>(true).gameObject.SetActive(false);
            }
            else
            {
                statusWindow.transform.GetChild(0).GetComponentInChildren<Button>(true).gameObject.SetActive(true);
                statusWindow.transform.GetChild(1).GetComponentInChildren<Button>(true).gameObject.SetActive(true);
                statusWindow.transform.GetChild(2).GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            }
            OnClickIcon(type);
            SetPlayer(player);
        }

        public void UpgradeStatus(string name)
        {
            GameObject.Find("MainMenu").GetComponent<MainMenu>().SetStats(name);
        }

        private void SetWeapon(GameObject charWeapon)
        {
            if (charWeapon == null)
                return;
            weapon.SetActive(true);
            if(charWeapon.GetComponent<ArcherGroup>())
            {
                weapon.transform.GetChild(0).gameObject.SetActive(true);
                weapon.transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                weapon.transform.GetChild(0).gameObject.SetActive(false);
                weapon.transform.GetChild(1).gameObject.SetActive(true);
            }
            weapon.transform.GetChild(2).GetComponent<Text>().text = charWeapon.GetComponent<ItemsHandler>().Level.ToString();
        }
        private void SetItem(GameObject charItem, int type)
        {
            if (charItem == null)
                return;
            items[type].SetActive(true);
            items[type].transform.GetChild(1).GetComponent<Text>().text = charItem.GetComponent<ItemsHandler>().Level.ToString();
        }
        public void OnClickIcon(int type)
        {
            this.type = type;
            if (type == -1)
            {
                SetUpgradeInfo(player.transform.Find("Weapon").GetChild(0).GetComponent<ItemsHandler>(), weapon.transform.GetChild(1).GetComponent<Image>().sprite);
            }
            else
                SetUpgradeInfo(player.transform.GetChild(0).Find(items[type].name).GetComponent<ItemsHandler>(), items[type].transform.GetChild(0).GetComponent<Image>().sprite);
        }

        private void SetUpgradeInfo(ItemsHandler holder, Sprite sprite)
        {
            UpgradeInfo.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
            int[] para, curent;
            string[] fields;
            holder.GetUpgradedCurentParams(out curent, out fields);
            holder.GetUpgradedParams(out para);
            for (int i = 0; i < 3; i++)
            {
                var field = UpgradeInfo.transform.GetChild(i + 1);
                field.gameObject.SetActive(false);
                if (curent.Length > i)
                {
                    field.GetChild(0).GetComponent<Text>().text = fields[i];
                    Debug.Log(fields[i]);
                    switch(fields[i])
                    {
                        case "ASPEED":
                            field.GetChild(1).GetComponent<Text>().text = String.Format("{0:0.00}", (float)curent[i] / 10000 * 100 / player.GetComponent<PlayerController>().weapon.GetComponent<Weapon>().BASESPEED) + "%";
                            field.GetChild(3).GetComponent<Text>().text = String.Format("{0:0.00}", (float)para[i] / 10000 * 100 / player.GetComponent<PlayerController>().weapon.GetComponent<Weapon>().BASESPEED) + "%";
                            break;
                        case "MSPEED":
                            field.GetChild(1).GetComponent<Text>().text = String.Format("{0:0.00}", (float)curent[i] / 10000 * 100 / 6) + "%";
                            field.GetChild(3).GetComponent<Text>().text = String.Format("{0:0.00}", (float)para[i] / 10000 * 100 / 6) + "%";
                            break;
                        default:
                            field.GetChild(1).GetComponent<Text>().text = curent[i].ToString();
                            field.GetChild(3).GetComponent<Text>().text = para[i].ToString();
                            break;
                    }
                    field.gameObject.SetActive(true);
                }
            }
            UpgradeInfo.transform.GetChild(5).GetComponent<Text>().text = holder.GetUpgradeCost().ToString();
            if(player.GetComponent<PlayerController>().Gold >= holder.GetUpgradeCost())
            {
                UpgradeInfo.transform.GetChild(6).GetComponent<Button>().onClick.RemoveAllListeners();
                UpgradeInfo.transform.GetChild(6).GetComponent<Button>().interactable = true;
                UpgradeInfo.transform.GetChild(6).GetComponent<Button>().onClick.AddListener(delegate { holder.LevelUp(player.GetComponent<PlayerController>().PlayerName); });
            }
            else
                UpgradeInfo.transform.GetChild(6).GetComponent<Button>().interactable = false;

        }
    }
}
