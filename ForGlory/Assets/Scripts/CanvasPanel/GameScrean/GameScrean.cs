using Assets.Scripts.Player;
using Assets.Scripts.Player.Player;
using Assets.Scripts.ServerClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasPanel.GameScrean
{
    public class Character
    {
        public string name;
        public string characterType;
        public int slot;
        public float[] cord;
    }

    public class GameScrean : MonoBehaviour
    {
        private MainMenu server;
        public Sprite genArcher;
        public Sprite genKnight;
        public Sprite genSwordsman;
        public CharSelect select;
        public Text error;

        void Awake()
        {
            StartCoroutine(LoadHUBArea());
        }

        private IEnumerator LoadHUBArea()
        {
            var asyncLoadClient = SceneManager.LoadSceneAsync("HUBArea", LoadSceneMode.Additive);
            while (!asyncLoadClient.isDone)
            {
                yield return null;
            }
        }

        // Use this for initialization
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            server = GameObject.Find("MainMenu").GetComponent<MainMenu>();
            server.OnGetChar();
            server.SetErrorMsgText(error);
        }

        // Update is called once per frame
        void Update()
        {

        }

        internal void GetUseUnits(string charName)
        {
            server.GetCharInUseUnits(charName);
        }

        internal void SetCharacters(string[] splitData)
        {
            select.Visible(true);
            for (int i = 1; i < splitData.Length; i++)
            {
                string[] charData = splitData[i].Split('&');
                GameObject g = GameObject.Find("Char" + charData[2]);
                g.transform.GetChild(0).gameObject.SetActive(true);
                g.transform.GetChild(1).gameObject.SetActive(false);
                g.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = charData[0];
                g.transform.GetChild(0).GetChild(2).GetComponent<Text>().text = charData[3];
                g.transform.GetChild(0).GetChild(3).GetComponent<Text>().text = charData[4] + "|" + charData[5]+"|"+ charData[6]+"|"+charData[7]+"|"+charData[8];
                SetImage(g.transform.GetChild(0).GetChild(1).GetComponent<Image>(), charData[1]);
            }
            GameObject.Find("CharSelectWindow").GetComponent<CharSelect>().CharInfo(0);

        }

        internal void SetPlayerSparite(GameObject player, string type)
        {
            SetImage(player.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>(), type);
        }

        public void SetPlayerUnits(string[] unitInfo)
        {
            InputController controller = GameObject.Find("ClickControler").GetComponent<InputController>();
            foreach(string info in unitInfo)
            {
                string[] splitInfo = info.Split('&');
                controller.SetUnitGroup(splitInfo[1], int.Parse(splitInfo[0]), int.Parse(splitInfo[3]), int.Parse(splitInfo[4]), int.Parse(splitInfo[5]));
            }
            select.PlayerHUD(true);
        }

        private void SetImage(Image image, string v)
        {
            image.sprite = Resources.Load<Sprite>(v);
        }
        private void SetImage(SpriteRenderer image, string v)
        {
            image.sprite = Resources.Load<Sprite>(v); ;
        }
        public void SetGameList(List<string> match)
        {
            GameObject.Find("ClickControler").GetComponent<GameManager>().SetUpList(match);
        }

        internal void CreateUnit(string[] splitInfo)
        {
            InputController controller = GameObject.Find("ClickControler").GetComponent<InputController>();
            controller.SetUnitGroup(splitInfo[1], int.Parse(splitInfo[0]), int.Parse(splitInfo[2]), int.Parse(splitInfo[3]), int.Parse(splitInfo[4]));
        }
    }
}
