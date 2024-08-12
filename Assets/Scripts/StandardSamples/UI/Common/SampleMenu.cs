﻿/*
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

#define SAMPLE_MENU_DEBUG

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System.Diagnostics;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using Debug = UnityEngine.Debug;

    public abstract class SampleMenuWithFriends : SampleMenu
    {
        public enum FriendInteractionState
        {
            Hidden,
            Disabled,
            Enabled
        }
        protected SampleMenuWithFriends(bool startsHidden = true) : base("NONE", startsHidden) { }

        /// <summary>
        /// Indicates that this object needs to refresh its UI.
        /// When this is true, the next time <see cref="UIFriendsMenu.Update"/> runs,
        /// <see cref="UIFriendsMenu.RefreshFriendUI"/> will be called.
        /// </summary>
        protected bool IsDirty { get; private set; } = true;

        public virtual FriendInteractionState GetFriendInteractionState(FriendData friendData)
        {
            return FriendInteractionState.Hidden;
        }

        public virtual string GetFriendInteractButtonText()
        {
            return string.Empty;
        }

        public virtual void OnFriendInteractButtonClicked(FriendData friendData)
        {
        }

        //Should the friend UI update interaction state from this source?
        public virtual bool IsFriendsUIDirty()
        {
            return IsDirty;
        }

        public virtual void SetDirtyFlag(bool isDirty = true)
        {
            IsDirty = isDirty;
        }

        /// <summary>
        /// This function is called before refreshing the <see cref="UIFriendsMenu"/>.
        /// This is an opportunity to refresh any caches or do processing work that should be done once,
        /// instead of processed for each call of <see cref="GetFriendInteractionState(FriendData)"/>.
        /// </summary>
        public virtual void OnFriendStateChanged()
        {

        }
    }

    

    public abstract class SampleMenu : AuthenticationListener
    {
        [Header("Controller")] 
        public GameObject UIFirstSelected;

        public GameObject UIParent;

        private bool? _hidden;

        public bool Hidden
        {
            get
            {
                return (!_hidden.HasValue) || _hidden.Value;
            }
            private set
            {
                _hidden = value;
            }
        }

        public bool StartsHidden { get; private set; }

        public string Label { get; private set; }

        protected SampleMenu(string label = "NONE", bool startsHidden = true)
        {
            StartsHidden = startsHidden;
            Label = label;
        }

        protected override void OnAuthenticationChanged(bool authenticated)
        {
            if (authenticated)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void Awake()
        {
            Log($"{Label}: Awake()");
            InternalAwake();

            if (StartsHidden)
            {
                Hide();
            }
        }
        
        protected virtual void InternalAwake()
        {
            // Default behavior is empty.
        }

        public void Update()
        {
            SetSelected();
            InternalUpdate();
        }

        public void Show()
        {
            Log($"{Label}: Show()");

            // Don't do anything if already showing.
            if (!Hidden) return;

            if (null == UIParent)
            {
                Log($"UIParent for {Label} is null for some reason.");
            }
            else
            {
                UIParent.SetActive(true);
            }

            // Make the gameObject active. This should make all the relevant
            // items in the sample menu visible and interactable. 

            if (null == gameObject)
            {
                Log($"GameObject for {Label} is null for some reason.");
                return;
            }
            else
            {
                gameObject.SetActive(true);
            }

            // Set the selected control to be the first selected element, so
            // long as that element can actually be selected.
            SetSelected();

            // Call whatever custom show logic there might be for the sample 
            // menu.
            InternalShow();

            // Flag as showing.
            Hidden = false;

            Log($"{Label}: Show() completed");
        }

        public void Hide()
        {
            Log($"{Label}: Hide()");

            // Don't do anything if already hidden.
            if (_hidden.HasValue && _hidden.Value) return;

            // Make the gameObject inactive. This should make all the relevant 
            // items in the sample menu invisible and non-interactable.
            gameObject.SetActive(false);

            // Call whatever custom hide logic there might be for the sample 
            // menu.
            InternalHide();

            // Flag as hidden.
            Hidden = true;

            Log($"{Label}: Hide() completed");
        }

        private void SetSelected()
        {
            // Controller: Detect if nothing is selected and controller input detected, and set default
            if (UIFirstSelected == null 
                || UIFirstSelected.activeSelf == false
                || UIFirstSelected.activeInHierarchy == false
                || EventSystem.current == null || EventSystem.current.currentSelectedGameObject != null
                || !InputUtility.WasGamepadUsedLastFrame())
            {
                return;
            }

            // Controller
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
            Debug.Log("Nothing currently selected, default to UIFirstSelected: EventSystem.current.currentSelectedGameObject = " + EventSystem.current.currentSelectedGameObject);
        }

        protected virtual void InternalUpdate() { }

        protected abstract void InternalShow();

        protected abstract void InternalHide();

        [Conditional("SAMPLE_MENU_DEBUG")]
        protected void Log(string message)
        {
            Debug.Log(message);
        }
    }
}