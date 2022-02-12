using Geo;
using Geo.Geodesy;
using Geo.Geometries;
using System.Diagnostics;
using VoronoiLib;
using VoronoiLib.Structures;

namespace RurouniJones.Dcs.FrontLine
{
    public enum CoalitionId
    {
        Neutral = 1,
        RedFor = 2,
        BlueFor = 3
    }

    public class Generator
    {
        private List<FortuneSite> Sites { get; set; }
        public double LeftLongitude { get; init; }
        public double BottomLatitude { get; init; }
        public double RightLongitude { get; init; }
        public double TopLatitude { get; }

        public Coordinate BottomLeftCorner { get; init; }
        public Coordinate TopLeftCorner { get; init; }
        public Coordinate BottomRightCorner { get; init; }
        public Coordinate TopRightCorner { get; init; }
        public List<Coordinate> Corners { get; init; }

        public Dictionary<Coordinate, UnitSite> ClosestSiteToCorner { get; init; } = new();


        public Generator(IEnumerable<FortuneSite> sites, double leftLongitude, double bottomLatitude, double rightLongitude, double topLatitude)
        {
            Sites = sites?.ToList() ?? throw new ArgumentNullException(nameof(sites));
            LeftLongitude = leftLongitude;
            BottomLatitude = bottomLatitude;
            RightLongitude = rightLongitude;
            TopLatitude = topLatitude;

            BottomLeftCorner = new Coordinate(leftLongitude, bottomLatitude);
            BottomRightCorner = new Coordinate(rightLongitude, bottomLatitude);
            TopLeftCorner = new Coordinate(leftLongitude, topLatitude);
            TopRightCorner = new Coordinate(rightLongitude, topLatitude);
            Corners = new() { BottomLeftCorner, BottomRightCorner, TopLeftCorner, TopRightCorner };

            foreach (var corner in Corners)
            {
                var site = Sites.OrderBy(site => corner.CalculateGreatCircleLine(((UnitSite)site).Center).Distance).First();
                ClosestSiteToCorner.Add(corner, (UnitSite)site);
            }
        }



        public List<CoalitionPolygon> GenerateFrontLines()
        {
            FortunesAlgorithm.Run(Sites.ToList(), LeftLongitude, BottomLatitude, RightLongitude, TopLatitude);
            Sites = Sites.AsParallel().Where(site => site.Cell.Count > 1).Select(site => CloseSite((UnitSite)site)).ToList();

            List<CoalitionPolygon> combinedSites = new();

            while (true)
            {
                if (Sites.Count() == 0)
                {
                    break;
                }
                var site = Sites.First();
                var addedSites = ReturnAllNeighborsForSite((UnitSite)site);

                var edges = new HashSet<VEdge>();
                foreach (var localSite in addedSites)
                {
                    edges.UnionWith(localSite.Cell.Where(edge => ((UnitSite)edge.Left)?.Coalition != ((UnitSite)edge.Right)?.Coalition).ToHashSet());
                }
                var center = ((UnitSite)site).Center;
                var coordinates = new HashSet<Coordinate>();
                foreach (var edge in edges)
                {
                    coordinates.Add(new Coordinate(edge.Start.X, edge.Start.Y));
                    coordinates.Add(new Coordinate(edge.End.X, edge.End.Y));
                }
                combinedSites.Add(ToCombinedCoalitionPolygon((UnitSite)site, coordinates));
                Sites.RemoveAll(x => addedSites.Contains(x));
            }

            return combinedSites;
        }

        internal List<CoalitionPolygon> GenerateUnitPolygons()
        {
            var edges = FortunesAlgorithm.Run(Sites.ToList(), LeftLongitude, BottomLatitude, RightLongitude, TopLatitude);
            Sites = Sites.AsParallel().Select(site => CloseSite((UnitSite)site)).ToList();

            return Sites.Select(site => ToCoalitionPolygon((UnitSite)site)).ToList();
        }

        private static CoalitionPolygon ToCombinedCoalitionPolygon(UnitSite site, HashSet<Coordinate> coordinates)
        {
            Debug.WriteLine($"Clockwise bearing for center X: {site.Center.Longitude}, Y:{site.Center.Latitude}:");
            return new CoalitionPolygon(site.Coalition,
                ToClockwiseLinearRing(site.Center, coordinates),
                new List<LinearRing>().ToArray());
        }

        private static CoalitionPolygon ToCoalitionPolygon(UnitSite site)
        {
            HashSet<Coordinate> coordinates = new();
            foreach(var edge in site.Cell) {
                coordinates.Add(new Coordinate(edge.Start.Y, edge.Start.X));
                coordinates.Add(new Coordinate(edge.End.Y, edge.End.X));
            }
            Debug.WriteLine($"Clockwise bearing for center X: {site.Center.Longitude}, Y:{site.Center.Latitude}:");
            return new CoalitionPolygon(site.Coalition,
                ToClockwiseLinearRing(site.Center, coordinates),
                new List<LinearRing>().ToArray());
        }

