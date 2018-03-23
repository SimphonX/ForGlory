using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCanvas : MonoBehaviour {
    public GameObject login;
    public GameObject register;
    public GameObject forgot;
    // Use this for initialization
    void Start () {
        login.SetActive(true);
        register.SetActive(false);
        forgot.SetActive(false);
    }
    public void Exit()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update () {
		
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
