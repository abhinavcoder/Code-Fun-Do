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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect; 
namespace Kinect_project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        KinectSensor _sensor ;
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void KinectSkeletonViewer_Loaded_1(object sender, RoutedEventArgs e)
        {

        }

    }
}
