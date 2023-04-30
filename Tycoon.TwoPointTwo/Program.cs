// See https://aka.ms/new-console-template for more information

using Tycoon.TwoPointTwo;

var fileContents = File.ReadAllLines(Path.Join("Resources", "map.csv"));

var map = new Map();
foreach (var line in fileContents[1..fileContents.Length])
{
    var arr = line.Split(',').Select(t => t.Trim()).ToArray();
    map.Add(new Location(arr[0]), new Location(arr[1]), Int32.Parse(arr[2]), Int32.Parse(arr[3]));
}

var from = args.Length == 2 ? args[0] : "Steamdrift";
var to = args.Length == 2 ? args[1] : "Leverstorm";

Console.WriteLine($"Computing shortest distance from {from} to {to}.");
var result = map.FindShortestRouteBetween(new Location(from), new Location(to));
if (result is null)
{
    Console.WriteLine($"Unable to find connections between {from} and {to}.");
}
else
{
    Console.WriteLine($"Found shortest route:");
    Console.WriteLine(result);    
}
