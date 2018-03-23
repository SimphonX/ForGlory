using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 9.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 9.0f;
        /*if (x != 0 || z != 0)
            GameObject.Find("Client").GetComponent<Client>().UpdatePosition();*/
        transform.Translate(x, 0, z);
    }
}
