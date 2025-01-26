using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Wardler
{
    internal class Vehicles
    {
        public static void Crawl()
        {
            Console.WriteLine("Initiating vehicle crawler...");
            List<string> urls = new List<string>();
            //HtmlWeb htmlWeb = new HtmlWeb();
            //HtmlDocument doc = htmlWeb.Load();
            //var body = doc.DocumentNode.SelectNodes("//a");
            //foreach (HtmlNode node in body)
            //{
            //    if (node.GetAttributeValue("href", "").StartsWith("/unit/"))
            //    {
            //        urls.Add(@"https://wiki.warthunder.com" + node.GetAttributeValue("href", ""));
            //        Console.WriteLine(node.GetAttributeValue("href", ""));
            //    }
            //}
            string data = "";
            using (WebClient client = new WebClient())
            {
                data = client.DownloadString("https://wiki.warthunder.com/ground?v=l&l_sk=name&l_sd=asc");
            }
            //Console.WriteLine(data);
            foreach (Match match in Regex.Matches(data, @"href=""\/unit\/.*?"""))
            {
                urls.Add(@"https://wiki.warthunder.com" + match.ToString().Replace("href=\"", "").Replace("\"", ""));
            }
            for (int i = 0; i < 1; i++)
            {
                Vehicle vehicle = new Vehicle("", "", 0, 0, 0, false, 0, "", 0);
                using (WebClient client = new WebClient())
                {
                    data = client.DownloadString(urls[i]);
                }
                Console.WriteLine(urls[i]);
                string[] values = Regex.Matches(data, @"<div class=""text-truncate"">.*?</div>")
                    .OfType<Match>().Select(x => x.ToString()
                    .Replace("<div class=\"text-truncate\">", "")
                    .Replace("&nbsp;", " ")
                    .Replace("</div>", "").ToUpper()).ToArray();
                vehicle.Country = values[0];
                vehicle.Type = values[1];
                values = Regex.Matches(data, @"<div class=""game-unit_name"">.*?</div>")
                    .OfType<Match>().Select(x => x.ToString()
                    .Replace("<div class=\"game-unit_name\">", "")
                    .Replace("&nbsp;", " ")
                    .Replace("</div>", "").ToUpper()).ToArray();
                vehicle.Name = values[0];
                values = Regex.Matches(data, @"<span class=""show-char-rb"">.*?</span>")
                    .OfType<Match>().Select(x => x.ToString()
                    .Replace("<span class=\"show-char-rb\">", "")
                    .Replace("&nbsp;", " ")
                    .Replace("</span>", "").ToUpper()).ToArray();
                vehicle.Speed = int.Parse(values[0]);
                values = Regex.Matches(data, @"<span class=""game-unit_chars-value"">\s*\d*\.\d t.*?</span>")
                    .OfType<Match>().Select(x => x.ToString()
                    .Replace("<span class=\"game-unit_chars-value\">", "")
                    .Replace(" ", "")
                    .Replace("t</span>", "").ToUpper()).ToArray();
                vehicle.Mass = double.Parse(values[0]);
            }
            List<Vehicle> vehicles = new List<Vehicle>();
        }
    }
    public class Vehicle
    {
        public string Name { get; set; } //
        public string Country { get; set; } //
        public int Speed { get; set; } //
        public double Mass { get; set; } //
        public int Crew { get; set; }
        public bool Regular { get; set; }
        public double Rating { get; set; }
        public string Type { get; set; } //
        public int Caliber { get; set; }
        public Vehicle(string name, string country, int speed, double mass, int crew, bool regular, double rating, string type, int caliber)
        {
            Name = name;
            Country = country;
            Speed = speed;
            Mass = mass;
            Crew = crew;
            Regular = regular;
            Rating = rating;
            Type = type;
            Caliber = caliber;
        }
    }
}
