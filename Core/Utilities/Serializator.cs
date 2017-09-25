using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace InSearch.Utilities
{
    public static class Serializator
    {
        public static string SerializeObject(object objectInstance)
        {
            var serializer = new XmlSerializer(objectInstance.GetType());
            var sb = new StringBuilder();

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, objectInstance);
            }

            return sb.ToString();
        }

        public static T DeserializeObject<T>(string data) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));
            object result;

            using (var reader = new StringReader(data))
            {
                result = serializer.Deserialize(reader);
            }
            return (T)result;
        }
    }
}
