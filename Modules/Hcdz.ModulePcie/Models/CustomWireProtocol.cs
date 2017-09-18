
using Pvirtech.TcpSocket.Scs.Communication.Messages;
using Pvirtech.TcpSocket.Scs.Communication.Protocols.BinarySerialization;
using System.Text;

namespace Hcdz.ModulePcie.Models
{
 
		public class CustomWireProtocol : BinarySerializationProtocol
		{
			protected override byte[] SerializeMessage(IScsMessage message)
			{
				return Encoding.UTF8.GetBytes(((ScsTextMessage)message).Text);
			}

			protected override IScsMessage DeserializeMessage(byte[] bytes)
			{
				//Decode UTF8 encoded text and create a ScsTextMessage object
				return new ScsTextMessage(Encoding.UTF8.GetString(bytes));
			}
		 
	}
}