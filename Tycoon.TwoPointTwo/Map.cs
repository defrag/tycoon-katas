using System.Diagnostics.Contracts;
using System.Text;

namespace Tycoon.TwoPointTwo;

record Location(string Value)
{
    public override string ToString() => Value;
}

record Route(IReadOnlyCollection<Route.Leg> Legs)
{
    internal record Leg(TimeSpan Elapsed, Location Location);
    
    public override string ToString()
    {
        var first = true;
        var b = new StringBuilder();
        foreach (var leg in Legs)
        {
            var hours = (int)Math.Floor(leg.Elapsed.TotalHours);
            b.Append(hours.ToString("D2"));
            b.Append(':');
            b.Append(leg.Elapsed.Minutes.ToString("D2"));
            b.Append(" ");
            b.Append(first ? "DEPART " : "ARRIVE ");
            b.Append(leg.Location);
            b.Append("\n");
            first = false;
        }

        return b.ToString();
    }
}

sealed class Map
{
    private record Milestone(Location Location, TimeSpan Elapsed, Milestone? Previous = null)
    {
        public Route AsRoute()
        {
            var curr = this;
            var l = new List<Route.Leg>();
            while (curr is not null)
            {
                l.Add(new Route.Leg(curr.Elapsed, curr.Location));
                curr = curr.Previous;
            }

            l.Reverse();
            return new Route(l);
        }
    }

    private record Road(Location Location, int Distance, int Speed)
    {
        public TimeSpan Duration => TimeSpan.FromHours((double)Distance / Speed);
    }
    
    private record AdjacencyList(List<Road> Neighbours)
    {
        public static AdjacencyList Empty() => new AdjacencyList(new List<Road>());

        public IReadOnlyCollection<Road> NeighboursFromFastest
            => Neighbours.OrderBy(s => s.Duration).ToList();
    }

    private Dictionary<Location, AdjacencyList> _map;
    
    public Map()
    {
        _map = new();
    }
    
    public void Add(Location from, Location to, int distance, int speed)
    {
        _map[from] = _map.GetValueOrDefault(from, AdjacencyList.Empty());
        _map[from].Neighbours.Add(new Road(to, distance, speed));

        _map[to] = _map.GetValueOrDefault(to, AdjacencyList.Empty());
        _map[to].Neighbours.Add(new Road(from, distance, speed));
    }

    public Route? FindShortestRouteBetween(Location from, Location to)
    {
        var queue = new PriorityQueue<Milestone, TimeSpan>();
        queue.Enqueue(new Milestone(from,  TimeSpan.Zero), TimeSpan.Zero);
        var visited = new List<Location>();
        
        while (queue.TryDequeue(out var element, out var elapsed))
        {
            if (visited.Contains(element.Location))
            {
                continue;
            }

            if (element.Location.Equals(to))
            {
                return element.AsRoute();
            }
            
            visited.Add(element.Location);

            foreach (var neighbour in _map[element.Location].NeighboursFromFastest)
            {
                if (!visited.Contains(neighbour.Location))
                {
                    var newElapsed = elapsed.Add(neighbour.Duration);
                    queue.Enqueue(
                        new Milestone(neighbour.Location, newElapsed, element), 
                        newElapsed
                    );
                }
            }
        }

        return null;
    }
}