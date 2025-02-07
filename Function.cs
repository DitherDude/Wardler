using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Wardler;

public class Function
{
    // Huge thank you to @Term164 for generously providing me with the following function!
    // I have left their (unmodified) code in this repo - ./speed.ts
    public static int Speed(string data)
    {
        double MaxRPM = double.Parse(Find(data, @"maxRPM"":\s.*?,")[0].Remove(0, 9).Replace(",", ""));
        double Radius = double.Parse(Find(data, @"driveGearRadius"":\s.*?,")[0].Remove(0, 18).Replace(",", ""));
        double MainRatio = double.Parse(Find(data, @"mainGearRatio"":\s.*?,")[0].Remove(0, 16).Replace(",", ""));
        double SideRatio = double.Parse(Find(data, @"sideGearRatio"":\s.*?,")[0].Remove(0, 16).Replace(",", ""));
        double GRCount = double.Parse(Find(Find(data, @"""gearRatios"":(.|\s)*?]")[0], @"\d.*")[^1]);
        return (int)Math.Round(((MaxRPM * Radius) / (MainRatio * SideRatio * GRCount)) * 0.12 * Math.PI,0);
    }
    public static string[] Find(string data, string regex)
    {
        string[] matches = Regex.Matches(data, regex)
        .OfType<Match>().Select(x => x.ToString().ToUpper()).ToArray();
        return matches;
    }
}