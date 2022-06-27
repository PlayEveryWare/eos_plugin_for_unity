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
