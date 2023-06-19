using Geo.Abstractions;
using Geo.Geometries;
using System.Text;

namespace RurouniJones.Dcs.FrontLine
{
    public class CoalitionPolygon : Polygon
    {
        public CoalitionId Coalition { get; init; }
        public CoalitionPolygon(CoalitionId coalition, LinearRing shell, LinearRing[] holes) : base(shell, holes)
        {
            Coalition = coalition;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Coalition: {Coalition}");
            foreach (var coord in Shell.Coordinates)
            {
                sb.AppendLine($"Coord: {coord}. ");
            }
            return sb.ToString();
        }

        public override bool Equals(object? obj)
        {
            return obj is CoalitionPolygon polygon &&
                   base.Equals(obj) &&
                   EqualityComparer<LinearRing>.Default.Equals(Shell, polygon.Shell) &&
                   EqualityComparer<SpatialReadOnlyCollection<LinearRing>>.Default.Equals(Holes, polygon.Holes);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Shell, Holes);
        }
    }
}