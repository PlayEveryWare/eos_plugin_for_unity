
//-------------------------------------------------------------------------
/// Helper class to manage the various handles returned by "AddNotifyOn.*" functions.
/// It should be the case that by using this class, the handle will be properly removed
/// when the NotifyEventHandle loses reference, including in cases where an unhandled 
/// exception occurs.
namespace PlayEveryWare.EpicOnlineServices
{
    public class NotifyEventHandle : GenericSafeHandle<ulong>
    {
        private RemoveDelegate removeDelegate;

        public delegate void RemoveDelegate(ulong aHandle);
        public NotifyEventHandle(ulong aLong, RemoveDelegate aRemoveDelegate) : base(aLong)
        {
            removeDelegate = aRemoveDelegate;
        }

        protected override void ReleaseHandle()
        {
            if(!IsValid())
            {
                return;
            }
            removeDelegate(handleObject);
            handleObject = 0;
        }

        public override bool IsValid()
        {
            return handleObject != 0;
        }
    }
}
