/*
* Copyright (c) 2021 PlayEveryWare
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
#endif

    public class UITooltipManager : MonoBehaviour
    {
        private static UITooltipManager s_instance;

        public RectTransform TooltipContainer;
        public Image TooltipBackground;
        public Text TooltipText;
        public LayoutElement TooltipLayout;
        [Tooltip("Amount of time for tooltip to appear on mouse hover in seconds")]
        public float HoverTime = 2f;

        private Canvas ownerCanvas;
        private float tooltipPaddingWidth = 0;
        private Vector3[] ttWorldCorners;
        private UITooltip currentTooltip = null;
        private UITooltipPosition currentTooltipPosition;

#if UNITY_2019_3_OR_NEWER
        // In case of disabled Domain Reload, reset static members before entering Play Mode.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitOnPlayMode()
        {
            s_instance = null;
        }
#endif

        public static UITooltipManager Instance
        {
            get
            {
                return s_instance;
            }
        }

        public void Awake()
        {
            if (s_instance != null)
            {
                Debug.LogError("UITooltipManager: instance already exists");
                Destroy(this);
                return;
            }

            s_instance = this;

            ttWorldCorners = new Vector3[4];
            ownerCanvas = GetComponentInParent<Canvas>();
            var tooltipLayoutGroup = TooltipContainer.GetComponent<LayoutGroup>();
            if (tooltipLayoutGroup != null)
            {
                tooltipPaddingWidth = tooltipLayoutGroup.padding.left + tooltipLayoutGroup.padding.right;
            }
        }

#if ENABLE_INPUT_SYSTEM
        public void Update()
        {
            if((Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.yButton.wasPressedThisFrame))
            {
                ToggleTooltip(EventSystem.current.currentSelectedGameObject);
            }
        }
#else
        public void Update()
        {
            if ((Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Joystick1Button3)) && EventSystem.current.currentSelectedGameObject != null)
            {
                ToggleTooltip(EventSystem.current.currentSelectedGameObject);
            }
        }
#endif

        private static Vector2 ScreenToCanvasPosition(Canvas canvas, Vector2 screenPosition)
        {
            var viewportPosition = new Vector3(screenPosition.x / Screen.width,
                                               screenPosition.y / Screen.height);
            return ViewportToCanvasPosition(canvas, viewportPosition);
        }

        private static Vector2 ViewportToCanvasPosition(Canvas canvas, Vector2 viewportPosition)
        {
            var centerBasedViewPortPosition = viewportPosition - new Vector2(0.5f, 0.5f);
            var canvasRect = canvas.GetComponent<RectTransform>();
            var scale = canvasRect.sizeDelta;
            return Vector2.Scale(centerBasedViewPortPosition, scale);
        }

        private struct PositionArea
        {
            public UITooltipPosition position;
            public float area;
        }

        //get side of element with most screen area
        private UITooltipPosition GetBestPosition()
        {
            var positionAreas = new List<PositionArea>();
            positionAreas.Add(new PositionArea() { position = UITooltipPosition.Left, area = ttWorldCorners[0].x * Screen.height });
            positionAreas.Add(new PositionArea() { position = UITooltipPosition.Right, area = (Screen.width - ttWorldCorners[2].x) * Screen.height });
            positionAreas.Add(new PositionArea() { position = UITooltipPosition.Top, area = Screen.width * (Screen.height - ttWorldCorners[1].y) });
            positionAreas.Add(new PositionArea() { position = UITooltipPosition.Bottom, area = Screen.width * ttWorldCorners[0].y });

            //sort from largest to smallest screen area
            positionAreas.Sort((PositionArea a, PositionArea b) => { return (int)Mathf.Sign(b.area - a.area); });
            return positionAreas[0].position;
        }

        //hide given tooltip if it's shown or vice-versa
        public void ToggleTooltip(GameObject tooltipObject)
        {
            if (tooltipObject == null)
            {
                return;
            }

            if (!tooltipObject.activeInHierarchy) 
            {
                HideTooltip(tooltipObject);
            }

            var tooltipComp = tooltipObject.GetComponent<UITooltip>();
            if (tooltipComp != null)
            {
                if (tooltipComp == currentTooltip)
                {
                    HideTooltip(tooltipObject);
                }
                else
                {
                    ShowTooltip(tooltipObject);
                }
            }
        }

        public void ShowTooltip(GameObject tooltipObject)
        {
            if (tooltipObject == null)
            {
                return;
            }

            var tooltipComp = tooltipObject.GetComponent<UITooltip>();
            ShowTooltip(tooltipComp);
        }

        public void ShowTooltip(UITooltip tooltipComp)
        {
            if (tooltipComp == null || tooltipComp == currentTooltip || string.IsNullOrWhiteSpace(tooltipComp.Text))
            {
                return;
            }

            currentTooltip = tooltipComp;
            currentTooltipPosition = tooltipComp.Position;

            (tooltipComp.transform as RectTransform).GetWorldCorners(ttWorldCorners);

            if (currentTooltipPosition == UITooltipPosition.Auto)
            {
                currentTooltipPosition = GetBestPosition();
            }

            TooltipLayout.preferredWidth = -1;
            TooltipText.text = tooltipComp.Text;
            TooltipContainer.gameObject.SetActive(true);

            //rebuild layout for new text
            LayoutRebuilder.ForceRebuildLayoutImmediate(TooltipContainer);
            UpdateTooltipLayout();
        }

        private void UpdateTooltipLayout()
        {
            //get rect of target screen area
            Vector2 blScreen = Vector2.zero, trScreen = Vector2.zero;
            switch (currentTooltipPosition)
            {
                case UITooltipPosition.Left:
                    blScreen = new Vector2(0, 0);
                    trScreen = new Vector2(ttWorldCorners[0].x, Screen.height);
                    break;

                case UITooltipPosition.Right:
                    blScreen = new Vector2(ttWorldCorners[2].x, 0);
                    trScreen = new Vector2(Screen.width, Screen.height);
                    break;

                case UITooltipPosition.Top:
                    blScreen = new Vector2(0, ttWorldCorners[1].y);
                    trScreen = new Vector2(Screen.width, Screen.height);
                    break;

                case UITooltipPosition.Bottom:
                    blScreen = new Vector2(0, 0);
                    trScreen = new Vector2(Screen.width, ttWorldCorners[0].y);
                    break;
            }

            Vector2 bottomLeft = ScreenToCanvasPosition(ownerCanvas, blScreen);
            Vector2 topRight = ScreenToCanvasPosition(ownerCanvas, trScreen);

            //get maximum width of screen area
            float preferredWidth = topRight.x - bottomLeft.x - tooltipPaddingWidth;
            if (currentTooltip.PreferredWidth > 0)
            {
                preferredWidth = Mathf.Min(preferredWidth, currentTooltip.PreferredWidth);
            }
            float currentWidth = (TooltipText.transform as RectTransform).sizeDelta.x;
            //cap tooltip width at maximum
            if (preferredWidth < currentWidth)
            {
                TooltipLayout.preferredWidth = preferredWidth;
            }
            //force rebuild for new layout width
            LayoutRebuilder.ForceRebuildLayoutImmediate(TooltipContainer);
            UpdateTooltipPosition();
        }

        private void UpdateTooltipPosition()
        {
            //get canvas space coords of target object rect
            var targetBL = ScreenToCanvasPosition(ownerCanvas, ttWorldCorners[0]);
            var targetTR = ScreenToCanvasPosition(ownerCanvas, ttWorldCorners[2]);
            var targetRect = (currentTooltip.transform as RectTransform).rect;
            var tooltipRect = TooltipContainer.rect;
            var canvasRect = (ownerCanvas.transform as RectTransform).rect;
            Vector2 tooltipPos = Vector2.zero;
            //position tooltip adjacent on desired side and adjust to fit screen
            switch (currentTooltipPosition)
            {
                case UITooltipPosition.Left:
                    tooltipPos.x = targetBL.x - tooltipRect.width / 2;
                    tooltipPos.y = Mathf.Clamp(targetBL.y + targetRect.height / 2, canvasRect.yMin + tooltipRect.height / 2, canvasRect.yMax - tooltipRect.height / 2);
                    break;

                case UITooltipPosition.Right:
                    tooltipPos.x = targetTR.x + tooltipRect.width / 2;
                    tooltipPos.y = Mathf.Clamp(targetBL.y + targetRect.height / 2, canvasRect.yMin + tooltipRect.height / 2, canvasRect.yMax - tooltipRect.height / 2);
                    break;

                case UITooltipPosition.Top:
                    tooltipPos.x = Mathf.Clamp(targetBL.x + targetRect.width / 2, canvasRect.xMin + tooltipRect.width / 2, canvasRect.xMax - tooltipRect.width / 2);
                    tooltipPos.y = targetTR.y + tooltipRect.height / 2;
                    break;

                case UITooltipPosition.Bottom:
                    tooltipPos.x = Mathf.Clamp(targetBL.x + targetRect.width / 2, canvasRect.xMin + tooltipRect.width / 2, canvasRect.xMax - tooltipRect.width / 2);
                    tooltipPos.y = targetBL.y - tooltipRect.height / 2;
                    break;
            }

            TooltipContainer.anchoredPosition = tooltipPos;
            TooltipContainer.gameObject.SetActive(true);
        }

        public void HideTooltip(GameObject tooltipObject)
        {
            if (tooltipObject == null || currentTooltip == null)
            {
                return;
            }

            var tooltipComp = tooltipObject.GetComponent<UITooltip>();
            HideTooltip(tooltipComp);
        }

        public void HideTooltip(UITooltip tooltipComp)
        {
            if (tooltipComp == currentTooltip)
            {
                TooltipContainer.gameObject.SetActive(false);
                currentTooltip = null;
            }
        }
    }
}