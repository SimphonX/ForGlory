using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

namespace Assets.Scripts.ServerClient
{
    [Serializable]
    public class User
    {
        public string username;
        public string password;
        public string email;
    }

    public class JsonHelper
    {
        public static T[] getJsonArray<T>(string json)
        {
            Debug.Log(json=="");
            string newJson = "{ \"array\": " + json + "}";
            if (json != "")
            {
                Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
                return wrapper.array;
            }
            return new T[0];
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }



    [Serializable]
    public class PlayerCharacter
    {
        public string name;
        public string username;
        public string characterType;
        public int slot;
        public int str, cons, def;
        public int level, progress;
        public int gold;

        public override string ToString()
        {
            return name + "&" + characterType + "&" + slot + "&" + level + "&" + progress + "&" + gold + "&" + str + "&" + cons + "&" + def;
        }
    }
    [Serializable]
    public class ItemData
    {
        public int id;
        public int itemId;
        public int level;
        public string type;
        public bool inUse;
        public string nameCharacter;
        public int unitId;

        public override string ToString()
        {
            return level + "&" + type + "&" + itemId;
        }
    }
    [Serializable]
    public class UnitsData
    {
        public int id;
        public int level;
        public string type;
        public string nameCharacter;
        public int slot;
        public int progress;

        public override string ToString()
        {
            return level + "&" + type + "&" + nameCharacter + "&" + progress + "&" + slot + "&" + id;
        }
    }


    public class MainServer : MonoBehaviour
    {
        private const int MAX_CONNECTION = 100;

        //change to server to server
        List<long[]> activeMatch = new List<long[]>();
        bool matchCreated;
        bool matchJoined;
        MatchInfo matchInfo;
        NetworkMatch networkMatch;


        private int port = 20001;
        private const int MAXMESSAGESIZE = 65535;

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
            public string playerName = "TEMP";
            public Vector3 position = new Vector3(3, 1, 3);
            public ServerClient() { }
            public ServerClient(int cnnId)
            {
                connectionId = cnnId;
            }
        }

        void Awake()
        {

            //networkMatch = gameObject.AddComponent<NetworkMatch>();
        }
        void OnApplicationQuit()
        {
            NetworkTransport.Shutdown();
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
            GetIP();
        }
        private void GetIP()
        {
            string localIpAddress = string.Empty;
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface nic in nics)
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                IPv4InterfaceStatistics adapterStat = (nic).GetIPv4Statistics();
                UnicastIPAddressInformationCollection uniCast = (nic).GetIPProperties().UnicastAddresses;

                if (uniCast != null)
                {
                    foreach (UnicastIPAddressInformation uni in uniCast)
                    {
                        if (adapterStat.UnicastPacketsReceived > 0
                            && adapterStat.UnicastPacketsSent > 0
                            && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        {
                            if (uni.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                localIpAddress = nic.GetIPProperties().UnicastAddresses[1].Address.ToString();

                                break;
                            }
                        }
                    }
                }
            }
            textField.text+= localIpAddress+"\n";
        }

