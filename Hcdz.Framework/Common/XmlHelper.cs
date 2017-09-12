using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Pvirtech.Framework.Common
{
    public class XmlHelper
    {
        private static readonly Dictionary<Type, XmlSerializer> _xmlSerializerCache = new Dictionary<Type, XmlSerializer>();

        public static XmlDocument GetXmlDoc(string xmlFileName)
        {
            XmlDocument xml = new XmlDocument();
            var stream = GetXmlStream(xmlFileName);
            xml.Load(stream);
            stream.Close();
            return xml;
        }

        public static Stream GetXmlStream(string xmlFileName)
        {
			using (FileStream stream = new FileStream(xmlFileName,FileMode.OpenOrCreate,FileAccess.Read))
			{
				return stream;
			}
        }

        #region 方法

        public static string Serializer<T>(object obj)
        {
            MemoryStream Stream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(typeof(T));
            try
            {
                //序列化对象
                xml.Serialize(Stream, obj);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            Stream.Position = 0;
            StreamReader sr = new StreamReader(Stream);
            string str = sr.ReadToEnd();

            sr.Dispose();
            Stream.Dispose();

            return str;
        }
        public static T Deserialize<T>(string xml)
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes =CreateDefaultXmlSerializer(typeof(T));
                    return (T)xmldes.Deserialize(sr);
                }
            }
            catch
            {

                return default(T);
            }
        }

        public static T LoadXmlFile<T>(string file)
        {
            try
            {
                XmlSerializer xs = CreateDefaultXmlSerializer(typeof(T));
                using (StreamReader sr = new StreamReader(file))
                {
                    T config = (T)xs.Deserialize(sr);
                    sr.Close();
                    return config;
                }
            }
            catch
            {
                return default(T);
            }

        }

        public void SaveXmlFile<T>(string file)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            StreamWriter sw = new StreamWriter(file);
            xs.Serialize(sw, this);
            sw.Close();
        }
        #endregion


        public static T GetXmlEntities<T>(string xmlFileName)
        {
            //List = new List<T>();
            Stream stream = GetXmlStream(xmlFileName);
            XmlSerializer serializer = CreateDefaultXmlSerializer(typeof(T));
            T list = (T)serializer.Deserialize(stream);
            return list;
        }


        public static XmlSerializer CreateDefaultXmlSerializer(Type type)
        {
            XmlSerializer serializer;

            if (_xmlSerializerCache.TryGetValue(type, out serializer))
			{
				return serializer;
			}
			else
			{
				var importer = new XmlReflectionImporter();
				var mapping = importer.ImportTypeMapping(type, null, null);
				serializer = new XmlSerializer(mapping);
				return _xmlSerializerCache[type] = serializer;
			}
		}


    }
}
