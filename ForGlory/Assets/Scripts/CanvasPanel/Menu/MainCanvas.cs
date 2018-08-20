using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.CanvasPanel.Menu
{
    public class MainCanvas : MonoBehaviour
    {
        public GameObject login;
        public GameObject register;
        public GameObject forgot;
        // Use this for initialization
        void Start()
        {
            login.SetActive(false);
            register.SetActive(false);
            forgot.SetActive(false);
        }
        public void Exit()
        {
            Application.Quit();
        }
        public void ToLogIn()
        {
            login.SetActive(true);
        }
        // Update is called once per frame
        void Update()
        {
            
        }

        public void ToRegister(bool type)
        {
            login.SetActive(type);
            register.SetActive(!type);
        }
        public void ToForgot(bool type)
        {
            login.SetActive(type);
            forgot.SetActive(!type);
        }
    }
}
