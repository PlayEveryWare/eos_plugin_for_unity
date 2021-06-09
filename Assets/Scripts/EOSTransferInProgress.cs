using System.Collections.Generic;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    public class EOSTransferInProgress
    {
        public bool Download = true;
        public uint TotalSize = 0;
        public uint CurrentIndex = 0;
        public List<char> Data = new List<char>();

        public bool Done()
        {
            return TotalSize == CurrentIndex;
        }
    }
}