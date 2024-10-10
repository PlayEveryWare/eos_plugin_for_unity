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

//#define SAMPLE_MENU_DEBUG

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

        /// <summary>
        /// Indicates that this object needs to refresh its UI.
        /// When this is true, the next time <see cref="UIFriendsMenu.Update"/> runs,
        /// <see cref="UIFriendsMenu.RefreshFriendUI"/> will be called.
        /// </summary>
        protected bool IsDirty { get; private set; } = true;

        protected SampleMenuWithFriends(bool startsHidden = true) : base(startsHidden, true) { }

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
        public bool RequiresAuthentication { get; }

        /// <summary>
        /// Indicates whether the SampleMenu starts out hidden or visible.
        /// NOTE: This Property might be something that can be removed, it
        ///       exists primarily to maintain functionality of current scenes.
        /// </summary>
        public bool StartsHidden { get; }

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

            Log($"SampleMenu created -> (StartsHidden = {StartsHidden}, RequiresAuthentication = {RequiresAuthentication}).");
        }

        /// <summary>
        /// Function that handles change in authentication.
        /// </summary>
        /// <param name="authenticated">
        /// True if the state has changed to authenticated, false otherwise.
        /// </param>
        /// <param name="authenticationChangeType">
        /// What kind of authentication change this is.</param>
        private void OnAuthenticationChanged(bool authenticated, AuthenticationListener.LoginChangeKind authenticationChangeType)
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
            // Default behavior is to take no action.
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
                Log($"UIParent is null for some reason.");
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
        /// Sets the focused control to be UIFirstSelected if doing so can be
        /// done. If reset is true, then ignore whether there already is a
        /// selected item. This is to maintain selected control when switching
        /// between menus.
        /// </summary>
        /// <param name="reset">
        /// If true, ignore whether there is already a selected control,
        /// otherwise only set the selected to UIFirstSelected if the current
        /// selected GameObject is null.
        /// </param>
        private void SetSelected(bool reset = true)
        {
            // Can't set selected if the event system is null
            if (null == EventSystem.current)
                return;

            // If the field member that holds a reference to the control that 
            // should be focused first is null, then it cannot be set as the
            // focused item.
            if (null == UIFirstSelected)
            {
                Debug.LogWarning($"{nameof(UIFirstSelected)} has not been set in the Unity Editor's inspector window, so the default selected control cannot be explicitly set.");
                return;
            }

            // Make sure that the field member that holds a reference to the 
            // control that should be focused first is active and can receive
            // input.
            if (!UIFirstSelected.activeSelf || !UIFirstSelected.activeInHierarchy)
            {
                Debug.Log($"{nameof(UIFirstSelected)} is either not active, not active in the hierarchy, or both - so it does not make sense to set it as the selected control, as it cannot receive input.");
                return;
            }

            // Honor the reset flag.
            if (!reset && null != EventSystem.current.currentSelectedGameObject)
                return;

            // If the currently selected game object is already UIFirstSelected
            // then take no action
            if (UIFirstSelected == EventSystem.current.currentSelectedGameObject)
                return;

            // Controller
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);

            Log($"Setting the selected object {EventSystem.current.currentSelectedGameObject} as the focused object.");
        }

        [Conditional("SAMPLE_MENU_DEBUG")]
        protected void Log(string message, [CallerMemberName] string memberName = "")
        {
            Debug.Log($"{GetType()}: {message} (called from {memberName}).");
        }
    }
}