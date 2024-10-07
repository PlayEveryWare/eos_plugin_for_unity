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
    using System;

    public class SampleSelectableStateHandler : MonoBehaviour
    {
        public struct SampleInteractableNewState
        {
            public bool Interactable;
            public string NewTooltipText;

            public SampleInteractableNewState(bool shouldBeInteractable, string newTooltipText = "")
            {
                Interactable = shouldBeInteractable;
                NewTooltipText = newTooltipText;
            }
        }

        public delegate SampleInteractableNewState GetSampleInteractable();

        private static event Action UpdateSelectableStates;

        private GetSampleInteractable sampleInteractableFunction { get; set; }

        [SerializeReference]
        private UITooltip attachedTooltip;

        [SerializeReference]
        private Selectable attachedSelectable;

        public void SetSampleInteractableAction(GetSampleInteractable inSampleInteractableFunction)
        {
            sampleInteractableFunction = inSampleInteractableFunction;
            UpdateFromFunction();
        }

        private void OnEnable()
        {
            UpdateSelectableStates += UpdateFromFunction;
        }

        private void OnDisable()
        {
            UpdateSelectableStates -= UpdateFromFunction;
        }

        void UpdateFromFunction()
        {
            if (sampleInteractableFunction == null)
            {
                return;
            }

            SampleInteractableNewState newState = sampleInteractableFunction();

            if (!string.IsNullOrEmpty(newState.NewTooltipText) && attachedTooltip != null)
            {
                if (newState.Interactable)
                {
                    // TODO: Restore the original tooltip text if it is interactable
                    // At the moment, none of the buttons that this was applied to
                    // had any tooltips to begin with
                    attachedTooltip.Text = string.Empty;
                }
                else
                {
                    attachedTooltip.Text = newState.NewTooltipText;
                }
            }

            if (attachedSelectable != null)
            {
                attachedSelectable.interactable = newState.Interactable;
            }
        }

        public static void NotifySampleSelectableUpdate()
        {
            UpdateSelectableStates?.Invoke();
        }
    }
}