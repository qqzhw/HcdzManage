using System;
using System.Runtime.Serialization;

namespace Pvirtech.Metro
{
    [Serializable]
    public class PvirtechAppsException : Exception
    {
        public PvirtechAppsException()
        {
        }

        public PvirtechAppsException(string message)
            : base(message)
        {
        }

        public PvirtechAppsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PvirtechAppsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}