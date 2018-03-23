using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteToCamera : MonoBehaviour {
    
    void Start()
    {
    }
    
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
    }
}
