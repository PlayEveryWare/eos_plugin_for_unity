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
    using UnityEngine.Events;

    public class SelectableStateHandler : MonoBehaviour
    {
        /// <summary>
        /// Represents a state for this component, indicating if it should be
        /// interactable and what tooltip should be displayed when
        /// moused over.
        /// </summary>
        public struct InteractableState
        {
            /// <summary>
            /// Indication of whether the associated Selectable should be
            /// interactable.
            /// </summary>
            public bool Interactable;

            /// <summary>
            /// The tooltip text that should be displayed on mouse over.
            /// If empty, a tooltip will not be displayed.
            /// </summary>
            public string TooltipText;

            public InteractableState(bool shouldBeInteractable, string newTooltipText = "")
            {
                Interactable = shouldBeInteractable;
                TooltipText = newTooltipText;
            }
        }

        /// <summary>
        /// The current state of the interactable this is attached to.
        /// This should be updated in the events assigned to
        /// <see cref="EventToUpdateState"/>.
        /// 
        /// It cannot be guaranteed that this is updated at the appropriate time.
        /// </summary>
        public InteractableState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                ApplySelectableStateChange();
            }
        }

        private InteractableState _state = new InteractableState(false);

        /// <summary>
        /// Event raised in <see cref="NotifySelectableUpdate"/> that informs
        /// all instances of this component to update their state through
        /// <see cref="EventToUpdateState"/>.
        /// </summary>
        private static event Action StateUpdated;

        /// <summary>
        /// When <see cref="StateUpdated"/> is raised, this event is called to 
        /// allow manager code to update this component's <see cref="State"/>.
        /// It is assumed manager code will update <see cref="State"/>
        /// appropriately inside the function assigned to this in the inspector.
        /// 
        /// This design paradigm allows inspector time configuration of state
        /// management.
        /// </summary>
        [SerializeField]
        private UnityEvent<SelectableStateHandler> EventToUpdateState;

        /// <summary>
        /// The attached UITooltip component, which has its text set by
        /// <see cref="State"/>'s <see cref="InteractableState.TooltipText"/>.
        /// 
        /// Optional. If not present, no tooltip will be updated.
        /// </summary>
        [SerializeReference]
        private UITooltip attachedTooltip;

        /// <summary>
        /// The associated Unity Selectable UI element this is associated with.
        /// <see cref="Selectable.interactable"/> is set by <see cref="State"/>'s
        /// <see cref="InteractableState.Interactable"/> value.
        /// 
        /// While optional, the purpose of this component is to augment
        /// Selectables.
        /// </summary>
        [SerializeReference]
        private Selectable attachedSelectable;

        private void OnEnable()
        {
            StateUpdated += UpdateSelectableState;
            UpdateSelectableState();
        }

        private void OnDisable()
        {
            StateUpdated -= UpdateSelectableState;
        }

        /// <summary>
        /// Updates this component's state through raising
        /// <see cref="EventToUpdateState"/>, which is presumed to contain
        /// functions that will update <see cref="State"/>. The setter in 
        /// <see cref="State"/> will then update the visual UI.
        /// 
        /// This is raised by <see cref="UpdateSelectableState"/>. As a public
        /// function, elements in the inspector could directly call this function
        /// as part of some event in order to update specific components.
        /// </summary>
        public void UpdateSelectableState()
        {
            EventToUpdateState.Invoke(this);
        }

        /// <summary>
        /// When <see cref="State"/> has its setter run, this handles applying
        /// the state to the UI.
        /// </summary>
        private void ApplySelectableStateChange()
        {
            if (attachedTooltip != null)
            {
                if (!string.IsNullOrEmpty(State.TooltipText))
                {
                    attachedTooltip.Text = State.TooltipText;
                }
                else
                {
                    attachedTooltip.Text = string.Empty;
                }
            }

            if (attachedSelectable != null)
            {
                attachedSelectable.interactable = State.Interactable;
            }
        }

        /// <summary>
        /// Notifies all instances of this component to update their state.
        /// </summary>
        public static void NotifySelectableUpdate()
        {
            StateUpdated?.Invoke();
        }
    }
}