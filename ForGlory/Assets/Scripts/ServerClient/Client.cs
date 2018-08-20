using Assets.Scripts.CanvasPanel.GameScrean;
using Assets.Scripts.Player;
using Assets.Scripts.Player.Player;
using Assets.Scripts.Player.Items;
using Assets.Scripts.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.ServerClient
{
    public class Player
    {
        public string playerName;
        public GameObject playerInfo;
        public GameObject player;
        public GameObject[] units;
        public int connectionId;
        public string team;
        public int num;
        public bool rdy;

        public Player()
        {
            units = new GameObject[3];
        }
    }

    public class Client : MonoBehaviour, IServerClient
    {
        bool m_MatchCreated;
        private MatchInfo m_MatchInfo;
        private NetworkMatch m_NetworkMatch;

        private const int MAX_CONNECTION = 100;
        private const int MAX_MESSAGE_SIZE = 65535;

        byte[] receiveBuffer;

        private int hostId;
        private int webHostId;

        private int ourClientId;
        private int connectionId;

        private int reliableChannel;
        private int unreliableChannel;

        private float connectionTime;
        private byte error;

        public bool connected = false;
        public bool isGameStarted = false;

        private string playerName;
        public GameObject playerPrefab;
        public GameObject unitPrefab;
        public GameObject playerCanvas;
        public GameObject playerListCanvas;
        public GameObject listInfoPrefab;
        private Player thisPlayer;
        public Text log;

        private InputController input;

        public List<Player> players = new List<Player>();
        void Awake()
        {
            input = GameObject.Find("ClickControler").GetComponent<InputController>();
            input.SetTextField(true);
            playerCanvas = GameObject.Find("PlayerHUB");
            log = GameObject.Find("ErrorText").GetComponent<Text>();
            m_NetworkMatch = gameObject.AddComponent<NetworkMatch>();
        }

        /*public MatchInfo GetMatchInfo()
        {
            return m_MatchInfo;
        }*/

        void Start()
        {
            playerCanvas.SetActive(false);
            playerListCanvas.SetActive(true);
            receiveBuffer = new byte[MAX_MESSAGE_SIZE];
            // While testing with multiple standalone players on one machine this will need to be enabled
            Application.runInBackground = true;
        }

        public virtual void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
        {
            if (success)
            {
                Utility.SetAccessTokenForNetwork(matchInfo.networkId, matchInfo.accessToken);

                m_MatchInfo = matchInfo;
                ConnectThroughRelay(matchInfo.address, matchInfo.port, matchInfo.networkId,
                    matchInfo.nodeId);
            }
            else
            {
                SceneManager.UnloadSceneAsync("Client");
            }
        }

        void ConnectThroughRelay(string relayIp, int relayPort, NetworkID networkId, NodeID nodeId)
        {
            SetupHost();

            byte error;
            NetworkTransport.ConnectToNetworkPeer(hostId, relayIp, relayPort, 0, 0, networkId, Utility.GetSourceID(), nodeId, out error);
            connected = true;
        }
        void SetupHost()
        {
            NetworkTransport.Init();
            ConnectionConfig cc = new ConnectionConfig();

            reliableChannel = cc.AddChannel(QosType.Reliable);
            unreliableChannel = cc.AddChannel(QosType.Unreliable);

            HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

            hostId = NetworkTransport.AddHost(topo);
        }
        public void StartClient(NetworkID networkId, string password)
        {
            m_NetworkMatch.JoinMatch(networkId, password, "", "", 0, 0, OnMatchJoined);
        }
        private void Update()
        {
            if (!connected)
                return;
            var rdyGame = true;
            if (GameObject.Find("Server") != null && players.ToArray().Length % 2 == 0)
            {
                foreach (Player pl in players)
                    if (!pl.rdy && pl.connectionId != ourClientId)
                        rdyGame = false;
            }
            else
                rdyGame = false;
            playerListCanvas.GetComponent<CharacterWindow>().SetStart(rdyGame);
                
            var networkEvent = NetworkEventType.Nothing;
            int channelId;
            int receivedSize;
            byte error;

            // Get events from the relay connection
            networkEvent = NetworkTransport.ReceiveRelayEventFromHost(hostId, out error);
            do
            {
                // Get events from the server/client game connection
                networkEvent = NetworkTransport.ReceiveFromHost(hostId, out connectionId, out channelId,
                receiveBuffer, (int)receiveBuffer.Length, out receivedSize, out error);
                if ((NetworkError)error != NetworkError.Ok)
                {
                    switch ((NetworkError)error)
                    {
                        case NetworkError.Timeout:

                            break;
                        default:
                            Debug.Log("Unexpected error : " + (NetworkError)error);
                            break;
                    }
                }

                switch (networkEvent)
                {
                    case NetworkEventType.DataEvent:
                        string msg = Encoding.Unicode.GetString(receiveBuffer, 0, receivedSize);
                        
                        string[] splitData = msg.Split('|');
                        if(splitData[0] != "PLAYERPOS")
                            Debug.Log("Client Rec : " + msg);
                        switch (splitData[0])
                        {
                            case "ASKINFO":
                                Debug.Log(msg);
                                OnAskInfo(splitData.Skip(1).ToArray());
                                break;
                            case "CNN":
                                OnConnect(splitData.Skip(1).ToArray());
                                break;
                            case "UPDATEPOS":
                                OnUpdateDest(splitData.Skip(1).ToArray());
                                break;
                            case "ATTACKUNIT":
                                OnAttackUnit(splitData.Skip(1).ToArray());
                                break;
                            case "PLAYERPOS":
                                OnUpdatePLayerPosition(splitData.Skip(1).ToArray());
                                break;
                            case "UPDATEPLAYERHP":
                                OnUpdatePlayerHP(splitData.Skip(1).ToArray());
                                break;
                            case "ATTCKMOVE":
                                OnAttackMove(int.Parse(splitData[1]));
                                break;
                            case "UPDATEUNITHP":
                                OnUpdateUnitHp(splitData.Skip(1).ToArray());
                                break;
                            case "DC":
                                OnDisconnectPlayer(int.Parse(splitData[1]));
                                break;
                            case "SETSTATE":
                                OnSetState(int.Parse(splitData[1]), bool.Parse(splitData[2]));
                                break;
                            case "READY":
                                SetReady(int.Parse(splitData[1]));
                                break;
                            case "START":
                                StartMatch();
                                break;
                            case "MATCHEND":
                                SaveMetchEnd(splitData.Skip(1).ToArray());
                                break;
                            default:
                                Debug.Log("Invalid message : " + msg);
                                break;
                        }
                        break;
                }
            } while (networkEvent != NetworkEventType.Nothing);
        }

        private void SaveMetchEnd(string[] splitData)
        {
            GameObject.Find("GameScreanCanvas").transform.GetChild(6).GetComponent<ResultWindow>().SetGoldAndXP(int.Parse(splitData[0]), int.Parse(splitData[1]));
            thisPlayer.player.GetComponent<Scripts.Player.Player.PlayerController>().LevelUp(int.Parse(splitData[1]), int.Parse(splitData[0]));
            foreach (GameObject unit in thisPlayer.units)
                if (unit != null)
                    unit.GetComponent<UnitGroup>().LevelUp(int.Parse(splitData[1]));
            SceneManager.UnloadSceneAsync("Client");
        }

        private void StartMatch()
        {
            playerListCanvas.SetActive(false);
            playerCanvas.SetActive(true);
            playerCanvas.transform.Find("ActionButton").gameObject.SetActive(false);
            playerCanvas.transform.Find("GameSelectWindow").gameObject.SetActive(false);
            playerCanvas.transform.Find("PlayerStatus").gameObject.SetActive(false);
            input.SetTextField(false);
        }

        private void SetReady(int cnnId)
        {
            var player = players.Find(x => x.connectionId == cnnId);
            player.rdy = !player.rdy;
            player.playerInfo.transform.parent.GetComponent<Image>().color = player.rdy ? Color.green : Color.red;
        }

        internal void ReadyGame()
        {
            string msg = "READY|" + ourClientId;

            Send(msg, reliableChannel);
        }
        internal void StartGame()
        {
            string msg = "START|" + ourClientId;

            Send(msg, reliableChannel);
        }

        private void ErrorReport(bool success, string extendedInfo)
        {
            if (!success)
                GameObject.Find("ErrorText").GetComponent<Text>().text = extendedInfo;
        }

        private void OnSetState(int cnnId, bool state)
        {
            var client = players.Find(x => x.connectionId == cnnId);
            foreach(GameObject unit in client.units)
            {
                if (unit != null)
                    unit.GetComponent<UnitGroup>().Stay = state;
            }
        }

        public void SetActive()
        {
            string msg = "ACTIVE|" + ourClientId;

            Send(msg, reliableChannel);
        }

        internal void SetUnitState(bool type)
        {
            string msg = "SETSTATE|" + ourClientId + "|" + type;

            Send(msg, reliableChannel);
        }

        private void OnUpdateUnitHp(string[] data)
        {
            var unit = players.Find(x => x.connectionId == int.Parse(data[0])).units[int.Parse(data[1])];
            if (unit == null)
                return;
            foreach (string str in data.Skip(2)) 
            {
                var splitInfo = str.Split('$');
                if(unit.transform.Find(splitInfo[1]))
                    unit.transform.Find(splitInfo[1]).GetComponent<Soldier>().HpLeft = int.Parse(splitInfo[0]);
            }
        }

        private void OnAttackMove(int cnnId)
        {
            if (cnnId == ourClientId)
                return;
            var player = players.Find(x => x.connectionId == cnnId);
            if (player == null)
                return;
            player.player.transform.GetChild(5).gameObject.SetActive(true);
            player.player.transform.GetChild(5).GetChild(0).GetComponent<Weapon>().SetAttack(true);

        }

        private void OnUpdatePlayerHP(string[] data)
        {
            var player = players.Find(x => x.connectionId == int.Parse(data[0]));
            if (player == null)
                return;
            player.player.GetComponent<Scripts.Player.Player.PlayerController>().HPLeft = int.Parse(data[1]);
        }

        public void UpdateUnitHP(int hpLeft, GameObject group, GameObject unit)
        {
            var player = players.Find(x => x.units.Contains(group));
            string msg = "UPDATEUNITHP|" + player.connectionId + "|" + group.transform.parent.name.Last() + "|" + unit.name + "|" + hpLeft;

            Send(msg, reliableChannel);
        }

        private void OnDisconnectPlayer(int cnnId)
        {
            var player = players.Find(x => x.connectionId == cnnId);

            Destroy(player.player);
            foreach (GameObject unit in player.units)
                Destroy(unit);
            Destroy(player.playerInfo);
            players.Remove(player);
        }

        internal void UpdatePlayerHP(int damage, GameObject playerData)
        {
            var name = playerData.transform.parent.parent.name;
            foreach (Player pl in players)
            {
                Debug.Log(pl.team == (name.First() == 'A' ? "teamA" : "teamB"));
                Debug.Log(pl.num == (int.Parse(name.Last().ToString()) - 1));
                Debug.Log(pl.team + " " + pl.num);
            }
                
            var player = players.Find(x => x.team == (name.First()=='A'?"teamA":"teamB") && x.num == int.Parse(name.Last().ToString())-1);
            string msg = "UPDATEPLAYERHP|" + ourClientId + "|" + player.connectionId + "|" + damage;
            
            Debug.Log(msg);
            Send(msg, reliableChannel);
        }

        public void PlayerAttackMove()
        {
            string msg = "ATTCKMOVE|" + ourClientId;

            Send(msg, reliableChannel);
        }

        private void OnUpdatePLayerPosition(string[] playerInfo)
        {
            if (ourClientId == int.Parse(playerInfo[0]))
                return;
            var player = players.Find(x => x.connectionId == int.Parse(playerInfo[0]));
            if (player != null && player.player != null && player.player.GetComponent<Scripts.Player.Player.PlayerController>() != null)
                player.player.GetComponent<Scripts.Player.Player.PlayerController>().SetNextPos(new Vector3(float.Parse(playerInfo[1]), float.Parse(playerInfo[2]), float.Parse(playerInfo[3])), new Vector3(float.Parse(playerInfo[4]), float.Parse(playerInfo[5]), float.Parse(playerInfo[6])), new Vector3(float.Parse(playerInfo[7]), float.Parse(playerInfo[8]), float.Parse(playerInfo[9])));
        }

        private void OnUpdateDest(string[] splitdata)
        {
            if (int.Parse(splitdata[0]) == ourClientId)
                return;
            var player = players.Find(x => x.connectionId == int.Parse(splitdata[0]));
            if (int.Parse(splitdata[1]) != -1)
                player.units[int.Parse(splitdata[1])].GetComponent<UnitGroup>().MoveUnit(new Vector3(float.Parse(splitdata[2]), float.Parse(splitdata[3]), float.Parse(splitdata[4])));
            else
            {
                if (player.units[0] != null)
                    player.units[0].GetComponent<UnitGroup>().MoveUnit(new Vector3(float.Parse(splitdata[2]), float.Parse(splitdata[3]), float.Parse(splitdata[4])));
                if (player.units[1] != null)
                    player.units[1].GetComponent<UnitGroup>().MoveUnit(new Vector3(float.Parse(splitdata[2]) - 5, float.Parse(splitdata[3]), float.Parse(splitdata[4])));
                if (player.units[2] != null)
                    player.units[2].GetComponent<UnitGroup>().MoveUnit(new Vector3(float.Parse(splitdata[2]) + 5, float.Parse(splitdata[3]), float.Parse(splitdata[4])));
            }

        }

        internal void AttackUnit(GameObject enemy, int unit)
        {
            if (unit != -1 && thisPlayer.units[unit] == null)
                return;
            string msg = "ATTACKUNIT|" + ourClientId + "|" + unit + "|" + enemy.transform.parent.name.Last() + "|" + enemy.transform.parent.parent.name.First() + "|" + enemy.transform.parent.parent.name.Last();
            Send(msg, reliableChannel);
        }

        private void OnAttackUnit(string[] splitdata)
        {
            if (int.Parse(splitdata[0]) == ourClientId)
                return;
            var player = players.Find(x => x.connectionId == int.Parse(splitdata[0]));
            var enemy = players.Find(x => x.team == (splitdata[3] == "A" ? "teamA" : "teamB") && x.num == int.Parse(splitdata[4]) - 1);

            if (int.Parse(splitdata[1]) != -1)
                player.units[int.Parse(splitdata[1])].GetComponent<UnitGroup>().AttackUnit(enemy.units[int.Parse(splitdata[2]) - 1]);
            else
            {
                if (player.units[0] != null)
                    player.units[0].GetComponent<UnitGroup>().AttackUnit(enemy.units[int.Parse(splitdata[2]) - 1]);
                if (player.units[1] != null)
                    player.units[1].GetComponent<UnitGroup>().AttackUnit(enemy.units[int.Parse(splitdata[2]) - 1]);
                if (player.units[2] != null)
                    player.units[2].GetComponent<UnitGroup>().AttackUnit(enemy.units[int.Parse(splitdata[2]) - 1]);
            }
        }

        public void UpdateDestination(Vector3 pos, int unit)
        {
            if (unit != -1 && thisPlayer.units[unit] == null)
                return;
            string msg = "UPDATEPOS|" + ourClientId + "|" + unit + "|" + pos.x + "|" + pos.y + "|" + pos.z;
            Send(msg, reliableChannel);
        }

        public void UpdatePlayerPosition(Vector3 pos, Vector3 rot, Vector3 mouseRot)
        {
            string msg = "PLAYERPOS|" + ourClientId + "|" + pos.x + "|" + pos.y + "|" + pos.z + "|" + rot.x + "|" + rot.y + "|" + rot.z + "|" + mouseRot.x + "|" + mouseRot.y + "|" + mouseRot.z;
            Send(msg, unreliableChannel);
        }

        private void OnConnect(string[] data)
        {
            if (int.Parse(data[1]) != ourClientId)
            {
                SpawnPlayer(data.Take(data.Length - 2).ToArray(), data[data.Length-2], int.Parse(data[data.Length - 1]), data[data.Length - 2].Equals("teamA") ? "A" : "B", 5, false);
            }
        }

        private void OnAskInfo(string[] splitData)
        {
            
            Player player = new Player();
            player.connectionId = int.Parse(splitData[0]);
            ourClientId = player.connectionId;
            player.team = splitData[1];
            player.rdy = false;
            player.num = int.Parse(splitData[2]);
            player.player = GameObject.FindGameObjectWithTag("Player");
            player.playerName = player.player.transform.GetChild(2).GetComponent<TextMesh>().text;
            player.units = GameObject.Find("ClickControler").GetComponent<InputController>().Units;
            thisPlayer = player;
            SetPosition(player, player.team.Last().ToString());
            SetInfoWindow(player);
            players.Add(player);
            int i = 1;
            foreach (string units in splitData[3].Split('&'))
                if (units != "")
                    SpawnPlayer(units.Split('^'), "teamA", i++, "A",5, bool.Parse(units.Split('^')[4]));
            i = 0;
            foreach (string units in splitData[4].Split('&'))
                if (units != "")
                    SpawnPlayer(units.Split('^'), "teamB", i++, "B",5, bool.Parse(units.Split('^')[4]));
            string msg = "CNN|" + player.playerName + "|" + player.connectionId + "|" +
                player.player.GetComponent<Scripts.Player.Player.PlayerController>().Type + "|" +
                player.player.GetComponent<Scripts.Player.Player.PlayerController>().Level + "|" + player.player.GetComponent<Scripts.Player.Player.PlayerController>().HP;
            i = 0;
            foreach (GameObject unit in player.units)
            {
                if (unit != null)
                    msg += "|" + unit.GetComponent<UnitGroup>().GetComponentInChildren<ControllUnit>().Level + "$" + unit.GetComponent<UnitGroup>().type.name + "$" + unit.GetComponent<UnitGroup>().GetSize() + "$" + unit.GetComponent<UnitGroup>().Hp + "$" + unit.GetComponent<UnitGroup>().PozitionHUB;
            }
            msg += "|" + player.team + "|" + player.num;
            Send(msg, reliableChannel);
        }

        private void SetInfoWindow(Player player)
        {
            player.playerInfo = Instantiate(listInfoPrefab, playerListCanvas.transform.GetChild(player.team.Equals("teamA") ? 0 : 1).GetChild(player.num));
            playerListCanvas.transform.GetChild(player.team.Equals("teamA") ? 0 : 1).GetChild(player.num).GetComponent<Image>().color = Color.red;
            player.playerInfo.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(player.player.GetComponent<Scripts.Player.Player.PlayerController>().Type);
            player.playerInfo.transform.GetChild(1).GetComponent<Text>().text = player.playerName;
            player.playerInfo.transform.GetChild(2).GetComponent<Text>().text = player.player.GetComponent<Scripts.Player.Player.PlayerController>().Level.ToString();
            for(int i = 0; i < 3; i++)
            {
                if(player.units[i] != null)
                {
                    player.playerInfo.transform.GetChild(3 + i).gameObject.SetActive(true);
                    player.playerInfo.transform.GetChild(3 + i).GetChild(0).GetComponent<Image>().sprite = player.units[i].GetComponent<UnitGroup>().Sprite;
                    player.playerInfo.transform.GetChild(3 + i).GetChild(1).GetComponent<Text>().text = player.units[i].GetComponent<UnitGroup>().Level.ToString();
                }
                
            }
        }

        private void SetPosition(Player player, string team)
        {
            Debug.Log(team + "SpawnPoint" + (player.num + 1));
            var spawn = GameObject.Find(team + "SpawnPoint" + (player.num + 1));
            player.player.transform.position = spawn.transform.GetChild(0).position;
            for (int i = 1; i <= 3; i++)
                if (player.units[i - 1] != null)
                    player.units[i - 1].GetComponent<UnitGroup>().SetStartPosition(spawn.transform.GetChild(i));
        }

        private void SpawnPlayer(string[] unitinfo, string team, int poz, string sim, int skip, bool rdy)
        {
            Player playerO = new Player();
            playerO.playerName = unitinfo[0];
            playerO.connectionId = int.Parse(unitinfo[1]);
            playerO.num = poz;
            playerO.team = team;
            GameObject avatar = Instantiate(playerPrefab, GameObject.Find(sim + "SpawnPoint" + (poz + 1)).transform.GetChild(0));
            if (!team.Equals(thisPlayer.team))
                avatar.name = "Enemy";
            avatar.GetComponent<Scripts.Player.Player.PlayerController>().SetPlayerParams(playerO.playerName, int.Parse(unitinfo[3]), unitinfo[2], true);
            avatar.GetComponent<Scripts.Player.Player.PlayerController>().ChangeController();
            playerO.player = avatar;
            playerO.rdy = rdy; 
            int i = 0;
            foreach (string info in unitinfo.Skip(skip))
            {
                i = int.Parse(info.Split('$')[5]);
                GameObject unit = Instantiate(unitPrefab, GameObject.Find(sim + "SpawnPoint" + (poz + 1)).transform.GetChild(i + 1));
                CreateUnit(unit, info.Split('$'), !team.Equals(thisPlayer.team) ? "Enemy" : "NotEnemy", avatar);
                playerO.units[i] = unit;
            }
            SetInfoWindow(playerO);
            players.Add(playerO);
        }
        private void CreateUnit(GameObject other, string[] info, string name, GameObject player)
        {
            UnitGroup type = null;
            switch (info[1])
            {
                case "generic_archer":
                    type = other.AddComponent<ArcherGroup>();
                    break;
                case "generic_swordsman":
                    type = other.AddComponent<Swordsman>();
                    break;
                case "generic_knight":
                    type = other.AddComponent<KnightGroup>();
                    break;
            }
            type.SetParams(int.Parse(info[0]));
            type.Init(name.Equals("Enemy") ? Color.red : Color.blue, name);
            type.SetSoldierStatus();
            type.SetPlayer(player);
        }
        private void RemovePlayer(int cnnId)
        {
            players.Remove(players.Find(x => x.connectionId == cnnId));
        }

        private void Send(string mess, int chennelId)
        {
            if (!connected)
                return;
            byte[] msg = Encoding.Unicode.GetBytes(mess);
            NetworkTransport.Send(hostId, connectionId, chennelId, msg, mess.Length * sizeof(char), out error);
            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError("Failed to send message: " + (NetworkError)error);
        }
        public void OnDisable()
        {
            m_NetworkMatch.DropConnection(m_MatchInfo.networkId, m_MatchInfo.nodeId, 0, null);
            NetworkTransport.Disconnect(hostId, connectionId, out error);
        }

        public void OnConnectionDropped(bool success, string extendedInfo)
        {
            OnDisable();
            hostId = -1;
            players.Clear();
            m_MatchInfo = null;
        }
        
    }
}