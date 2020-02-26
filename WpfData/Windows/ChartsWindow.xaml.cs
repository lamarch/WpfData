using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Linq;

using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using System.Drawing;
using System.Windows.Media;

namespace WpfData
{
    /// <summary>
    /// Logique d'interaction pour ChartsWindow.xaml
    /// </summary>
    public partial class ChartsWindow : Window, INotifyPropertyChanged
    {
        private static readonly System.Windows.Media.Brush dangerBrush = new SolidColorBrush(Colors.Red);

        /*
        private CartesianMapper<NetworkDataUsage> cartesianMapperBase = new CartesianMapper<NetworkDataUsage>().X(u => u.DateTime.Ticks).Fill((u) => u.CurrentDownloadRate > Octet.FromMega(1) ? dangerBrush : null).Stroke((u) => u.CurrentDownloadRate > Octet.FromMega(1) ? dangerBrush : null);
        */

        public SeriesCollection dataRateCollection { get; set; }
        public SeriesCollection dataTotalCollection { get; set; }

        public Func<double, string> labelFormatterRate_X { get; set; } = u => new DateTime((long)u).ToString("HH:mm:ss");
        public Func<double, string> labelFormatterRate_Y { get; set; } = u => Octet.FromOctet((long)u).ToString(1);



        public ChartsWindow (ChartValues<NetworkDataUsage> chartValues)
        {

            chartValues.CollectionChanged += (a, b) =>
            {
                SetAxisLimit(DateTime.Now);
            };

            AxisXStep = TimeSpan.FromSeconds(60).Ticks;
            AxisXUnit = TimeSpan.TicksPerSecond;

            this.SetAxisLimit(DateTime.Now);

            var mapperRateDown = Mappers.Xy<NetworkDataUsage>().X(u => u.DateTime.Ticks).Y(u => u.CurrentDownloadRate.GetOctets());
            var mapperRateUp = Mappers.Xy<NetworkDataUsage>().X(u => u.DateTime.Ticks).Y(u => u.CurrentUploadRate.GetOctets());

            var blueGradientBrush = new LinearGradientBrush()
            {
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint = new System.Windows.Point(0, 1)
            };
            blueGradientBrush.GradientStops.Add(new GradientStop(Colors.DodgerBlue, 0));
            blueGradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));

            var orangeGradientBrush = new LinearGradientBrush()
            {
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint = new System.Windows.Point(0, 1)
            };
            orangeGradientBrush.GradientStops.Add(new GradientStop(Colors.Gold, 0));
            orangeGradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));

            this.InitializeComponent();
            DataContext = this;

            dataRateCollection = new SeriesCollection()
            {
                new LineSeries(mapperRateDown)
                {
                    Values = chartValues,
                    LabelPoint = p => ((NetworkDataUsage)p.Instance).CurrentDownloadRate.ToString(),
                    PointGeometry = null,
                    Title = "Trafic - Téléchargement",
                    Stroke = new SolidColorBrush(Colors.Blue),
                    Fill = blueGradientBrush
                },
                new LineSeries(mapperRateUp)
                {
                    Values = chartValues,
                    LabelPoint = p => ((NetworkDataUsage)p.Instance).CurrentUploadRate.ToString(),
                    PointGeometry = null,
                    Title = "Trafic - Envoi",
                    Stroke = new SolidColorBrush(Colors.Goldenrod),
                    Fill = orangeGradientBrush
                }

            };

            /*
            dataTotalCollection = new SeriesCollection()
            {
                new LineSeries(mapperDown)
                {
                    Values = chartValues,
                    LabelPoint = p => ((NetworkDataUsage)p.Instance).TotalDownload.ToString()
                },
                new LineSeries(mapperUp)
                {
                    Values = chartValues,
                    LabelPoint = p => ((NetworkDataUsage)p.Instance).TotalUpload.ToString()

                },
                new LineSeries(mapperTotal)
                {
                    Values = chartValues,
                    LabelPoint = p => ((NetworkDataUsage)p.Instance).GetTotal().ToString()
                }
            };
            */
        }

        private double _axisMax;
        
        public double AxisXMax
        {
            get => this._axisMax;
            set
            {
                if ( this._axisMax != value )
                {
                    this._axisMax = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        private double _axisMin;
        public double AxisXMin
        {
            get => this._axisMin;
            set
            {
                if ( this._axisMin != value )
                {
                    this._axisMin = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        
        public void SetAxisLimit (DateTime time)
        {
            
            AxisXMax = time.Ticks + TimeSpan.FromSeconds(5).Ticks;
            AxisXMin = time.Ticks - TimeSpan.FromMinutes(2.5).Ticks;
        }
        
        public double AxisXStep { get; set; }
        public double AxisXUnit { get; set; }

        public double AxisYMin { get => 0; }
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged ([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

    }
}
