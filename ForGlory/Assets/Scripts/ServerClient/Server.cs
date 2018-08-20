using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Assets.Scripts.ServerClient
{
    public class Server : MonoBehaviour, IServerClient
    {
        MatchInfo matchInfo;

        NetworkMatch m_NetworkMatch;

        byte[] m_ReceiveBuffer;

        private const int MAX_MESSAGE_SIZE = 65535;
        private int MAX_CONNECTION = 10;

        private int port;

        private int hostId;

        private int reliableChannel;
        private int unreliableChannel;

        private byte error;
        public bool matchStarted = false;

        private int sizeA, sizeB;

        private string password;

        public ServerClient[] teamA = new ServerClient[6];
        public ServerClient[] teamB = new ServerClient[6];

        public MatchInfo GetMatchInfo()
        {
            return matchInfo;
        }

        public void OnDisable()
        {
            NetworkTransport.DisconnectNetworkHost(hostId, out error);
        }

        private void ErrorReport(bool success, string extendedInfo)
        {
            if (!success)
                GameObject.Find("ErrorText").GetComponent<Text>().text = extendedInfo;
        }

        public void OnConnectionDropped(bool success, string extendedInfo)
        {
            OnDisable();
            hostId = -1;
            teamA = new ServerClient[6];
            teamB = new ServerClient[6];
        }

        public class ServerClient
        {
            public ServerClient(int cnnId)
            {
                connectionId = cnnId;
                playerName = "";
                type = "";
                level = -1;
                
                active = 1;
            }
            public int connectionId;
            public string playerName;
            public string type;
            public int level;
            public int hp;
            public int active;
            public bool rdy = false;
            public UnitInfo[] unitInfo = new UnitInfo[5];

            public override string ToString()
            {
                string str = playerName + "^" + connectionId + "^" + type + "^" + level+"^"+rdy;
                foreach (UnitInfo info in unitInfo)
                {
                    if(info != null)
                        str += "^" + info.ToString();
                }
                return str;
            }
        }

        public class UnitInfo
        {
            public int level;
            public string type;
            public int[] size = new int[2];
            public int slot;
            public List<Soldier> soldiers;
            public int unitHp;
            public UnitInfo(int level, string type, int unitHP, int[] size, int poz)
            {
                this.level = level;
                this.type = type;
                slot = poz;
                unitHp = unitHP / size[0] / size[1];
                this.size = size;
                soldiers = new List<Soldier>();
                for (float i = -((float)size[0] - 1) / 2; i <= size[0] / 2; i++)
                {
                    for (float j = -((float)size[1] - 1) / 2; j <= size[1] / 2; j++)
                    {
                        soldiers.Add(new Soldier(unitHp , "Bot" + i + "" + j));
                    }
                }
            }

            public override string ToString()
            {
                return level + "$" + type + "$" + size[0] + "$" + size[1] + "$" + unitHp + "$" + slot;
            }
        }
        public class Soldier
        {
            public int hp;
            public string name;
            public bool active;
            public Soldier(int hp, string name)
            {
                active = true;
                this.hp = hp;
                this.name = name;
            }

            public override string ToString()
            {
                return hp.ToString() + "$" + name;
            }
        }

        void Awake()
        {
            port = UnityEngine.Random.Range(10000, 20000);
            sizeA = sizeB = 0;
            m_NetworkMatch = gameObject.AddComponent<NetworkMatch>();
        }
        void Start()
        {
            m_ReceiveBuffer = new byte[MAX_MESSAGE_SIZE];
            // While testing with multiple standalone players on one machine this will need to be enabled
            Application.runInBackground = true;


        }
        public void StartServer(string name, string pass, uint size)
        {
            password = pass;
            MAX_CONNECTION = (int)size*2+1;
            m_NetworkMatch.CreateMatch(name, size * 2 + 1, true, pass, "", "", 0, 0, OnMatchCreate);
        }
        private void StartServer()
        {
            NetworkTransport.Init();
            ConnectionConfig cc = new ConnectionConfig();

            reliableChannel = cc.AddChannel(QosType.Reliable);
            unreliableChannel = cc.AddChannel(QosType.Unreliable);

            HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

            hostId = NetworkTransport.AddHost(topo, port);
        }

        public void OnMatchCreate(bool success, string extendedInfo, MatchInfo m_matchInfo)
        {
            if (success)
            {
                matchInfo = m_matchInfo;
                Utility.SetAccessTokenForNetwork(matchInfo.networkId, matchInfo.accessToken);
                
                StartServer(matchInfo.address, matchInfo.port, matchInfo.networkId, matchInfo.nodeId);
            }
            else
            {
                SceneManager.UnloadSceneAsync("Server");
            }
        }
        void StartServer(string relayIp, int relayPort, NetworkID networkId, NodeID nodeId)
        {
            StartServer();
            byte error;
            NetworkTransport.ConnectAsNetworkHost(hostId, relayIp, relayPort, networkId, Utility.GetSourceID(), nodeId, out error);
            
        }

        private IEnumerator StartClient(NetworkID networkId)
        {
            var asyncLoadClient = SceneManager.LoadSceneAsync("Client", LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync("HUBArea");
            while (!asyncLoadClient.isDone)
            {
                yield return null;
            }
            GameObject.Find("Client").GetComponent<Client>().StartClient(networkId, password);
            
        }

        private void Update()
        {
            if (hostId == -1)
                return;
            int winner;
            if(matchStarted && MatchEnd(out winner))
            {
                EndMatch(winner);
            }

            var networkEvent = NetworkEventType.Nothing;
            int connectionId;
            int channelId;
            int dataSize;
            byte error;
            networkEvent = NetworkTransport.ReceiveRelayEventFromHost(hostId, out error);
            do
            {
                networkEvent = NetworkTransport.ReceiveFromHost(hostId, out connectionId, out channelId,
                m_ReceiveBuffer, (int)m_ReceiveBuffer.Length, out dataSize, out error);
                switch (networkEvent)
                {
                    case NetworkEventType.ConnectEvent:
                        if(matchStarted)
                        {
                            OnDisconnect(connectionId);
                        }
                        else
                            OnConnection(connectionId);
                        break;
                    case NetworkEventType.DataEvent:
                        string msg = Encoding.Unicode.GetString(m_ReceiveBuffer, 0, dataSize);
                        string[] splitData = msg.Split('|');
                            Debug.Log("Server: " + msg);
                        switch (splitData[0])
                        {
                            case "CNN":
                                OnInfoGet(splitData.Skip(1).ToArray(), connectionId, msg);
                                break;
                            case "UPDATEPOS":
                                Send(msg, reliableChannel, GetList(teamA, teamB));
                                break;
                            case "ATTACKUNIT":
                                Send(msg, reliableChannel, GetList(teamA, teamB));
                                break;
                            case "PLAYERPOS":
                                Send(msg, unreliableChannel, GetList(teamA, teamB));
                                break;
                            case "UPDATEPLAYERHP":
                                OnUpdatePlayerHp(splitData.Skip(1).ToArray());
                                break;
                            case "ATTCKMOVE":
                                Send(msg, reliableChannel, GetList(teamA, teamB));
                                break;
                            case "UPDATEUNITHP":
                                OnUpdateUnitHp(splitData.Skip(1).ToArray());
                                break;
                            case "SETSTATE":
                                Send(msg, reliableChannel, GetList(teamA, teamB));
                                break;
                            case "ACTIVE":
                                OnActive(connectionId);
                                break;
                            case "READY":
                                GetList(teamA, teamB).Find(x => x.connectionId == connectionId).rdy = !GetList(teamA, teamB).Find(x => x.connectionId == connectionId).rdy;
                                Send(msg, reliableChannel, GetList(teamA, teamB));
                                break;
                            case "START":
                                OnStartMatch(msg);
                                break;
                            default:
                                Debug.Log("Invalid message : " + msg + "\n");
                                break;
                        }
                        break;
                    case NetworkEventType.DisconnectEvent:
                        OnDisconnect(connectionId);
                        break;
                }
            } while (networkEvent != NetworkEventType.Nothing);

        }

        private void EndMatch(int win)
        {
            string msg = "MATCHEND|";
            Send(msg + (int)((MAX_CONNECTION - 1) / 2 * 500 * (win == 0 ? 1.5 : 0.9)) + "|" + ((MAX_CONNECTION - 1) / 2 + (win == 0 ? 3 : 5)), reliableChannel, teamA.ToList());
            Send(msg + (int)((MAX_CONNECTION - 1) / 2 * 500 * (win == 1 ? 1.5 : 0.9)) + "|" + ((MAX_CONNECTION - 1) / 2 + (win == 1 ? 3 : 5)), reliableChannel, teamB.ToList());
            SceneManager.UnloadSceneAsync("Server");
        }

        private void OnStartMatch(string msg)
        {
            matchStarted = true;
            Send(msg, reliableChannel, GetList(teamA, teamB));
        }

        private bool MatchEnd(out int win)
        {
            win = -1;
            var data = true;
            foreach(ServerClient team in teamA)
            {
                if (team != null && team.active > 0)
                {
                    data = false;
                }
                    
            }
            
            if(data)
            {
                win = 0;
                return data;
            }
            data = true;
            foreach (ServerClient team in teamB)
            {
                if (team != null && team.active > 0)
                    data = false;
            }
            if (data)
            {
                win = 1;
                return data;
            }
            return data;
        }

        private void OnActive(int cnnId)
        {
            GetList(teamA, teamB).Find(x => x.connectionId == cnnId).active--;
        }

        private void OnUpdateUnitHp(string[] data)
        {
            var player = GetList(teamA, teamB).Find(x => x.connectionId == int.Parse(data[0]));
            var unit = player.unitInfo[int.Parse(data[1]) - 1].soldiers.Find(x => x.name == data[2]);
            unit.hp -= int.Parse(data[3]);
            
            string msg = "UPDATEUNITHP|" + player.connectionId + "|" + (int.Parse(data[1])-1);
            foreach (Soldier sol in player.unitInfo[int.Parse(data[1])-1].soldiers)
                msg += "|" + sol.ToString();
            if (unit.hp <= 0)
            {
                unit.active = false;
            }
            Send(msg, reliableChannel, GetList(teamA, teamB));
        }

        private void OnUpdatePlayerHp(string[] data)
        {
            var player = GetList(teamA, teamB).Find(x => x.connectionId == int.Parse(data[1]));
            player.hp -= int.Parse(data[2]);
            string msg = "UPDATEPLAYERHP|" + player.connectionId + "|" + player.hp;

            Send(msg, reliableChannel, GetList(teamA, teamB));
        }

        private void OnInfoGet(string[] data, int cnnId, string msg)
        {
            ServerClient client = GetList(teamA, teamB).FirstOrDefault(x => x.connectionId == cnnId);
            client.playerName = data[0];
            client.type = data[2];
            client.level = int.Parse(data[3]);
            client.hp = int.Parse(data[4]);
            foreach (string unitInfo in data.Skip(5).Take(data.Length - 7))
            {
                string[] splitInfo = unitInfo.Split('$');
                UnitInfo unit = new UnitInfo(int.Parse(splitInfo[0]), splitInfo[1], int.Parse(splitInfo[4]), new int[]{ int.Parse(splitInfo[2]), int.Parse(splitInfo[3]) }, int.Parse(splitInfo[5]));
                client.unitInfo[int.Parse(splitInfo[5])] = unit;
                client.active++;
            }
            Send(msg, reliableChannel, GetList(teamA, teamB));
        }

        private void OnDisconnect(int connectionId)
        {
            Debug.Log(GetList(teamA, teamB).ToArray().Length);
            if (teamA.ToList().Find(x => x.connectionId == connectionId) != null)
            {
                for(int i = 0; i <= 5; i++)
                {
                    if(teamA[i] != null && teamA[i].connectionId == connectionId)
                    {
                        teamA[i] = null;
                        sizeA--;
                        break;
                    }
                }
                
            }
            else
            {
                for (int i = 0; i <= 5; i++)
                {
                    if (teamB[i] != null && teamB[i].connectionId == connectionId)
                    {
                        teamB[i] = null;
                        sizeB--;
                        break;
                    }
                }
            }
            string msg = "DC|" + connectionId;
            Send(msg, reliableChannel, GetList(teamA, teamB));
        }

        public void OnConnection(int cnnId)
        {
            string msg = "ASKINFO|" + cnnId + "|";
            ServerClient c = new ServerClient(cnnId);
            if (sizeA < sizeB)
            {
                for (int i = 0; i <= (MAX_CONNECTION - 1) / 2; i++)
                    if (teamA[i] == null)
                    {
                        sizeA++;
                        teamA[i] = c;
                        msg += "teamA|" + i + "|";
                        break;
                    }
            }
            else
            {
                for (int i = 0; i <= (MAX_CONNECTION - 1) / 2; i++)
                    if (teamB[i] == null)
                    {
                        sizeB++;
                        teamB[i] = c;
                        msg += "teamB|" + i + "|";
                        break;
                    }
            }
            foreach (ServerClient player in teamA)
            {
                if (player != null && player.playerName != "")
                    msg += player.ToString() + "&";
            }
            msg = msg.Trim('&');
            msg += "|";
            foreach (ServerClient player in teamB)
            {
                if (player != null && player.playerName != "")
                    msg += player.ToString() + "&";
            }
            msg = msg.Trim('&');
            Send(msg, reliableChannel, cnnId);
        }

        private void Send(string mess, int chennelId, int cnnId)
        {
            List<ServerClient> c = new List<ServerClient>();
            c.Add(GetList(teamA, teamB).Find(x => x.connectionId == cnnId));
            Send(mess, chennelId, c);
        }
        private void Send(string mess, int chennelId, List<ServerClient> c)
        {
            c.RemoveAll(x => x == null);
            byte[] msg = Encoding.Unicode.GetBytes(mess);
            foreach (ServerClient sc in c)
            {
                NetworkTransport.Send(hostId, sc.connectionId, chennelId, msg, mess.Length * sizeof(char), out error);
            }
        }
        private List<T> GetList<T>(T[] listA, T[] listB)
        {
            List<T> temp = new List<T>(listA.ToList());
            temp.AddRange(listB.ToList());
            temp.RemoveAll(x => x == null);
            return temp;
        }
    }
}