        private void Update()
        {
            if (!isStarted)
                return;

            int recHostId;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[MAXMESSAGESIZE];
            int bufferSize = MAXMESSAGESIZE;
            int dataSize;
            byte error;
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
            switch (recData)
            {
                case NetworkEventType.ConnectEvent:
                    OnConnection(connectionId);
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    textField.text += msg + "\n";
                    string[] splitData = msg.Split('|');

                    switch (splitData[0])
                    {
                        case "LOGIN":
                            StartCoroutine(OnLogIn(splitData[1], splitData[2], connectionId));
                            break;
                        case "REGISTER":
                            StartCoroutine(OnRegister(splitData[1], splitData[2], splitData[3], connectionId));
                            break;
                        case "SENDPASS":
                            StartCoroutine(OnForgotPass(splitData[1], connectionId));
                            break;
                        case "GETCHAR":
                            StartCoroutine(OnGetChar(splitData[1], connectionId));
                            break;
                        case "CREATECHAR":
                            StartCoroutine(CreateCharacter(splitData[1], splitData[2], int.Parse(splitData[3]), splitData[4], connectionId));
                            break;
                        case "DELETECHAR":
                            StartCoroutine(OnDeleteCharacter(splitData[1], splitData[2], connectionId));
                            break;
                        case "GETINUSEUNITS":
                            StartCoroutine(OnGetInUseUnits(splitData[1], connectionId));
                            break;
                        case "SETPROGRESS":
                            StartCoroutine(OnSetProgress(int.Parse(splitData[1]), int.Parse(splitData[2]), int.Parse(splitData[3]),bool.Parse(splitData[4]), connectionId));
                            break;
                        case "SETUNITPROGRESS":
                            StartCoroutine(OnSetUnitProgress(int.Parse(splitData[1]), int.Parse(splitData[2]), int.Parse(splitData[3]), connectionId));
                            break;
                        case "GETCHARITEMS":
                            StartCoroutine(OnGetPlayerItems(splitData[1], connectionId));
                            break;
                        case "UPGRADEITEM":
                            StartCoroutine(OnUpgradeItem(splitData[2], int.Parse(splitData[1]), int.Parse(splitData[3]), connectionId));
                            break;
                        case "UPGRADESTAT":
                            StartCoroutine(OnUpgradeStats(splitData[1], connectionId));
                            break;
                        case "DELETEUNIT":
                            StartCoroutine(OnDeleteUnit(int.Parse(splitData[1]), connectionId));
                            break;
                        case "CREATEUNIT":
                            StartCoroutine(OnCreateUnit(splitData[1],splitData[2],int.Parse(splitData[3]), connectionId));
                            break;
                        case "UPDATESLOT":
                            StartCoroutine(OnUpdateSlot(int.Parse(splitData[1]), int.Parse(splitData[2]), connectionId));
                            break;
                        case "REMOVEMATCH":
                            OnStartGame(connectionId);
                            break;
                        case "GAMESLIST":
                            OnGetGameList(connectionId);
                            break;
                        case "CREATEGAME":
                            OnCreateGame(Int64.Parse(splitData[1]), connectionId);
                            break;
                        default:
                            Debug.Log("Invalid message : " + msg);
                            break;
                    }
                    break;
                case NetworkEventType.DisconnectEvent:
                    OnDisconnect(connectionId);
                    break;
            }

        }

        private void OnCreateGame(long id, int cnnId)
        {
            activeMatch.Add(new long [] { id, cnnId});
        }

        private void OnGetGameList(int cnnId)
        {
            string msg = "ACTIVEMATCH";
            foreach (long[] match in activeMatch)
            {
                msg += "|" + match[0];
            }
            Send(msg, cnnId);
        }

        private void OnStartGame(int cnnId)
        {
            activeMatch.RemoveAll(x => x[1] == cnnId);
        }

