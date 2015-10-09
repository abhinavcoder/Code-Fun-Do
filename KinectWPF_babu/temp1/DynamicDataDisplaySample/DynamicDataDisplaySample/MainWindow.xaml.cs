using System.Windows;
using DynamicDataDisplaySample.VoltageViewModel;
using System.Windows.Threading;
using System;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using System.Windows.Media;
using System.ComponentModel;
using Microsoft.Kinect;

using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.WPF;
using Microsoft.Speech.Recognition;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DynamicDataDisplaySample
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        KinectSensor sensor = KinectSensor.KinectSensors[0];

        #region "Variables"
        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double PI = 3.14 ;
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;


        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;
        private double jointspeed;
        private double x1;
        #endregion

        private int _maxVoltage;
        public int MaxVoltage
        {
            get { return _maxVoltage; }
            set { _maxVoltage = value; this.OnPropertyChanged("MaxVoltage"); }
        }

        private int _minVoltage;
        public int MinVoltage
        {
            get { return _minVoltage; }
            set { _minVoltage = value; this.OnPropertyChanged("MinVoltage"); }
        }

        public VoltagePointCollection voltagePointCollection; 
        DispatcherTimer updateCollectionTimer;
        private int i = 0;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            voltagePointCollection = new VoltagePointCollection();               

            updateCollectionTimer = new DispatcherTimer();
            updateCollectionTimer.Interval = TimeSpan.FromMilliseconds(40);
            updateCollectionTimer.Tick += new EventHandler(updateCollectionTimer_Tick);
            updateCollectionTimer.Start();

            var ds = new EnumerableDataSource<VoltagePoint>(voltagePointCollection);
            ds.SetXMapping(x => dateAxis.ConvertToDouble(x.Date));
            ds.SetYMapping(y => y.Voltage);
            plotter.AddLineGraph(ds, Colors.Green, 2, "SPEED"); // to use this method you need "using Microsoft.Research.DynamicDataDisplay;"

            MaxVoltage = 1;
            MinVoltage = -1;
            //After Initialization subscribe to the loaded event of the form 
            Loaded += MainWindow_Loaded;

            //After Initialization subscribe to the unloaded event of the form
            //We use this event to stop the sensor when the application is being closed.
            Unloaded += MainWindow_Unloaded;

            if (sensor != null)
            {
                sensor.EnableAllStreams();

                sensor.Start();
                voiceController.SpeechRecognized += VoiceController_SpeechRecognized;

                //  KinectSensor sensor = SensorExtensions.Default();
                List<string> phrases = new List<string> { "Open", "Close", };

                voiceController.StartRecognition(sensor, phrases);
            }
        }
        VoiceController voiceController = new VoiceController();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           // KinectSensor sensor1 = SensorExtensions.Default();

        }
        void VoiceController_SpeechRecognized(object sender, Microsoft.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            string text = e.Result.Text;
            tb.Text = text;
            tb2.Text = "qw";
            voiceController.Speak("I recognized the words: " + text);
        }
        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            //stop the Sestor 
            sensor.Stop();

        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            //Create a Drawing Group that will be used for Drawing 
            this.drawingGroup = new DrawingGroup();

            //Create an image Source that will display our skeleton
            this.imageSource = new DrawingImage(this.drawingGroup);

            //Display the Image in our Image control
            Image.Source = imageSource;

            try
            {
                //Check if the Sensor is Connected
                if (sensor.Status == KinectStatus.Connected)
                {
                    //Start the Sensor
                    sensor.Start();
                    //Tell Kinect Sensor to use the Default Mode(Human Skeleton Standing) || Seated(Human Skeleton Sitting Down)
                    sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                    //Subscribe to te  Sensor's SkeletonFrameready event to track the joins and create the joins to display on our image control
                    sensor.SkeletonFrameReady += sensor_SkeletonFrameReady;
                    //nice message with Colors to alert you if your sensor is working or not
                    Message.Text = "Kinect Ready";
                    Message.Background = new SolidColorBrush(Colors.Green);
                    Message.Foreground = new SolidColorBrush(Colors.White);

                    // Turn on the skeleton stream to receive skeleton frames
                    this.sensor.SkeletonStream.Enable();
                }
                else if (sensor.Status == KinectStatus.Disconnected)
                {
                    //nice message with Colors to alert you if your sensor is working or not
                    Message.Text = "Kinect Sensor is not Connected";
                    Message.Background = new SolidColorBrush(Colors.Orange);
                    Message.Foreground = new SolidColorBrush(Colors.Black);

                }
                else if (sensor.Status == KinectStatus.NotPowered)
                {//nice message with Colors to alert you if your sensor is working or not
                    Message.Text = "Kinect Sensor is not Powered";
                    Message.Background = new SolidColorBrush(Colors.Red);
                    Message.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
        }

        /// <summary>
        //When the Skeleton is Ready it must draw the Skeleton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            //declare an array of Skeletons
            Skeleton[] skeletons = new Skeleton[1];

            //Opens a SkeletonFrame object, which contains one frame of skeleton data.
            using (SkeletonFrame skeletonframe = e.OpenSkeletonFrame())
            {
                //Check if the Frame is Indeed open 
                if (skeletonframe != null)
                {

                    skeletons = new Skeleton[skeletonframe.SkeletonArrayLength];

                    // Copies skeleton data to an array of Skeletons, where each Skeleton contains a collection of the joints.
                    skeletonframe.CopySkeletonDataTo(skeletons);

                    //draw the Skeleton based on the Default Mode(Standing), "Seated"
                    if (sensor.SkeletonStream.TrackingMode == SkeletonTrackingMode.Default)
                    {
                        //Draw standing Skeleton
                        DrawStandingSkeletons(skeletons);
                    }
                    else if (sensor.SkeletonStream.TrackingMode == SkeletonTrackingMode.Seated)
                    {
                        //Draw a Seated Skeleton with 10 joints
                        DrawSeatedSkeletons(skeletons);
                    }
                }

            }


        }
        //Thi Function Draws the Standing  or Default Skeleton
        private void DrawStandingSkeletons(Skeleton[] skeletons)
        {

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                //Draw a Transparent background to set the render size or our Canvas
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                //If the skeleton Array has items 
                if (skeletons.Length != 0)
                {
                    //Loop through the Skeleton joins
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);


                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(this.centerPointBrush,
                                           null,
                                           this.SkeletonPointToScreen(skel.Position), BodyCenterThickness, BodyCenterThickness);

                        }

                    }


                }

                //Prevent Drawing outside the canvas 
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));

            }
        }


        private void DrawSeatedSkeletons(Skeleton[] skeletons)
        {

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                //Draw a Transparent background to set the render size 
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);


                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(this.centerPointBrush,
                                           null,
                                           this.SkeletonPointToScreen(skel.Position), BodyCenterThickness, BodyCenterThickness);

                        }

                    }


                }

                //Prevent Drawing outside the canvas 
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));

            }
        }



        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
           // jointspeed = SpeedCalculate2(skeleton);
            spatting(skeleton);
        }

        private double SpeedCalculate2(Skeleton skeleton)
        {
            Joint hipJoint = skeleton.Joints[JointType.HipLeft];
            Joint kneeJoint = skeleton.Joints[JointType.KneeLeft];
            Joint ankleJoint = skeleton.Joints[JointType.AnkleLeft];
            double hipkneeAng = PI / 2 - Math.Abs(Math.Atan((hipJoint.Position.Y - kneeJoint.Position.Y) / (hipJoint.Position.X - kneeJoint.Position.X)));
            double kneeankleAng = PI / 2 - Math.Abs(Math.Atan((kneeJoint.Position.Y - ankleJoint.Position.Y) / (kneeJoint.Position.X - ankleJoint.Position.X)));
            double hipkneeLen = Math.Sqrt(Math.Pow((hipJoint.Position.Y - kneeJoint.Position.Y), 2) + Math.Pow((hipJoint.Position.X - kneeJoint.Position.X), 2));
            double kneeankleLen = Math.Sqrt(Math.Pow((kneeJoint.Position.Y - ankleJoint.Position.Y), 2) + Math.Pow((kneeJoint.Position.X - ankleJoint.Position.X), 2));

            double anklePos = Math.Abs(hipkneeLen * Math.Sin(hipkneeAng) - kneeankleLen * Math.Sin(kneeankleAng));
            double delTime = 1 / 25.0;// timeCapture - ballLastUpdateTime;

            double speed = anklePos / delTime;
            return speed;
        }
        private void spatting(Skeleton skeleton)
        {
            Joint head = skeleton.Joints[JointType.Head];
            Joint shoulderCentre = skeleton.Joints[JointType.ShoulderCenter];
            Joint spine = skeleton.Joints[JointType.Spine];
            Joint hipCentre = skeleton.Joints[JointType.HipCenter];
            Joint shoulderLeft = skeleton.Joints[JointType.ShoulderLeft];
            Joint elbowLeft = skeleton.Joints[JointType.ElbowLeft];
            Joint handLeft = skeleton.Joints[JointType.HandLeft];
            Joint shoulderRight  = skeleton.Joints[JointType.ShoulderRight];
            Joint elbowRight = skeleton.Joints[JointType.ElbowRight];
            Joint handRight = skeleton.Joints[JointType.HandRight];

            Joint hipLeft = skeleton.Joints[JointType.HipLeft];
            Joint kneeLeft = skeleton.Joints[JointType.KneeLeft];
            Joint ankleLeft = skeleton.Joints[JointType.AnkleLeft];
            Joint hipRight = skeleton.Joints[JointType.HipRight];
            Joint kneeRight = skeleton.Joints[JointType.KneeRight];
            Joint ankleRight = skeleton.Joints[JointType.AnkleRight];

            // code for checking the alignment of head shoulder and spine. it should be in a straight line.
           double headshoulder = PI/2  - Math.Abs(Math.Atan((head.Position.Y-shoulderCentre.Position.Y)/(head.Position.X-shoulderCentre.Position.X))) ;
           double shoulderspin = PI/2 -  Math.Abs(Math.Atan((shoulderCentre.Position.Y - spine.Position.Y) / (shoulderCentre.Position.X - spine.Position.X)));
           int check = 1;
           if ((headshoulder < PI / 18) && (shoulderspin < PI / 18))
               check = 1;
           else
               check = 0;

            // code to check if left a=leg is straight

           double leftkneehip = PI / 2 - Math.Abs(Math.Atan((kneeLeft.Position.Y - hipLeft.Position.Y) / (kneeLeft.Position.X - hipLeft.Position.X)));
           double leftkneeankkle = PI / 2 - Math.Abs(Math.Atan((kneeLeft.Position.Y - ankleLeft.Position.Y) / (kneeLeft.Position.X - ankleLeft.Position.X)));

           if ((leftkneehip < PI / 15) && (leftkneeankkle < PI / 15) && check==1)
               check = 1;
           else
               check = 0;

            // code to ckeck if right leg is straight
           double rightkneehip = PI / 2 - Math.Abs(Math.Atan((kneeRight.Position.Y - hipRight.Position.Y) / (kneeRight.Position.X - hipRight.Position.X)));
           double rightkneeankle = PI / 2 - Math.Abs(Math.Atan((kneeRight.Position.Y - ankleRight.Position.Y) / (kneeRight.Position.X - ankleRight.Position.X)));

           if ((rightkneehip < PI / 15) && (rightkneeankle < PI / 15) && check==1)
               check = 1;
           else
               check = 0;

           double distelbowhandleftX = (elbowLeft.Position.X) - handLeft.Position.X;
           double distelbowleftY=(elbowLeft.Position.Y) - handLeft.Position.Y;
           double distelbowhandrightX = (elbowRight.Position.X) - handRight.Position.X;
            double distelbowhandrightY = (elbowRight.Position.Y) - handRight.Position.Y;
            if ((Math.Abs(distelbowhandrightX) < 0.1) && (Math.Abs(distelbowhandleftX) < 0.1) && (Math.Abs(distelbowhandrightY) < 0.25) && (Math.Abs(distelbowhandrightY) < 0.25) && check == 1)
                check = 1;
            else
                check = 0;
            

            
           if (check == 0)
               tb.Text = "load";
           else
               tb.Text = "peace";

        //    return false;
          
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked || joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred && joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;

            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }


        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }
        void updateCollectionTimer_Tick(object sender, EventArgs e)
        {
            i++;
            t1.Text = "sandeep";
            x1 = jointspeed * 15 / 8;
            int x2 = (int)x1;
            if (x2 < 0)
                x2 = 0;
            if (x2 > (int)(40 * 25 / 48))
                x2 = (int)(40 * 25 / 48);
            voltagePointCollection.Add(new VoltagePoint(x2,DateTime.Now));
            //x1 = 100;
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            x1 = 5;
        }

        private void tb_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
