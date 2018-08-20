using Assets.Scripts.ServerClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CanvasPanel.GameScrean
{
    public class CreateChar : MonoBehaviour
    {

        private string charName = "";
        private string charType = "";
        private int slot = 0;
        public Image charImg;
        public Text error;

        public int Slot
        {
            get
            {
                return slot;
            }

            set
            {
                slot = value;
            }
        }

        public string PlayerType { get; set; }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void CharacterSlot(int slot)
        {
            this.slot = slot;
        }
        public void CharName(string name)
        {
            charName = name;
            Debug.Log(charName);
        }
        public void CharType(Image name)
        {
            this.PlayerType = name.gameObject.name;
            charImg.sprite = name.sprite;
            Debug.Log(PlayerType);
        }
        public void CreatCharacter()
        {
            if (PlayerType.Equals(""))
            {
                error.text = "Select charcter";
                return;
            }
            if (charName.Equals(""))
            {
                error.text = "Enter character name";
                return;
            }
            if (charName.Length < 5)
            {
                error.text = "Character name longer than 4 symbols";
                return;
            }
            if (slot > 2 || slot < 0)
            {
                error.text = "slot????";
                return;
            }

            GameObject.Find("MainMenu").GetComponent<MainMenu>().OnCreateCharacter(charName, PlayerType, slot);
        }
    }
}
