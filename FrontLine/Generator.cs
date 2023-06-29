using Geo;
using Geo.Geodesy;
using Geo.Geometries;
using SharpVoronoiLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

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
        private List<VoronoiSite> Sites { get; set; }

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

        public Dictionary<Coordinate, List<UnitSite>> ClosestSitesToCorner { get; init; } = new();


        public Generator(IEnumerable<VoronoiSite> sites, double leftLongitude, double bottomLatitude, double rightLongitude, double topLatitude)
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
        }

        #region Generating Individual Polygons
        public List<CoalitionPolygon> GenerateUnitPolygons()
        {
            List<VoronoiEdge> edges = VoronoiPlane.TessellateOnce(
                Sites,
                LeftLongitude, BottomLatitude,
                RightLongitude, TopLatitude
            );

            //CoalitionPolygons = Sites.AsParallel().Select(site => ToCoalitionPolygon((UnitSite)site)).ToList();
            CoalitionPolygons = Sites.Select(site => ToCoalitionPolygon((UnitSite)site)).ToList();

            return CoalitionPolygons
                .OrderBy(polygon => polygon.Shell.Coordinates.First().Latitude)
                .ThenBy(polygon => polygon.Shell.Coordinates.First().Longitude)
                .ToList();
        }

        #endregion

        #region Generating FrontLines
        public List<CoalitionPolygon> GenerateFrontLines()
        {
            var plane = new VoronoiPlane(LeftLongitude, BottomLatitude, RightLongitude, TopLatitude);
            plane.SetSites(Sites);
            plane.Tessellate();

            plane.MergeSites(
                (site1, site2) => CoalitionMergeDecision((UnitSite)site1, (UnitSite)site2)
            );

            CoalitionPolygons = Sites.AsParallel().Select(site => ToCoalitionPolygon((UnitSite)site)).ToList();
            CoalitionPolygons.RemoveAll( site => site == null);

            return CoalitionPolygons
                .OrderBy(polygon => polygon.Shell.Coordinates.First().Latitude)
                .ThenBy(polygon => polygon.Shell.Coordinates.First().Longitude)
                .ToList();
        }

        private VoronoiSiteMergeDecision CoalitionMergeDecision(UnitSite site1, UnitSite site2)
        {
            return site1.Coalition == site2.Coalition ? VoronoiSiteMergeDecision.MergeIntoSite1 : VoronoiSiteMergeDecision.DontMerge;
        }

        private static CoalitionPolygon ToCoalitionPolygon(UnitSite site)
        {
            List<Coordinate> coordinates = new();

            if(site.ClockwiseCell.Count() == 0)
            {
                return null;
            }

            var cells = site.ClockwiseCell.ToList();

            var borders = new List<List<Coordinate>>()
            {
                getBorder(cells)
            };
            
            // At this point we have at least one set of coordinates. But if there are still coordinates left then we may have holes
            // In our polygon

            while(cells.Count > 0)
            {
                borders.Add(getBorder(cells));
            }
            borders.RemoveAll(border => border == null);

            if(borders.Count > 1) {
                var orderedBorders = borders.OrderBy(border => getBorderLength(border)).Reverse();
                var shell = new LinearRing(orderedBorders.First());

                var holes = new List<LinearRing>();

                foreach(var border in orderedBorders.Skip(1))
                {
                    holes.Add(new LinearRing(border));
                }


                return new CoalitionPolygon(site.Coalition,
                    shell,
                    holes.ToArray());
            } else if(borders.Count == 1)
            {
                return new CoalitionPolygon(site.Coalition,
                    new LinearRing(borders.First()),
                    new List<LinearRing>().ToArray());
            }
            {
                return null;
            }
        }

        private static List<Coordinate> getBorder(List<VoronoiEdge> cells)
        {
            var startCell = cells.First();
            var startPoint = startCell.Start;

            List<Coordinate> coordinates = new()
            {
                new Coordinate(startPoint.Y, startPoint.X)
            };

            int i = 0;
            bool stop = false;
            while (stop == false)
            {

                var latest = coordinates.Last();
                foreach (var cell in cells)
                {
                    var start = new Coordinate(cell.Start.Y, cell.Start.X);
                    var end = new Coordinate(cell.End.Y, cell.End.X);
                    if (start == latest)
                    {
                        coordinates.Add(end);
                        cells.Remove(cell);
                        break;
                    }
                    else if (end == latest)
                    {
                        coordinates.Add(start);
                        cells.Remove(cell);
                        break;
                    }
                }
                i++;
                if (coordinates.First() == coordinates.Last())
                {
                    stop = true;
                }
                if(i > 100)
                {
                    return null;
                }
            }

            return coordinates;
        }

        private static double getBorderLength(List<Coordinate> points)
        {
            double distance = 0;
            for (int i = 0; i < points.Count-1; i++)
            {
                var start = points[i];
                var end = points[i+1];
                distance += Math.Sqrt(Math.Pow(end.Longitude - start.Longitude, 2) + Math.Pow(end.Latitude - start.Latitude, 2));
            }
            return distance;
        }

        #endregion
    }
}
