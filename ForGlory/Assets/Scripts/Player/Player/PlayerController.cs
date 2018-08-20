using Assets.Scripts.CanvasPanel.GameScrean;
using Assets.Scripts.Player.Items;
using Assets.Scripts.ServerClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Player.Player
{
    public class PlayerController : MonoBehaviour
    {
        public RuntimeAnimatorController mainPlayer;
        public RuntimeAnimatorController animationPlayer;
        public Animator animator;

        public int level = 0;
        public int progress;
        public int hPLeft;
        private const int BASEXP = 20;
        private const int DMGINC = 2;
        private const int HPINC = 25;
        private const int DEFINC = 2;
        private const float XPINC = 0.2f;
        private const float DMGINCLVL = 0.3f;
        private const float HPINCLVL = 0.3f;
        private const float DEFINCLVL = 0.3f;

        public int str = 10;
        public int cons = 10;
        public int def = 10;

        private int gold;

        private Vector3 privPos = Vector3.zero;
        private Vector3 nextPos = Vector3.zero;

        private Quaternion privRot = Quaternion.identity;
        private Quaternion nextRot = Quaternion.identity;

        public GameObject weapon;
        public GameObject tacticalCamera;
        public List<GameObject> equippedItems = new List<GameObject>();

        public Transform progressBar;
        public Transform infoField;

        private string playerName;

        private float updateInterval;

        private float interval = 10;

        private string type;

        public int Level { get { return level; } }

        public int Gold { get { return gold; } }

        public int Progress { get { return progress; } }

        public int HP { get { return (int)Mathf.Floor(cons * HPINC * (Mathf.Pow(Level, HPINCLVL))) + GetItemsHP(); } }

        private Vector3 playerPos;

        private Vector3 direction;

        public Vector3 GetDirection()
        {
            return direction;
        }

        internal void TakeDamage(int damage)
        {
            var damageDone = damage - (int)(GetDefence() * Random.Range(0.5f, 0.8f));
            var hPLeft = damageDone >= 20? damageDone : Random.Range(1, 20);
            GameObject client = GameObject.Find("Client");
            if (client != null && client.GetComponent<Client>().connected)
                client.GetComponent<Client>().UpdatePlayerHP(hPLeft, gameObject);
        }

        public void ChangeController()
        {
            animator.runtimeAnimatorController = animationPlayer;
        }

        public int HPLeft { get { return hPLeft; } set { hPLeft = value; } }

        public int HPProc { get { return hPLeft; } }

        public int STR { get { return str; } }
        public int CONST { get { return cons; } }
        public int DEF { get { return def; } }

        public int GetUpgradePoints()
        {
            return level * 2 - str - cons - def + 30;
        }

        public string PlayerName { get { return playerName; } }

        public string Type { get { return type; } }

        public int GetDamage()
        {
            return (int)Mathf.Floor(str * DMGINC * (Mathf.Pow(Level, DMGINCLVL)))+GetItemsDmg();
        }
        public int GetDefence()
        {
            return (int)Mathf.Floor(def * DEFINC * (Mathf.Pow(Level, DEFINCLVL)))+GetItemsDef();
        }

        void Awake()
        {
            playerPos = transform.position;
            progressBar = GameObject.Find("GameScreanCanvas").transform.GetChild(2).GetChild(10);
            infoField = GameObject.Find("GameScreanCanvas").transform.GetChild(2).GetChild(2);
            gameObject.SetActive(false);
            if(tacticalCamera != null)
                tacticalCamera.SetActive(false);
            //hP = hPLeft = (int)Mathf.Floor(cons * HPINC * (Mathf.Pow(Level, HPINCLVL)));
        }
        private void SetProgressBar()
        {
            progressBar.GetChild(0).GetComponent<Slider>().value = 100 * progress / NextLevel();
            progressBar.GetChild(1).GetComponent<Text>().text = NextLevel().ToString();
            progressBar.GetChild(3).GetComponent<Text>().text = progress.ToString();
        }

        private void SetInfoField()
        {
            infoField.GetChild(0).GetComponent<Image>().sprite = Resources.Load(Type, typeof(Sprite)) as Sprite;
            infoField.GetChild(1).GetComponent<Text>().text = playerName;
            infoField.GetChild(2).GetComponent<Text>().text = level.ToString();
        }

        void FixedUpdate()
        {
            direction = transform.position - playerPos;
            playerPos = transform.position;
            if (GetComponent<PlayerMovement>() != null)
                GetComponent<PlayerMovement>().SetSpeed(GetItemsMoveSpeed());
            weapon.GetComponent<Weapon>().SetSpeed(GetItemsAttSpeed());
            if (GameObject.Find("HealthUI") && GetComponent<PlayerMovement>() != null)
            {
                GameObject.Find("HealthUI").transform.GetChild(0).GetComponent<Slider>().value = 100 * hPLeft / HP;
                GameObject.Find("HealthUI").transform.GetChild(1).GetComponent<Text>().text = HP.ToString();
                GameObject.Find("HealthUI").transform.GetChild(3).GetComponent<Text>().text = hPLeft.ToString();
            }

            if (hPLeft <= 0 && level > 0)
            {
                if (tacticalCamera != null)
                {
                    OnDestroy();
                    gameObject.SetActive(false);
                }
                else
                    Destroy(gameObject);
                return;
            }
                
            if (nextPos == Vector3.zero && privPos == Vector3.zero)
                return;
            updateInterval += Time.deltaTime;

            //GetComponent<Rigidbody>().MovePosition(nextPos);
            transform.position = Vector3.Lerp(transform.position, nextPos, Time.deltaTime * interval);
            transform.GetChild(5).transform.rotation = Quaternion.Lerp(transform.GetChild(5).transform.rotation, nextRot, Time.deltaTime * interval);
        }

        internal void SetNextPos(Vector3 next, Vector3 rot, Vector3 mouseRot)
        {
            updateInterval = 0;

            privPos = transform.position;
            nextPos = next;

            privRot = transform.GetChild(5).transform.rotation;
            nextRot = Quaternion.Euler(rot);

            transform.GetChild(0).eulerAngles = mouseRot;

        }

        public void IncProgress(int value)
        {
            progress += value;
            SetProgressBar();
        }

        public void SetPlayerParams(string playerName, int level, string type, int progress, Vector3 pos, int gold)
        {
            this.gold = gold;
            SetPlayerParams(playerName, level, type, progress);
            SetPosition(pos);
        }

        public void SetPlayerParams(string playerName, int level, string type, int progress)
        {
            this.progress = progress;
            this.level = level;
            SetProgressBar();
            SetPlayerParams(playerName, type);
            transform.GetChild(3).gameObject.SetActive(true);
        }

        public void SetPlayerParams(string playerName, string type)
        {
            this.playerName = playerName;
            transform.GetChild(2).GetComponent<TextMesh>().text = playerName;
            hPLeft = HP;
            this.type = type;
            switch (type)
            {
                case "generic_knight":
                    weapon.AddComponent<SwordAndShield>();
                    break;
                case "generic_swordsman":
                    weapon.AddComponent<OneHandSword>();
                    break;
                case "generic_two_hands_swordsman":
                    weapon.AddComponent<TwoHandsSword>();
                    break;
            }
            GameObject.Find("MainMenu").GetComponent<MainMenu>().GetCharItems(playerName, gameObject);
            GameObject.Find("GameScrean").GetComponent<GameScrean>().SetPlayerSparite(gameObject, type);
            SetInfoField();
        }

        public void UpdatePlayerStats(int Str, int Cons, int Def)
        {
            def = Def;
            str = Str;
            cons = Cons;
            SetParams();
        }

        public void UpgradePlayerItem(int code, int gold)
        {
            this.gold = gold;
            equippedItems.ForEach(x => x.GetComponent<ItemsHandler>().IncLevel(code));
        }

        internal void SetPlayerItems(string[] items)
        {
            foreach(string temp in items)
            {
                string[] itemsInfo = temp.Split('&');
                
                if (!itemsInfo[1].Contains('_'))
                {
                    GameObject item = Instantiate(Resources.Load("Items/" + itemsInfo[1] + "Prefab", typeof(GameObject)), transform.GetChild(0)) as GameObject;
                    item.GetComponent<ItemsHandler>().SetLevel(int.Parse(itemsInfo[0]), int.Parse(itemsInfo[2]));
                    item.name = itemsInfo[1];
                    equippedItems.Add(item);
                }
                else
                {
                    weapon.GetComponent<ItemsHandler>().SetLevel(int.Parse(itemsInfo[0]), int.Parse(itemsInfo[2]));
                    equippedItems.Add(weapon);
                }
            }
            SetParams();
            gameObject.SetActive(true);
        }
        public void SetPlayerParams(string playerName, int level, string type, bool other)
        {
            this.level = level;
            SetPlayerParams(playerName, type);
            transform.GetChild(1).gameObject.SetActive(false);
        }

        private void SetParams()
        {
            hPLeft = HP;
            
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }
        void OnDestroy()
        {
            if (GetComponent<PlayerMovement>() == null)
                return;
            tacticalCamera.SetActive(true);
            tacticalCamera.transform.position = transform.position;
            GameObject.Find("GameScreanCanvas").transform.GetChild(4).gameObject.SetActive(true);
            GameObject.Find("GameScreanCanvas").transform.GetChild(2).gameObject.SetActive(false);
            GameObject client = GameObject.Find("Client");
            if (client != null && client.GetComponent<Client>().connected)
                client.GetComponent<Client>().SetActive();
        }

        public void LevelUp(int progress, int gold, bool bar = false)
        {
            var level = this.level;
            progress += this.progress;
            var formula = NextLevel();
            if (progress >= formula)
            {
                progress -= formula;
                level++;
            }
            GameObject.Find("MainMenu").GetComponent<MainMenu>().SaveProgress(progress, level, gold, bar);
        }

        public int NextLevel()
        {
            return (int)Mathf.Floor(BASEXP * (Mathf.Pow(Level + 1, XPINC)));
        }

        public void SetRewards(int progress, int gold, int level)
        {
            this.level = level;
            this.gold = gold;
            this.progress = progress;
            SetProgressBar();
            SetInfoField();
        }
        public void SetGold(int gold)
        {
            this.gold += gold;
        }
        public int GetItemsDmg()
        {
            int data = 0;
            foreach (GameObject item in equippedItems)
                data += item.GetComponent<ItemsHandler>().GetDamage();
            return data;
        }
        public int GetItemsDef()
        {
            int data = 0;
            foreach (GameObject item in equippedItems)
                data += item.GetComponent<ItemsHandler>().GetDefence();
            return data;
        }
        public int GetItemsHP()
        {

            int data = 0;
            foreach (GameObject item in equippedItems)
                data += item.GetComponent<ItemsHandler>().GetHP();
            return data;
        }
        public float GetItemsAttSpeed()
        {
            float data = 0;
            foreach (GameObject item in equippedItems)
                data += item.GetComponent<ItemsHandler>().GetAttackSpeed();
            return data;
        }
        public float GetItemsMoveSpeed()
        {
            float data = 0;
            foreach (GameObject item in equippedItems)
                data += item.GetComponent<ItemsHandler>().GetMoveSpeed();
            return data;
        }
    }
}
