using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using Assets.Scripts.ServerClient;
using Assets.Scripts.Player.Player;
using System;
using Assets.Scripts.Units;

namespace Assets.Scripts.CanvasPanel.GameScrean
{
    public class BarracksWindow:MonoBehaviour
    {
        public Image selectedUnitBuy;
        public GameObject buyButton;
        public Text priceLable;
        public InputController controller;
        public GameObject unitInfoPrefab;
        public GameObject[] unitList;
        public Transform allUnits;
        public GameObject unitInfo;
        public GameObject sellectedUnitSell;
        public GameObject sellButton;

        public int price;

        public MainMenu menu;
        public GameObject player;
        private void Start()
        {
            unitList = new GameObject[10];
            buyButton.SetActive(false);
            menu = GameObject.Find("MainMenu").GetComponent<MainMenu>();
            gameObject.SetActive(false);
        }

        private void SetInfoField(int i)
        {
            var group = unitList.ElementAt(i).GetComponent<UnitGroup>();
            unitInfo.transform.GetChild(0).GetComponent<Image>().sprite = group.Sprite;
            unitInfo.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = group.Level.ToString();
            unitInfo.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = group.Dmg.ToString();
            unitInfo.transform.GetChild(1).GetChild(2).GetComponent<Text>().text = group.Def.ToString();
            unitInfo.transform.GetChild(1).GetChild(3).GetComponent<Text>().text = group.Hp.ToString();
            unitInfo.transform.GetChild(1).GetChild(4).GetComponent<Text>().text = group.Progress.ToString();
            unitInfo.transform.GetChild(1).GetChild(5).GetComponent<Text>().text = group.NextLevel().ToString();
            sellectedUnitSell = group.gameObject;
            if (group.Barracks.transform.GetChild(2).GetComponent<Toggle>().isOn)
                sellButton.SetActive(false);
            else
            {
                sellButton.GetComponentInChildren<Text>().text = group.Price.ToString();
                sellButton.SetActive(true);
            }
                
        }

        public GameObject SetUnit(GameObject unit)
        {
            for(int i = 0; i < 10; i++)
            {
                if (unitList[i] == null)
                {
                    unitList[i] = unit;
                    return SetInfoField(unit, i);
                }
            }
            return null;
        }
        private GameObject SetInfoField(GameObject unit, int i)
        {
            var unitInfo = Instantiate(unitInfoPrefab, allUnits.GetChild(i));
            unitInfo.transform.GetChild(0).GetComponent<Image>().sprite = unit.GetComponent<UnitGroup>().Sprite;
            unitInfo.transform.GetChild(1).GetComponent<Text>().text = unit.GetComponent<UnitGroup>().Level.ToString();
            unitInfo.GetComponent<Button>().onClick.AddListener(delegate { SetInfoField(i); });
            if (unit.GetComponent<UnitGroup>().PozitionHUB != -1)
            {
                unitInfo.transform.GetChild(2).GetComponent<Toggle>().isOn = true;
                var selected = Instantiate(unitInfo, transform.GetChild(3).GetChild(unit.GetComponent<UnitGroup>().PozitionHUB));
                selected.GetComponent<Button>().onClick.AddListener(delegate { SetInfoField(i); });
                Destroy(selected.transform.GetChild(2).gameObject);
            }
            unitInfo.transform.GetChild(2).GetComponent<Toggle>().onValueChanged.AddListener(delegate { UseUnit(unit.GetComponent<UnitGroup>().PozitionHUB, unit); });
            return unitInfo;
        }

        private void UseUnit(int pos, GameObject unit)
        {
            
            if (pos != -1)
            {
                Destroy(transform.GetChild(3).GetChild(pos).GetChild(0).gameObject);
                controller.RemoveUnit(unit.GetComponent<UnitGroup>().PozitionHUB);
                unit.GetComponent<UnitGroup>().DestroyInfo();
                menu.SetSlot(unit.GetComponent<UnitGroup>().Id, -1);
            }
            else
            {
                var i = controller.SelectUnit(unit);
                if(i == -1)
                {
                    unit.GetComponent<UnitGroup>().Barracks.transform.GetChild(2).GetComponent<Toggle>().isOn = false;
                    return;
                }
                menu.SetSlot(unit.GetComponent<UnitGroup>().Id, i);
                sellButton.SetActive(false);
                unit.GetComponent<UnitGroup>().SetUnitPoz(i);
                var selected = Instantiate(unit.GetComponent<UnitGroup>().Barracks, transform.GetChild(3).GetChild(i));
                selected.GetComponent<Button>().onClick.AddListener(delegate { SetInfoField(i); });
                Destroy(selected.transform.GetChild(2).gameObject);
            }
        }

        void Update()
        {
            if (player == null)
            {
                player = controller.player;
                return;
            }
            if (GameObject.Find("Client") != null)
                foreach (GameObject unit in unitList)
                {
                    if (unit != null && unit.GetComponent<UnitGroup>().PozitionHUB == -1 && unit.activeSelf)
                        unit.SetActive(false);
                }
            else
                foreach (GameObject unit in unitList)
                    if (unit != null && unit.GetComponent<UnitGroup>().PozitionHUB == -1 && !unit.activeSelf)
                        unit.SetActive(true);
            if (player.GetComponent<PlayerController>().Gold >= price)
                buyButton.GetComponent<Button>().interactable = true;
            else
                buyButton.GetComponent<Button>().interactable = false;
        }

        public void SetPrice(int price)
        {
            this.price = price;
            priceLable.text = price.ToString();
        }

        public void OnSelectUnit(Image unit)
        {
            selectedUnitBuy.sprite = unit.sprite;
            buyButton.SetActive(true);
        }

        public void OnBuy()
        {
            foreach(GameObject unit  in unitList)
            {
                if(unit == null)
                {
                    player.GetComponent<PlayerController>().SetGold(-price);
                    menu.CreateUnit(selectedUnitBuy.sprite.name, player.GetComponent<PlayerController>().PlayerName, price);
                    return;
                }
            }
            buyButton.SetActive(false);
            return;
        }

        public void OnSell()
        {
            buyButton.SetActive(true);
            sellButton.SetActive(false);
            
            sellectedUnitSell.GetComponent<UnitGroup>().player.GetComponent<PlayerController>().LevelUp(0, sellectedUnitSell.GetComponent<UnitGroup>().Price, true);
            sellectedUnitSell.GetComponent<UnitGroup>().player.GetComponent<PlayerController>().SetGold(sellectedUnitSell.GetComponent<UnitGroup>().Price);
            menu.DeleteUnit(sellectedUnitSell.GetComponent<UnitGroup>().Id);
            Destroy(sellectedUnitSell.GetComponent<UnitGroup>().Barracks);
            Destroy(sellectedUnitSell);
        }
    }
}
