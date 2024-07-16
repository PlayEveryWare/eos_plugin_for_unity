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
    /// <summary>
    /// Common test parameters shared between the test scene and unit/integration tests.
    /// </summary>
    public static class TestCommon
    {
        public const string TestResultFilename =
#if UNITY_SWITCH
            "TestResults-Switch";
#elif UNITY_PS4
            "TestResults-PS4";
#elif UNITY_PS5
            "TestResults-PS5";
#elif UNITY_GAMECORE_XBOXONE
            "TestResults-XBoxOne";
#elif UNITY_IOS
            "TestResults-IOS";
#elif UNITY_ANDROID
            "TestResults-Android";
#elif (UNITY_STANDALONE_WIN || UNITY_WSA) && PLATFORM_64BITS
            "TestResults-x64";
#elif UNITY_STANDALONE_WIN && PLATFORM_32BITS
            "TestResults-x32";
#else
            "TestResults";
#endif

        public const string SearchBucketIdKey = "bucket";
        public const string LobbyBucketId = "TestServerLobbyBucket";
        public const string LobbyPrivateBucketId = "TestServerPrivateLobbyBucket";
        public const string SessionBucketId = "TestServerSessionBucket";
        public const string SessionPrivateBucketId = "TestServerPrivateSessionBucket";
        public const string SessionName = "TestServerSession";
        public const string SessionPrivateName = "TestServerPrivateSession";
        public const string LevelKey = "LEVEL";
        public const string LobbyLevelName = "FOREST";
        public const string SessionLevelName = "CASTLE";
    }
}
