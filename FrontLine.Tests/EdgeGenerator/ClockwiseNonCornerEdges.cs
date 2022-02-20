using Geo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VoronoiLib.Structures;

namespace RurouniJones.Dcs.FrontLine.Tests.EdgeGenerator
{
    /// <summary>
    /// Test the ability for the edge generator to generate clockwise edges when not connecting
    /// corners.
    /// Note that all our internal processing and debug display is lon/lat to match up with X/Y used
    /// in the VoronoiLib. However the `Coordinate` class we are also using from `Geo` is lat/lon so 
    /// they are reversed in the test data we setup
    /// </summary>
    [TestClass]
    public class ClockwiseNonCornerEdges
    {
        private static readonly Coordinate _bottomLeftCorner = new(0, 0);

        private static readonly Coordinate _topLeftCorner = new(10, 0);

        private static readonly Coordinate _topRightCorner = new(10, 10);

        private static readonly Coordinate _bottomRightCorner = new(0, 10);

        private static readonly UnitSite _unitSite = new(0, 0, CoalitionId.Neutral);

        private static readonly FrontLine.EdgeGenerator _edgeGenerator = new(_bottomLeftCorner, _topLeftCorner, _topRightCorner, _bottomRightCorner);

        [TestMethod]
        public void WhenOnLeftBorder_ThenConnectFromLowestIntersect()
        {
            var startIntersectPoint = new Coordinate(7, 0);
            var endIntersectPoint = new Coordinate(5, 0);

            var result = _edgeGenerator.CreateClockwiseNonCornerEdge(startIntersectPoint, endIntersectPoint, _unitSite);

            var expectedStart = new VPoint(0, 5);
            var expectedEnd = new VPoint(0, 7);

            Assert.AreEqual(expectedStart.X, result.Start.X, "Start X");
            Assert.AreEqual(expectedStart.Y, result.Start.Y, "Start Y");
            Assert.AreEqual(expectedEnd.X, result.End.X, "End X");
            Assert.AreEqual(expectedEnd.Y, result.End.Y, "End Y");
        }

        [TestMethod]
        public void WhenOnTopBorder_ThenConnectFromLeftMostIntersect()
        {
            var startIntersectPoint = new Coordinate(10, 7);
            var endIntersectPoint = new Coordinate(10, 5);

            var result = _edgeGenerator.CreateClockwiseNonCornerEdge(startIntersectPoint, endIntersectPoint, _unitSite);

            var expectedStart = new VPoint(5, 10);
            var expectedEnd = new VPoint(7, 10);

            Assert.AreEqual(expectedStart.X, result.Start.X, "Start X");
            Assert.AreEqual(expectedStart.Y, result.Start.Y, "Start Y");
            Assert.AreEqual(expectedEnd.X, result.End.X, "End X");
            Assert.AreEqual(expectedEnd.Y, result.End.Y, "End Y");
        }

        [TestMethod]
        public void WhenOnRightBorder_ThenConnectFromHighestIntersect()
        {
            var startIntersectPoint = new Coordinate(5, 10);
            var endIntersectPoint = new Coordinate(7, 10);

            var result = _edgeGenerator.CreateClockwiseNonCornerEdge(startIntersectPoint, endIntersectPoint, _unitSite);

            var expectedStart = new VPoint(10, 7);
            var expectedEnd = new VPoint(10, 5);

            Assert.AreEqual(expectedStart.X, result.Start.X, "Start X");
            Assert.AreEqual(expectedStart.Y, result.Start.Y, "Start Y");
            Assert.AreEqual(expectedEnd.X, result.End.X, "End X");
            Assert.AreEqual(expectedEnd.Y, result.End.Y, "End Y");
        }

        [TestMethod]
        public void WhenOnBottomBorder_ThenConnectFromRightMostIntersect()
        {
            var startIntersectPoint = new Coordinate(0, 5);
            var endIntersectPoint = new Coordinate(0, 7);

            var result = _edgeGenerator.CreateClockwiseNonCornerEdge(startIntersectPoint, endIntersectPoint, _unitSite);

            var expectedStart = new VPoint(7, 0);
            var expectedEnd = new VPoint(5, 0);

            Assert.AreEqual(expectedStart.X, result.Start.X, "Start X");
            Assert.AreEqual(expectedStart.Y, result.Start.Y, "Start Y");
            Assert.AreEqual(expectedEnd.X, result.End.X, "End X");
            Assert.AreEqual(expectedEnd.Y, result.End.Y, "End Y");
        }
    }
}
