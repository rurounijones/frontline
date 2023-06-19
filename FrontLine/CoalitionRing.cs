using Geo;
using Geo.Geometries;
using System.Text;

namespace RurouniJones.Dcs.FrontLine
{
    public class CoalitionRing : LinearRing
    {
        public CoalitionId Coalition { get; init; }
        public CoalitionRing(CoalitionId coalition, List<Coordinate> coordinates) : base(coordinates)
        {
            Coalition = coalition;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Coalition: {Coalition}");
            foreach (var coord in Coordinates)
            {
                sb.AppendLine($"Coord: {coord}");
            }
            return sb.ToString();
        }
    }
}