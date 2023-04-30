using System.Diagnostics.Contracts;

namespace Tycoon.TwoPointOne;

record Location(string Value);

record Route(IReadOnlyCollection<Location> Locations)
{
    public override string ToString()
    {
        return string.Join(" -> ", Locations.Select(s => s.Value));
    }
}

sealed class Map
{
    private record Milestone(Location Location, Milestone? Previous = null)
    {
        [Pure]
        public Milestone MoveTo(Location to) => new Milestone(to, new Milestone(this));
        
        public Route AsRoute()
        {
            var curr = this;
            var l = new List<Location>();
            while (curr is not null)
            {
                l.Add(curr.Location);
                curr = curr.Previous;
            }

            l.Reverse();
            return new Route(l);
        }
    }
    private record Road(Location Location, int Distance);
    
    private record AdjacencyList(List<Road> Neighbours)
    {
        public static AdjacencyList Empty() => new AdjacencyList(new List<Road>());

        public IReadOnlyCollection<Road> NeighboursFromClosest 
            => Neighbours.OrderBy(s => s.Distance).ToList();
    }

    private Dictionary<Location, AdjacencyList> _map;
    
    public Map()
    {
        _map = new();
    }
    
    public void Add(Location from, Location to, int distance)
    {
        _map[from] = _map.GetValueOrDefault(from, AdjacencyList.Empty());
        _map[from].Neighbours.Add(new Road(to, distance));

        _map[to] = _map.GetValueOrDefault(to, AdjacencyList.Empty());
        _map[to].Neighbours.Add(new Road(from, distance));
    }

    public Route? FindShortestRouteBetween(Location from, Location to)
    {
        var queue = new PriorityQueue<Milestone, int>();
        queue.Enqueue(new Milestone(from), 0);
        var visited = new List<Location>();
        
        while (queue.TryDequeue(out var element, out var distance))
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

            foreach (var neighbour in _map[element.Location].NeighboursFromClosest)
            {
                if (!visited.Contains(neighbour.Location))
                {
                    var newDistance = distance + neighbour.Distance;
                    queue.Enqueue(
                        element.MoveTo(neighbour.Location), 
                        newDistance
                    );
                }
            }
        }

        return null;
    }
}