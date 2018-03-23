using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player
{
    public string playerName;
    public GameObject avatar;
    public int connectionId;
}

public class Client : MonoBehaviour {
    private const int MAX_CONNECTION = 100;

    private int port = 5701;

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

    private string playerName;
    public GameObject playerPrefab;
    public GameObject canvas;
    private GameObject thisPlayer;
    public Text log;

    public List<Player> players = new List<Player>();
    
    public void Connect()
    {
        //Doess the player have a name
        string pname = GameObject.Find("NameInput").GetComponent<InputField>().text;

        if(pname == "")
        {
            Debug.Log("You must enter a name!");
            return;
        }

        playerName = pname;

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
                log.text += msg+"\n";
                string[] splitData = msg.Split('|');
                switch (splitData[0])
                {
                    case "ASKNAME":
                        OnAskName(splitData);
                        break;
                    case "CNN":
                        string[] data = splitData[3].Split('^');
                        SpawnPlayer(splitData[1], int.Parse(splitData[2]), new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2])));
                        break;
                    case "DC":
                        log.text += splitData[1] + "\n";
                        RemovePlayer(int.Parse(splitData[1]));
                        break;
                    case "UPDATEPOS":
                        UpdatePosition(splitData);
                        break;
                    default:
                        Debug.Log("Invalid message : " + msg);
                        break;
                }
                break;
        }
        
    }

    public void UpdatePosition()
    {
        string pos = "UPDATEPOS" + "|" + ourClientId + "*" + thisPlayer.transform.position.x + "^" + thisPlayer.transform.position.y + "^" + thisPlayer.transform.position.z;
        Send(pos, unreliableChannel);
    }

    private void UpdatePosition(string[] splitData)
    {
        for (int i = 1; i < splitData.Length; i += 2)
        {
            UpdateData(players.Find(x => x.connectionId==int.Parse(splitData[i])), splitData[i + 1]);
        }
    }

    private bool UpdateData(Player player, string positions)
    {
        string[] allPosData = positions.Split('*');
        string[] tempData = allPosData[0].Split('^');
        player.avatar.transform.position = new Vector3(float.Parse(tempData[0]), float.Parse(tempData[1]), float.Parse(tempData[2]));
        //Debug.Log(thisPlayer.transform.position.ToString());
        return true;
    }

    private void OnAskName(string [] data)
    {
        ourClientId = int.Parse(data[1]);

        Send("NAMEIS|" + playerName, reliableChannel);

        for(int i = 2; i < data.Length; i++)
        {
            string[] d = data[i].Split('%');
            string[] pos = d[2].Split('^');
            SpawnPlayer(d[0], int.Parse(d[1]), new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2])));
        }
    }

    private void SpawnPlayer( string playerName, int cnnId, Vector3 pos)
    {
        GameObject go = Instantiate(playerPrefab) as GameObject;
        go.transform.position = pos;

        if(cnnId == ourClientId)
        {
            canvas.SetActive(false);
            log.gameObject.SetActive(true);
            go.AddComponent<PlayerMovement>();
            go.transform.GetChild(1).gameObject.SetActive(true);
            thisPlayer = go;
            isStarted = true;
        }
        go.name = "Player" + cnnId;
        go.transform.GetChild(2).GetComponent<TextMesh>().text = playerName;
        Player p = new Player();
        p.avatar = go;
        p.playerName = playerName;
        p.connectionId = cnnId;
        p.avatar.transform.position = pos;
        players.Add(p);
    }

    private void RemovePlayer(int cnnId)
    {
        players.Remove(players.Find(x => x.connectionId == cnnId));
    }

    private void Send(string mess, int chennelId)
    {
        byte[] msg = Encoding.Unicode.GetBytes(mess);
        NetworkTransport.Send(hostId, connectionId, chennelId, msg, mess.Length * sizeof(char), out error);
    }
}
