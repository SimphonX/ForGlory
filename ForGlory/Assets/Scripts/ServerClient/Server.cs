using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
namespace Assets.Scripts.ServerClient
{
    public class Server : MonoBehaviour
    {
        private const int MAX_CONNECTION = 100;

        private int port = 5701;

        private int hostId;
        private int webHostId;
        public Text textField;

        private int reliableChannel;
        private int unreliableChannel;

        private bool isStarted = false;
        private byte error;

        private List<ServerClient> clients = new List<ServerClient>();

        private class ServerClient
        {
            public int connectionId;
            public string playerName;
            public Vector3 position = new Vector3(3, 1, 3);
        }

        private void Start()
        {
            NetworkTransport.Init();
            ConnectionConfig cc = new ConnectionConfig();

            reliableChannel = cc.AddChannel(QosType.Reliable);
            unreliableChannel = cc.AddChannel(QosType.Unreliable);

            HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

            hostId = NetworkTransport.AddHost(topo, port, null);
            webHostId = NetworkTransport.AddWebsocketHost(topo, port, null);

            isStarted = true;
        }

        private void Update()
        {
            if (!isStarted)
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
                case NetworkEventType.ConnectEvent:
                    textField.text += ("Player " + connectionId + " has connected\n");
                    OnConnection(connectionId);
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    textField.text = (msg);
                    string[] splitData = msg.Split('|');
                    switch (splitData[0])
                    {
                        case "NAMEIS":
                            OnNameIs(connectionId, splitData[1]);
                            break;
                        case "UPDATEPOS":
                            //textField.text += msg;
                            //string[] data = splitData[1].Split('*');
                            //OnPositionChange(int.Parse(data[0]), data[1]);
                            OnPositionChange(msg);
                            break;
                        default:
                            textField.text += ("Invalid message : " + msg + "\n");
                            break;
                    }
                    break;
                case NetworkEventType.DisconnectEvent:
                    textField.text += ("Player " + connectionId + " has desconnected\n");
                    OnDisconnect(connectionId);
                    break;
            }

        }

        //private void OnPositionChange(int cnnId, string position)
        private void OnPositionChange(string msg)
        {
            /*string[] pos = position.Split('^');
            clients.Find(x => x.connectionId == cnnId).position = new Vector3(int.Parse(pos[0]), int.Parse(pos[1]), int.Parse(pos[2]));
            Send(pos, unreliableChannel, clients);*/
            Send(msg, unreliableChannel, clients);
        }

        private void OnDisconnect(int connectionId)
        {

            clients.Remove(clients.Find(x => x.connectionId == connectionId));
            string msg = "DC|" + connectionId;
            Send(msg, reliableChannel, clients);

        }

        public void OnConnection(int cnnId)
        {
            string msg = "ASKNAME|" + cnnId + "|";
            foreach (ServerClient sc in clients)
                msg += sc.playerName + "%" + sc.connectionId + "%" + sc.position.x + "^" + sc.position.y + "^" + sc.position.z + "|";
            msg = msg.Trim('|');

            ServerClient c = new ServerClient();
            c.connectionId = cnnId;
            c.playerName = "TEMP";
            clients.Add(c);

            Send(msg, reliableChannel, cnnId);
        }

        private void OnNameIs(int cnnId, string playerName)
        {
            clients.Find(x => x.connectionId == cnnId).playerName = playerName;

            Send("CNN|" + playerName + "|" + cnnId + "|" + 3 + "^" + 1 + "^" + 3, reliableChannel, clients);
        }

        private void Send(string mess, int chennelId, int cnnId)
        {
            List<ServerClient> c = new List<ServerClient>();
            c.Add(clients.Find(x => x.connectionId == cnnId));
            Send(mess, chennelId, c);
        }
        private void Send(string mess, int chennelId, List<ServerClient> c)
        {
            textField.text += ("Sending :" + mess + "\n");
            byte[] msg = Encoding.Unicode.GetBytes(mess);
            foreach (ServerClient sc in c)
            {
                NetworkTransport.Send(hostId, sc.connectionId, chennelId, msg, mess.Length * sizeof(char), out error);
            }
        }
    }
}
