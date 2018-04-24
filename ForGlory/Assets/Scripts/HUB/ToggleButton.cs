using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.HUB
{
    class ToggleButton :MonoBehaviour
    {
        public void TogglePanel(GameObject panel)
        {
            panel.SetActive(!panel.activeSelf);
        }
    }
}
