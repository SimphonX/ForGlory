using Assets.Scripts.ServerClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Player.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        private float ZoomAmount = 0;
        private float MaxToClamp = 4;
        private float ROTSpeed = 10;
        public float time = 0;
        [Range(0.001f, 2f)]
        private static float timeIntervals = 0.1f;
        private float baseSpeed = 0;
        private float itemSpeed = 0;
        public float speed = 0;
        
        private SphereCollider collider;
        private GameObject actionButton;

        public GameObject NPC { get; set; }

        public float Speed { get { return speed; } }

        public void SetSpeed(float var)
        {
            itemSpeed = var;
        }
        public GameObject camera;

        public InputController input;

        void Start()
        {
            input = GameObject.Find("ClickControler").GetComponent<InputController>();
            if (camera == null)
                camera = transform.GetChild(1).gameObject;
            camera.SetActive(true);
            baseSpeed = speed = tag == "Player" ? 6.0f : 50.0f;
            collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 1.5f;
            collider.isTrigger = true;
            actionButton = GameObject.Find("GameScreanCanvas").transform.GetChild(2).GetChild(0).gameObject;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            speed = baseSpeed + itemSpeed;
            if (input.textfield)
                return;
            Movement();

            if (!GameObject.Find("ClickControler").GetComponent<InputController>().map && Input.GetMouseButton(0))
            {
                transform.GetChild(5).gameObject.SetActive(true);
            }
            if(Input.GetMouseButton(1))
                Mouselook();
            Zooming();

            if(name == "NotEnemy")
            {
                time += Time.deltaTime;
                if(time >= timeIntervals)
                {
                    time = 0;
                    GameObject client = GameObject.Find("Client");
                    if (client != null && client.GetComponent<Client>().connected)
                        client.GetComponent<Client>().UpdatePlayerPosition(transform.position, transform.GetChild(5).transform.rotation.eulerAngles, transform.GetChild(0).transform.rotation.eulerAngles);
                }
            }
            if(GetComponent<PlayerController>() != null)
                RotateToMouse();
        }

        private void RotateToMouse()
        {
            var p1 = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            var p2 = Camera.main.WorldToViewportPoint(transform.GetChild(6).position);
            var orbVector = p2 - p1;
            float angle = Mathf.Atan2(orbVector.y, orbVector.x) * Mathf.Rad2Deg;
            transform.GetChild(6).rotation = Quaternion.AngleAxis(angle, Vector3.down);
        }

        void OnTriggerStay(Collider other)
        {
            if (other.tag != "NPC")
                return;
            NPC = other.gameObject;
            actionButton.SetActive(true);
        }

        void OnTriggerExit(Collider other)
        {
            if (other.tag != "NPC")
                return;
            actionButton.SetActive(false);
        }

        private void Zooming()
        {
            ZoomAmount += Input.GetAxis("Mouse ScrollWheel");
            ZoomAmount = Mathf.Clamp(ZoomAmount, -MaxToClamp, MaxToClamp);
            
            var translate = Mathf.Min(Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")), MaxToClamp - Mathf.Abs(ZoomAmount));
            camera.transform.Translate(Vector3.forward * translate * ROTSpeed * Mathf.Sign(Input.GetAxis("Mouse ScrollWheel")));

        }

        private void Mouselook()
        {

            transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * 4.0f, 0));
            float X = transform.rotation.eulerAngles.x;
            float Y = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(X, Y, 0);

        }
        private void Movement()
        {
            var x = Input.GetAxis("Horizontal") * Time.deltaTime * speed;
            var z = Input.GetAxis("Vertical") * Time.deltaTime * speed;
            transform.Translate(x, 0, z);
        }
    }
}
