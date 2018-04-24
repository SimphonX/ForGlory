using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        private float ZoomAmount = 0;
        private float MaxToClamp = 4;
        private float ROTSpeed = 10;
        private float speed;

        Vector2 _mouseAbsolute;
        Vector2 _smoothMouse;
        public Vector2 clampInDegrees = new Vector2(360, 40);
        public Vector2 sensitivity = new Vector2(2, 2);
        public Vector2 smoothing = new Vector2(3, 3);
        public Vector2 targetDirection;
        public Vector2 targetCharacterDirection;
        public GameObject camera;
        void Start()
        {
            speed =  name == "Player" ?  4.0f :  50.0f;
        }

        // Update is called once per frame
        void Update()
        {
            Movement();
            if (name != "Player")
            {
                if(Input.GetMouseButton(1))Mouselook();
                Zooming();
            }
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
