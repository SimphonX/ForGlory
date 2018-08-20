using Assets.Scripts.ServerClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasPanel.GameScrean
{
    public class CharacterWindow : MonoBehaviour
    {
        public GameObject startButton;
        public GameObject rdyButton;

        // Use this for initialization
        void Start()
        { 
            if (GameObject.Find("Server") != null)
                startButton.SetActive(true);
            else
                rdyButton.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void SetStart(bool status)
        {
            startButton.GetComponent<Button>().interactable = status;
            ColorBlock block = new ColorBlock();
            Color green = new Color32(36, 150, 51, 255);
            block.normalColor = status ? Color.green : Color.red;
            block.highlightedColor = status ? green : Color.red;
            startButton.GetComponent<Button>().colors = block;
        }
        public void StartGame()
        {
            GameObject.Find("MainMenu").GetComponent<MainMenu>().OnRemoveGame();
            GameObject client = GameObject.Find("Client");
            if (client != null && client.GetComponent<Client>().connected)
                client.GetComponent<Client>().StartGame();
        }
        public void ReadyGame()
        {
            GameObject client = GameObject.Find("Client");
            if (client != null && client.GetComponent<Client>().connected)
                client.GetComponent<Client>().ReadyGame();
        }
    }
}
