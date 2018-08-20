using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Assets.Scripts.ServerClient;
using UnityEngine.Networking.Match;
using UnityEngine.Events;
using UnityEngine.Networking.Types;

namespace Assets.Scripts.Player
{
    class GameManager:MonoBehaviour
    {
        
        private NetworkMatch m_NetworkMatch;
        public GameObject infoFieldPrefab;
        public GameObject passwordField;
        [SerializeField] public GameObject contentField;
        private List<MatchInfoSnapshot> m_MatchList = new List<MatchInfoSnapshot>();
        private string password;

        private List<string> activeMatch;

        void Start()
        {
            m_NetworkMatch = gameObject.AddComponent<NetworkMatch>();
        }

        public void OnCreatGame()
        {
            GameObject fieldInfo = GameObject.Find("MoreInfo");
            var iField = fieldInfo.GetComponentsInChildren<InputField>();
            var badFields = iField.Where(x => x.text == "" && x.name != "PasswordInput").ToArray().Length;

            foreach (InputField field in iField)
                field.image.color = field.text == "" && field.name != "PasswordInput" ? Color.red: Color.white;
            if (badFields == 0)
            {
                StartCoroutine(RunServer(iField));
                fieldInfo.transform.gameObject.SetActive(false);
                fieldInfo.transform.parent.gameObject.SetActive(false);
            }
        }

        private IEnumerator RunServer(InputField[] iField)
        {
            password = iField[2].text;
            var asyncLoadLevel = SceneManager.LoadSceneAsync("Server", LoadSceneMode.Additive);
            while (!asyncLoadLevel.isDone)
            {
                yield return null;
            }
            GameObject.Find("Server").GetComponent<Server>().StartServer(iField[0].text, iField[2].text, uint.Parse(iField[1].text));
            while (GameObject.Find("Server").GetComponent<Server>().GetMatchInfo() == null)
            {
                yield return null;
            }
            m_NetworkMatch.ListMatches(0, 20, "", false, 0, 0, OnMatchListServer);
            iField[0].text = "New game";
            iField[1].text = "4";
            iField[2].text = "";
        }

        private void OnMatchListServer(bool success, string extendedInfo, List<MatchInfoSnapshot> responseData)
        {
            if (success && responseData != null)
            {
                foreach(var match in responseData)
                    if(match.networkId == GameObject.Find("Server").GetComponent<Server>().GetMatchInfo().networkId)
                    {
                        GameObject.Find("MainMenu").GetComponent<MainMenu>().CreateGame(match.networkId.ToString());
                        JoinGame(match, password);
                    }
                        
            }
        }

        public void SetUpList(List<string> match)
        {
            activeMatch = match;
            m_NetworkMatch.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
        }
        public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
        {
            if (success && matches != null)
            {
                SetUpList(matches);
            }
        }
        private void SetUpList(List<MatchInfoSnapshot> matches)
        {
            RectTransform tran = contentField.transform as RectTransform;
            tran.sizeDelta = new Vector2(0, (40 * (contentField.transform.childCount)) - 443);
            for (int i = 0; i < contentField.transform.childCount; i++)
                Destroy(contentField.transform.GetChild(i).gameObject);

            
            foreach (var dataStr in matches)
            {
                if(activeMatch.FirstOrDefault(x => x.Equals(dataStr.networkId.ToString())) != null)
                {
                    GameObject newItem = Instantiate(infoFieldPrefab) as GameObject;
                    var fields = newItem.GetComponentsInChildren<Text>().Take(4).ToArray();
                    fields[0].text = dataStr.name;
                    fields[1].text = (dataStr.currentSize - 1) + "/" + (dataStr.maxSize - 1);
                    fields[2].text = dataStr.isPrivate ? "Yes" : "No";
                    newItem.GetComponentInChildren<Button>().onClick.AddListener(delegate { JoinGame(dataStr); });
                    newItem.transform.SetParent(contentField.transform, false);
                }
            }
        }
        private void JoinGame(MatchInfoSnapshot matchInfo, string pass = "", bool test = true)
        {
            if (matchInfo.isPrivate && pass == "")
            {
                passwordField.SetActive(test);
                InputField text = passwordField.transform.GetChild(3).GetComponent<InputField>();
                passwordField.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(delegate { JoinGame(matchInfo, text.text, false); });
                return;
            }
            StartCoroutine(StartClient(matchInfo.networkId, pass));
            UnloadScene<GameObject>("HUBArea");
        }
        private IEnumerator StartClient(NetworkID networkID, string pass)
        {
            var asyncLoadClient = SceneManager.LoadSceneAsync("Client", LoadSceneMode.Additive);
            while (!asyncLoadClient.isDone)
            {
                yield return null;
            }
            GameObject.Find("Client").GetComponent<Client>().StartClient(networkID, pass);
        }

        public void KillAllServersAndClients()
        {
            UnloadScene<Server>("Client", true);
            UnloadScene<Client>("Server", true);
        }

        private void UnloadScene<T>(string name,bool type = false)
        {
            if (SceneManager.GetSceneByName(name).isLoaded)
            {
                if (type)
                {
                    var info = GameObject.Find(name).GetComponent<T>() as IServerClient;
                    info.OnDisable();
                }
                SceneManager.UnloadSceneAsync(name);
            }
        }

    }
}
