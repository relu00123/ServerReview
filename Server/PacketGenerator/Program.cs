using System;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using (XmlReader r = XmlReader.Create("PDL.xml", settings))
            {
                // XML파일의 헤더를 건너 뛴다.
                r.MoveToContent();

                while (r.Read())
                {
                    // Depth는 몇개로 파고드는지 && 닫는것이 아닌지
                    // 시작하는 것은 Elemnt이고 끝나는 것은 EndElement이다. 
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);
                    //Console.WriteLine(r.Name + " " + r["name"]);
                }
            }

        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement) // 혹시 모르니까. 들어올일 없을것임
                return;

            if (r.Name.ToLower() != "packet") // 혹시 모르니까
            {
                Console.WriteLine("Invalid packet node");
                return;
            }

            string packetName = r["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
            }

            ParseMembers(r);
        }

        public static void ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            int depth = r.Depth + 1;
            while (r.Read())
            {
                if (r.Depth != depth)
                    break;

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.Write("Member without name");
                    return;
                }

                string memberType = r.Name.ToLower();

                switch (memberType)
                {
                    case "bool":
                    case "byte":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                    case "string":
                    case "list":
                        break;
                    default:
                        break;
                }
            }
        }
    }
}