using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NetModelsLibrary
{
    public static class BusSerializer
    {
        public static string Serialize<T>(T obj)
        {
            XmlSerializer serializer = new(typeof(T));
            MemoryStream temp = new();
            serializer.Serialize(temp, obj);
            temp.Position = 0;
            return new StreamReader(temp).ReadToEnd();
        }
        public static T Deserialize<T>(string s)
        {
            XmlSerializer serializer = new(typeof(T));
            using (TextReader tr = new StringReader(s))
            {
                T? res = (T?)serializer.Deserialize(tr);
                if (res == null) throw new Exception("Deserialization returned null");
                return res;
            }
        }
    }
}
