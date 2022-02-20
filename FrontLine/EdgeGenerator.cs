using Geo;
using VoronoiLib.Structures;

namespace RurouniJones.Dcs.FrontLine
{
    /// <summary>
    /// Generates edges when we need to close a polygon
    /// </summary>
    internal class EdgeGenerator
    {
        private Coordinate BottomLeftCorner { get; }
        private Coordinate TopLeftCorner { get; }
        private Coordinate TopRightCorner { get; }
        private Coordinate BottomRightCorner { get; }

        public EdgeGenerator(Coordinate bottomLeftCorner, Coordinate topLeftCorner, Coordinate topRightCorner, Coordinate bottomRightCorner)
        {
            BottomLeftCorner = bottomLeftCorner;
            TopLeftCorner = topLeftCorner;
            TopRightCorner = topRightCorner;
            BottomRightCorner = bottomRightCorner;
        }

        public VEdge CreateClockwiseCornerEdge(Coordinate corner, Coordinate edge, FortuneSite site)
        {
            var cornerPoint = new VPoint(corner.Longitude, corner.Latitude);
            var edgePoint = new VPoint(edge.Longitude, edge.Latitude);

            if (corner == BottomLeftCorner)
            {
                // We are going up the left border from the bottom left corner
                if (cornerPoint.X.Equals(edgePoint.X))
                {
                    return new VEdge(cornerPoint, edgePoint, site);
                }
                // We are going along the bottom border to the bottom left corner
                else if (cornerPoint.Y.Equals(edgePoint.Y))
                {
                    return new VEdge(edgePoint, cornerPoint, site);
                }
            }

            if (corner == TopLeftCorner)
            {
                // We are going up the left border to the top left corner
                if (cornerPoint.X.Equals(edgePoint.X))
                {
                    return new VEdge(edgePoint, cornerPoint, site);
                }
                // We are going along the top border from top left corner
                else if (cornerPoint.Y.Equals(edgePoint.Y))
                {
                    return new VEdge(cornerPoint, edgePoint, site);
                }
            }

            if (corner == TopRightCorner)
            {
                // We are going down the right border from the top right corner
                if (cornerPoint.X.Equals(edgePoint.X))
                {
                    return new VEdge(cornerPoint, edgePoint, site);
                }
                // We are going along the top border to the top right corner
                else if (cornerPoint.Y.Equals(edgePoint.Y))
                {
                    return new VEdge(edgePoint, cornerPoint, site);
                }
            }

            if (corner == BottomRightCorner)
            {
                // We are going down the right border to the bottom right corner
                if (cornerPoint.X.Equals(edgePoint.X))
                {
                    return new VEdge(edgePoint, cornerPoint, site);
                }
                // We are going along the bottom border from the bottom right corner
                else if (cornerPoint.Y.Equals(edgePoint.Y))
                {
                    return new VEdge(cornerPoint, edgePoint, site);
                }
            }

            throw new Exception("Could not create a clockwise edge");
        }

        public VEdge CreateClockwiseNonCornerEdge(Coordinate start, Coordinate end, FortuneSite site)
        {
            var startPoint = new VPoint(start.Longitude, start.Latitude);
            var endPoint = new VPoint(end.Longitude, end.Latitude);

            // If we are on the left border
            if (startPoint.X.Equals(BottomLeftCorner.Longitude))
            {
                // Then go bottom to top
                if (startPoint.Y < endPoint.Y)
                {
                    return new VEdge(startPoint, endPoint, site);
                }
                else
                {
                    return new VEdge(endPoint, startPoint, site);
                }
            }

            // If we are on the bottom border
            if (startPoint.Y.Equals(BottomLeftCorner.Latitude))
            {
                // Then go right to right
                if (startPoint.X < endPoint.X)
                {
                    return new VEdge(endPoint, startPoint, site);
                }
                else
                {
                    return new VEdge(startPoint, endPoint, site);
                }
            }

            // If we are on the right border
            if (startPoint.X.Equals(TopRightCorner.Longitude))
            {
                // Then go top to bottom
                if (startPoint.Y < endPoint.Y)
                {
                    return new VEdge(endPoint, startPoint, site);
                }
                else
                {
                    return new VEdge(startPoint, endPoint, site);
                }
            }

            // If we are on the top border
            if (startPoint.Y.Equals(TopRightCorner.Latitude))
            {
                // Then go left to right
                if (startPoint.X < endPoint.X)
                {
                    return new VEdge(startPoint, endPoint, site);
                }
                else
                {
                    return new VEdge(endPoint, startPoint, site);
                }
            }

            throw new Exception("Could not create a clockwise edge");
        }
    }
}
