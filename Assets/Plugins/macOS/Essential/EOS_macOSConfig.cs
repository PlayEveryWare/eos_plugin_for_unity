
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

using System;
using UnityEngine;
using System.Collections.Generic;

namespace PlayEveryWare.EpicOnlineServices
{
    // Flags specificly for macOS
    public class EOS_macOSConfig : ICloneableGeneric<EOS_macOSConfig>, IEmpty
    {
        public List<string> flags;

        public EOSConfig overrideValues;

        //-------------------------------------------------------------------------
        public EOS_macOSConfig Clone()
        {
            return (EOS_macOSConfig)this.MemberwiseClone();
        }

        //-------------------------------------------------------------------------
        public bool IsEmpty()
        {
            return EmptyPredicates.IsEmptyOrNullOrContainsOnlyEmpty(flags) &&
                EmptyPredicates.IsEmptyOrNull(overrideValues);
        }


        //-------------------------------------------------------------------------
#if !EOS_DISABLE
        public Epic.OnlineServices.IntegratedPlatform.IntegratedPlatformManagementFlags flagsAsIntegratedPlatformManagementFlags()
        {
            return EOSConfig.flagsAsIntegratedPlatformManagementFlags(flags);
        }
#endif
    }
}
