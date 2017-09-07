using Prism.Events; 
using System;
using System.Collections.Generic;
using System.Windows;

namespace Pvirtech.Framework.Domain
{
	//public class RunCommandArgs : PubSubEvent<CommandMessage>
	//{

	//}
 
    public class CommonEventArgs<T> : PubSubEvent<T>
    {

    }
    public class CustomMapEventArgs : PubSubEvent<object>
    {

    }
    public class TimeSpanEventArgs : PubSubEvent<long>
    {

    }

    public class MapEventArgs<T> : PubSubEvent<Dictionary<string, T>>
    {

    }
    
}
