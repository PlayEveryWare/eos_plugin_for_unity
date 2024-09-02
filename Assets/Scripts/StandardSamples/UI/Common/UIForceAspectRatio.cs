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

    public class ForceAspectRatio : MonoBehaviour
    {

        [Tooltip("Aspect ratio to force window to")]
        public float TargetAspectRatio = 16.0f / 9.0f;

        [Tooltip("Delay between window resize and aspect ratio adjustment, in seconds")]
        public float AdjustmentDelay = 0.5f;

        int lastScreenWidth;
        int lastScreenHeight;

        int lastCorrectedScreenWidth;
        int lastCorrectedScreenHeight;

        // Start is called before the first frame update
        void Start()
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        // Update is called once per frame
        void Update()
        {
#if !UNITY_EDITOR
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            CancelInvoke("AdjustResolution");
            Invoke("AdjustResolution", AdjustmentDelay);

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
#endif
        }

        void AdjustResolution()
        {
            int width = Screen.width;
            int height = Screen.height;

            if (lastCorrectedScreenWidth != width) // changing width
            {
                int newHeight = (int)Mathf.Round(width / TargetAspectRatio);
                Screen.SetResolution(width, newHeight, false);
                lastCorrectedScreenHeight = newHeight;
            }
            else if (lastCorrectedScreenHeight != height) // changing height
            {
                int newWidth = (int)Mathf.Round(height * TargetAspectRatio);
                Screen.SetResolution(newWidth, height, false);
                lastCorrectedScreenWidth = newWidth;
            }
        }
    }
}
