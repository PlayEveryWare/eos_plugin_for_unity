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

namespace PlayEveryWare.EpicOnlineServices.Tests.Config
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Config = EpicOnlineServices.Config;

    public class EOSConfigTests
    {
        [Test]
        public void ProductName_MustNotBeEmpty()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.productName = string.Empty;
        }

        [Test]
        public void ProductVersion_MustNotBeEmpty()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.productVersion = string.Empty;
        }

        [Test]
        public void ProductId_MustBeValidGUID()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.productID = "notaguid";
        }

        [Test]
        public void SandboxID_MustBeValidGUID_OrHavePrefix()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.sandboxID = "notaguid";
        }

        [Test]
        public void DeploymentID_MustBeValidGUID()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.deploymentID = "notaguid";
        }

        [Test]
        public void PlatformTags_MustParse()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.platformOptionsFlags = new List<string>() { "invalidoptions " };
        }

        [Test]
        public void AuthScopeFlags_MustParse()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.authScopeOptionsFlags = new List<string>() { "invalidoptions " };
        }

        [Test]
        public void TicketBudgetInMilliseconds_MustBeValidUnit()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();

            // Unsure how to force this to be an invalid value at this point; is the test to check the configuration loader by string?
            // config.tickBudgetInMilliseconds = -100;
        }

        [Test]
        public void ThreadAffinity_AllParse()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.ThreadAffinity_HTTPRequestIO = "abc";
            config.ThreadAffinity_networkWork = "abc";
            config.ThreadAffinity_P2PIO = "abc";
            config.ThreadAffinity_RTCIO = "abc";
            config.ThreadAffinity_storageIO = "abc";
            config.ThreadAffinity_webSocketIO = "abc";
        }
    }
}