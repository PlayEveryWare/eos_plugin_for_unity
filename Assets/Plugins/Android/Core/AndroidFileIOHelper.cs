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

namespace PlayEveryWare.EpicOnlineServices
{
    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;

    public class AndroidFileIOHelper
    {
        public static async Task<string> ReadAllText(string filePath)
        {
            using UnityWebRequest request = UnityWebRequest.Get(filePath);
            request.timeout = 2; //seconds till timeout
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            string text = null;

            switch (request.result)
            {
                case UnityWebRequest.Result.InProgress:
                    // This should not happen, because of the Task.Yield() call
                    // above.
                    Debug.LogWarning("AndroidFileIOHelper: For some reason " +
                                     "the request is still in progress.");
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError($"AndroidFileIOHelper: " +
                                   $"UnityWebRequest for path " +
                                   $"\"{filePath},\" failed with error code " +
                                   $"'{request.result},' and error, " +
                                   $"\"{request.error}.\"");
                    break;
                case UnityWebRequest.Result.Success:
                    text = request.downloadHandler.text;
                    break;
                default:
                    ArgumentException unknownResultException =
                        new($"Unrecognized result returned from {nameof(UnityWebRequest)}: {request.result}.");
                    Debug.LogException(unknownResultException);
                    throw unknownResultException;
            }

            return text;
        }
    }
}
