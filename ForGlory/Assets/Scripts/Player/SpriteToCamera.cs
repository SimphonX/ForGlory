using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class SpriteToCamera : MonoBehaviour
    {

        void Start()
        {
        }

        void Update()
        {
            transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
        }
    }
}
