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

namespace PlayEveryWare.EpicOnlineServices
{
    using Common;
    using System;

    /// <summary>
    /// Contains information about the product entered by the user from the Epic
    /// Developer Portal.
    /// </summary>
    public class ProductConfig : Config
    {
        /// <summary>
        /// The product ID is a unique GUID labeled "Product ID" in the Epic
        /// Developer Portal. The name for this value can be set to anything -
        /// it is used as a label for user interface purposes - and is allowed
        /// to differ from the label given to it on the Developer Portal.
        /// </summary>
        public Named<Guid> ProductId;

        /// <summary>
        /// The set of Clients as defined within the Epic Developer Portal. For
        /// EOS to function, at least one of these must be set, and the
        /// platform config needs to indicate which one to use. (If none is
        /// explicitly indicated, and only one is defined, that one will be
        /// used).
        /// </summary>
        public SortedSetOfNamed<WrappedClientCredentials> Clients;

        /// <summary>
        /// The set of Sandboxes as defined within the Epic Developer Portal.
        /// For EOS to function, at least one of these must be set, and it must
        /// match the deployment indicated by the platform config.
        /// </summary>
        public SortedSetOfNamed<SandboxId> Sandboxes;

        /// <summary>
        /// Backing field member for the Deployments property.
        /// </summary>
        private SortedSetOfNamed<Deployment> _deployments;

        /// <summary>
        /// The set of Deployments as defined within the Epic Developer Portal.
        /// Every Deployment must point to a Sandbox that 
        /// </summary>
        public SortedSetOfNamed<Deployment> Deployments
        {
            get
            {
                return _deployments;
            }
        }

        public void AddDeployment(string name, Deployment deployment)
        {
            if (!Sandboxes.Contains(deployment.SandboxId))
            {
                Sandboxes.Add(deployment.SandboxId);
            }

            _deployments.Add(name, deployment);
        }

        static ProductConfig()
        {
            RegisterFactory(() => new ProductConfig());
        }

        protected ProductConfig() : base("eos_product_config.json")
        {
            Clients = new SortedSetOfNamed<WrappedClientCredentials>("Client");
            Sandboxes = new SortedSetOfNamed<SandboxId>("Sandbox");
            _deployments = new SortedSetOfNamed<Deployment>("Deployment");
        }
    }
}