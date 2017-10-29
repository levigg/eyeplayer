using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.ComponentModel;

namespace Eyeplayer
{
    /// <summary>
    /// Interaction logic for Calibration.xaml
    /// </summary>
    public partial class Calibration : Window
    {
        //private ICalibrationViewModel _viewModel;
        public static readonly DependencyProperty CalibrationDotRadiusProperty = DependencyProperty.Register("CalibrationDotRadius", typeof(double), typeof(Calibration));
        public Calibration()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }
        public double CalibrationDotRadius
        {
            get
            {
                return (double)GetValue(CalibrationDotRadiusProperty);
            }
            set
            {
                SetValue(CalibrationDotRadiusProperty, value);
            }
        }
        private void ExitAction()
        {
            Close();
        }
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((ICalibrationViewModel)DataContext).PropertyChanged += (s, e1) =>
            {
                if (string.Equals(e1.PropertyName, "CalibrationDotPosition"))
                {
                    StartAnimation();
                }
            };
        }
        private void StartAnimation()
        {
            var animation = (DoubleAnimation)FindResource("ShrinkingCalibrationDotAnimation");
            this.BeginAnimation(CalibrationDotRadiusProperty, animation);
        }

        private void CalibrationDotAnimation_Completed(object sender, EventArgs e)
        {
            ((ICalibrationViewModel)DataContext).CalibrationDotAnimationCompleted();
        }

        private void MainWindowInstance_Loaded(object sender, RoutedEventArgs e)
        {
            this.Activate();
            this.Focus();
        }

       
    }
}
