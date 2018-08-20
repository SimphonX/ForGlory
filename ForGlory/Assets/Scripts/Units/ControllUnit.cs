using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Units
{
    public class ControllUnit : MonoBehaviour
    {
        public Soldier preSold;
        private int[] size = new int[2];
        private string types;
        public Sprite type;
        public List<Soldier> soldiers = new List<Soldier>();
        public Vector3 targetPosition;
        public float speed;
        private float x, z;
        public NavMeshAgent mNavMeshAgent;
        private Transform deltaPos;
        private bool attack = false;
        private int hp;

        public float HpProc
        {
            get
            {
                return (float)HpLeft * 100 / (GetComponentInParent<UnitGroup>().Hp);
            }
        }

        public int GetSize()
        {
            return size[0] * size[1];
        }

        public int Level { get; set; }

        public int HpLeft
        {
            get
            {
                var hpLeft = 0;
                foreach(Soldier sol in Soldiers)
                    hpLeft += sol.HpLeft;
                return hpLeft;
            }
        }

        public List<Soldier> Soldiers
        {
            get
            {
                return soldiers;
            }
        }

        public int[] Size
        {
            get
            {
                return size;
            }
        }

        void Start()
        {
            
        }

        internal void RemoveSoldier(Soldier sol)
        {
            soldiers.Remove(sol);
        }

        public void CreatUnits(bool isYours)
        {
            for (float i = -((float)size[0] -1)/ 2; i <= size[0] / 2; i++) 
            {
                for (float j = -((float)size[1]-1) / 2; j <= size[1] / 2; j++)
                {
                    Soldier sol = Instantiate(preSold, gameObject.transform.position, Quaternion.identity);
                    sol.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = type;
                    sol.gameObject.name = "Bot" + i + "" + j;
                    sol.SetPosition(i*2, j*2);
                    sol.IsYours = isYours;
                    sol.GetComponent<Soldier>().DeltaPos = transform;
                    sol.gameObject.transform.parent = transform.parent;
                    sol.transform.position = new Vector3(deltaPos.position.x + i*2, deltaPos.position.y, deltaPos.position.z + j*2);
                    soldiers.Add(sol);
                }
            }
        }

        void Update()
        {
            if (Soldiers.Count > 0)
                if(mNavMeshAgent.destination != targetPosition) mNavMeshAgent.destination = targetPosition;
            else
                Destroy(transform.parent.gameObject);
            
        }

        public void SetTargetLocation(Vector3 targPos)
        {
            Soldiers.ForEach(x => x.MoveUnit());
            targetPosition = targPos;
        }

        public void Init(bool isYours)
        {
            CreatUnits(isYours);
        }

        public void SetStartPos(Vector3 pos)
        {
            mNavMeshAgent.Warp(pos);
            Soldiers.ForEach(x=> x.SetStartPos(pos));
        }

        public void SetParams(Sprite type, Transform data, int level)
        {
            Level = level;
            this.type = type;
            size[0] = 5;
            size[1] = 4;
            SetStats();
            deltaPos = data;
            targetPosition = deltaPos.position;
            Soldiers.ForEach(x => x.MoveUnit());
        }

        public void SetStats()
        {
            speed = 6;
            mNavMeshAgent.speed = speed;
        }

        public void SetColliderSize(BoxCollider box)
        {
            box.transform.localScale = new Vector3(Size[0]*2, 0,Size[1]*2);
        }
        
        public void AttackUnits(GameObject arrow, GameObject enemy)
        {
            if(enemy.transform.parent.tag == "Player")
            {
                soldiers.ForEach(x => x.AttackUnit(enemy.transform.parent.gameObject, enemy.transform.parent.gameObject, arrow));
                return;
            }
            int k = 2;
            for (int i = 0; i < Soldiers.ToArray().Length; i++)
            {
                Soldiers.ToArray()[i].AttackUnit(enemy.transform.parent.gameObject, enemy.transform.parent.GetChild(k).gameObject, arrow);
                k++;
                if (k == enemy.transform.parent.childCount)
                    k = 2;

            }

        }

        public void AttackUnits(GameObject enemy)
        {
            Debug.Log(enemy.name);
            if (enemy.tag == "Player")
            {
                soldiers.ForEach(x => x.AttackUnit(enemy, enemy));
                return;
            }
            int k = 2;
            for (int i = 0; i < soldiers.ToArray().Length; i++)
            {
                Debug.Log(soldiers[i]);
                soldiers[i].AttackUnit(enemy, enemy.transform.GetChild(k++).gameObject);
                if (k == enemy.transform.childCount)
                    k = 2;
                
            }
        }

        internal void RemoveUnits()
        {
            Debug.Log(Soldiers.ToArray().Length);
            //foreach(Soldier sol in soldiers)
            soldiers.ForEach(x => Destroy(x.gameObject));
            Soldiers.Clear();
        }
    }
}