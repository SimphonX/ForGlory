using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Units
{
    
    public class UnitGroup : MonoBehaviour
    {
        protected int level;

        protected ControllUnit controll;
        protected ParticleSystem particalSystem;
        protected BoxCollider rayBox;

        public GameObject arrow;
        public Sprite type;

        protected Vector3 movePos;
        public bool attack, move; //idle;
        public bool started = false;
        public float timeLeft = 0;
        public GameObject enemy;

        public int Level { get; }

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
        void Start()
        {
            
        }
        public void SetParams(int level, int poz = -1)
        {
            this.level = level;
            controll.SetParams(type, transform);
            if (poz != -1)
                SetInfoHUD(poz);
        }

        private void SetInfoHUD(int poz)
        {
            GameObject UnitInfoHUD = GameObject.Find("Canvas").transform.GetChild(2).GetChild(2).GetChild(poz).gameObject;
            UnitInfoHUD.transform.GetChild(0).GetComponent<Image>().sprite=type;
            UnitInfoHUD.transform.GetChild(1).GetComponent<Slider>().value = controll.HpProc;
            UnitInfoHUD.transform.GetChild(1).GetChild(2).GetComponent<Text>().text = controll.HpLeft.ToString();
            UnitInfoHUD.transform.GetChild(2).GetComponent<Text>().text = type.name;
            UnitInfoHUD.transform.GetChild(3).GetComponent<Text>().text = controll.Level.ToString();
            UnitInfoHUD.SetActive(true);
        }

        public void Init(Color color, string name)
        {
            controll.Init();
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

        protected virtual void SetParams()
        {
        }

        protected virtual void Update()
        {
            if (transform.childCount <= 2 && started)
                Destroy(gameObject);
            particalSystem.transform.position = controll.transform.position;
            if (movePos != null && Vector3.Distance(controll.transform.position, movePos) <= 0.5)
            {
                move = false;
                attack = true;
            }
                
        }

        /*public void Stay()
        {
            idle = true;
        }*/

        public void MoveUnit(Vector3 pos)
        {
            if (!move || !attack)
            {
                move = true;
                attack = false;
                enemy = null;
            }
            movePos = pos;
            controll.transform.LookAt(pos);
            controll.SetTargetLocation(pos);
        }

        public void AttackUnit(GameObject attackUnit)
        {
            enemy = attackUnit.transform.GetChild(0).gameObject;
            transform.GetChild(0).GetComponent<ControllUnit>().SetTargetLocation(enemy.transform.position);
            move = true;
            attack = true;
            controll.transform.LookAt(enemy.transform);
            //idle = false;
        }
    }
}
