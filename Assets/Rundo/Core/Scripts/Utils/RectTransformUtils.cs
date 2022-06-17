using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rundo.Core.Utils
{
    public static class RectTransformUtils
    {
        public static void SnapToCursor(RectTransform parent, RectTransform snap, bool flipHorizontal = false, bool flipVertical = false)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Input.mousePosition,
                null, out var localPoint);

            snap.anchoredPosition = localPoint;

            FlipRectTransformIfOutsideOfScreen(snap);
        }

        public static void FlipRectTransformIfOutsideOfScreen(RectTransform rectTransform)
        {
            if (IsRectTransformOutsideOfScreen(rectTransform, out var horizontal, out var vertical))
            {
                if (horizontal)
                    RectTransformUtility.FlipLayoutOnAxis(rectTransform, 0, true, false);
                if (vertical)
                    RectTransformUtility.FlipLayoutOnAxis(rectTransform, 1, true, false);
            }
        }
        
        public static bool IsRectTransformOutsideOfScreen(RectTransform rectTransform, out bool horizontal, out bool vertical)
        {
            horizontal = false;
            vertical = false;
            
            var rootCanvas = rectTransform.GetComponentInParent<Canvas>().rootCanvas;
                    
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            RectTransform rootCanvasRectTransform = rootCanvas.GetComponent<RectTransform>();
            Rect rootCanvasRect = rootCanvasRectTransform.rect;
            for (int axis = 0; axis < 2; axis++)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector3 corner = rootCanvasRectTransform.InverseTransformPoint(corners[i]);
                    if ((corner[axis] < rootCanvasRect.min[axis] &&
                         !Mathf.Approximately(corner[axis], rootCanvasRect.min[axis])) ||
                        (corner[axis] > rootCanvasRect.max[axis] &&
                         !Mathf.Approximately(corner[axis], rootCanvasRect.max[axis])))
                    {
                        if (axis == 0)
                            horizontal = true;
                        if (axis == 1)
                            vertical = true;
                        break;
                    }
                }
            }

            return (horizontal || vertical);
        }

        /// <summary>
        /// Create a blocker that blocks clicks to other controls while the dropdown list is open.
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to obtain a blocker GameObject.
        /// </remarks>
        /// <param name="rootCanvas">The root canvas the dropdown is under.</param>
        /// <returns>The created blocker object</returns>
        public static GameObject CreateUIBlocker(Canvas rootCanvas, Canvas dropDownCanvas, Transform optionsContent, UnityAction hideCallback)
        {
            // Create blocker GameObject.
            GameObject blocker = new GameObject("Blocker");

            // Setup blocker RectTransform to cover entire root canvas area.
            RectTransform blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.SetParent(rootCanvas.transform, false);
            blockerRect.anchorMin = Vector3.zero;
            blockerRect.anchorMax = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            // Make blocker be in separate canvas in same layer as dropdown and in layer just below it.
            Canvas blockerCanvas = blocker.AddComponent<Canvas>();
            blockerCanvas.overrideSorting = true;
            Canvas dropdownCanvas = dropDownCanvas.GetComponent<Canvas>();
            blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
            blockerCanvas.sortingOrder = dropdownCanvas.sortingOrder - 1;

            // Find the Canvas that this dropdown is a part of
            Canvas parentCanvas = null;
            Transform parentTransform = optionsContent.transform.parent;
            while (parentTransform != null)
            {
                parentCanvas = parentTransform.GetComponent<Canvas>();
                if (parentCanvas != null)
                    break;

                parentTransform = parentTransform.parent;
            }

            // If we have a parent canvas, apply the same raycasters as the parent for consistency.
            if (parentCanvas != null)
            {
                Component[] components = parentCanvas.GetComponents<BaseRaycaster>();
                for (int i = 0; i < components.Length; i++)
                {
                    Type raycasterType = components[i].GetType();
                    if (blocker.GetComponent(raycasterType) == null)
                    {
                        blocker.AddComponent(raycasterType);
                    }
                }
            }
            else
            {
                // Add raycaster since it's needed to block.
                if (blocker.TryGetComponent<GraphicRaycaster>(out _) == false)
                    blocker.AddComponent<GraphicRaycaster>();
            }

            // Add image since it's needed to block, but make it clear.
            Image blockerImage = blocker.AddComponent<Image>();
            blockerImage.color = Color.clear;

            // Add button since it's needed to block, and to close the dropdown when blocking area is clicked.
            Button blockerButton = blocker.AddComponent<Button>();
            blockerButton.onClick.AddListener(hideCallback);

            return blocker;
        }
    }
}
