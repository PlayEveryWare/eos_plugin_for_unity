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
    using Epic.OnlineServices.Platform;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using static PlayEveryWare.EpicOnlineServices.EOSConfig;
    using Config = EpicOnlineServices.Config;

    public class EOSConfigTests
    {
        [Test]
        public void ProductName_MustNotBeEmpty()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.productName = string.Empty;

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                Assert.Fail($"Config should have failing attributes.");
            }

            Assert.IsTrue(failuresIncludeExpectedFailure<NonEmptyStringFieldValidatorAttribute>(
                nameof(EOSConfig.productName), 
                failingAttributes,
                NonEmptyStringFieldValidatorAttribute.FieldIsEmptyMessage),
                "There should be a failure of the expected type and message.");
        }

        [Test]
        public void ProductName_SuccessfulParsing()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.productName = "My Valid Product Name";

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                // If there are no errors, then this test is a success
                // The config might be failing in other ways
                return;
            }

            Assert.IsFalse(failuresIncludeExpectedFailure<NonEmptyStringFieldValidatorAttribute>(
                nameof(EOSConfig.productName),
                failingAttributes,
                NonEmptyStringFieldValidatorAttribute.FieldIsEmptyMessage),
                "Product Name should not have errors describing it as an empty field.");
        }

        [Test]
        public void ProductVersion_MustNotBeEmpty()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.productVersion = string.Empty;

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                Assert.Fail($"Config should have failing attributes.");
            }

            Assert.IsTrue(failuresIncludeExpectedFailure<NonEmptyStringFieldValidatorAttribute>(
                nameof(EOSConfig.productVersion), 
                failingAttributes,
                NonEmptyStringFieldValidatorAttribute.FieldIsEmptyMessage),
                "There should be a failure of the expected type and message.");
        }

        [Test]
        public void ProductVersion_SuccessfulParsing()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.productVersion = "123.456";

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                // If there are no errors, then this test is a success
                // The config might be failing in other ways
                return;
            }

            Assert.IsFalse(failuresIncludeExpectedFailure<NonEmptyStringFieldValidatorAttribute>(
                nameof(EOSConfig.productVersion),
                failingAttributes,
                NonEmptyStringFieldValidatorAttribute.FieldIsEmptyMessage),
                "Product Version should not have errors describing it as an empty field.");
        }

        [Test]
        public void ProductId_MustBeValidGUID()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.productID = "notaguid";

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                Assert.Fail($"Config should have failing attributes.");
            }

            Assert.IsTrue(failuresIncludeExpectedFailure<GUIDFieldValidatorAttribute>(
                nameof(EOSConfig.productID),
                failingAttributes,
                GUIDFieldValidatorAttribute.NotAGuidMessage),
                "There should be a failure of the expected type and message.");
        }

        [Test]
        public void ProductId_SuccessfulParsing()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.productID = Guid.NewGuid().ToString();

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                // If there are no errors, then this test is a success
                // The config might be failing in other ways
                return;
            }

            Assert.IsFalse(failuresIncludeExpectedFailure<GUIDFieldValidatorAttribute>(
                nameof(EOSConfig.productID),
                failingAttributes,
                GUIDFieldValidatorAttribute.NotAGuidMessage),
                "Product Id should not have errors describing it as an invalid GUID.");
        }

        [Test]
        public void SandboxID_MustBeValidGUID_OrHavePrefix()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.sandboxID = "notaguid";

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                Assert.Fail($"Config should have failing attributes.");
            }

            Assert.IsTrue(failuresIncludeExpectedFailure<DevelopmentEnvironmentFieldValidatorAttribute>(
                nameof(EOSConfig.sandboxID),
                failingAttributes,
                DevelopmentEnvironmentFieldValidatorAttribute.FieldDidNotMatchMessage),
                "There should be a failure of the expected type and message.");
        }

        [Test]
        public void SandboxID_SuccessfulParsing_WithGUID()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.sandboxID = Guid.NewGuid().ToString();

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                // If there are no errors, then this test is a success
                // The config might be failing in other ways
                return;
            }

            Assert.IsFalse(failuresIncludeExpectedFailure<DevelopmentEnvironmentFieldValidatorAttribute>(
                nameof(EOSConfig.sandboxID),
                failingAttributes,
                DevelopmentEnvironmentFieldValidatorAttribute.FieldDidNotMatchMessage),
                "Sandbox Id should not have errors relating to it failing to parse.");
        }

        [Test]
        public void SandboxID_SuccessfulParsing_WithDevelopmentEnvironment()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.sandboxID = "p-1234567890ABCDEFGHIJKLMNOPQRST";

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                // If there are no errors, then this test is a success
                // The config might be failing in other ways
                return;
            }

            Assert.IsFalse(failuresIncludeExpectedFailure<DevelopmentEnvironmentFieldValidatorAttribute>(
                nameof(EOSConfig.sandboxID),
                failingAttributes,
                DevelopmentEnvironmentFieldValidatorAttribute.FieldDidNotMatchMessage),
                "Sandbox Id should not have errors relating to it failing to parse.");
        }

        [Test]
        public void DeploymentOverride_MustBeValid()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.sandboxDeploymentOverrides = new List<SandboxDeploymentOverride>()
            {
                new SandboxDeploymentOverride()
                {
                    sandboxID = "abc",
                    deploymentID = "abc"
                }
            };

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                Assert.Fail($"Config should have failing attributes.");
            }

            Assert.IsTrue(failuresIncludeExpectedFailure<GUIDFieldValidatorAttribute>(
                nameof(SandboxDeploymentOverride.deploymentID),
                failingAttributes,
                GUIDFieldValidatorAttribute.NotAGuidMessage),
                "Deployment ID in sandbox override should have failure relating to it.");

            Assert.IsTrue(failuresIncludeExpectedFailure<DevelopmentEnvironmentFieldValidatorAttribute>(
                nameof(SandboxDeploymentOverride.sandboxID),
                failingAttributes,
                DevelopmentEnvironmentFieldValidatorAttribute.FieldDidNotMatchMessage),
                "Sandbox ID in sandbox override should have failure relating to it.");
        }

        [Test]
        public void DeploymentOverride_SuccessfulParse()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.sandboxDeploymentOverrides = new List<SandboxDeploymentOverride>()
            {
                new SandboxDeploymentOverride()
                {
                    sandboxID = Guid.NewGuid().ToString(),
                    deploymentID = Guid.NewGuid().ToString()
                }
            };

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                // If there are no errors, then this test is a success
                // The config might be failing in other ways
                return;
            }

            Assert.IsFalse(failuresIncludeExpectedFailure<GUIDFieldValidatorAttribute>(
                nameof(SandboxDeploymentOverride.deploymentID),
                failingAttributes,
                GUIDFieldValidatorAttribute.NotAGuidMessage),
                "Deployment ID in sandbox override should not have error relating to GUID failing to parse.");

            Assert.IsFalse(failuresIncludeExpectedFailure<DevelopmentEnvironmentFieldValidatorAttribute>(
                nameof(SandboxDeploymentOverride.sandboxID),
                failingAttributes,
                DevelopmentEnvironmentFieldValidatorAttribute.FieldDidNotMatchMessage),
                "Sandbox ID in sandbox override should not have error relating to it failing to parse.");
        }

        [Test]
        public void DeploymentID_MustBeValidGUID()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.deploymentID = "notaguid";

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                Assert.Fail($"Config should have failing attributes.");
            }

            Assert.IsTrue(failuresIncludeExpectedFailure<GUIDFieldValidatorAttribute>(
                nameof(EOSConfig.deploymentID),
                failingAttributes,
                GUIDFieldValidatorAttribute.NotAGuidMessage),
                "There should be a failure of the expected type and message.");
        }

        [Test]
        public void DeploymentID_SuccessfulParse()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.deploymentID = Guid.NewGuid().ToString();

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                // If there are no errors, then this test is a success
                // The config might be failing in other ways
                return;
            }

            Assert.IsFalse(failuresIncludeExpectedFailure<GUIDFieldValidatorAttribute>(
                nameof(EOSConfig.deploymentID),
                failingAttributes,
                GUIDFieldValidatorAttribute.NotAGuidMessage),
                "There should be a failure of the expected type and message.");
        }

        [Test]
        public void PlatformTags_MustParse()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.platformOptionsFlags = new List<string>() { "invalidoptions " };

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                Assert.Fail($"Config should have failing attributes.");
            }

            Assert.IsTrue(failuresIncludeExpectedFailure<ParsesToPlatformFlagFieldValidatorAttribute>(
                nameof(EOSConfig.platformOptionsFlags),
                failingAttributes,
                ParsesToEnumFieldValidatorAttribute.FailedToParseTokensMessage),
                "There should be a failure of the expected type and message.");
        }

        [Test]
        public void PlatformTags_SuccessfulParse()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.platformOptionsFlags = new List<string>() { PlatformFlags.DisableOverlay.ToString(), PlatformFlags.WindowsEnableOverlayD3D9.ToString()};

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                // If there are no errors, then this test is a success
                // The config might be failing in other ways
                return;
            }

            Assert.IsFalse(failuresIncludeExpectedFailure<ParsesToPlatformFlagFieldValidatorAttribute>(
                nameof(EOSConfig.platformOptionsFlags),
                failingAttributes,
                ParsesToEnumFieldValidatorAttribute.FailedToParseTokensMessage),
                "There should not be a failure regarding failing to parse Platform Tags.");
        }

        [Test]
        public void AuthScopeFlags_MustParse()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.authScopeOptionsFlags = new List<string>() { "invalidoptions " };

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                Assert.Fail($"Config should have failing attributes.");
            }

            Assert.IsTrue(failuresIncludeExpectedFailure<ParsesToAuthScopeFieldValidatorAttribute>(
                nameof(EOSConfig.authScopeOptionsFlags),
                failingAttributes,
                ParsesToEnumFieldValidatorAttribute.FailedToParseTokensMessage),
                "There should be a failure of the expected type and message.");
        }

        [Test]
        public void AuthScope_SuccessfulParse()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.platformOptionsFlags = new List<string>() { PlatformFlags.DisableOverlay.ToString(), PlatformFlags.WindowsEnableOverlayD3D9.ToString() };

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                // If there are no errors, then this test is a success
                // The config might be failing in other ways
                return;
            }

            Assert.IsFalse(failuresIncludeExpectedFailure<ParsesToAuthScopeFieldValidatorAttribute>(
                nameof(EOSConfig.authScopeOptionsFlags),
                failingAttributes,
                ParsesToEnumFieldValidatorAttribute.FailedToParseTokensMessage),
                "There should not be a failure regarding failing to parse Auth Scope.");
        }

        [Test]
        public void ThreadAffinity_MustParse()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.ThreadAffinity_HTTPRequestIO = "abc";
            config.ThreadAffinity_networkWork = "abc";
            config.ThreadAffinity_P2PIO = "abc";
            config.ThreadAffinity_RTCIO = "abc";
            config.ThreadAffinity_storageIO = "abc";
            config.ThreadAffinity_webSocketIO = "abc";

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                Assert.Fail($"Config should have failing attributes.");
            }

            Assert.IsTrue(failuresIncludeExpectedFailure<ParsesToUlongFieldValidatorAttribute>(
                nameof(EOSConfig.ThreadAffinity_HTTPRequestIO),
                failingAttributes,
                ParsesToUlongFieldValidatorAttribute.FailedToParseMessage),
                "There should be a failure of the expected type and message for ThreadAffinity_HTTPRequestIO.");

            Assert.IsTrue(failuresIncludeExpectedFailure<ParsesToUlongFieldValidatorAttribute>(
                nameof(EOSConfig.ThreadAffinity_networkWork),
                failingAttributes,
                ParsesToUlongFieldValidatorAttribute.FailedToParseMessage),
                "There should be a failure of the expected type and message for ThreadAffinity_networkWork.");

            Assert.IsTrue(failuresIncludeExpectedFailure<ParsesToUlongFieldValidatorAttribute>(
                nameof(EOSConfig.ThreadAffinity_P2PIO),
                failingAttributes,
                ParsesToUlongFieldValidatorAttribute.FailedToParseMessage),
                "There should be a failure of the expected type and message for ThreadAffinity_P2PIO.");

            Assert.IsTrue(failuresIncludeExpectedFailure<ParsesToUlongFieldValidatorAttribute>(
                nameof(EOSConfig.ThreadAffinity_RTCIO),
                failingAttributes,
                ParsesToUlongFieldValidatorAttribute.FailedToParseMessage),
                "There should be a failure of the expected type and message for ThreadAffinity_RTCIO.");

            Assert.IsTrue(failuresIncludeExpectedFailure<ParsesToUlongFieldValidatorAttribute>(
                nameof(EOSConfig.ThreadAffinity_storageIO),
                failingAttributes,
                ParsesToUlongFieldValidatorAttribute.FailedToParseMessage),
                "There should be a failure of the expected type and message for ThreadAffinity_storageIO.");

            Assert.IsTrue(failuresIncludeExpectedFailure<ParsesToUlongFieldValidatorAttribute>(
                nameof(EOSConfig.ThreadAffinity_webSocketIO),
                failingAttributes,
                ParsesToUlongFieldValidatorAttribute.FailedToParseMessage),
                "There should be a failure of the expected type and message for ThreadAffinity_webSocketIO.");
        }

        [Test]
        public void ThreadAffinity_SuccessfulParse()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.ThreadAffinity_HTTPRequestIO = "12345678";
            config.ThreadAffinity_networkWork = "12345678";
            config.ThreadAffinity_P2PIO = "12345678";
            config.ThreadAffinity_RTCIO = "12345678";
            config.ThreadAffinity_storageIO = "12345678";
            config.ThreadAffinity_webSocketIO = "12345678";

            if (!config.TryGetFailingValidatorAttributes(out List<FieldValidatorFailure> failingAttributes))
            {
                // If there are no errors, then this test is a success
                // The config might be failing in other ways
                return;
            }

            Assert.IsFalse(failuresIncludeExpectedFailure<ParsesToUlongFieldValidatorAttribute>(
                nameof(EOSConfig.ThreadAffinity_HTTPRequestIO),
                failingAttributes,
                ParsesToUlongFieldValidatorAttribute.FailedToParseMessage),
                "There should not be an error relating to parsing ThreadAffinity_HTTPRequestIO.");

            Assert.IsFalse(failuresIncludeExpectedFailure<ParsesToUlongFieldValidatorAttribute>(
                nameof(EOSConfig.ThreadAffinity_networkWork),
                failingAttributes,
                ParsesToUlongFieldValidatorAttribute.FailedToParseMessage),
                "There should not be an error relating to parsing ThreadAffinity_networkWork.");

            Assert.IsFalse(failuresIncludeExpectedFailure<ParsesToUlongFieldValidatorAttribute>(
                nameof(EOSConfig.ThreadAffinity_P2PIO),
                failingAttributes,
                ParsesToUlongFieldValidatorAttribute.FailedToParseMessage),
                "There should not be an error relating to parsing ThreadAffinity_P2PIO.");

            Assert.IsFalse(failuresIncludeExpectedFailure<ParsesToUlongFieldValidatorAttribute>(
                nameof(EOSConfig.ThreadAffinity_RTCIO),
                failingAttributes,
                ParsesToUlongFieldValidatorAttribute.FailedToParseMessage),
                "There should not be an error relating to parsing ThreadAffinity_RTCIO.");

            Assert.IsFalse(failuresIncludeExpectedFailure<ParsesToUlongFieldValidatorAttribute>(
                nameof(EOSConfig.ThreadAffinity_storageIO),
                failingAttributes,
                ParsesToUlongFieldValidatorAttribute.FailedToParseMessage),
                "There should not be an error relating to parsing ThreadAffinity_storageIO.");

            Assert.IsFalse(failuresIncludeExpectedFailure<ParsesToUlongFieldValidatorAttribute>(
                nameof(EOSConfig.ThreadAffinity_webSocketIO),
                failingAttributes,
                ParsesToUlongFieldValidatorAttribute.FailedToParseMessage),
                "There should not be an error relating to parsing ThreadAffinity_webSocketIO.");
        }

        /// <summary>
        /// Determines if the provided list of failures contains an expected failure
        /// </summary>
        /// <typeparam name="T">The kind of validator attribute that is expected.</typeparam>
        /// <param name="fieldName">The name of the field that is being checked for.</param>
        /// <param name="failures">A list of all failures from validation.</param>
        /// <param name="message">
        /// A specific error message to check for.
        /// Optional. If null or empty, this is not used.
        /// </param>
        /// <returns>True if the expected failure is within the list.</returns>
        private bool failuresIncludeExpectedFailure<T>(string fieldName, List<FieldValidatorFailure> failures, string message = "") where T : FieldValidatorAttribute
        {
            foreach (FieldValidatorFailure currentFailure in failures)
            {
                if (currentFailure.FailingAttribute is not T)
                {
                    continue;
                }

                if (currentFailure.FieldInfo.Name != fieldName)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(message))
                {
                    if (currentFailure.FailingMessage != message)
                    {
                        continue;
                    }
                }

                return true;
            }

            return false;
        }
    }
}