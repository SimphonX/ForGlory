using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class MainMenu : MonoBehaviour {
    
    private const int MAX_CONNECTION = 100;

    private int port = 5100;

    private int hostId;
    private int webHostId;

    private int ourClientId;
    private int connectionId;

    private int reliableChannel;
    private int unreliableChannel;

    private float connectionTime;
    private bool isConnected= false;
    private bool isStarted = false;
    private byte error;
    private GameScrean game;

    private string playerName;
    public Text errorMsg;

    public List<Player> players = new List<Player>();

    void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, 0);

        connectionId = NetworkTransport.Connect(hostId, "127.0.0.1", port, 0, out error);
        connectionTime = Time.time;
        isConnected = true;
    }
    private void OnConnect()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, 0);

        connectionId = NetworkTransport.Connect(hostId, "127.0.0.1", port, 0, out error);
        connectionTime = Time.time;
        isConnected = true;
    }

    internal void DeleteChar(string charName)
    {
        string msg = "DELETECHAR|" + charName + "|" + playerName;
        Send(msg, reliableChannel);
    }

    public void Connect()
    {
        DontDestroyOnLoad(this);
        string pname = GameObject.Find("UsernameInput").GetComponent<InputField>().text;
        string pass = GameObject.Find("PasswordInput").GetComponent<InputField>().text;

        if (pname == "" || pass == "")
        {
            errorMsg.text = "Enter username and password";
            return;
        }
        playerName = pname;
        OnAskLogIn(pname, pass);
    }

    private void Update()
    {
        if(!isConnected)
            return;
        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

        switch (recData)
        {
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log(msg);
                errorMsg.text = "";
                string[] splitData = msg.Split('|');
                Debug.Log(splitData[0]);
                switch (splitData[0])
                {
                    case "ASKLOGIN":
                        break;
                    case "ERROR":
                        errorMsg.text = splitData[1];
                        break;
                    case "CNN":
                        LogIn(int.Parse(splitData[1]));
                        break;
                    case "CHARINFO":
                        game = GameObject.Find("GameScrean").GetComponent<GameScrean>();
                        game.SetCharacters(splitData);
                        break;
                    default:
                        Debug.Log("Invalid message : " + msg);
                        break;
                }
                break;
        }
        
    }
    internal void OnCreateCharacter(string charName, string charType, int slot)
    {
        UTF8Encoding ue = new UTF8Encoding();
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        string msg = "CREATECHAR|";

        msg += charName+"|"+charType+"|"+slot+"|"+playerName;
        Debug.Log(msg);
        Send(msg, reliableChannel);
    }

    private void LogIn(int cnnId)
    {
        ourClientId = cnnId;
        isStarted = true;
        SceneManager.LoadScene("GameScrean", LoadSceneMode.Single);
    }

    public void SetErrorMsgText(Text error)
    {
        errorMsg = error;
    }

    public void OnGetChar()
    {
        UTF8Encoding ue = new UTF8Encoding();
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        string msg = "GETCHAR|";

        msg += playerName;
        Debug.Log(msg);
        Send(msg, reliableChannel);
    }

    public void OnForgotPass()
    {
        string email = GameObject.Find("RemEmailInput").GetComponent<InputField>().text;
        if (email == "")
        {
            errorMsg.text = "Enter email";
            return;
        }
        if (!email.Contains("@") || !email.Contains("."))
        {
            errorMsg.text = "Bed email";
            return;
        }
        SendPassword(email);
    }

    public void OnRegister()
    {
        string username = GameObject.Find("RegUsernameInput").GetComponent<InputField>().text;
        string password = GameObject.Find("RegPasswordInput").GetComponent<InputField>().text;
        string email = GameObject.Find("RegEmailInput").GetComponent<InputField>().text;
        if (username == "" || password == "" || email == "")
        {
            errorMsg.text = "Enter username, password and email";
            return;
        }
        if (!email.Contains("@") || !email.Contains("."))
        {
            errorMsg.text = "Bed email";
            return;
        }
        playerName = username;
        Register(username, password, email);
    }
    private void Register(string username, string password, string email)
    {
        UTF8Encoding ue = new UTF8Encoding();
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        string msg = "REGISTER|";
        
        msg += username + "|" + password + "|" + email;
        Send(msg, reliableChannel);
    }

    private void SendPassword(string email)
    {
        UTF8Encoding ue = new UTF8Encoding();
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        string msg = "SENDPASS|"+email;
        Send(msg, reliableChannel);
    }

    private void OnAskLogIn(string username, string password)
    {
        UTF8Encoding ue = new UTF8Encoding();
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        string msg = "LOGIN|";
        msg += username + "|";
        msg += password;
        Send(msg, reliableChannel);
        Debug.Log(msg);
    }

    private void Send(string mess, int chennelId)
    {
        byte[] msg = Encoding.Unicode.GetBytes(mess);
        NetworkTransport.Send(hostId, connectionId, chennelId, msg, mess.Length * sizeof(char), out error);
    }
}
