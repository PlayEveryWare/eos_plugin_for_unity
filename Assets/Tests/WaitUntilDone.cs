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
 */

namespace PlayEveryWare.EpicOnlineServices.Tests
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Custom yield that has a timeout so it will complete when either the predicate is true or
    /// the timeout is reached.
    /// </summary>
    public sealed class WaitUntilDone : CustomYieldInstruction
    {
        private readonly Func<bool> _predicate;
        
        private float _timeout;

        private bool WaitForDoneProcess()
        {
            _timeout -= Time.deltaTime;
            return _timeout <= 0f || _predicate();
        }

        public override bool keepWaiting => !WaitForDoneProcess();

        public WaitUntilDone(float timeout, Func<bool> predicate)
        {
            this._predicate = predicate;
            this._timeout = timeout;
        }
    }
}