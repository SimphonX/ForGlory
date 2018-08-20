using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Scripts.CanvasPanel.GameScrean;
using Assets.Scripts.CanvasPanel.Menu;
using Assets.Scripts.Player.Player;

namespace Assets.Scripts.ServerClient
{
    public class MainMenu : MonoBehaviour
    {

        private const int MAX_CONNECTION = 100;

        private const int port = 20001;

        private const string ipAdress = "127.0.0.1";

        public string connectServerIP { get; set; }
        public int connectPort { get; set; }

        private int hostId;
        private int webHostId;

        private int ourClientId;
        private int connectionId;

        private int reliableChannel;
        private int unreliableChannel;

        private float connectionTime;
        private bool isConnected = false;
        private byte error;
        private GameScrean game;

        private GameObject itemsCaller;

        private string playerName;

        public Text errorMsg;

        //public List<Player> players = new List<Player>();

        void Start()
        {
            errorMsg.text = "Server unavailable ";
            OnConnect();
        }
        void OnApplicationQuit()
        {
            NetworkTransport.Shutdown();
            Destroy(gameObject);
        }

        private void OnConnect()
        {

            NetworkTransport.Init();
            ConnectionConfig cc = new ConnectionConfig();

            reliableChannel = cc.AddChannel(QosType.Reliable);
            unreliableChannel = cc.AddChannel(QosType.Unreliable);

            HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

            hostId = NetworkTransport.AddHost(topo, 0);

            connectionId = NetworkTransport.Connect(hostId, ipAdress, port, 0, out error);
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

        internal void GetCharItems(string playerName, GameObject playerItems)
        {
            itemsCaller = playerItems;
            string msg = "GETCHARITEMS|" + playerName;

            Send(msg, reliableChannel);
        }

        internal void CreateGame(string networkId)
        {
            string msg = "CREATEGAME|" + networkId;
            Send(msg, reliableChannel);
        }

        private void Update()
        {
            if (!isConnected)
                return;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            byte error;
            NetworkEventType recData = NetworkTransport.ReceiveFromHost(hostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

            switch (recData)
            {
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    
                    errorMsg.text = "";
                    string[] splitData = msg.Split('|');
                    Debug.Log(msg);
                    switch (splitData[0])
                    {
                        case "ASKLOGIN":
                            errorMsg.text = "";
                            GameObject.Find("MainCanvas").GetComponent<MainCanvas>().ToLogIn();
                            break;
                        case "ERROR":
                            errorMsg.text = splitData[1];
                            break;
                        case "CNN":
                            LogIn(int.Parse(splitData[1]));
                            break;
                        case "YES":
                            GameObject.Find("MainCanvas").GetComponent<MainCanvas>().ToRegister(true);
                            break;
                        case "CHARINFO":
                            game = GameObject.Find("GameScrean").GetComponent<GameScrean>();
                            game.SetCharacters(splitData);
                            break;
                        case "UNITSINUSE":
                            game.SetPlayerUnits(splitData.Skip(1).ToArray());
                            break;
                        case "METCHENDOK":
                            OnSetProgress(int.Parse(splitData[1]), int.Parse(splitData[2]), int.Parse(splitData[3]));
                            break;
                        case "METCHENDUNITOK":
                            OnSetUnitProgress(int.Parse(splitData[1]), int.Parse(splitData[2]), int.Parse(splitData[3]), int.Parse(splitData[4]));
                            break;
                        case "INUSEITEMCHAR":
                            if (itemsCaller != null)
                                itemsCaller.GetComponent<Scripts.Player.Player.PlayerController>().SetPlayerItems(splitData.Skip(1).ToArray());
                            break;
                        case "UPGRADE":
                            if (itemsCaller != null)
                                itemsCaller.GetComponent<Scripts.Player.Player.PlayerController>().UpgradePlayerItem(int.Parse(splitData[2]),int.Parse(splitData[3]));
                            break;
                        case "UPDATESTATS":
                            if (itemsCaller != null)
                                itemsCaller.GetComponent<Scripts.Player.Player.PlayerController>().UpdatePlayerStats(int.Parse(splitData[1]), int.Parse(splitData[2]), int.Parse(splitData[3]));
                            break;
                        case "CREATEUNIT":
                            game.CreateUnit(splitData.Skip(1).ToArray());
                            break;
                        case "ACTIVEMATCH":
                            game.SetGameList(splitData.Skip(1).ToList());
                            break;
                        default:
                            Debug.Log("Invalid message : " + msg);
                            break;
                    }
                    break;
                case NetworkEventType.DisconnectEvent:
                    name = "GameServer";
                    SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                    errorMsg.text = "Server unavailable ";
                    Destroy(GameObject.Find("GameServer"));
                    break;
            }

        }

        internal void SetSlot(int id, int pos)
        {
            string msg = "UPDATESLOT|" + id + "|" + pos;
            Send(msg, reliableChannel);
        }

        internal void OnRemoveGame()
        {
            string msg = "REMOVEMATCH";

            Send(msg, reliableChannel);
        }

        public void CreateUnit(string name, string playerName, int price)
        {
            string msg = "CREATEUNIT|" + name + "|" + playerName + "|" + price;
            Send(msg, reliableChannel);
        }

        public void DeleteUnit(int id)
        {
            string msg = "DELETEUNIT|" + id;
            Send(msg, reliableChannel);
        }

        internal void SetStats(string name)
        {
            string msg = "UPGRADESTAT|"+name;
            Send(msg, reliableChannel);
        }

        private void OnSetUnitProgress(int progress, int level, int slot, int id)
        {
            GameObject.Find("ClickControler").GetComponent<InputController>().BackToMainUnit(progress, level, slot, id);
        }

        internal void GetGameList()
        {
            string msg = "GAMESLIST|";
            Send(msg, reliableChannel);
        }

        private void OnSetProgress(int progress, int gold, int level)
        {
            GameObject.Find("ClickControler").GetComponent<InputController>().BackToMain(progress, level, gold);
        }

        internal void GetCharInUseUnits(string charName)
        {
            string msg = "GETINUSEUNITS|";

            msg += charName;
            Send(msg, reliableChannel);
        }
        internal void OnCreateCharacter(string charName, string charType, int slot)
        {
            string msg = "CREATECHAR|";

            msg += charName + "|" + charType + "|" + slot + "|" + playerName;
            Send(msg, reliableChannel);
        }

        private void LogIn(int cnnId)
        {
            ourClientId = cnnId;
            Destroy(GameObject.Find("GameScrean"));
            SceneManager.LoadScene("GameScrean", LoadSceneMode.Single);
        }

        public void SetErrorMsgText(Text error)
        {
            errorMsg = error;
        }

        public void OnGetChar()
        {
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

        public void UpgradeItem(int code, string playerName, int gold)
        {
            string msg = "UPGRADEITEM|" + code + "|" + playerName+"|"+gold;

            Send(msg, reliableChannel);
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

        internal void SaveProgress(int progress, int level, int gold, bool bar)
        {
            string msg = "SETPROGRESS|"+progress+"|"+level+"|"+gold+"|"+bar;
            Send(msg, reliableChannel);
        }

        internal void SaveUnitProgress(int progress, int level, int pos)
        {
            string msg = "SETUNITPROGRESS|" + progress + "|" + level + "|" + pos;
            Send(msg, reliableChannel);
        }

        private void Register(string username, string password, string email)
        {
            string msg = "REGISTER|";

            msg += username + "|" + password + "|" + email;
            Send(msg, reliableChannel);
        }

        private void SendPassword(string email)
        {
            string msg = "SENDPASS|" + email;
            Send(msg, reliableChannel);
        }

        private void OnAskLogIn(string username, string password)
        {
            string msg = "LOGIN|";
            msg += username + "|";
            msg += password;
            Send(msg, reliableChannel);
        }

        private void Send(string mess, int chennelId)
        {
            byte[] msg = Encoding.Unicode.GetBytes(mess);
            NetworkTransport.Send(hostId, connectionId, chennelId, msg, mess.Length * sizeof(char), out error);
        }
    }
}
