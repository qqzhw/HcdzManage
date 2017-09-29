using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hcdz.ModulePcie.Models
{
	public class ClientMessage
	{

		public ClientMessage()
		{
			ContextIds = new List<string>();
		}
		public int Id { get; set; }
		public string MessageContent { get; set; }
		public int UserId { get; set; }

		public string FormNodeId { get; set; }

		public List<string> ContextIds { get; set; }
		public int EventId { get; set; }
		public string EventNo { get; set; }
		public int MessageType { get; set; }
		public int MsgCode { get; set; }
		public bool Status { get; set; }
	}
}
