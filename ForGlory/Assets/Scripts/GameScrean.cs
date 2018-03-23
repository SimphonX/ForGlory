using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character
{
    public string name;
    public string characterType;
    public int slot;
    public float[] cord;
}

public class GameScrean : MonoBehaviour {
    private MainMenu server;
    public Sprite genArcher;
    public Sprite genKnight;
    public Sprite genSwordsman;
    public CharSelect select;
    public Text error;
	// Use this for initialization
	void Start () {
        server = GameObject.Find("MainMenu").GetComponent<MainMenu>();
        server.OnGetChar();
        server.SetErrorMsgText(error);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    internal void SetCharacters(string[] splitData)
    {
        select.Visible(true);
        for (int i = 1; i < splitData.Length; i++)
        {
            string[] charData = splitData[i].Split('&');
            GameObject g = GameObject.Find("Char"+charData[2]);
            g.transform.GetChild(0).gameObject.SetActive(true);
            g.transform.GetChild(1).gameObject.SetActive(false);
            g.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = charData[0];
            SetImage(g.transform.GetChild(0).GetChild(1).GetComponent<Image>(), charData[1]);
        }
        GameObject.Find("CharacterWindow").GetComponent<CharSelect>().CharInfo(0);
        
    }

    internal void SetPlayerSparite(GameObject player, string type)
    {
        SetImage(player.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>(),type);
    }

    private void SetImage(Image image, string v)
    {
        if (v.Equals("generic_archer")) image.sprite = genArcher;
        if (v.Equals("generic_knight")) image.sprite = genKnight;
        if (v.Equals("generic_swordsman")) image.sprite = genSwordsman;
    }
    private void SetImage(SpriteRenderer image, string v)
    {
        if (v.Equals("generic_archer")) image.sprite = genArcher;
        if (v.Equals("generic_knight")) image.sprite = genKnight;
        if (v.Equals("generic_swordsman")) image.sprite = genSwordsman;
    }
}
