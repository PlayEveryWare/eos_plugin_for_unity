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

using UnityEngine;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class SampleSceneUIContainer : MonoBehaviour, ISampleSceneUI
    {
        private Vector2 initialAnchorMin;
        private Vector2 initialSizeDelta;

        private void Start()
        {
            var rt = transform as RectTransform;
            initialAnchorMin = rt.anchorMin;
            initialSizeDelta = rt.sizeDelta;
        }

        public void ShowMenu()
        {
            var childElements = GetComponentsInChildren<ISampleSceneUI>(true);
            foreach (var childElement in childElements)
            {
                if ((childElement as MonoBehaviour)?.gameObject == gameObject)
                {
                    continue;
                }

                childElement.ShowMenu();
            }
        }

        public void HideMenu()
        {
            var childElements = GetComponentsInChildren<ISampleSceneUI>(true);
            foreach (var childElement in childElements)
            {
                if ((childElement as MonoBehaviour)?.gameObject == gameObject)
                {
                    continue;
                }

                childElement.HideMenu();
            }
        }

        public void SetFullscreen(bool fullscreen)
        {
            var rt = transform as RectTransform;
            if (fullscreen)
            {
                rt.anchorMin = new Vector2(initialAnchorMin.x, 0);
                rt.sizeDelta = new Vector2(initialSizeDelta.x, 0);
            }
            else
            {
                rt.anchorMin = initialAnchorMin;
                rt.sizeDelta = initialSizeDelta;
            }
        }
    }
}