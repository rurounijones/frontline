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
                    var distance = corner.CalculateGreatCircleLine(((UnitSite)site).Center).Distance.SiValue;
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
            return Sites.Select(site => ToUnitPolygon((UnitSite)site))
                .OrderBy(polygon => polygon.Shell.Coordinates.First().Latitude)
                .ThenBy(polygon => polygon.Shell.Coordinates.First().Longitude)
                .ToList();
        }

        private static CoalitionPolygon ToUnitPolygon(UnitSite site)
        {
            HashSet<Coordinate> coordinates = new();
            foreach (var edge in site.Cell)
            {
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
            foreach (var pair in bearings)
            {
                Debug.WriteLine($"{pair.Key}. Coords: X: {pair.Value.Longitude}, Y:{pair.Value.Latitude}");
            }
            var sortedCoordinates = bearings.Values.ToList();
            sortedCoordinates.Add(sortedCoordinates.First());

            return new LinearRing(sortedCoordinates);
        }

        #endregion

        #region Generating FrontLines
        public List<CoalitionPolygon> GenerateFrontLines()
        {
            GenerateSiteCells();

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
            //Sites = Sites.AsParallel().Where(site => site.Cell.Count > 1).Select(site => CloseSite((UnitSite)site)).ToList();
            Sites = Sites.AsParallel().Select(site => CloseSite((UnitSite)site)).ToList();
            return edges;
        }

        // Close the site if we have any edges that go out of bounds
        private FortuneSite CloseSite(UnitSite site)
        {
            var zeroLengthEdges = site.Cell.Where(edge => edge.Start.X.Equals(edge.End.X) && edge.Start.Y.Equals(edge.End.Y)).ToList();
            if (zeroLengthEdges.Count > 0)
            {

                //FIXME: THIS IS A HACK. WE DO NOT KNOW WHERE THESE ZERO LENGTH EDGES ARE COMING FROM
                foreach (var edge in zeroLengthEdges)
                {
                    Debug.WriteLine($"Zero Length edge found for site. Center {site.Center}. Edge X/Y {edge.Start.X}/{edge.Start.Y}");
                    site.Cell.Remove(edge);
                }
                //throw new Exception("The Site has edges with the same start and end points");

            }

            if (IsInBounds(site)) {
                return site;
            }
            var outOfBoundsCoords = site.Coordinates.Where(coord => !IsInBounds(coord)).ToHashSet();
            var start = outOfBoundsCoords.First();
            var end = outOfBoundsCoords.Last();

            // First find out of we are a closest site to any of the map corners
            List<Coordinate> corners = new();
            foreach (var pair in ClosestSitesToCorner)
            {
                if (pair.Value.Contains(site))
                {
                    corners.Add(pair.Key);
                }
            }

            if(corners.Count == 1)
            {
                // We are the closest site to a single corner which means we need to create two edges connecting the corner
                // to our dangling edges
                var corner = corners[0];
                site.AddEdge(EdgeGenerator.CreateClockwiseCornerEdge(corner, start, site));
                site.AddEdge(EdgeGenerator.CreateClockwiseCornerEdge(corner, end, site));
            } else if(corners.Count == 2)
            {
                // We are the closest site to two corners which means we need to create three edges connecting our dangling
                // edges to each corner AND an additional edge connecting the two corners

                // First we connect the two corners
                site.AddEdge(EdgeGenerator.CreateClockwiseCornerEdge(corners[0], corners[1], site));

                // Then we connect each corner to the closes dangling edge if there is a gap
                foreach(var corner in corners)
                {
                    if(corner.CalculateGreatCircleLine(start).Distance < corner.CalculateGreatCircleLine(end).Distance)
                    {
                        site.AddEdge(EdgeGenerator.CreateClockwiseCornerEdge(corner, start, site));
                    }
                    else
                    {
                        site.AddEdge(EdgeGenerator.CreateClockwiseCornerEdge(corner, end, site));
                    }
                }
            } else if(corners.Count == 3)
            {
                HashSet<VEdge> connectedCorners = new();
                foreach(var corner in corners)
                {
                    var connections = corners.Where(otherCorner => corner != otherCorner && (otherCorner.Latitude == corner.Latitude || otherCorner.Longitude == corner.Longitude)).ToList();
                    foreach(var connection in connections)
                    {
                        connectedCorners.Add(EdgeGenerator.CreateClockwiseCornerEdge(corner, connection, site));
                    }
                }
                foreach(var connectedCorner in connectedCorners)
                {
                    site.AddEdge(connectedCorner);
                }
            }
            else
            {
                // We are not the closest site to any corner which means we connect our two dangling edges instead
                // along the border
                site.AddEdge(EdgeGenerator.CreateClockwiseNonCornerEdge(start, end, site));
            }

            return site;
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