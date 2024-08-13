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

#define SAMPLE_MENU_DEBUG

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
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

        protected SampleMenuWithFriends(bool startsHidden = true) : base(startsHidden) { }

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

    /// <summary>
    /// Contains implementation common to all sample menus.
    /// </summary>
    public abstract class SampleMenu : MonoBehaviour
    {
        /// <summary>
        /// Editable within the Unity Editor, it should be set to the control
        /// which should have focus when the SampleMenu first becomes visible.
        /// </summary>
        [Header("Controller")] 
        public GameObject UIFirstSelected;

        /// <summary>
        /// Editable within the Unity Editor, it should be set to the parent
        /// of the SampleMenu in the Scene hierarchy.
        /// </summary>
        public GameObject UIParent;

        /// <summary>
        /// Indicates whether the SampleMenu is hidden or not. If unset (has no
        /// value) that indicates that it should be hidden, but has not yet
        /// been set to be hidden explicitly.
        /// </summary>
        private bool? _hidden;

        /// <summary>
        /// Indicates whether the SampleMenu is hidden or not.
        /// </summary>
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

        /// <summary>
        /// Indicates whether the SampleMenu requires a user be authenticated
        /// in order to interact with the scene.
        /// </summary>
        public bool RequiresAuthentication { get; private set; }

        /// <summary>
        /// Indicates whether the SampleMenu starts out hidden or visible.
        /// NOTE: This Property might be something that can be removed, it
        ///       exists primarily to maintain functionality of current scenes.
        /// </summary>
        public bool StartsHidden { get; private set; }

        /// <summary>
        /// Constructor for SampleMenu.
        /// </summary>
        /// <param name="startsHidden">
        /// Indicates whether the SampleMenu should be visible or hidden when it
        /// launches. Most of them should be hidden.
        /// </param>
        /// <param name="requiresAuthentication">
        /// Indicates whether the SampleMenu requires a user be authenticated in
        /// order for it to be used. Most of them require authentication.
        /// </param>
        protected SampleMenu(bool startsHidden = true, bool requiresAuthentication = true)
        {
            StartsHidden = startsHidden;
            RequiresAuthentication = requiresAuthentication;
        }

        /// <summary>
        /// Function that handles change in authentication.
        /// </summary>
        /// <param name="authenticated">
        /// True if the state has changed to authenticated, false otherwise.
        /// </param>
        private void OnAuthenticationChanged(bool authenticated)
        {
            if (authenticated || !RequiresAuthentication)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        /// <summary>
        /// When overriding, call this base implementation first.
        /// </summary>
        protected virtual void Awake()
        {
            // When awake is called, subscribe to the authentication change
            // event on AuthenticationListener.
            AuthenticationListener.Instance.AuthenticationChanged += OnAuthenticationChanged;
        }

        /// <summary>
        /// When overriding, call this base implementation first.
        /// </summary>
        protected virtual void OnDestroy()
        {
            // When OnDestroy is called, remove the subscription from the 
            // AuthenticationChanged event.
            AuthenticationListener.Instance.AuthenticationChanged -= OnAuthenticationChanged;
        }

        /// <summary>
        /// When overriding, call this base implementation first.
        /// </summary>
        protected virtual void OnEnable()
        {
            // If the scene should start hidden
            if (StartsHidden)
            {
                Hide();
            }
        }

        /// <summary>
        /// When overriding, call this base implementation first.
        /// </summary>
        protected virtual void Update()
        {
            // If nothing is selected, set the selected (focused) control to be
            // UIFirstSelected (if that field has been set).
            SetSelected();
        }

        /// <summary>
        /// Shows the SampleMenu. If overriding, make sure to call this base
        /// implementation first.
        /// </summary>
        public virtual void Show()
        {
            Log($"Show() started");

            // Don't do anything if already showing.
            if (!Hidden) return;

            if (null == UIParent)
            {
                Log($"UIParent for is null for some reason.");
            }
            else
            {
                UIParent.SetActive(true);
            }

            // Make the gameObject active. This should make all the relevant
            // items in the sample menu visible and interactable. 
            if (null == gameObject)
            {
                Log($"GameObject for is null for some reason.");
                return;
            }
            else
            {
                gameObject.SetActive(true);
            }

            // Set the selected control to be the first selected element, so
            // long as that element can actually be selected.
            SetSelected();

            // Flag as showing.
            Hidden = false;

            Log($"Show() completed");
        }

        /// <summary>
        /// Hides the SampleMenu. If overriding, make sure to call this base
        /// implementation first.
        /// </summary>
        public virtual void Hide()
        {
            Log($"Hide() started");

            // Don't do anything if already hidden.
            if (_hidden.HasValue && _hidden.Value) return;

            if (null == UIParent)
            {
                Log($"UIParent is null for some reason.");
            }
            else
            {
                UIParent.SetActive(false);
            }

            // Make the gameObject inactive. This should make all the relevant 
            // items in the sample menu invisible and non-interactable.
            gameObject.SetActive(false);

            // Flag as hidden.
            Hidden = true;

            Log($"Hide() completed");
        }

        /// <summary>
        /// Sets the focused control to UIFirstSelected, assuming it is both
        /// defined and active in the hierarchy.
        /// </summary>
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

        [Conditional("SAMPLE_MENU_DEBUG")]
        protected void Log(string message, [CallerMemberName] string memberName = "")
        {
            Debug.Log($"{GetType()}: {message} (called from {memberName}).");
        }
    }
}