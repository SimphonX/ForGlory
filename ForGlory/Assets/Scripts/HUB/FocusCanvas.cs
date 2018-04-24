using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.HUB
{
    class FocusCanvas : MonoBehaviour, IPointerClickHandler
    {
        private RectTransform rectTransform;

        void Awake()
        {
            rectTransform = transform as RectTransform;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            rectTransform.SetAsLastSibling();
        }
    }
}
