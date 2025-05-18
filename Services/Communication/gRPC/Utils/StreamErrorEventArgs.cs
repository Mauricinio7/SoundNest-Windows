using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.gRPC.Utils
{
    public class StreamErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public string Context { get; }

        public StreamErrorEventArgs(Exception ex, string context)
        {
            Exception = ex;
            Context = context;
        }
    }
}
