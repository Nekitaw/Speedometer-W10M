using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Devices.Geolocation;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Power;
using Windows.System.Power;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace spedo
{
    public partial class MainPage1 : Page
    {
        
        private Geolocator _geolocator;
        private DisplayRequest _displayRequest;

        private Geoposition _lastPosition;
        private double _totalDistanceMeters = 0;

        public MainPage1()
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            
            InitializeComponent();

            StartBatteryMonitoring();

            fullscreenmode();
        }


        protected void fullscreenmode()
        {
            // Pantalla completa
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
        }

        private async void OnStartClick(object sender, RoutedEventArgs e)
        {

            if (_displayRequest == null)
            {
                _displayRequest = new DisplayRequest();
                _displayRequest.RequestActive(); // Mantener pantalla encendida
            }

            _geolocator = new Geolocator
            {
                DesiredAccuracyInMeters = 5,
                ReportInterval = 1000 // en milisegundos
            };

            _geolocator.PositionChanged += Geolocator_PositionChanged;

            BtnStart.Visibility = Visibility.Collapsed;
            SpeedTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 147, 250, 145));
        }

        private async void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {

            var pos = args.Position;

            if (_lastPosition != null)
            {
                var pos1 = _lastPosition.Coordinate.Point.Position;
                var pos2 = pos.Coordinate.Point.Position;

                double dist = CalcularDistancia(pos1, pos2);
                _totalDistanceMeters += dist;
            }

            _lastPosition = pos;
            // La velocidad viene en metros por segundo
            double? speed = args.Position.Coordinate.Speed;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (speed.HasValue)
                {
                    // Convertimos a km/h si querés
                    double velocidadKmh = speed.Value * 3.6;
                    int velC = (int)Math.Ceiling(velocidadKmh);
                    SpeedTextBlock.Text = $"{velC}";
                }
                else
                {
                    SpeedTextBlock.Text = "0";
                }

                OdometerTextBlock.Text = $"{_totalDistanceMeters / 1000:F0} km";
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void StartBatteryMonitoring()
        {
            Battery.AggregateBattery.ReportUpdated += Battery_ReportUpdated;
            UpdateBatteryStatus();
        }

        private async void Battery_ReportUpdated(Battery sender, object args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, UpdateBatteryStatus);
        }

        private void UpdateBatteryStatus()
        {
            var report = Battery.AggregateBattery.GetReport();
            if (report.RemainingCapacityInMilliwattHours.HasValue && report.FullChargeCapacityInMilliwattHours.HasValue)
            {
                var percent = 100.0 * report.RemainingCapacityInMilliwattHours.Value / report.FullChargeCapacityInMilliwattHours.Value;
                BatteryTextBlock.Text = $"{percent:F0}%";
            }
            else
            {
                BatteryTextBlock.Text = "N/D%";
            }
        }

        private double CalcularDistancia(BasicGeoposition pos1, BasicGeoposition pos2)
        {
            const double RadioTierra = 6371000; // en metros

            double lat1 = pos1.Latitude * Math.PI / 180.0;
            double lon1 = pos1.Longitude * Math.PI / 180.0;
            double lat2 = pos2.Latitude * Math.PI / 180.0;
            double lon2 = pos2.Longitude * Math.PI / 180.0;

            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return RadioTierra * c;
        }
    }
}