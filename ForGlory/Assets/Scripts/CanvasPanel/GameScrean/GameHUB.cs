using Assets.Scripts.Player;
using Assets.Scripts.Player.Player;
using System;
using UnityEngine;


namespace Assets.Scripts.CanvasPanel.GameScrean
{
    public class GameHUB : MonoBehaviour
    {
        public PlayerController player; 
        public GameObject gameHUB;
        public GameObject worldMap;
        public GameObject menuScreen;
        void Start()
        {
            gameHUB.SetActive(true);
            worldMap.SetActive(false);
        }

        public void ViewSwitch (bool state)
        {
            gameHUB.SetActive(state);
            worldMap.SetActive(!state);
        }

        public void MenuSwitch()
        {
            menuScreen.SetActive(!menuScreen.activeSelf);
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}
