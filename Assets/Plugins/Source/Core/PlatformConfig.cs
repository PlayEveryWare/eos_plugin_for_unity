/*PlayEveryWare.EpicOnlineServices
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
    using System.Collections.Generic;

    /// <summary>
    /// Represents a set of configuration data for use by the EOS Plugin for Unity
    /// </summary>
    [Serializable]
    public abstract class PlatformConfig : Config
    {
        protected PlatformManager.Platform Platform;
        public List<string> flags;
        public EOSConfig overrideValues;

        protected PlatformConfig(PlatformManager.Platform platform) : 
            base(PlatformManager.GetConfigFileName(platform))
        {
            this.Platform = platform;
        }

#if !EOS_DISABLE
        public Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformManagementFlags flagsAsIntegratedPlatformManagementFlags()
        {
            return EOSConfig.flagsAsIntegratedPlatformManagementFlags(flags);
        }
#endif
    }
}