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
    using UnityEngine;

    /// <summary>
    /// Class <c>UISafeAreaAnchor</c> anchors UI elements to corners of the safe rendering area on mobile devices (screen notches, cutouts, etc.)
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UISafeAreaAnchor : MonoBehaviour
    {
        public enum AnchorX
        {
            Original,
            ScreenLeft,
            SafeAreaLeft,
            SafeAreaRight,
            ScreenRight
        }

        public enum AnchorY
        {
            Original,
            ScreenBottom,
            SafeAreaBottom,
            SafeAreaTop,
            ScreenTop
        }

        public AnchorX LeftAnchor = AnchorX.Original;
        public AnchorY BottomAnchor = AnchorY.Original;
        public AnchorX RightAnchor = AnchorX.Original;
        public AnchorY TopAnchor = AnchorY.Original;

        void OnEnable()
        {
            UIScreenRotationEventController.OnScreenRotatated += OnScreenRotated;
            UpdateAnchors();
        }

        void OnDisable()
        {
            UIScreenRotationEventController.OnScreenRotatated -= OnScreenRotated;
        }

        private void OnScreenRotated(DeviceOrientation newOrientation)
        {
            UpdateAnchors();
        }

        private void SetAnchorX(ref Vector2 anchor, AnchorX anchorPoint, ref Vector2 safeAreaMin, ref Vector2 safeAreaMax)
        {
            switch (anchorPoint)
            {
                case AnchorX.ScreenLeft:
                    anchor.x = 0;
                    break;
                case AnchorX.SafeAreaLeft:
                    anchor.x = safeAreaMin.x;
                    break;
                case AnchorX.SafeAreaRight:
                    anchor.x = safeAreaMax.x;
                    break;
                case AnchorX.ScreenRight:
                    anchor.x = 1;
                    break;
            }
        }

        private void SetAnchorY(ref Vector2 anchor, AnchorY anchorPoint, ref Vector2 safeAreaMin, ref Vector2 safeAreaMax)
        {
            switch (anchorPoint)
            {
                case AnchorY.ScreenBottom:
                    anchor.y = 0;
                    break;
                case AnchorY.SafeAreaBottom:
                    anchor.y = safeAreaMin.y;
                    break;
                case AnchorY.SafeAreaTop:
                    anchor.y = safeAreaMax.y;
                    break;
                case AnchorY.ScreenTop:
                    anchor.y = 1;
                    break;
            }
        }

        private void UpdateAnchors()
        {
            var safeArea = Screen.safeArea;
            var rt = transform as RectTransform;
            Vector2 safeAreaMin = new Vector2(safeArea.xMin/Screen.width, safeArea.yMin/Screen.height);
            Vector2 safeAreaMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);

            var newAnchorMin = rt.anchorMin;
            SetAnchorX(ref newAnchorMin, LeftAnchor, ref safeAreaMin, ref safeAreaMax);
            SetAnchorY(ref newAnchorMin, BottomAnchor, ref safeAreaMin, ref safeAreaMax);

            var newAnchorMax = rt.anchorMax;
            SetAnchorX(ref newAnchorMax, RightAnchor, ref safeAreaMin, ref safeAreaMax);
            SetAnchorY(ref newAnchorMax, TopAnchor, ref safeAreaMin, ref safeAreaMax);

            rt.anchorMin = newAnchorMin;
            rt.anchorMax = newAnchorMax;
        }
    }
}