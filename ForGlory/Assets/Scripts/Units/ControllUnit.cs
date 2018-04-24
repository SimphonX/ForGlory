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
        private List<Soldier> soldiers = new List<Soldier>();
        private Vector3 targetPosition;
        public float speed;
        private float x, z;
        public NavMeshAgent mNavMeshAgent;
        private Transform deltaPos;
        private bool attack = false;

        public float HpProc
        {
            get
            {
                var hpLeft = 0;
                var hpMax = 0;
                soldiers.ForEach(x => hpLeft += x.HpLeft);
                soldiers.ForEach(x => hpMax += x.Hp);
                return (float)hpLeft /hpMax*100;
            }
        }

        public int Level { get; set; }

        public int HpLeft
        {
            get
            {
                var hpLeft = 0;
                soldiers.ForEach(x => hpLeft += x.HpLeft);
                return hpLeft;
            }
        }

        public List<Soldier> Soldiers { get; }

        public int[] Size { get; set; }

        void Start()
        {
            
        }

        public void CreatUnits()
        {
            for (float i = -((float)Size[0] -1)/ 2; i <= Size[0] / 2; i++) 
            {
                for (float j = -((float)Size[1]-1) / 2; j <= Size[1] / 2; j++)
                {
                    Soldier sol = Instantiate(preSold, gameObject.transform.position, Quaternion.identity);
                    sol.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = type;
                    sol.gameObject.name = "Bot" + i + "" + j;
                    sol.SetPosition(i, j);
                    sol.GetComponent<Soldier>().DeltaPos = transform;
                    sol.gameObject.transform.parent = transform.parent;
                    sol.transform.position = new Vector3(deltaPos.position.x + i, deltaPos.position.y, deltaPos.position.z + j);
                    Soldiers.Add(sol);
                }
            }
        }

        void Update()
        {
            if (Soldiers.Count > 0)
            {
                if(mNavMeshAgent.destination != targetPosition) mNavMeshAgent.destination = targetPosition;
            }
            /*if (Soldiers.Count < 0)
                Destroy(transform.parent.);*/
            
        }

        public void SetTargetLocation(Vector3 targPos)
        {
            soldiers.ForEach(x => x.MoveUnit());
            targetPosition = targPos;
            attack = false;
        }
        
        public void Init()
        {
            CreatUnits();
        }

        public void SetParams(Sprite type, Transform data)
        {
            this.type = type;
            SetStats();
            deltaPos = data;
            targetPosition = deltaPos.position;
            soldiers.ForEach(x => x.MoveUnit());
        }
        public void SetStats()
        {
            speed = 3;
            mNavMeshAgent.speed = speed;
        }
        public void SetColliderSize(BoxCollider box)
        {
            box.transform.localScale = new Vector3(Size[0], 0,Size[1]);
        }
        
        public void AttackUnits(GameObject arrow, GameObject enemy)
        {
            int k = 2;
            for (int i = 0; i < soldiers.ToArray().Length; i++)
            {
                soldiers.ToArray()[i].AttackUnit(enemy, enemy.transform.parent.GetChild(k).gameObject, arrow);
                k++;
                if (k == enemy.transform.parent.childCount)
                    k = 2;

            }

        }
        public void AttackUnits(GameObject enemy)
        {
            if (attack) return;
            int k = 2;
            Debug.Log(attack);
            attack = true;
            for (int i = 0; i < soldiers.ToArray().Length; i++)
            {
                soldiers.ToArray()[i].AttackUnit(enemy, enemy.transform.parent.GetChild(k).gameObject);
                k++;
                if (k == enemy.transform.parent.childCount)
                    k = 2;
                
            }
        }
    }
}