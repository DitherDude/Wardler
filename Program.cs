using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

namespace Wardler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Vehicles.CheckCrawl();
            Vehicles.Wardle();
        }
    }
}
