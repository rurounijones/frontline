using Geo;
using SharpVoronoiLib;
using System.Text;

namespace RurouniJones.Dcs.FrontLine
{
    public class UnitSite : VoronoiSite
    {
        public CoalitionId Coalition { get; init; }
        public double Latitude { get { return Y; } }
        public double Longitude { get { return X; } }
        public Coordinate Center { get; init; }
        public HashSet<Coordinate> Coordinates => Cell.SelectMany(edge => new HashSet<Coordinate>()
                        {
                            new Coordinate(edge.Start.Y, edge.Start.X),
                            new Coordinate(edge.End.Y, edge.End.X),
                        }
                    ).ToHashSet();

        public UnitSite(double longitude, double latitude, CoalitionId coalition) : base(longitude, latitude)
        {
            Coalition = coalition;
            Center = new Coordinate(latitude, longitude);
        }       

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"UnitSite: {Coalition}, Center {Center}");
            return sb.ToString();
        }
    }
}