using System;
using UnityEngine;


namespace Assets.Scripts.HUB
{
    class GameHUB : MonoBehaviour
    {
        public GameObject gameHUB;
        public GameObject worldMap;
        public void ViewSwitch (bool state)
        {
            gameHUB.SetActive(state);
            worldMap.SetActive(!state);
        }
    }
}
