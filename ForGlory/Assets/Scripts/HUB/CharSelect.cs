﻿using Assets.Scripts.Player;
using Assets.Scripts.ServerClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.HUB
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
        public GameObject player;
        public List<Transform> spawnPoint = new List<Transform>();
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
            player.AddComponent<PlayerMovement>();
            player.GetComponent<PlayerMovement>().camera = player.transform.GetChild(1).gameObject;
            player.transform.GetChild(1).gameObject.SetActive(true);
            string playerName = chars[createWindow.GetComponent<CreateChar>().Slot].transform.GetChild(0).GetChild(0).GetComponent<Text>().text;
            string playerType = chars[createWindow.GetComponent<CreateChar>().Slot].transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite.name;
            GameObject.Find("GameScrean").GetComponent<GameScrean>().GetUseUnits(playerName);
            player.transform.GetChild(2).GetComponent<TextMesh>().text = playerName;
            GameObject.Find("GameScrean").GetComponent<GameScrean>().SetPlayerSparite(player, playerType);
            player.transform.position = spawnPoint[0].position;
        }
        public void PlayerHUD(bool set)
        {

            selectWindow.SetActive(!set);
            gameMenu.SetActive(set);
        }
    }
}
