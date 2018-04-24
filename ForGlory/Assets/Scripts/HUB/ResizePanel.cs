using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.HUB
{
    class ResizePanel:MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        private Vector2 minSize = new Vector2(400, 200);
        private Vector2 maxSize = new Vector2(1920, 1080);

        private RectTransform rectTransform;
        private Vector2 currentPointerPosition;
        private Vector2 previousPointerPosition;

        void Awake()
        {
            rectTransform = transform.parent.GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            rectTransform.SetAsLastSibling();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out previousPointerPosition);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (rectTransform == null)
                return;

            Vector2 sizeDelta = rectTransform.sizeDelta;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out currentPointerPosition);
            Vector2 resizeValue = currentPointerPosition - previousPointerPosition;

            sizeDelta += new Vector2(resizeValue.x, -resizeValue.y);
            sizeDelta = new Vector2(
                Mathf.Clamp(sizeDelta.x, minSize.x, maxSize.x),
                Mathf.Clamp(sizeDelta.y, minSize.y, maxSize.y)
                );

            rectTransform.sizeDelta = sizeDelta;

            previousPointerPosition = currentPointerPosition;
        }
    }
}
