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

            if (!FieldValidator.TryGetFailingValidatorAttributes(config, out List<FieldValidatorFailure> failingAttributes))
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

            if (!FieldValidator.TryGetFailingValidatorAttributes(config, out List <FieldValidatorFailure> failingAttributes))
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

            if (!FieldValidator.TryGetFailingValidatorAttributes(config, out List<FieldValidatorFailure> failingAttributes))
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

            if (!FieldValidator.TryGetFailingValidatorAttributes(config, out List<FieldValidatorFailure> failingAttributes))
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

            if (!FieldValidator.TryGetFailingValidatorAttributes(config, out List<FieldValidatorFailure> failingAttributes))
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

            if (!FieldValidator.TryGetFailingValidatorAttributes(config, out List<FieldValidatorFailure> failingAttributes))
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

            if (!FieldValidator.TryGetFailingValidatorAttributes(config, out List<FieldValidatorFailure> failingAttributes))
            {
                Assert.Fail($"Config should have failing attributes.");
            }

            Assert.IsTrue(failuresIncludeExpectedFailure<SandboxIDFieldValidatorAttribute>(
                nameof(EOSConfig.sandboxID),
                failingAttributes,
                SandboxIDFieldValidatorAttribute.FieldDidNotMatchMessage),
                "There should be a failure of the expected type and message.");
        }

        [Test]
        public void SandboxID_SuccessfulParsing_WithGUID()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.sandboxID = Guid.NewGuid().ToString();

            if (!FieldValidator.TryGetFailingValidatorAttributes(config, out List<FieldValidatorFailure> failingAttributes))
            {
                // If there are no errors, then this test is a success
                // The config might be failing in other ways
                return;
            }

            Assert.IsFalse(failuresIncludeExpectedFailure<SandboxIDFieldValidatorAttribute>(
                nameof(EOSConfig.sandboxID),
                failingAttributes,
                SandboxIDFieldValidatorAttribute.FieldDidNotMatchMessage),
                "Sandbox Id should not have errors relating to it failing to parse.");
        }

        [Test]
        public void SandboxID_SuccessfulParsing_WithDevelopmentEnvironment()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.sandboxID = "p-1234567890ABCDEFGHIJKLMNOPQRST";

            if (!FieldValidator.TryGetFailingValidatorAttributes(config, out List<FieldValidatorFailure> failingAttributes))
            {
                // If there are no errors, then this test is a success
                // The config might be failing in other ways
                return;
            }

            Assert.IsFalse(failuresIncludeExpectedFailure<SandboxIDFieldValidatorAttribute>(
                nameof(EOSConfig.sandboxID),
                failingAttributes,
                SandboxIDFieldValidatorAttribute.FieldDidNotMatchMessage),
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

            if (!FieldValidator.TryGetFailingValidatorAttributes(config, out List<FieldValidatorFailure> failingAttributes))
            {
                Assert.Fail($"Config should have failing attributes.");
            }

            Assert.IsTrue(failuresIncludeExpectedFailure<GUIDFieldValidatorAttribute>(
                nameof(SandboxDeploymentOverride.deploymentID),
                failingAttributes,
                GUIDFieldValidatorAttribute.NotAGuidMessage),
                "Deployment ID in sandbox override should have failure relating to it.");

            Assert.IsTrue(failuresIncludeExpectedFailure<SandboxIDFieldValidatorAttribute>(
                nameof(SandboxDeploymentOverride.sandboxID),
                failingAttributes,
                SandboxIDFieldValidatorAttribute.FieldDidNotMatchMessage),
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

            if (!FieldValidator.TryGetFailingValidatorAttributes(config, out List<FieldValidatorFailure> failingAttributes))
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

            Assert.IsFalse(failuresIncludeExpectedFailure<SandboxIDFieldValidatorAttribute>(
                nameof(SandboxDeploymentOverride.sandboxID),
                failingAttributes,
                SandboxIDFieldValidatorAttribute.FieldDidNotMatchMessage),
                "Sandbox ID in sandbox override should not have error relating to it failing to parse.");
        }

        [Test]
        public void DeploymentID_MustBeValidGUID()
        {
            EOSConfig config = EOSConfig.Get<EOSConfig>();
            config.deploymentID = "notaguid";

            if (!FieldValidator.TryGetFailingValidatorAttributes(config, out List<FieldValidatorFailure> failingAttributes))
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

            if (!FieldValidator.TryGetFailingValidatorAttributes(config, out List<FieldValidatorFailure> failingAttributes))
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