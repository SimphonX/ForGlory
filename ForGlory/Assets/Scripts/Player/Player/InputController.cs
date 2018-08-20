using Assets.Scripts.CanvasPanel.GameScrean;
using Assets.Scripts.ServerClient;
using Assets.Scripts.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Player.Player
{
    public class InputController : MonoBehaviour
    {
        public GameObject[] units;
        public GameObject unitGroupPrefab;
        private Vector3 targetPosition;
        public GameObject player;
        public GameHUB playerHUB;
        public GameObject worldMap;
        public bool map = false;
        private int unit = -1;
        public bool seleted = false;
        public ParticleSystem clickEffect;
        public GameObject tacticalCamera;
        public BarracksWindow barracks;
        public bool textfield = false;
        public int allunits = 4;

        public GameObject[] Units
        {
            get
            {
                return units;
            }
        }

        public void SetTextField(bool state)
        {
            textfield = state;
        }

        // Use this for initialization
        void Start()
        {
            units = new GameObject[3];
            targetPosition = Vector3.zero;
        }
        public void SetSelected(bool status)
        {
            seleted = status;
        }
        public void SetPlayer(GameObject player)
        {
            this.player = player;
        }
        public void SetUnitGroup(string type, int level, int progress,int i, int id = -1)
        {
            GameObject unitGroup;
            if (i == -1)
                unitGroup = Instantiate(unitGroupPrefab, GameObject.Find("UnitGroupSpawn" + allunits++).transform.position, Quaternion.identity).gameObject;
            else
                unitGroup = Instantiate(unitGroupPrefab, GameObject.Find("UnitGroupSpawn" + (i + 1)).transform.position, Quaternion.identity).gameObject;
            switch (type)
            {
                case "generic_archer":
                    unitGroup.AddComponent<ArcherGroup>();
                    break;
                case "generic_knight":
                    unitGroup.AddComponent<KnightGroup>();
                    break;
                case "generic_swordsman":
                    unitGroup.AddComponent<Swordsman>();
                    break;
            }
            unitGroup.GetComponent<UnitGroup>().SetParams(level,progress, i, id);
            unitGroup.GetComponent<UnitGroup>().Init(Color.blue, "NotEnemy", true);
            unitGroup.GetComponent<UnitGroup>().SetSoldierStatus();
            unitGroup.GetComponent<UnitGroup>().SetPlayer(player, barracks.SetUnit(unitGroup));
            if(i != -1)
                units[i]=unitGroup;
            map = false;
        }

        public void RemoveUnit(int i)
        {
            units[i] = null;
        }
        public int SelectUnit(GameObject unit)
        {
            for(int i = 0; i < 3; i++)
                if(units[i] == null)
                {
                    units[i] = unit;
                    return i;
                }
            return -1;
        }

        // Update is called once per frame
        void Update()
        {
            if (textfield)
                return;
            if (map || player == null)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                    SetUnit(0);
                if (Input.GetKeyDown(KeyCode.F2))
                    SetUnit(1);
                if (Input.GetKeyDown(KeyCode.F3))
                    SetUnit(2);
                if (Input.GetKeyDown(KeyCode.Q))
                    SetUnit(-1);
                if (Input.GetMouseButtonDown(0))
                {
                    SetUnitStay(false);
                    SetUnitPosition();
                }
                    
            }
            if (player == null && !map)
            {
                player = GameObject.Find("GameScreanCanvas").transform.Find("CharSelectWindow").GetComponent<CharSelect>().Player;
                return;
            }
                
            if (playerHUB.transform.GetChild(0).gameObject.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.E))
                    OpenWindow(player.GetComponent<PlayerMovement>().NPC, true);
            }
            else if (player.GetComponent<PlayerMovement>() != null && player.GetComponent<PlayerMovement>().NPC != null)
                OpenWindow(player.GetComponent<PlayerMovement>().NPC, false);
            if (Input.GetKeyDown(KeyCode.M) && seleted && player != null)
            {
                player.GetComponent<PlayerMovement>().enabled = !player.GetComponent<PlayerMovement>().enabled;
                tacticalCamera.GetComponent<PlayerMovement>().enabled = !tacticalCamera.GetComponent<PlayerMovement>().enabled;
                player.transform.GetChild(1).gameObject.SetActive(!player.transform.GetChild(1).gameObject.activeSelf);
                tacticalCamera.SetActive(!tacticalCamera.activeSelf);
                tacticalCamera.transform.position = player.transform.position;
                map = !map;
                SetUnit(-1);
                playerHUB.ViewSwitch(!map);
            }
            
            if (Input.GetKeyDown(KeyCode.X))
                SetUnitStay(true);
            if (Input.GetKeyDown(KeyCode.Z))
                SetUnitStay(false);
        }

        private void SetUnit(int num)
        {
            unit = num;
            if (num != -1)
            {
                worldMap.transform.GetChild(0).GetChild(0).GetComponent<Image>().enabled = false;
                worldMap.transform.GetChild(0).GetChild(1).GetComponent<Image>().enabled = false;
                worldMap.transform.GetChild(0).GetChild(2).GetComponent<Image>().enabled = false;
                worldMap.transform.GetChild(0).GetChild(num).GetComponent<Image>().enabled = true;
            }
            else
            {
                worldMap.transform.GetChild(0).GetChild(0).GetComponent<Image>().enabled = true;
                worldMap.transform.GetChild(0).GetChild(1).GetComponent<Image>().enabled = true;
                worldMap.transform.GetChild(0).GetChild(2).GetComponent<Image>().enabled = true;
            }
                
        }
        private void SetUnitStay(bool type)
        {
            GameObject client = GameObject.Find("Client");
            if (client != null)
                client.GetComponent<Client>().SetUnitState(type);
            foreach (GameObject u in units)
                if(u != null)
                    u.GetComponent<UnitGroup>().Stay = type;
        }

        private void OpenWindow(GameObject nPC, bool status)
        {
            switch(nPC.name)
            {
                case "GamesManager":
                    playerHUB.transform.Find("GameSelectWindow").gameObject.SetActive(status);
                    if(status)
                        GetGamesList();
                    break;
                case "PlayerStatus":
                    var window = playerHUB.transform.Find("PlayerStatus");
                    window.gameObject.SetActive(status);
                    window.GetComponent<StatusWindow>().SetPlayer(player);
                    break;
                case "BarracksManager":
                    window = playerHUB.transform.Find("BarracksWindow");
                    window.gameObject.SetActive(status);
                    break;
            }
        }

        private void GetGamesList()
        {
            GameObject.Find("MainMenu").GetComponent<MainMenu>().GetGameList();
            //GameObject.Find("GameScrean").GetComponent<GameScrean>().SetGameList();
        }

        private void SetUnitPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000))
            {

                targetPosition = hit.point;
                if (hit.transform.parent != null && hit.transform.parent.name == "Enemy")
                    SetAttack(hit.transform.parent.gameObject);
                else
                    SetPosition(targetPosition); 
                ParticleSystem data = Instantiate(clickEffect, targetPosition, Quaternion.identity);
                Destroy(data.gameObject, 4.0f);
            }
    }
        private void SetAttack(GameObject enemy)
        {
            GameObject client = GameObject.Find("Client");
            if (client != null)
                client.GetComponent<Client>().AttackUnit(enemy, unit);
            if (unit != -1)
                units[unit].GetComponent<UnitGroup>().AttackUnit(enemy);
            else
            {
                if (units[0] != null)
                    units[0].GetComponent<UnitGroup>().AttackUnit(enemy);
                if (units[1] != null)
                    units[1].GetComponent<UnitGroup>().AttackUnit(enemy);
                if (units[2] != null)
                    units[2].GetComponent<UnitGroup>().AttackUnit(enemy);
            }
        }
        private void SetPosition( Vector3 tarPos)
        {
            GameObject client = GameObject.Find("Client");
            if (client != null)
                client.GetComponent<Client>().UpdateDestination(tarPos, unit);
            if (unit != -1)
            {
                if (units[unit] != null)
                    units[unit].GetComponent<UnitGroup>().MoveUnit(tarPos);
            }  
            else
            {
                if (units[0] != null)
                    units[0].GetComponent<UnitGroup>().MoveUnit(new Vector3(tarPos.x, tarPos.y, tarPos.z));
                if (units[1] != null)
                    units[1].GetComponent<UnitGroup>().MoveUnit(new Vector3(tarPos.x-5, tarPos.y, tarPos.z));
                if (units[2] != null)
                    units[2].GetComponent<UnitGroup>().MoveUnit(new Vector3(tarPos.x+5, tarPos.y, tarPos.z));
            }
        }

        internal void BackToMainUnit(int progress, int level, int slot, int id)
        {
            units[slot].GetComponent<UnitGroup>().SetParams(level, progress);
            units[slot].GetComponent<UnitGroup>().SetSoldierStatus();
        }

        public void BackToMain(int progress, int level, int gold)
        {
            StartCoroutine(LoadHUBArea());
            player.GetComponent<PlayerController>().SetRewards(progress, gold, level);
        }
        private IEnumerator LoadHUBArea()
        {
            var asyncLoadClient = SceneManager.LoadSceneAsync("HUBArea", LoadSceneMode.Additive);
            while (!asyncLoadClient.isDone)
            {
                yield return null;
            }
            var spawn = GameObject.Find("Spawn");
            player.SetActive(true);
            player.GetComponent<PlayerController>().HPLeft = player.GetComponent<PlayerController>().HP;
            player.GetComponent<PlayerController>().tacticalCamera.SetActive(false);
            player.transform.position = spawn.transform.GetChild(0).position;
            int i = 1;
            foreach(GameObject unit in units)
            {
                if(unit != null)
                {
                    unit.GetComponent<UnitGroup>().DeleteUnits();
                    unit.transform.position = spawn.transform.GetChild(i++).position;
                    unit.GetComponent<UnitGroup>().Init(Color.blue, "NotEnemy", true);
                    unit.GetComponent<UnitGroup>().SetSoldierStatus();
                    unit.GetComponent<UnitGroup>().SetStartPosition(unit.transform);
                    unit.SetActive(true);
                }
            }
        }

    }
}
