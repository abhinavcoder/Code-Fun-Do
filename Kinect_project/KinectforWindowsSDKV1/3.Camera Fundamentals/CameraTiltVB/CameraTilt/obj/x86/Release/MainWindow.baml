   M S B A M L   `   `   `  ���� ^  ZMicrosoft.Samples.Kinect.WpfViewers, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null�^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers#Microsoft.Samples.Kinect.WpfViewers   
CameraTilt   CameraTilt.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     mmy^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers     -     � Title$  
MainWindow��$	��350��$	��650��.���  � ColumnDefinitions �� $
��366* ��� $
��262* �.+��� -   $��slider1��$	��171��$
��Left=�$��
124,31,0,0q�$	��Top=�$��38�� �� Orientation$ Vertical=� � Maximum$ 27Q� � Minimum$	 -27Q� � SmallChange$ 1Q� �� TickPlacement$ TopLeft=� �� Interval$ 1�� �� IsSnapToTickEnabled . $��1���� -   $��label1��$��61��$
��Left=�$��
161,84,0,0q�$	��Top=�$��36(�$��1������ 	 �� Path$	 Value�
 �� ElementName$
 slider1���� -   $��button1��$��Set Tilt��$��51��$
��Left=�$��
362,91,0,0q�$	��Top=�$	��122��$��28(�$��2��=   7Microsoft.Samples.Kinect.WpfViewers.KinectSensorChooser  -   $��kinectSensorChooser1��$
��Left=�$	��Top=�$	��328��$��	19,27,0,0q��� -   $��label2��$��Current Angle:��$��36(�$��61��$
��Left=�$��282,214,0,0q�$	��Top=�$��2���� -   $��lblCurrentAngle��$��36(�$��61��$
��Left=�$��161,214,0,0q�$	��Top=�$	��...��$	��123��$��1��