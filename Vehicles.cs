using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Wardler
{
    internal class Vehicles
    {
        public static string data = "";
        public static List<string> urls = new List<string>();
        public static List<Vehicle> vehicles = new List<Vehicle>();
        public static void CheckCrawl()
        {
            if (File.Exists("vehicles.war"))
            {
                using (StreamReader sr = new StreamReader("vehicles.war"))
                {
                    foreach (string line in File.ReadLines("vehicles.war"))
                    {
                        string[] data = line.Split(" ");
                        Vehicle vehicle = new Vehicle(data[0], data[1], int.Parse(data[2]), double.Parse(data[3]), int.Parse(data[4]), bool.Parse(data[5]), double.Parse(data[6]), data[7], double.Parse(data[8]));
                    }
                }
            }
            else
            {
                Crawl();
                using (StreamWriter file = new StreamWriter("vehicles.war"))
                {
                    foreach (Vehicle vehicle in vehicles)
                    {
                        file.Write(vehicle.Name + " ");
                        file.Write(vehicle.Country + " ");
                        file.Write(vehicle.Speed + " ");
                        file.Write(vehicle.Mass + " ");
                        file.Write(vehicle.Crew + " ");
                        file.Write(vehicle.Regular + " ");
                        file.Write(vehicle.Rating + " ");
                        file.Write(vehicle.Type + " ");
                        file.Write(vehicle.Caliber + "\n");
                    }
                    file.Flush();
                    file.Close();
                }
            }
        }
        public static void Crawl()
        {
            Console.WriteLine("Initiating vehicle crawler...");
            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://wiki.warthunder.com/ground?v=l&l_sk=name&l_sd=asc");
                var response = client.Send(request);
                using var reader = new StreamReader(response.Content.ReadAsStream());
                data = reader.ReadToEnd();
            }
            //Console.WriteLine(data);
            foreach (Match match in Regex.Matches(data, @"href=""\/unit\/.*?"""))
            {
                urls.Add(@"https://wiki.warthunder.com" + match.ToString().Replace("href=\"", "").Replace("\"", ""));
                //Console.WriteLine(urls[^1]);
            }
            //Console.ReadLine();
            int cnt = 0;
            Console.Write($"\rCrawling {cnt}/{urls.Count}... ");
            Parallel.For(0, urls.Count, i =>
            {
                short ok = 0;
                /// Parallel.For is quick. However, it is also
                /// extremely unreliable. Since I'm not a normal person,
                /// instead of using a normal for, I'll just perform the
                /// operation twice, as occasionally the regex returns
                /// with no elements when there should be (but sometimes
                /// it also returns no elements b/c the Wiki is missing
                /// the info)
                while (ok < 2)
                {
                    Vehicle vehicle = new Vehicle("", "", 0, 0, 0, false, 0, "", 0);
                    try
                    {
                        Vehicles_Crawl(i, vehicle);
                        ok = 2;
                    }
                    catch (Exception ex)
                    {
                        ok++;
                        if (ok == 2)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                }
                cnt++;
                Console.Write($"\rCrawling {cnt}/{urls.Count}... ");
            });
        }
        private static void Vehicles_Crawl(int i, Vehicle vehicle)
        {
            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, urls[i]);
                var response = client.Send(request);
                using var reader = new StreamReader(response.Content.ReadAsStream());
                data = reader.ReadToEnd();
            }
            //Enjoy the Great Mess (my API)
            string[] values = Regex.Matches(data, @"<div class=""text-truncate"">.*?</div>")
                .OfType<Match>().Select(x => x.ToString()
                .Replace("<div class=\"text-truncate\">", "")
                .Replace("&nbsp;", " ")
                .Replace(" ", " ")
                .Replace("</div>", "").ToUpper()).ToArray();
            vehicle.Country = values[0];
            vehicle.Type = values[1];
            values = Regex.Matches(data, @"<div class=""game-unit_name"">.*?</div>")
                .OfType<Match>().Select(x => x.ToString()
                .Replace("<div class=\"game-unit_name\">", "")
                .Replace("&nbsp;", " ")
                .Replace(" ", " ")
                .Replace("</div>", "").ToUpper()).ToArray();
            vehicle.Name = values[0];
            values = Regex.Matches(data, @"<span class=""show-char-rb"">.*?</span>")
                .OfType<Match>().Select(x => x.ToString()
                .Replace("<span class=\"show-char-rb\">", "")
                .Replace("&nbsp;", " ")
                .Replace("</span>", "").ToUpper()).ToArray();
            vehicle.Speed = int.Parse(values[0]);
            values = Regex.Matches(data, @"<span class=""game-unit_chars-value"">\s*.* t\s*?</span>")
                .OfType<Match>().Select(x => x.ToString()
                .Replace("<span class=\"game-unit_chars-value\">", "")
                .Replace(" ", "")
                .Replace("t</span>", "").ToUpper()).ToArray();
            vehicle.Mass = double.Parse(values[0]);
            values = Regex.Matches(data, @"<span class=""game-unit_chars-value"">\s*\d* persons.*?</span>")
                .OfType<Match>().Select(x => x.ToString()
                .Replace("<span class=\"game-unit_chars-value\">", "")
                .Replace(" ", "")
                .Replace("persons</span>", "").ToUpper()).ToArray();
            vehicle.Crew = int.Parse(values[0]);
            vehicle.Regular = !data.Contains("<div class=\"game-unit_card-info_title\">Status</div>");
            values = Regex.Matches(data, @"<div\s*class=""mode"">RB</div>\s*<div\s*class=""value"">\d*.\d")
                .OfType<Match>().Select(x => x.ToString()
                .Replace(" ", "")
                .Replace("<divclass=\"mode\">RB</div>\n<divclass=\"value\">", "")
                .ToUpper()).ToArray();
            vehicle.Rating = double.Parse(values[0]);
            values = Regex.Matches(data, @"<span class=""game-unit_weapon-title"">(?!<).*?m")
                .OfType<Match>().Select(x => x.ToString()
                .Replace(" ", "")
                .Replace("<spanclass=\"game-unit_weapon-title\">", "")
                .Replace("m", "").ToUpper()
                .Replace("28/20", "20")
                .Replace("4,7C", "47")
                .Replace("<AHREF=\"/COLLECTIONS/WEAPON/14_5", "14.5")
                .Replace("<AHREF=\"/COLLECTIONS/WEAPON/12_7", "12.7")).ToArray();
            // POV when the Wiki is missing some info so u
            // have to hardcode it urself:
            switch (vehicle.Name)
            {
                case "ADATS (M113)":
                case "AFT09":
                    vehicle.Caliber = 152.0;
                    break;
                case "GAZ-AAA (DSHK)":
                    vehicle.Caliber = 12.7;
                    break;
                case "FLARAKRAD":
                case string a when a.EndsWith("ITO 90M"):
                    vehicle.Caliber = 165.0;
                    break;
                case "FLARAKPZ 1":
                case "XM975":
                case "LOSAT":
                    vehicle.Caliber = 163.0;
                    break;
                case "ANTELOPE":
                case "CM25":
                case string b when b.EndsWith("IMP.CHAPARRAL"):
                case string c when c.EndsWith("M113A1 (TOW)"):
                case "M901":
                case "PVRBV 551":
                case "UDES 33":
                    vehicle.Caliber = 127.0;
                    break;
                case "HQ17":
                case "TOR-M1":
                    vehicle.Caliber = 239.0;
                    break;
                case "KHRIZANTEMA-S":
                    vehicle.Caliber = 155.0;
                    break;
                case "ASRAD-R":
                case "LVRBV 701":
                    vehicle.Caliber = 105.0;
                    break;
                case "HOVET":
                case "M163":
                case "MACHBET":
                    vehicle.Caliber = 20.0;
                    break;
                case "MEPHISTO":
                    vehicle.Caliber = 136.0;
                    break;
                case string d when d.Contains("OSA-AK"):
                    vehicle.Caliber = 209.6;
                    break;
                case "OZELOT":
                    vehicle.Caliber = 70.0;
                    break;
                case "SANTAL":
                    vehicle.Caliber = 90.0;
                    break;
                case "SHTURM-S":
                case "STORMER HVM":
                    vehicle.Caliber = 130.0;
                    break;
                case string e when e.StartsWith("STRELA-10M"):
                    vehicle.Caliber = 120.0;
                    break;
                case "TYPE 81 (C)":
                case "TYPE 93":
                    vehicle.Caliber = 80.0;
                    break;
                case "ZACHLAM TAGER":
                    vehicle.Caliber = 164.0;
                    break;
                default:
                    vehicle.Caliber = double.Parse(Regex.Replace(values[0], @"\d*X", ""));
                    break;
            }
            vehicles.Add(vehicle);
        }
    }
    public class Vehicle
    {
        public string Name { get; set; } //
        public string Country { get; set; } //
        public int Speed { get; set; } //
        public double Mass { get; set; } //
        public int Crew { get; set; } //
        public bool Regular { get; set; } //
        public double Rating { get; set; } //
        public string Type { get; set; } //
        public double Caliber { get; set; } //
        public Vehicle(string name, string country, int speed, double mass, int crew, bool regular, double rating, string type, double caliber)
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
