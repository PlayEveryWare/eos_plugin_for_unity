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

namespace PlayEveryWare.EpicOnlineServices
{
    using Common;

    /// <summary>
    /// This class contains information about the set of deployments and
    /// sandboxes that a single EOS plugin project can be configured to
    /// deploy to.
    /// </summary>
    public class ProductionEnvironments
    {
        /// <summary>
        /// Deployments are different environments within defined Sandboxes.
        /// One sandbox can have more than one Deployment.
        /// </summary>
        public SetOfNamed<Deployment> Deployments { get; } = new("Deployment");

        /// <summary>
        /// Sandboxes are different siloed categories of production environment.
        /// One sandbox can have more than one deployment.
        /// </summary>
        public SetOfNamed<SandboxId> Sandboxes { get; } = new("Sandbox");

        /// <summary>
        /// Adds a Sandbox to the Production Environment.
        /// </summary>
        /// <param name="sandbox">
        /// The sandbox to add.</param>
        /// <returns>
        /// True if the sandbox was added successfully, false otherwise.
        /// </returns>
        public bool AddSandbox(Named<SandboxId> sandbox)
        {
            return Sandboxes.Add(sandbox);
        }

        public bool AddNewSandbox()
        {
            return Sandboxes.Add();
        }

        /// <summary>
        /// Removes a Sandbox from the Production Environment.
        /// </summary>
        /// <param name="sandbox">
        /// The Sandbox to remove from the production environment.
        /// </param>
        /// <returns>
        /// True if the Sandbox was removed, false otherwise. If there is a
        /// defined deployment that references the Sandbox, then removing it is
        /// disallowed.
        /// </returns>
        public bool RemoveSandbox(Named<SandboxId> sandbox)
        {
            foreach (Named<Deployment> deployment in Deployments)
            {
                if (deployment.Value.SandboxId.Equals(sandbox.Value))
                {
                    // TODO: Tell user that the sandbox cannot be removed,
                    //       because it is currently referenced by a deployment.
                    return false;
                }
            }

            return Sandboxes.Remove(sandbox);
        }

        /// <summary>
        /// Adds a Deployment to the Production Environment, adding the sandbox
        /// if it does not already exist in the set of sandboxes.
        /// </summary>
        /// <param name="deployment">
        /// The Deployment to add.
        /// </param>
        /// <returns>
        /// True if the deployment was added, false otherwise.
        /// </returns>
        public bool AddDeployment(Deployment deployment)
        {
            // Add the sandbox (will do nothing if the sandbox already exists).
            Sandboxes.Add(deployment.SandboxId);

            // Add the deployment to the list of deployments
            return Deployments.Add(deployment);
        }

        public bool AddNewDeployment()
        {
            return Deployments.Add();
        }

        /// <summary>
        /// Removes a Deployment from the Production Environment.
        /// </summary>
        /// <param name="deployment">
        /// The Deployment to remove from the Production Environment.
        /// </param>
        /// <returns>
        /// True if the Deployment was successfully removed, false otherwise.
        /// </returns>
        public bool RemoveDeployment(Named<Deployment> deployment)
        {
            return Deployments.Remove(deployment);
        }
    }
}