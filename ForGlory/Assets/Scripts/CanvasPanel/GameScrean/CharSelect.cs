using Assets.Scripts.Player;
using Assets.Scripts.Player.Player;
using Assets.Scripts.ServerClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasPanel.GameScrean
{
    public class CharSelect : MonoBehaviour
    {
        public GameObject startButton;
        public GameObject createButton;
        public GameObject deleteButton;
        public List<GameObject> chars = new List<GameObject>();
        public GameObject createWindow;
        public GameObject selectWindow;
        public GameObject gameMenu;
        private GameObject player;
        public GameObject playerPrefab;
        public List<Transform> spawnPoint = new List<Transform>();

        public GameObject Player
        {
            get
            {
                return player;
            }
        }

        // Use this for initialization
        void Start()
        {
            createWindow.SetActive(false);
            gameMenu.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void CharInfo(int slot)
        {

            createWindow.GetComponent<CreateChar>().Slot = slot;
            foreach (GameObject g in chars)
                g.GetComponent<Image>().color = Color.white;
            chars[slot].GetComponent<Image>().color = Color.red;
            if (!chars[slot].transform.GetChild(1).gameObject.activeSelf)
            {
                startButton.SetActive(true);
                deleteButton.SetActive(true);
                createButton.SetActive(false);
            }
            else
            {
                startButton.SetActive(false);
                deleteButton.SetActive(false);
                createButton.SetActive(true);
            }
        }
        public void Visible(bool set)
        {
            createWindow.SetActive(!set);
            selectWindow.SetActive(set);
        }
        public void DeleteCharacter()
        {
            string charName = chars[createWindow.GetComponent<CreateChar>().Slot].transform.GetChild(0).GetChild(0).GetComponent<Text>().text;
            GameObject.Find("MainMenu").GetComponent<MainMenu>().DeleteChar(charName);
        }
        public void StartGame()
        {
            player = Instantiate(playerPrefab);
            player.AddComponent<PlayerMovement>();
            player.GetComponent<PlayerController>().tacticalCamera = GameObject.Find("CameraMotor");
            GameObject.Find("CameraMotor").SetActive(false);
            player.name = "NotEnemy";
            string playerName = chars[createWindow.GetComponent<CreateChar>().Slot].transform.GetChild(0).GetChild(0).GetComponent<Text>().text;
            string playerType = chars[createWindow.GetComponent<CreateChar>().Slot].transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite.name;
            var level = int.Parse(chars[createWindow.GetComponent<CreateChar>().Slot].transform.GetChild(0).GetChild(2).GetComponent<Text>().text);
            var info = chars[createWindow.GetComponent<CreateChar>().Slot].transform.GetChild(0).GetChild(3).GetComponent<Text>().text.Split('|');
            var progress = int.Parse(info[0]);
            var gold = int.Parse(info[1]);
            player.GetComponent<PlayerController>().SetPlayerParams(playerName, level, playerType, progress, spawnPoint[0].position, gold);
            player.GetComponent<PlayerController>().UpdatePlayerStats(int.Parse(info[2]), int.Parse(info[3]), int.Parse(info[4]));
            GameObject.Find("GameScrean").GetComponent<GameScrean>().GetUseUnits(playerName);
        }
        public void PlayerHUD(bool set)
        {
            selectWindow.SetActive(!set);
            gameMenu.SetActive(set);
        }
        
    }
}
