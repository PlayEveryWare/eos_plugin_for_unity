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
    using UnityEngine.EventSystems;

    [RequireComponent(typeof(RectTransform))]
    public class UICameraPanel : UIBehaviour
    {
        public Camera TargetCamera;

        private Vector3[] corners = new Vector3[4];
        private bool updatePending = false;

        protected override void OnEnable()
        {
            UpdateCameraDelayed();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            UpdateCameraDelayed();
        }

        protected override void OnCanvasHierarchyChanged()
        {
            UpdateCameraDelayed();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            UpdateCameraDelayed();
        }

        protected override void OnTransformParentChanged()
        {
            UpdateCameraDelayed();
        }

        protected override void OnCanvasGroupChanged()
        {
            UpdateCameraDelayed();
        }

        private void UpdateCamera()
        {
            (transform as RectTransform).GetWorldCorners(corners);
            Bounds bounds = new Bounds(corners[0], Vector3.zero);
            for (var i = 1; i < 4; i++)
            {
                bounds.Encapsulate(corners[i]);
            }
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            Rect camRect = new Rect(bounds.min / screenSize, bounds.size / screenSize);
            TargetCamera.rect = camRect;
        }

        void UpdateCameraDelayed()
        {
            if (updatePending || !gameObject.activeInHierarchy)
            {
                return;
            }

            StartCoroutine(DelayUpdate());
        }

        IEnumerator DelayUpdate()
        {
            updatePending = true;
            yield return null;
            UpdateCamera();
            updatePending = false;
        }
    }
}