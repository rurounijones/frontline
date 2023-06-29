using Geo;
using Geo.Geometries;
using MapControl;
using RurouniJones.Dcs.FrontLine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FrontLine.Visualizer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private const double _leftLongitude = 0;
        private const double _bottomLatitude = 0;
        private const double _rightLongitude = 10;
        private const double _topLatitude = 10;

        private Dictionary<string, List<UnitSite>> _sampleSets = Samples.Sites;
        private HashSet<UnitSite> _currentSites = new();


        public List<string> SampleSetNames
        {
            get => _sampleSets.Keys.ToList();
        }

        private List<CoalitionPolygon> _neutralPolygons = new();
        public List<CoalitionPolygon> NeutralPolygons
        {
            get => _neutralPolygons;
            set => SetProperty(ref _neutralPolygons, value);
        }

        private List<CoalitionPolygon> _redForPolygons = new();
        public List<CoalitionPolygon> RedForPolygons
        {
            get => _redForPolygons;
            set => SetProperty(ref _redForPolygons, value);
        }

        private List<CoalitionPolygon> _blueForPolygons = new();
        public List<CoalitionPolygon> BlueForPolygons
        {
            get => _blueForPolygons;
            set => SetProperty(ref _blueForPolygons, value);
        }

        private List<CoalitionPolygon> _mapEdgePolygons = new();
        public List<CoalitionPolygon> MapEdgePolygons
        {
            get => _mapEdgePolygons;
            set => SetProperty(ref _mapEdgePolygons, value);
        }

        private Location _mapLocation;
        public Location MapLocation
        {
            get => _mapLocation;
            set => SetProperty(ref _mapLocation, value);
        }

        private int _zoomLevel = 7;
        public int ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }

        private Location _mouseLocation;
        public Location MouseLocation
        {
            get => _mouseLocation;
            set => SetProperty(ref _mouseLocation, value);
        }

        private HashSet<UnitSite> _sites = new();
        public HashSet<UnitSite> Sites
        {
            get => _sites;
            set => SetProperty(ref _sites, value);
        }

        private HashSet<Coordinate> _coordinates = new();
        public HashSet<Coordinate> Coordinates
        {
            get => _coordinates;
            set => SetProperty(ref _coordinates, value);
        }

        private ICommand _addPointCommand;
        public ICommand AddPointCommand
        {
            get { return _addPointCommand ??= new DelegateCommand(AddPoint); }
        }

        private ICommand _generateUnitPolygonsCommand;
        public ICommand GenerateUnitPolygonsCommand
        {
            get { return _generateUnitPolygonsCommand ??= new DelegateCommand(GenerateUnitPolygons); }
        }

        private ICommand _generateVoronoiCommand;
        public ICommand GenerateVoronoiCommand
        {
            get { return _generateVoronoiCommand ??= new DelegateCommand(GenerateFrontlines); }
        }

        private ICommand _addSampleSetCommand;
        public ICommand AddSampleSetCommand
        {
            get { return _addSampleSetCommand ??= new DelegateCommand(AddSampleSet); }
        }

        private ICommand _clearMapCommand;
        public ICommand ClearMapCommand
        {
            get { return _clearMapCommand ??= new DelegateCommand(ClearMap); }
        }

        public MainViewModel()
        {
            MapLocation = new Location(5, 5);
            ZoomLevel = 7;
            MouseLocation = new Location(5, 5);
            Sites = new HashSet<UnitSite>() {};

            List<Coordinate> mapCorners = new() {
                new Coordinate(_topLatitude,    _rightLongitude),
                new Coordinate(_bottomLatitude, _rightLongitude),
                new Coordinate(_bottomLatitude, _leftLongitude),
                new Coordinate(_topLatitude,    _leftLongitude),
                new Coordinate(_topLatitude,    _rightLongitude) // Close ring by going back to the beginning
            };
            LinearRing[] holes = Array.Empty<LinearRing>();

            MapEdgePolygons = new()
            {
                new CoalitionPolygon(CoalitionId.Neutral, new LinearRing(mapCorners), holes)
            };
        }

        public void AddPoint(object? parameter)
        {
        }

        public void AddSampleSet(object? parameter)
        {
            if (parameter == null) return;
            RedForPolygons = new List<CoalitionPolygon>();
            BlueForPolygons = new List<CoalitionPolygon>();
            NeutralPolygons = new List<CoalitionPolygon>();
            Coordinates = new HashSet<Coordinate>();
            _currentSites = _sampleSets[parameter.ToString()].ToHashSet();
            Sites = CopyCurrentSites();
        }

        public void ClearMap(object? parameter)
        {
            _currentSites.Clear();
            Sites = new HashSet<UnitSite>();
            RedForPolygons = new List<CoalitionPolygon>();
            BlueForPolygons = new List<CoalitionPolygon>();
            NeutralPolygons = new List<CoalitionPolygon>();
            Coordinates = new HashSet<Coordinate>();
        }

        private HashSet<UnitSite> CopyCurrentSites()
        {
            return _currentSites.Select(site => new UnitSite(site.Longitude, site.Latitude, site.Coalition)).ToHashSet();
        }

        public void GenerateUnitPolygons(object? parameter)
        {
            if (_currentSites.Count == 0) return;
            Sites = CopyCurrentSites();

            var generator = new Generator(Sites, _leftLongitude, _bottomLatitude, _rightLongitude, _topLatitude);
            var results = generator.GenerateUnitPolygons();

            NeutralPolygons = results.Where(x => x.Coalition == CoalitionId.Neutral).ToList();
            RedForPolygons = results.Where(x => x.Coalition == CoalitionId.RedFor).ToList();
            BlueForPolygons = results.Where(x => x.Coalition == CoalitionId.BlueFor).ToList();

            HashSet<Coordinate> coordinates = new();
            foreach (var polygon in results)
            {
                foreach (var shellPoint in polygon.Shell.Coordinates)
                {
                    coordinates.Add(shellPoint);
                }
            }
            Coordinates = coordinates;
        }

        public void GenerateFrontlines(object? parameter)
        {
            if (_currentSites.Count == 0) return;
            Sites = CopyCurrentSites();

            var generator = new Generator(Sites, _leftLongitude, _bottomLatitude, _rightLongitude, _topLatitude);
            var results = generator.GenerateFrontLines();
            NeutralPolygons = results.Where(x => x.Coalition == CoalitionId.Neutral).ToList();
            RedForPolygons = results.Where(x => x.Coalition == CoalitionId.RedFor).ToList();
            BlueForPolygons = results.Where(x => x.Coalition == CoalitionId.BlueFor).ToList();

            HashSet<Coordinate> coordinates = new();
            foreach (var polygon in results)
            {
                foreach (var shellPoint in polygon.Shell.Coordinates)
                {
                    coordinates.Add(shellPoint);
                }
            }
            Coordinates = coordinates;
        }
    }
}
