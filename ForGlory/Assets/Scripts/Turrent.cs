using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turrent : MonoBehaviour {
    public Transform enemy;
    public GameObject arrow;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var arr = Instantiate(arrow, transform.position, Quaternion.identity).GetComponent<Arrow>();
            arr.Launch(enemy);
        }
    }
}
