namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public abstract class SampleSceneWithFriendsUI<T> : SampleSceneUI<T>, IFriendInteractionSource
        where T : IEOSSubManager, new()
    {
        protected bool _isDirty = false;

        protected EOSFriendsManager FriendsManager
        {
            get
            {
                return EOSManager.Instance.GetOrCreateManager<EOSFriendsManager>();
            }
        }

        public virtual IFriendInteractionSource.FriendInteractionState GetFriendInteractionState(FriendData friendData)
        {
            return IFriendInteractionSource.FriendInteractionState.Hidden;
        }

        public virtual string GetFriendInteractButtonText()
        {
            return string.Empty;
        }

        public virtual void OnFriendInteractButtonClicked(FriendData friendData)
        {
            // default behavior is to take no action.
        }

        public virtual bool IsDirty()
        {
            // default behavior is to return false.
            return false;
        }

        public virtual void ResetDirtyFlag()
        {
            // default behavior is unclear / inconsistent in implementing classes
            // (does it set the dirty flag to true or false when it's reset?)
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EOSManager.Instance.RemoveManager<EOSFriendsManager>();
        }

        public override void HideMenu()
        {
            base.HideMenu();
            FriendsManager.OnLoggedOut();
        }
    }

    public abstract class SampleSceneUI<T> : MonoBehaviour, ISampleSceneUI where T : IEOSSubManager, new()
    {
        [Header("Controller")]
        public GameObject UIController;

        [Header("Parent")]
        public GameObject UIParent;

        protected T Manager
        {
            get
            {
                return EOSManager.Instance.GetOrCreateManager<T>();
            }
        }

        protected virtual void OnDestroy()
        {
            EOSManager.Instance.RemoveManager<T>();
        }

        public virtual void ShowMenu()
        {
            UIParent?.SetActive(true);

            if (null != UIController && UIController.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(UIController);
            }
        }

        public virtual void HideMenu()
        {
            UIParent?.SetActive(false);
            Manager.OnLoggedOut();
        }
    }
}