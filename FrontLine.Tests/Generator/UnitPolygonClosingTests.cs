using Geo;
using Geo.Geometries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using VoronoiLib.Structures;

namespace RurouniJones.Dcs.FrontLine.Tests.Generator
{
    /// <summary>
    /// Test the ability for the generator to close open polygons when they intersect with a map edge
    /// Note that all our internal processing and debug display is lon/lat to match up with X/Y used
    /// in the VoronoiLib. However the `Coordinate` class we are also using from `Geo` is lat/lon so 
    /// they are reversed in the test data we setup
    /// </summary>
    [TestClass]
    public class UnitPolygonClosingTests
    {
        private static readonly LinearRing[] NO_HOLES = System.Array.Empty<LinearRing>();

        #region Polygons without internal holes

        // Test that we correctly close polygons when a map corner forms part of the polygon
        [TestMethod]
        public void TestCornerClosing()
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
                        new Coordinate(1.5, 1.5),
                        new Coordinate(10, 1.5),
                        new Coordinate(10, 10),
                    }),
                    NO_HOLES),
                new CoalitionPolygon(CoalitionId.BlueFor,
                    new LinearRing(new List<Coordinate>()
                    {
                        new Coordinate(1.5, 10),
                        new Coordinate(0, 10),
                        new Coordinate(0, 1.5),
                        new Coordinate(1.5, 1.5),
                        new Coordinate(1.5, 10),
                    }),
                    NO_HOLES),
                new CoalitionPolygon(CoalitionId.BlueFor,
                    new LinearRing(new List<Coordinate>()
                    {
                        new Coordinate(10, 1.5),
                        new Coordinate(1.5, 1.5),
                        new Coordinate(1.5, 0),
                        new Coordinate(10, 0),
                        new Coordinate(10, 1.5),
                    }),
                    NO_HOLES)
            };

            var generator = new FrontLine.Generator(units, 0, 0, 10, 10);
            var actual = generator.GenerateUnitPolygons();

            foreach(var polygon in expected)
            {
                CollectionAssert.Contains(actual, polygon);
            }
        }

        // Test that we correctly close a polygon when no map corners form part of it
        [TestMethod]
        public void TestSameBorderClosing()
        {
            var units = new List<FortuneSite>()
            {
                new UnitSite(5, 1, CoalitionId.RedFor),
                new UnitSite(5, 2, CoalitionId.BlueFor),
                new UnitSite(4, 1, CoalitionId.BlueFor),
                new UnitSite(6, 1, CoalitionId.BlueFor)
            };

            var expected = new CoalitionPolygon(CoalitionId.RedFor,
                    new LinearRing(new List<Coordinate>()
                    {
                        new Coordinate(1.5, 5.5),
                        new Coordinate(0, 5.5),
                        new Coordinate(0, 4.5),
                        new Coordinate(1.5, 4.5),
                        new Coordinate(1.5, 5.5)
                    }),
                    NO_HOLES);

            var generator = new FrontLine.Generator(units, 0, 0, 10, 10);
            var actual = generator.GenerateUnitPolygons();

            CollectionAssert.Contains(actual, expected);
        }

        // Test that we correctly close a polygon when two map corners form part of it
        [TestMethod]
        public void TestTwoCornerClosing()
        {
            var units = new List<FortuneSite>()
            {
                new UnitSite(5, 1, CoalitionId.RedFor),
                new UnitSite(5, 9, CoalitionId.BlueFor),
            };

            var expected = new List<CoalitionPolygon>()
            {
                new CoalitionPolygon(CoalitionId.RedFor,
                    new LinearRing(new List<Coordinate>()
                    {
                        new Coordinate(5, 10),
                        new Coordinate(0, 10),
                        new Coordinate(0, 0),
                        new Coordinate(5, 0),
                        new Coordinate(5, 10)
                    }),
                    NO_HOLES),
                new CoalitionPolygon(CoalitionId.BlueFor,
                    new LinearRing(new List<Coordinate>()
                    {
                        new Coordinate(10, 10),
                        new Coordinate(5, 10),
                        new Coordinate(5, 0),
                        new Coordinate(10, 0),
                        new Coordinate(10, 10)
                    }),
                    NO_HOLES)
            };

            var generator = new FrontLine.Generator(units, 0, 0, 10, 10);
            var actual = generator.GenerateUnitPolygons();

            foreach (var polygon in expected)
            {
                CollectionAssert.Contains(actual, polygon);
            }
        }

        #endregion

        [TestMethod]
        public void TestClosingNonEdgePolygon()
        {
            var units = new List<FortuneSite>()
            {
                new UnitSite(4, 4, CoalitionId.RedFor),
                new UnitSite(4, 5, CoalitionId.BlueFor),
                new UnitSite(5, 5, CoalitionId.BlueFor),
                new UnitSite(5, 4, CoalitionId.BlueFor),
                new UnitSite(3, 4, CoalitionId.BlueFor),
                new UnitSite(4, 3, CoalitionId.BlueFor),
                new UnitSite(3, 3, CoalitionId.BlueFor),
            };

            var expected = new List<CoalitionPolygon>()
            {
                new CoalitionPolygon(CoalitionId.RedFor,
                    new LinearRing(new List<Coordinate>()
                    {
                        new Coordinate(4.5, 4.5),
                        new Coordinate(3.5, 4.5),
                        new Coordinate(3.5, 3.5),
                        new Coordinate(4.5, 3.5),
                        new Coordinate(4.5, 4.5)
                    }),
                    NO_HOLES),
            };

            var generator = new FrontLine.Generator(units, 0, 0, 10, 10);
            var actual = generator.GenerateUnitPolygons();

            foreach (var polygon in expected)
            {
                CollectionAssert.Contains(actual, polygon);
            }
        }
    }
}