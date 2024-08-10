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

    public abstract class ISampleSceneUIWithFriends : ISampleSceneUI
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

    public abstract class ISampleSceneUI : MonoBehaviour
    {
        [Header("Controller")] 
        public GameObject UIFirstSelected;
        

        public abstract void ShowMenu();
        public abstract void HideMenu();
    }
}