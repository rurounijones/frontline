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

        private List<CoalitionPolygon> CoalitionPolygons { get; set; }

        public double LeftLongitude { get; init; }
        public double BottomLatitude { get; init; }
        public double RightLongitude { get; init; }
        public double TopLatitude { get; }

        public Coordinate BottomLeftCorner { get; init; }
        public Coordinate TopLeftCorner { get; init; }
        public Coordinate BottomRightCorner { get; init; }
        public Coordinate TopRightCorner { get; init; }
        public List<Coordinate> Corners { get; init; }
        private EdgeGenerator EdgeGenerator { get; init; }

        public Dictionary<Coordinate, List<UnitSite>> ClosestSitesToCorner { get; init; } = new();


        public Generator(IEnumerable<FortuneSite> sites, double leftLongitude, double bottomLatitude, double rightLongitude, double topLatitude)
        {
            Sites = sites.ToList() ?? throw new ArgumentNullException(nameof(sites));
            LeftLongitude = leftLongitude;
            BottomLatitude = bottomLatitude;
            RightLongitude = rightLongitude;
            TopLatitude = topLatitude;

            BottomLeftCorner = new Coordinate(bottomLatitude, leftLongitude);
            BottomRightCorner = new Coordinate(bottomLatitude, rightLongitude);
            TopLeftCorner = new Coordinate(topLatitude, leftLongitude);
            TopRightCorner = new Coordinate(topLatitude, rightLongitude);
            Corners = new() { BottomLeftCorner, BottomRightCorner, TopLeftCorner, TopRightCorner };

            EdgeGenerator = new EdgeGenerator(BottomLeftCorner, TopLeftCorner, TopRightCorner, BottomRightCorner);

            foreach (var corner in Corners)
            {
                Dictionary<double, List<UnitSite>> closestSites = new();
                foreach(var site in Sites)
                {
                    double distance = GetDistance(((UnitSite)site).Center, corner);
                    if(closestSites.ContainsKey(distance))
                    {
                        closestSites[distance].Add((UnitSite)site);
                    } else
                    {
                        closestSites.Add(distance, new List<UnitSite>() { (UnitSite)site });
                    }
                }
                var lowestDistance = closestSites.Keys.OrderBy(key => key).ToList().First();

                ClosestSitesToCorner.Add(corner, closestSites[lowestDistance]);
            }
        }

        #region Generating Individual Polygons
        public List<CoalitionPolygon> GenerateUnitPolygons()
        {
            GenerateSiteCells();
            return CoalitionPolygons
                .OrderBy(polygon => polygon.Shell.Coordinates.First().Latitude)
                .ThenBy(polygon => polygon.Shell.Coordinates.First().Longitude)
                .ToList();
        }

        #endregion

        #region Generating FrontLines
        public List<CoalitionPolygon> GenerateFrontLines()
        {
            GenerateSiteCells();
            return CoalitionPolygons
                .OrderBy(polygon => polygon.Shell.Coordinates.First().Latitude)
                .ThenBy(polygon => polygon.Shell.Coordinates.First().Longitude)
                .ToList();
            /*
            List<CoalitionPolygon> combinedSites = new();

            while (true)
            {
                if (Sites.Count() == 0)
                {
                    break;
                }
                var site = Sites.OrderBy(site => ((UnitSite)site).Center.Latitude)
                    .ThenBy(site => ((UnitSite)site).Center.Longitude)
                    .First();
                var addedSites = ReturnAllNeighborsForSite((UnitSite)site);

                var edgeSet = new HashSet<VEdge>();
                foreach (var localSite in addedSites)
                {
                    edgeSet.UnionWith(localSite.Cell.Where(edge => ((UnitSite)edge.Left)?.Coalition != ((UnitSite)edge.Right)?.Coalition).ToHashSet());
                }
                var center = ((UnitSite)site).Center;
                var coordinates = new List<Coordinate>();

                var currentEdge = edgeSet
                    .OrderByDescending(edge => edge.Start.X)
                    .ThenByDescending(edge => edge.Start.Y)
                    .First();
                coordinates.Add(new Coordinate(currentEdge.Start.Y, currentEdge.Start.X));
                coordinates.Add(new Coordinate(currentEdge.End.Y, currentEdge.End.X));

                edgeSet.Remove(currentEdge);

                while (edgeSet.Count > 0)
                {
                    var nextEdge = edgeSet.FirstOrDefault(newEdge => (newEdge.Start.X.Equals(currentEdge.End.X) && newEdge.Start.Y.Equals(currentEdge.End.Y)));
                    if (nextEdge != null)
                    {
                        coordinates.Add(new Coordinate(nextEdge.End.Y, nextEdge.End.X));
                        currentEdge = nextEdge;
                        edgeSet.Remove(currentEdge);
                    }
                    else
                    {
                        //break;
                        throw new Exception($"Expected an edge in the list to exist with Start X:{currentEdge.End.X}, Y:{currentEdge.End.Y}");
                    }
                }
                coordinates.Add(coordinates.First());

                combinedSites.Add(ToCombinedCoalitionPolygon((UnitSite)site, coordinates));
                Sites.RemoveAll(x => addedSites.Contains(x));
            }

            return combinedSites;
            */
        }
        private static CoalitionPolygon ToCombinedCoalitionPolygon(UnitSite site, List<Coordinate> coordinates)
        {
            Debug.WriteLine($"Clockwise bearing for center X: {site.Center.Longitude}, Y:{site.Center.Latitude}:");
            return new CoalitionPolygon(site.Coalition, new LinearRing(coordinates), new List<LinearRing>().ToArray());
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

        #endregion

        #region shared
        private LinkedList<VEdge>? GenerateSiteCells()
        {
            var edges = FortunesAlgorithm.Run(Sites.ToList(), LeftLongitude, BottomLatitude, RightLongitude, TopLatitude);
            var coalitionPolygons = Sites.AsParallel().Select(site => CloseSite((UnitSite)site)).ToList();
            coalitionPolygons.RemoveAll(polygon => polygon == null);
            CoalitionPolygons = coalitionPolygons;
            return edges;
        }

        // Close the site if we have any edges that go out of bounds, otherwise just return the polygon
        private CoalitionPolygon CloseSite(UnitSite site)
        {
            if(site.Cell == null || site.Cell.Count == 0)
            {
                return null;
            }

            if (IsInBounds(site)) {
                return ToCoalitionPolygon(site.Coalition, site.Points);
            }

            var outOfBoundsCoords = site.Coordinates.Where(coord => !IsInBounds(coord)).ToHashSet();

            // First find out of we are a closest site to any of the map corners
            List<Coordinate> corners = new();
            foreach (var pair in ClosestSitesToCorner)
            {
                if (pair.Value.Contains(site))
                {
                    corners.Add(pair.Key);
                }
            }
            foreach(var corner in corners) {               
                site.AddEdge(new VEdge(new VPoint(corner.Longitude, corner.Latitude), site.Cell[0].Start, site.Cell[0].Left));
            }

            return ToCoalitionPolygon(site.Coalition, site.Points);
        }

        private CoalitionPolygon ToCoalitionPolygon(CoalitionId coalition, IEnumerable<VPoint> points)
        {
            List<Coordinate> coordinates = new();
            foreach (var point in points)
            {
                coordinates.Insert(0, new Coordinate(point.Y, point.X));
            }
            // Close the polygon
            coordinates.Insert(0, coordinates.Last());

            return new CoalitionPolygon(coalition,
                new LinearRing(coordinates),
                new List<LinearRing>().ToArray());
        }

        private static double GetDistance(Coordinate start, Coordinate end)
        {
            return Math.Sqrt(Math.Pow(end.Longitude - start.Longitude, 2) + Math.Pow(end.Latitude - start.Latitude, 2));
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

        #endregion
    }
}