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

    public class ProductionEnvironments
    {
        private SortedSetOfNamed<Deployment> _deployments = new("Deployment");
        private SortedSetOfNamed<SandboxId> _sandboxes = new("Sandbox");

        public SortedSetOfNamed<Deployment> Deployments
        {
            get
            {
                return _deployments;
            }
        }

        public SortedSetOfNamed<SandboxId> Sandboxes
        {
            get
            {
                return _sandboxes;
            }
        }

        /// <summary>
        /// Adds a Sandbox to the Production Environment.
        /// </summary>
        /// <param name="sandbox">
        /// The sandbox to add.</param>
        /// <returns>
        /// True if the sandbox was added successfully, false otherwise.
        /// </returns>
        public bool AddSandbox(Named<SandboxId> sandbox = null)
        {
            return _sandboxes.Add(sandbox);
        }

        public bool AddNewSandbox()
        {
            return _sandboxes.Add();
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
            foreach (Named<Deployment> deployment in _deployments)
            {
                if (deployment.Value.SandboxId.Equals(sandbox.Value))
                {
                    // TODO: Tell user that the sandbox cannot be removed,
                    //       because it is currently referenced by a deployment.
                    return false;
                }
            }

            return _sandboxes.Remove(sandbox);
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
        public bool AddDeployment(Named<Deployment> deployment)
        {
            if (_deployments.ContainsName(deployment.Name))
            {
                // TODO: Tell user deployment with same name already exists, so
                //       this one cannot be added.
            }

            if (_deployments.Contains(deployment.Value))
            {
                // TODO: Tell user deployment with same Id already exists, so 
                //       this one cannot be added.
            }

            // If the sandbox does not already exist, then add it to the list of
            // sandboxes.
            if (!_sandboxes.Contains(deployment.Value.SandboxId))
            {
                // Add the sandbox.
                _sandboxes.Add(deployment.Value.SandboxId);
            }

            // Add the deployment to the list of deployments
            return _deployments.Add(deployment);
        }

        public bool AddNewDeployment()
        {
            return _deployments.Add();
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
            return _deployments.Remove(deployment);
        }
    }
}