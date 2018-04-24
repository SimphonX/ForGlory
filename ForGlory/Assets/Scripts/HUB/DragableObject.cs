using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.HUB
{
    class DragableObject : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        private const string DRAGABLETAG = "MovableObject";
        private Vector2 pointerOffset;
        private RectTransform canvasRectTransform;
        private RectTransform panelRectTransform;

        void Awake()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if(canvas != null)
            {
                canvasRectTransform = canvas.transform as RectTransform;
                panelRectTransform = transform.parent as RectTransform;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (panelRectTransform == null)
                return;

            Vector2 pointerPosition = ClampToWindow(eventData);
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform, pointerPosition, eventData.pressEventCamera, out localPointerPosition
                ))
            {
                panelRectTransform.localPosition = localPointerPosition - pointerOffset;
            }
        }

        private Vector2 ClampToWindow(PointerEventData eventData)
        {
            Vector2 rawPointerPosition = eventData.position;

            Vector3[] canvasCorners = new Vector3[4];
            canvasRectTransform.GetWorldCorners(canvasCorners);

            float clampedX = Mathf.Clamp(rawPointerPosition.x, canvasCorners[0].x, canvasCorners[2].x);
            float clampedY = Mathf.Clamp(rawPointerPosition.y, canvasCorners[0].y, canvasCorners[2].y);

            return new Vector2(clampedX, clampedY);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            panelRectTransform.SetAsLastSibling();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, eventData.position, eventData.pressEventCamera, out pointerOffset);
        }
    }
}
