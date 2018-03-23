using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>> (newJson);
        return wrapper.array;
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
    public int x, y, z;

    public override string ToString()
    {
        return name + "&" + characterType + "&" + slot + "&" + x + "^" + y + "^" + z;
    }
}


public class MainServer : MonoBehaviour {

    private const int MAX_CONNECTION = 100;

    private int port = 5100;

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
                    default:
                        Debug.Log("Invalid message : " + msg);
                        break;
                }
                break;
            case NetworkEventType.DisconnectEvent:

                break;
        }

    }

    private IEnumerator OnDeleteCharacter(string charName, string username, int cnnId)
    {
        var request = new UnityWebRequest("http://localhost:5000/api/character/"+charName, "DELETE");

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
        {
            textField.text += "sdasdasdasd\n";
            StartCoroutine(OnGetChar(username, cnnId));
        }
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
        textField.text += playerName + "\n";
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
            textField.text += json + "\n";
            
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
            Send(msg, reliableChannel,cnnId);
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
            Send("CNN|" + cnnId, reliableChannel, cnnId);
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
            if(www.responseCode == 404 || www.responseCode == 402) Send("ERROR|User doesn't exist", cnnId);
            else Send("ERROR|"+www.error, cnnId);
        }
        else
        {
            ServerClient c = new ServerClient();
            c.connectionId = cnnId;
            c.playerName = username;
            clients.Add(c);
            Send("CNN|"+cnnId, reliableChannel, cnnId);
        }
    }

    private void OnConnection(int cnnId)
    {
        string msg = "ASKLOGIN";
        textField.text += msg;
        
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