        private IEnumerator OnUpdateSlot(int id, int pos, int cnnId)
        {
            UnityWebRequest www = UnityWebRequest.Put("http://localhost:5000/api/units/inuse/" + id + "/" + pos, "{}");

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (www.responseCode == 404 || www.responseCode == 402) Send("ERROR|character doesn't exist", cnnId);
                else Send("ERROR|" + www.error, cnnId);
            }
        }

        private IEnumerator OnCreateUnit(string type, string name,int gold, int cnnId)
        {
            string jsonPost = type + "|" + name + "|" + gold;
            textField.text += jsonPost + "\n";
            var request = new UnityWebRequest("http://localhost:5000/api/units", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPost);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                if (request.responseCode == 404 || request.responseCode == 402) Send("ERROR|", cnnId);
                else Send("ERROR|" + request.error, cnnId);
            }
            else
            {
                string msg = "CREATEUNIT|";
                string json = request.downloadHandler.text;
                UnitsData c = JsonUtility.FromJson<UnitsData>(json);
                msg += c.level + "|" + c.type + "|" + c.progress + "|" + c.slot + "|" + c.id;

                Send(msg, reliableChannel, cnnId);
            }
        }

        private IEnumerator OnDeleteUnit(int id, int cnnId)
        {
            UnityWebRequest www = UnityWebRequest.Delete("http://localhost:5000/api/units/" + id);

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (www.responseCode == 404 || www.responseCode == 402) Send("ERROR|character doesn't exist", cnnId);
                else Send("ERROR|" + www.error, cnnId);
            }
        }

        private IEnumerator OnUpgradeStats(string name, int cnnId)
        {
            var player = clients.Find(x => x.connectionId == cnnId);
            string jsonPost = "{ \""+name+"\":" + 1 + " }";
            textField.text += jsonPost + "\nhttp://localhost:5000/api/character/" + player.playerName + "\n";
            UnityWebRequest www = new UnityWebRequest("http://localhost:5000/api/character/" + player.playerName, "PUT");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPost);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (www.responseCode == 404 || www.responseCode == 402) Send("ERROR|", cnnId);
                else Send("ERROR|" + www.error, cnnId);
            }
            else
            {
                string msg = www.downloadHandler.text;
                Send(msg.Replace("\"", ""), reliableChannel, cnnId);
            }
        }

        private IEnumerator OnUpgradeItem(string name, int code, int gold, int cnnId)
        {
            string jsonPost = "{ 'code':" +code+", 'gold':"+gold+"}";
            UnityWebRequest www = new UnityWebRequest("http://localhost:5000/api/items/" + name, "PUT");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPost);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (www.responseCode == 404 || www.responseCode == 402) Send("ERROR|", cnnId);
                else Send("ERROR|" + www.error, cnnId);
            }
            else
            {
                string msg = www.downloadHandler.text;
                Send(msg.Replace("\"",""), reliableChannel, cnnId);
            }
        }

        private IEnumerator OnGetPlayerItems(string charName, int cnnId)
        {
            UnityWebRequest www = UnityWebRequest.Get("http://localhost:5000/api/items/char/" + charName);

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (www.responseCode == 404 || www.responseCode == 402) Send("ERROR|character doesn't exist", cnnId);
                else Send("ERROR|" + www.error, cnnId);
            }
            else
            {
                string json = www.downloadHandler.text;

                string msg = "INUSEITEMCHAR";
                ItemData[] c = JsonHelper.getJsonArray<ItemData>(json);
                textField.text += json + "\n";
                foreach (ItemData ch in c)
                {
                    msg += "|" + ch.ToString();
                }
                Send(msg, reliableChannel, cnnId);
            }
        }

        private IEnumerator OnSetUnitProgress(int progress, int level, int pos, int cnnId)
        {
            var player = clients.Find(x => x.connectionId == cnnId);
            string jsonPost = "{'progress':" + progress + ", 'level':" + level + "}";

            UnityWebRequest www = new UnityWebRequest("http://localhost:5000/api/units/progress/" + player.playerName+"/"+pos, "PUT");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPost);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (www.responseCode == 404 || www.responseCode == 402) Send("ERROR|Bed request", cnnId);
                else Send("ERROR|" + www.error, cnnId);
            }
            else
            {
                string msg = "METCHENDUNITOK|";
                string json = www.downloadHandler.text;
                UnitsData c = JsonUtility.FromJson<UnitsData>(json);
                msg += c.progress + "|" + c.level + "|" + c.slot + "|" + c.id;

                Send(msg, reliableChannel, cnnId);
            }
        }

        private IEnumerator OnSetProgress(int progress, int level, int gold, bool bar, int cnnId)
        {
            var player = clients.Find(x => x.connectionId == cnnId);
            string jsonPost = "{'progress':" + progress + ", 'level':" + level + ", 'gold':" + gold + "}";

            UnityWebRequest www = new UnityWebRequest("http://localhost:5000/api/character/progress/" + player.playerName, "PUT");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPost);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (www.responseCode == 404 || www.responseCode == 402) Send("ERROR|Bed request", cnnId);
                else Send("ERROR|" + www.error, cnnId);
            }
            else if(!bar)
            {
                string msg = "METCHENDOK|";
                string json = www.downloadHandler.text;
                PlayerCharacter c = JsonUtility.FromJson<PlayerCharacter>(json);
                msg += c.progress + "|" + c.gold + "|" + c.level;
                
                Send(msg, reliableChannel, cnnId);
            }
        }

        private void OnDisconnect(int connectionId)
        {
            activeMatch.RemoveAll(x => x[1] == connectionId);
            var player = clients.Remove(clients.Find(x => x.connectionId == connectionId));
        }

        private IEnumerator OnGetInUseUnits(string charName, int cnnId)
        {
            UnityWebRequest www = UnityWebRequest.Get("http://localhost:5000/api/units/" + charName);

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (www.responseCode == 404 || www.responseCode == 402) Send("ERROR|character doesn't exist", cnnId);
                else Send("ERROR|" + www.error, cnnId);
            }
            else
            {
                string json = www.downloadHandler.text;

                string msg = "UNITSINUSE";
                UnitsData[] c = JsonHelper.getJsonArray<UnitsData>(json);
                //textField.text += c[1].ToString() + "\n asdasdasd";
                foreach (UnitsData ch in c)
                {
                    msg += "|" + ch.ToString();
                }
                Send(msg, reliableChannel, cnnId);
            }
        }

        private IEnumerator OnDeleteCharacter(string charName, string username, int cnnId)
        {
            var request = new UnityWebRequest("http://localhost:5000/api/character/" + charName, "DELETE");

            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                if (request.responseCode == 404) Send("ERROR|character doesn't exist", cnnId);
                else Send("ERROR|" + request.error, cnnId);
            }
            else
            {
                StartCoroutine(OnGetChar(username, cnnId));
                Send("CNN|" + cnnId, reliableChannel, cnnId);
            }
        }

        private IEnumerator CreateCharacter(string charName, string charType, int slot, string username, int cnnId)
        {
            string jsonPost = "{'name':'" + charName + "', 'charactertype': '" + charType + "', 'username':'" + username + "', 'slot':'" + slot + "'}";
            var request = new UnityWebRequest("http://localhost:5000/api/character", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPost);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                if (request.responseCode == 409) Send("ERROR|Character name already exist", cnnId);
                else
                {
                    if (request.responseCode == 418) Send("ERROR|Server error", cnnId);
                    else Send("ERROR|" + request.error, cnnId);
                }
            }
            else
                StartCoroutine(OnGetChar(username, cnnId));
        }

        private IEnumerator OnForgotPass(string email, int cnnId)
        {
            UnityWebRequest www = UnityWebRequest.Get("http://localhost:5000/api/user/" + email);

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (www.responseCode == 404 || www.responseCode == 402) Send("ERROR|User with " + email + " email doesn't exist", cnnId);
                else Send("ERROR|" + www.error, cnnId);
            }
            else
            {
                string json = www.downloadHandler.text;
                SendMail(JsonUtility.FromJson<User>(json));
            }
        }

        private IEnumerator OnGetChar(string playerName, int cnnId)
        {
            UnityWebRequest www = UnityWebRequest.Get("http://localhost:5000/api/character/" + playerName);

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (www.responseCode == 404 || www.responseCode == 402) Send("ERROR|User with " + playerName + " username doesn't exist", cnnId);
                else Send("ERROR|" + www.error, cnnId);
            }
            else
            {
                string json = www.downloadHandler.text;

                string msg = "CHARINFO";
                if (json.Contains("{"))
                {
                    PlayerCharacter[] c = JsonHelper.getJsonArray<PlayerCharacter>(json);

                    foreach (PlayerCharacter ch in c)
                    {
                        msg += "|" + ch.ToString();
                    }
                }
                else msg += "|";
                Send(msg, reliableChannel, cnnId);
            }
        }
        private void SendMail(User user)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("zizimas@hotmail.com");

            mail.To.Add(user.email);

            mail.Subject = "Test Smtp Mail";

            mail.Body = "Testing SMTP mail from GMAIL";


            SmtpClient smtpServer = new SmtpClient("smtp.hotmail.com", 587);

            smtpServer.Credentials = new System.Net.NetworkCredential("zizimas@hotmail.com", "flatron1235") as ICredentialsByHost;

            smtpServer.EnableSsl = true;

            ServicePointManager.ServerCertificateValidationCallback =

            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)

            { return true; };

            smtpServer.Send(mail);
        }

        private IEnumerator OnRegister(string username, string password, string email, int cnnId)
        {
            string jsonPost = "{'username':'" + username + "', 'password': '" + password + "', 'email':'" + email + "'}";

            var request = new UnityWebRequest("http://localhost:5000/api/user", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPost);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                if (request.responseCode == 404 || request.responseCode == 402) Send("ERROR|Email or username already in use", cnnId);
                else Send("ERROR|" + request.error, cnnId);
            }
            else
            {
                ServerClient c = new ServerClient();
                c.connectionId = cnnId;
                c.playerName = username;
                clients.Add(c);
                Send("YES", reliableChannel, cnnId);
            }
        }

        private IEnumerator OnLogIn(string username, string password, int cnnId)
        {
            UnityWebRequest www = UnityWebRequest.Get("http://localhost:5000/api/user");
            www.SetRequestHeader("username", username);
            www.SetRequestHeader("password", password);

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (www.responseCode == 404 || www.responseCode == 402) Send("ERROR|User doesn't exist", cnnId);
                else Send("ERROR|" + www.error, cnnId);
            }
            else
            {
                ServerClient c = new ServerClient();
                c.connectionId = cnnId;
                c.playerName = username;
                clients.Add(c);
                Send("CNN|" + cnnId, reliableChannel, cnnId);
            }
        }

        private void OnConnection(int cnnId)
        {
            string msg = "ASKLOGIN";

            Send(msg, cnnId);
        }

        private void Send(string mess, int chennelId, int cnnId)
        {
            List<ServerClient> c = new List<ServerClient>();
            c.Add(clients.Find(x => x.connectionId == cnnId));
            Send(mess, chennelId, c);
        }
        private void Send(string mess, int cnnId)
        {
            List<ServerClient> c = new List<ServerClient>();
            c.Add(new ServerClient(cnnId));
            Send(mess, reliableChannel, c);
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