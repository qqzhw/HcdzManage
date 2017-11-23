using Newtonsoft.Json;
using Pvirtech.TcpSocket.Scs.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hcdz.WPFServer.Models
{
    public class TcpClientModel
    {
        public TcpClientModel()
        {
            ConnectedTime = DateTime.Now;
        }
        public int Id { get; set; }
        [JsonIgnore]
        public IScsClient Client { get; set; }
        public bool IsConnected { get; set; }
        [JsonIgnore]
        public FileStream TcpStream { get; set; } 
        public string MessageText { get; set; } 
        public int Status { get; set; }
        public string IP { get; set; } 
        public int Port { get; set; } 
        public long DataSize { get; set; }
        public string FileDir { get; set; }
        public DateTime ConnectedTime { get; set; }
    }
}
