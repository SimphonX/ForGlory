using Assets.Scripts.Player.Player;
using Assets.Scripts.ServerClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Player.Items
{
    public abstract class Weapon: ItemsHandler
    {
        public bool attack = false;

        protected int BASEDAMAGE;
        public float bASESPEED;

        public int dmg;
        
        public float attackInterval = 0;
        public float startRotation = -140;
        public float rotateAngle;

        private float attackTime = 0;

        public InputController input;

        public float BASESPEED { get { return bASESPEED; } }

        public abstract void Awake();
        void FixedUpdate()
        {
            //transform.GetChild(6).rotation = Quaternion.Euler(new Vector3(0f, startRotation - LookAtMouse(), 0f));
            AttackMove();

            attack = false;
            if (Input.GetMouseButton(0) && !input.textfield)
            {
                attack = true;
            }
        }
        private void AttackMove()
        {
            if (!attack && attackTime == 0)
            {
                transform.parent.gameObject.SetActive(false);
                return;
            }
            
            transform.parent.gameObject.SetActive(true);
            
            if(attackTime == 0)
            {
                GameObject client = GameObject.Find("Client");
                if (client != null && client.GetComponent<Client>().connected)
                    client.GetComponent<Client>().PlayerAttackMove();
            }
            attackTime += Time.deltaTime;
            if (transform.parent.parent.GetComponent<PlayerMovement>())
                transform.parent.rotation = Quaternion.Euler(new Vector3(0f, -transform.parent.parent.rotation.eulerAngles.y + startRotation - LookAtMouse() + rotateAngle * attackTime / attackInterval, 0f));
            if (attackTime >= attackInterval)
                attackTime = 0;
        }

        public void SetAttack(bool att)
        {
            attack = true;
            attackTime += Time.deltaTime;
            transform.parent.rotation = Quaternion.Euler(new Vector3(0f, -transform.parent.parent.rotation.eulerAngles.y + startRotation - LookAtMouse() + rotateAngle * attackTime / attackInterval, 0f));
        }

        public void SetSpeed(float speed)
        {
            attackInterval = speed;
        }

        private float LookAtMouse()
        {
            Vector2 positionOnScreen = Camera.main.WorldToViewportPoint(transform.position);

            Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));

            return AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);
        }

        float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
        {
            return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
        }
        public abstract void OnTriggerEnter(Collider other);

        public override int GetDefence()
        {
            return 0;
        }

        public override int GetHP()
        {
            return 0;
        }

        public override float GetMoveSpeed()
        {
            return 0;
        }
        public override float GetAttackSpeed()
        {
            return BASESPEED;
        }

        public override int GetDamage()
        {
            return dmg;
        }
    }
}
