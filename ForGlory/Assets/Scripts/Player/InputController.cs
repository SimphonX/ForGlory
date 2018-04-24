using Assets.Scripts.HUB;
using Assets.Scripts.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Player
{
    public class InputController : MonoBehaviour
    {
        public GameObject[] units;
        public GameObject unitGroupPrefab;
        private Vector3 targetPosition;
        private GameObject player;
        private bool stay = false;
        private GameHUB playerHUB;
        private bool map = false;
        private int unit = -1;
        private bool seleted = false;
        public ParticleSystem clickEffect;
        // Use this for initialization
        void Start()
        {
            targetPosition = GameObject.Find("UnitGroupSpawn" +1).transform.position;
            player = GameObject.Find("Player");
            units = new GameObject[5];
            targetPosition = Vector3.zero;
            playerHUB = GameObject.Find("PlayerHUB").GetComponent<GameHUB>();

            /*for (int i = 0; i < 3; i++)
            {
                GameObject unitGroup = Instantiate(unitGroupPrefab, GameObject.Find("UnitGroupSpawn" + (i + 1)).transform.position, Quaternion.identity).gameObject;
                unitGroup.GetComponent<UnitGroup>().SetParams("generic_archer", 1, i);
                unitGroup.GetComponent<UnitGroup>().Init(Color.blue, "PlayerUnit");
                units[i] = unitGroup;
            }*/
            /*var unitdata = Instantiate(unitGroupPrefab, GameObject.Find("UnitGroupSpawn" + 4).transform.position, Quaternion.identity) as GameObject;
            
            unitdata.AddComponent<ArcherGroup>();
            unitdata.GetComponent<ArcherGroup>().SetParams(1);
            unitdata.GetComponent<ArcherGroup>().Init(Color.red, "Enemy");
            unitdata.transform.GetChild(1).localScale += new Vector3(20, 18, 0);*/
            
        }
        public void SetSelected(bool status)
        {
            seleted = status;
        }
        public void SetUnitGroup(string type, int level, int i)
        {
            GameObject unitGroup = Instantiate(unitGroupPrefab, GameObject.Find("UnitGroupSpawn" + (i + 1)).transform.position, Quaternion.identity).gameObject;
            switch (type)
            {
                case "generic_archer":
                    unitGroup.AddComponent<ArcherGroup>();
                    unitGroup.GetComponent<ArcherGroup>().SetParams(level, i);
                    unitGroup.GetComponent<ArcherGroup>().Init(Color.blue, "PlayerUnit");
                    break;
                case "generic_knight":
                    unitGroup.AddComponent<KnightGroup>();
                    unitGroup.GetComponent<KnightGroup>().SetParams(level, i);
                    unitGroup.GetComponent<KnightGroup>().Init(Color.blue, "PlayerUnit");
                    break;
                case "generic_swordsman":
                    unitGroup.AddComponent<Swordsman>();
                    unitGroup.GetComponent<Swordsman>().SetParams(level, i);
                    unitGroup.GetComponent<Swordsman>().Init(Color.blue, "PlayerUnit");
                    break;
            }
            units[i]=unitGroup;

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M) && seleted)
            {
                player.GetComponent<PlayerMovement>().enabled = !player.GetComponent<PlayerMovement>().enabled;
                player.transform.GetChild(5).GetComponent<PlayerMovement>().enabled = !player.transform.GetChild(5).GetComponent<PlayerMovement>().enabled;
                player.transform.GetChild(1).gameObject.SetActive(!player.transform.GetChild(1).gameObject.activeSelf);
                player.transform.GetChild(5).gameObject.SetActive(!player.transform.GetChild(1).gameObject.activeSelf);
                map = !map;
                playerHUB.ViewSwitch(!map);
            }
            if (map)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                    unit = 0;
                if (Input.GetKeyDown(KeyCode.F2))
                    unit = 1;
                if (Input.GetKeyDown(KeyCode.F3))
                    unit = 2;
                if (Input.GetKeyDown(KeyCode.Q))
                    unit = -1;
                if (Input.GetMouseButton(0))
                    SetUnitPosition();
            }
            else if (Vector3.Distance(player.transform.position, targetPosition) > 3 && !stay)
            {
                unit = -1;
                targetPosition = player.transform.position;
                SetUnitPosition();
            }
            if (Input.GetKeyDown(KeyCode.X))
                stay = true;
            if (Input.GetKeyDown(KeyCode.Z))
                stay = false;
        }
        private void SetUnitPosition()
        {
            if (map)
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
        }
        private void SetAttack(GameObject enemy)
        {
            if (unit != -1)
                units[unit].GetComponent<UnitGroup>().AttackUnit(enemy);
            else
            {
                Debug.Log(units[0]); Debug.Log(units[1]); Debug.Log(units[2]);
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
            if(unit != -1)
                units[unit].GetComponent<UnitGroup>().MoveUnit(tarPos);
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
    }
}
