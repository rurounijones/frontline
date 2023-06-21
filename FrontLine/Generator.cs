using Geo;
using Geo.Geodesy;
using Geo.Geometries;
using SharpVoronoiLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

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
            foreach (var point in site.ClockwisePoints)
            {
                coordinates.Add(new Coordinate(point.Y, point.X));
            }

            // Close the polygon
            coordinates.Insert(0, coordinates.Last());

            return new CoalitionPolygon(site.Coalition,
                new LinearRing(coordinates),
                new List<LinearRing>().ToArray());
        }

        #endregion
    }
}
