 
using Pvirtech.TcpSocket.Scs.Communication.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie.Models
{
	 
	public class CustomWireProtocolFactory : IScsWireProtocolFactory
	{
		public IScsWireProtocol CreateWireProtocol()
		{
			return new CustomWireProtocol();
		}
	}
}
