using Geo;
using Geo.Geometries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using VoronoiLib.Structures;

namespace RurouniJones.Dcs.FrontLine.Tests.Generator
{
    /// <summary>
    /// Test the ability for the generator to combine polygons when neighbours belong to the same
    /// coalition
    /// Note that all our internal processing and debug display is lon/lat to match up with X/Y used
    /// in the VoronoiLib. However the `Coordinate` class we are also using from `Geo` is lat/lon so 
    /// they are reversed in the test data we setup
    /// </summary>
    [TestClass]
    public class PolygonCombiningTests
    {
        private static readonly LinearRing[] NO_HOLES = System.Array.Empty<LinearRing>();

        [TestMethod]
        public void TestCombiningPolygons()
        {
            var units = new List<FortuneSite>()
            {
                new UnitSite(1, 1, CoalitionId.RedFor),
                new UnitSite(2, 2, CoalitionId.BlueFor),
                new UnitSite(1, 2, CoalitionId.BlueFor),
                new UnitSite(2, 1, CoalitionId.BlueFor)
            };

            var expected = new List<CoalitionPolygon>()
            {
                new CoalitionPolygon(CoalitionId.RedFor,
                    new LinearRing(new List<Coordinate>()
                    {
                        new Coordinate(1.5, 1.5),
                        new Coordinate(0, 1.5),
                        new Coordinate(0, 0),
                        new Coordinate(1.5, 0),
                        new Coordinate(1.5, 1.5)
                    }),
                    NO_HOLES),
                new CoalitionPolygon(CoalitionId.BlueFor,
                    new LinearRing(new List<Coordinate>()
                    {
                        new Coordinate(10, 10),
                        new Coordinate(1.5, 10),
                        new Coordinate(0, 10),
                        new Coordinate(0, 1.5),
                        new Coordinate(1.5, 1.5),
                        new Coordinate(1.5, 0),
                        new Coordinate(10, 0),
                        new Coordinate(10, 1.5),
                        new Coordinate(10, 10),
                    }),
                    NO_HOLES)
            };

            var generator = new FrontLine.Generator(units, 0, 0, 10, 10);
            var actual = generator.GenerateFrontLines();

            foreach (var polygon in expected)
            {
                CollectionAssert.Contains(actual, polygon);
            }
        }

        [TestMethod]
        public void TestCombiningPolygonsWithCloseUnitsOfSameCoalition()
        {
            var units = new List<FortuneSite>()
            {
                new UnitSite(1, 1, CoalitionId.RedFor),
                new UnitSite(2, 2, CoalitionId.BlueFor),
                new UnitSite(3, 3, CoalitionId.BlueFor),
                new UnitSite(3, 4, CoalitionId.BlueFor),
                new UnitSite(4, 3, CoalitionId.BlueFor),
                new UnitSite(1, 2, CoalitionId.BlueFor),
                new UnitSite(2, 1, CoalitionId.BlueFor)
            };

            var expected = new List<CoalitionPolygon>()
            {
                new CoalitionPolygon(CoalitionId.RedFor,
                    new LinearRing(new List<Coordinate>()
                    {
                        new Coordinate(1.5, 1.5),
                        new Coordinate(0, 1.5),
                        new Coordinate(0, 0),
                        new Coordinate(1.5, 0),
                        new Coordinate(1.5, 1.5)
                    }),
                    NO_HOLES),
                new CoalitionPolygon(CoalitionId.BlueFor,
                    new LinearRing(new List<Coordinate>()
                    {
                        new Coordinate(10, 10),
                        new Coordinate(0, 10),
                        new Coordinate(0, 5),
                        new Coordinate(0, 1.5),
                        new Coordinate(1.5, 1.5),
                        new Coordinate(1.5, 0),
                        new Coordinate(5, 0),
                        new Coordinate(10, 0),
                        new Coordinate(10, 10),
                    }),
                    NO_HOLES)
            };

            var generator = new FrontLine.Generator(units, 0, 0, 10, 10);
            var actual = generator.GenerateFrontLines();

            foreach (var polygon in expected)
            {
                CollectionAssert.Contains(actual, polygon);
            }
        }
    }
}