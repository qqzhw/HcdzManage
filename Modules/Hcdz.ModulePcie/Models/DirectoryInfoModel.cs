using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie.Models
{
    public class DirectoryInfoModel
    {
        public string Name { get; set; }        
        public bool IsDir { get; set; }
        public string Icon { get; set; }
        public DateTime CreationTime { get; set; }
        public DirectoryInfo Directory { get; set; }
        public DirectoryInfo Parent { get; set; }
        public DirectoryInfo Root { get; set; }
        public string DirectoryName { get; set; }
        public bool Exists { get; set; }
        public string Extension { get; set; }
        public string FullName { get; set; }
       // public string FullPath { get; set; } 
      //  public string OriginalPath { get; set; }
        public bool IsReadOnly { get; set; }
        public DateTime LastAccessTime { get; set; }
        public string LastWriteTime { get; set; }
        public long Length { get; set; } 
        public string LengthText { get; set; }
    }
}
