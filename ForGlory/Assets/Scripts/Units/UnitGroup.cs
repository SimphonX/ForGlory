using Assets.Scripts.CanvasPanel.GameScrean;
using Assets.Scripts.ServerClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Units
{
    
    public abstract class UnitGroup : MonoBehaviour
    {
        protected int hp;
        protected int dmg;
        protected int def;
        protected int price;
        public int id;
        public int Hp { get { return hp * controll.GetSize(); }  }
        public int Dmg { get { return dmg * controll.GetSize(); } }
        public int Def { get { return def * controll.GetSize(); } }
        public int Price { get { return price; } }
        public int Id { get { return id; } }

        public int progress;

        public int Progress { get { return progress; } }

        public int MAXDISTANCE;
        public int MINDISTANCE;
        public int MIDDISTANCE;
        
        private const int BASEXP = 20;
        private const float XPINC = 0.2f;

        protected int level;

        protected ControllUnit controll;
        protected ParticleSystem particalSystem;
        protected BoxCollider rayBox;
        public GameObject player;
        public GameObject barracks;

        public GameObject arrow;
        public Sprite type;
        public string targetType;
        public string typeName;

        public Vector3 movePos;
        public bool attack, move; //idle;
        public bool started = false;
        public bool stay = false;
        public float timeLeft = 0;
        public GameObject enemy;
        private int pozitionHUB = -1;
        protected string unitName;

        protected bool yourUnit;

        protected int pos = -1;

        void Disable()
        {
            if (!yourUnit)
                return;
            GameObject client = GameObject.Find("Client");
            if (client != null && client.GetComponent<Client>().connected)
                client.GetComponent<Client>().SetActive();
            gameObject.SetActive(false);
        }

        public int Level { get { return level; } }

        public Sprite Sprite { get { return type; } }

        public bool Stay { get { return stay; } set { stay = value; if (value) MoveUnit(transform.GetChild(0).position); if (value) move = false; if (value) attack = false; } }

        public int PozitionHUB { get { return pozitionHUB; } }

        public GameObject Barracks { get {return barracks; }  }

        public string UnitName { get { return unitName; } }

        protected virtual void Awake()
        {
            
            attack = move = /*idle =*/ false;
            particalSystem = transform.GetChild(1).GetComponent<ParticleSystem>();
            rayBox = transform.GetChild(0).GetChild(0).GetComponent<BoxCollider>();
            particalSystem.transform.localScale += new Vector3(20, 18, 0);
            arrow = Resources.Load("ArrowObject", typeof(GameObject)) as GameObject;
            controll = transform.GetChild(0).GetComponent<ControllUnit>();
            controll.type = type;
            movePos = controll.transform.position;
        }

        public void SetPlayer(GameObject player, GameObject barracks = null)
        {
            this.player = player;
            this.barracks = barracks;
        }
        void Start()
        {
            
        }
        public abstract void SetSoldierStatus();
        public void SetParams(int level, int progress)
        {
            if (barracks != null)
            {
                barracks.transform.GetChild(1).GetComponent<Text>().text = level.ToString();
            }
            this.level = level;
            this.progress = progress;
        }

        public void SetParams(int level, int progress = 0,int poz = -1, int id = -1)
        {
            if (barracks != null)
            {
                barracks.transform.GetChild(1).GetComponent<Text>().text = level.ToString();
            }
            this.id = id;
            this.level = level;
            this.progress = progress;
            controll.SetParams(type, transform, level);
            pozitionHUB = poz;
        }

        private void SetInfoHUD(int poz, Transform infoWindow)
        {
            pos = poz;
            GameObject UnitInfoHUD = infoWindow.GetChild(poz).gameObject;
            UnitInfoHUD.transform.GetChild(0).GetComponent<Image>().sprite=type;
            UnitInfoHUD.transform.GetChild(1).GetComponent<Slider>().value = controll.HpProc;
            UnitInfoHUD.transform.GetChild(1).GetChild(2).GetComponent<Text>().text = controll.HpLeft.ToString();
            UnitInfoHUD.transform.GetChild(2).GetComponent<Text>().text = UnitName;
            UnitInfoHUD.transform.GetChild(3).GetComponent<Text>().text = controll.Level.ToString();
            UnitInfoHUD.SetActive(true);
        }

        internal void SetUnitPoz(int positionInHUB)
        {
            pozitionHUB = positionInHUB;
        }

        public void Init(Color color, string name, bool isYours = false)
        {
            targetType = name == "Enemy" ? "NotEnemy" : "Enemy";
            yourUnit = isYours;
            controll.Init(isYours);
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0.5f) }
                );
            this.name = name;
            var particales = particalSystem.colorOverLifetime;
            particales.color = new ParticleSystem.MinMaxGradient(gradient);
            controll.SetColliderSize(rayBox);
            started = true;
            SetParams();
        }

        public int NextLevel()
        {
            return (int)Mathf.Floor(BASEXP * (Mathf.Pow(Level+1, XPINC)));
        }

        protected abstract void SetParams();

        protected virtual void Update()
        {
            if (PozitionHUB != -1)
            {
                SetInfoHUD(PozitionHUB, GameObject.Find("GameScreanCanvas").transform.GetChild(2).GetChild(3));
                SetInfoHUD(PozitionHUB, GameObject.Find("GameScreanCanvas").transform.GetChild(4).GetChild(0));
            }
            if (transform.childCount <= 2 && started)
            {
                if (yourUnit)
                    Disable();
                else
                    Destroy(gameObject);
                return;
            }
                
            particalSystem.transform.position = controll.transform.position;
            if (movePos != null && Vector3.Distance(controll.transform.position, movePos) <= 0.5)
            {
                move = false;
            }
        }

        public string GetSize()
        {
            return controll.Size[0].ToString() + "$" + controll.Size[1].ToString();
        }

        /*public void Stay()
        {
            idle = true;
        }*/

        public void DestroyInfo()
        {
            GameObject.Find("GameScreanCanvas").transform.GetChild(2).GetChild(3).GetChild(pozitionHUB).gameObject.SetActive(false);
            GameObject.Find("GameScreanCanvas").transform.GetChild(4).GetChild(0).GetChild(pozitionHUB).gameObject.SetActive(false);
            pozitionHUB = -1;
        }

        public void MoveUnit(Vector3 pos)
        {
            move = true;
            attack = false;
            enemy = null;

            timeLeft = 0;

            movePos = pos;
            controll.transform.LookAt(pos);
            controll.SetTargetLocation(pos);
        }

        public void AttackUnit(GameObject attackUnit)
        {
            move = true;
            attack = true;

            timeLeft = 0;

            movePos = attackUnit.transform.GetChild(0).position;
            enemy = attackUnit.transform.GetChild(0).gameObject;
            controll.transform.LookAt(movePos);
            transform.GetChild(0).GetComponent<ControllUnit>().SetTargetLocation(enemy.transform.position);
            //idle = false;
        }

        public void SetStartPosition(Transform pos)
        {
            transform.SetPositionAndRotation(pos.position, pos.rotation);
            controll.SetStartPos(pos.position);
            MoveUnit(pos.position);
            move = false;
            attack = false;
            enemy = null;
        }
        public void LevelUp(int progress)
        {
            var level = this.level;
            progress += this.progress;
            var formula = NextLevel();
            if (progress >= formula)
            {
                progress -= formula;
                level++;
            }
            GameObject.Find("MainMenu").GetComponent<MainMenu>().SaveUnitProgress(progress, level, pos);
        }

        internal void DeleteUnits()
        {
            controll.RemoveUnits();
        }
    }
}
