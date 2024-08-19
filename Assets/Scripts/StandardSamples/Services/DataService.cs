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
    using Samples;
    using System.Collections.Generic;

    public abstract class DataService : EOSService
    {
        /// <summary>
        /// Indicates the maximum number of bytes that can be read at a time
        /// during data transfer operations.
        /// </summary>
        protected const uint MaxChunkSize = 4096;

        /// <summary>
        /// Stores the file transfers that are currently in progress for the
        /// given data service.
        /// </summary>
        protected Dictionary<string, EOSTransferInProgress> _transfersInProgress;

        /// <summary>
        /// Stores the locally cached copy of the data that the service manages.
        /// </summary>
        protected Dictionary<string, string> _storageData;

        /// <summary>
        /// Clears all the current transfers that the data service is managing.
        /// </summary>
        protected abstract void ClearCurrentTransfers();

        /// <summary>
        /// Stores the name of the currently transferring file.
        /// </summary>
        protected string CurrentTransferName;

        /// <summary>
        /// Clears local storage and clears the current transfers that are
        /// being managed by the data storage service. If overridden, make
        /// certain to first call this method within the overridden
        /// implementation.
        /// </summary>
        protected override void OnLoggedOut()
        {
            // TODO: Need to clear / cancel currently active transfer tasks.
            _storageData.Clear();
            ClearCurrentTransfers();
        }
    }
}