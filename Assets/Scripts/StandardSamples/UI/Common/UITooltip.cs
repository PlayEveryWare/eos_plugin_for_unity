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
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    public enum UITooltipPosition
    {
        Auto,
        Left,
        Right,
        Top,
        Bottom
    }

    [RequireComponent(typeof(Selectable))]
    public class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDeselectHandler
    {
        [SerializeField]
        [Tooltip("String that will appear in tooltip box.")]
        private string text;
        [SerializeField]
        [Tooltip("Side of object on which to place tooltip. Auto selects side with most screen area.")]
        private UITooltipPosition position;
        [Tooltip("Maximum width of tooltip. -1 to fit screen.")]
        public float PreferredWidth = -1;

        //hide tooltip if parameters are changed at runtime
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                Hide();
                text = value;
            }
        }

        public UITooltipPosition Position
        {
            get
            {
                return position;
            }
            set
            {
                Hide();
                position = value;
            }
        }

        private Coroutine tooltipTimer;

        private void Hide()
        {
            if (UITooltipManager.Instance != null)
            {
                UITooltipManager.Instance.HideTooltip(this);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (UITooltipManager.Instance != null)
            {
                tooltipTimer = StartCoroutine(ShowTooltipWithDelay(UITooltipManager.Instance.HoverTime));
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltipTimer != null)
            {
                StopCoroutine(tooltipTimer);
                tooltipTimer = null;
            }
            Hide();
        }

        IEnumerator ShowTooltipWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            UITooltipManager.Instance.ShowTooltip(this);
            tooltipTimer = null;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Hide();
        }

        public void OnDisable()
        {
            Hide();
        }

        public void OnDestroy()
        {
            Hide();
        }
    }
}