        private static LinearRing ToClockwiseLinearRing(Coordinate center, HashSet<Coordinate> coordinates)
        {
            var bearings = new SortedDictionary<double, Coordinate>();
            foreach (var coordinate in coordinates)
            {
                var bearing = center.CalculateGreatCircleLine(coordinate).Bearing12;
                bearings.Add(bearing, coordinate);
            }
            foreach(var pair in bearings)
            {
                Debug.WriteLine($"{pair.Key}. Coords: X: {pair.Value.Longitude}, Y:{pair.Value.Latitude}");
            }
            var sortedCoordinates = bearings.Values.ToList();
            sortedCoordinates.Add(sortedCoordinates.First());

            return new LinearRing(sortedCoordinates);
        }

        // Close the site if we have any edges that go out of bounds
        private FortuneSite CloseSite(UnitSite site)
        {
            if(IsInBounds(site)) {
                return site;
            }
            var outOfBoundsCoords = site.Coordinates.Where(coord => !IsInBounds(coord)).ToHashSet();
            if(outOfBoundsCoords.Count != 2)
            {
                return site;
                //throw new Exception("Other than 2 coordinates were found to be out of bounds. This should not be possible");
            }
            var start = outOfBoundsCoords.First();
            var end = outOfBoundsCoords.Last();

            // First find out of we are the closest site to any of the map corners
            List<Coordinate> corners = new();
            foreach (var pair in ClosestSiteToCorner)
            {
                if (pair.Value == site)
                {
                    corners.Add(pair.Key);
                }
            }

            if(corners.Count == 1)
            {
                // We are the closest site to a single corner which means we need to create two edges connecting the corner
                // to our dangling edges
                var corner = corners[0];
                var cornerPoint = new VPoint(corner.Longitude, corner.Latitude);
                site.AddEdge(new VEdge(new VPoint(start.Longitude, start.Latitude), cornerPoint, site));
                site.AddEdge(new VEdge(cornerPoint, new VPoint(end.Longitude, end.Latitude), site));
            } else if(corners.Count == 2)
            {
                // We are the closest site to two corners which means we need to create three edges connecting our dangling
                // edges to each corner AND an additional edge connecting the two corners

                // First we connect the two corners
                site.AddEdge(new VEdge(new VPoint(corners[0].Longitude, corners[0].Latitude), new VPoint(corners[1].Longitude, corners[1].Latitude), site));

                // Then we connect each corner to the closes dangling edge
                foreach(var corner in corners)
                {
                    if(corner.CalculateGreatCircleLine(start).Distance < corner.CalculateGreatCircleLine(start).Distance)
                    {
                        site.AddEdge(new VEdge(new VPoint(corner.Longitude, corner.Latitude), new VPoint(start.Longitude, start.Latitude), site));
                    }
                    else
                    {
                        site.AddEdge(new VEdge(new VPoint(corner.Longitude, corner.Latitude), new VPoint(end.Longitude, end.Latitude), site));
                    }
                }
            }
            else
            {
                // We are not the closest site to any corner which means we connect our two dangling edges instead
                site.AddEdge(new VEdge(new VPoint(start.Longitude, start.Latitude), new VPoint(end.Longitude, end.Latitude), site));
            }

            return site;
        }

        public HashSet<UnitSite> ReturnAllNeighborsForSite(UnitSite site)
        {
            var addedSites = new HashSet<UnitSite>() { site };
            addedSites.UnionWith(GetNeighbors(site, addedSites));
            Debug.WriteLine($"Total Neighbors for is {addedSites.Count}");
            return addedSites;
        }

        public HashSet<UnitSite> GetNeighbors(UnitSite site, HashSet<UnitSite> existing)
        {
            var neighbors = site.Neighbors.Select(n => (UnitSite)n)
                .Where(n => site.Coalition == n.Coalition)
                .Where(n => !existing.Contains(n)).
                ToList();
            Debug.WriteLine($"Neighbor count: {neighbors.Count}, {existing.Count}");
            existing.UnionWith(neighbors);
            if (neighbors.Count > 0)
            {
                foreach (var neighbor in neighbors)
                {
                    GetNeighbors(neighbor, existing);
                }
            }
            return existing;
        }

        private bool IsInBounds(UnitSite site)
        {
            foreach(var coordinate in site.Coordinates)
            {
                if(!IsInBounds(coordinate)) { 
                    return false;
                }
            }
            return true;
        }

        private bool IsInBounds(Coordinate coordinate)
        {
            int leftLongitude = coordinate.Longitude.CompareTo(LeftLongitude);
            int rightLongitude = coordinate.Longitude.CompareTo(RightLongitude);

            int topLatitude = coordinate.Latitude.CompareTo(TopLatitude);
            int bottomLatitude = coordinate.Latitude.CompareTo(BottomLatitude);

            if (topLatitude < 0 && bottomLatitude > 0 &&
               leftLongitude > 0 && rightLongitude < 0)
            {
                return true;
            }
            return false;
        }
    }
}