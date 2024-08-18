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
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class UISampleSceneUIContainer : MonoBehaviour
    {
        /// <summary>
        /// Container for the sample scene UI.
        /// </summary>
        public LayoutElement ContainerLayout;

        /// <summary>
        /// Cache to keep track of the initial height of the flexible height for the ContainerLayout.
        /// </summary>
        private float InitialFlexHeight;

        private void Start()
        {
            // Set the initial flex height.
            InitialFlexHeight = ContainerLayout.flexibleHeight;
        }

        public void ShowMenu()
        {
            foreach (var menu in GetContainedSampleMenus())
            {
                menu.Show();
            }
        }

        public void HideMenu()
        {
            foreach (var menu in GetContainedSampleMenus())
            {
                menu.Hide();
            }
        }

        /// <summary>
        /// Gets all children ISampleSceneUI objects that are not this one.
        /// </summary>
        /// <returns>Enumerable of sample scenes contained within.</returns>
        private IEnumerable<SampleMenu> GetContainedSampleMenus()
        {
            return GetComponentsInChildren<SampleMenu>(true).Where(element =>
                element is MonoBehaviour behaviour && behaviour.gameObject != gameObject);
        }

        /// <summary>
        /// Sets the visibility of the demo container by changing the height to zero, or restoring the height to what it initially was set to on Start.
        /// </summary>
        /// <param name="visible">Whether or not to hide the demo scene container.</param>
        public void SetVisible(bool visible)
        {
            ContainerLayout.flexibleHeight = visible ? InitialFlexHeight : 0;
        }
    }
}