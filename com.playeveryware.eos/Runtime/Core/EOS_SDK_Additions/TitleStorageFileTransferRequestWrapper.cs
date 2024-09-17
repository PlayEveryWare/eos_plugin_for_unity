/*
 * Copyright (c) 2024 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 */

namespace PlayEveryWare.EpicOnlineServices
{
    using Epic.OnlineServices;
    using Epic.OnlineServices.TitleStorage;

    /// <summary>
    /// Used to wrap the TitleStorageFileTransferRequest class provided by the
    /// EOS SDK. This is to treat all FileTransferRequest classes provided by
    /// the EOS SDK as if they shared an interface (even though they do not).
    /// </summary>
    public class TitleStorageFileTransferRequestWrapper
        : FileRequestTransferWrapper<TitleStorageFileTransferRequest>
    {
        public TitleStorageFileTransferRequestWrapper(TitleStorageFileTransferRequest instance)
            : base(instance)
        {
        }

        public static implicit operator TitleStorageFileTransferRequestWrapper(
            TitleStorageFileTransferRequest instance)
        {
            return new TitleStorageFileTransferRequestWrapper(instance);
        }

        public override Result CancelRequest()
        {
            return null == _instance ? Result.Success : _instance.CancelRequest();
        }

        public override void Release()
        {
            _instance?.Release();
            _instance = null;
        }
    }
}