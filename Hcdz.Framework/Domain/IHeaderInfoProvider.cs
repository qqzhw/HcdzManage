using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pvirtech.Framework.Domain
{
    public interface IHeaderInfoProvider<T>
    {
        T HeaderInfo { get; }
    }
}
