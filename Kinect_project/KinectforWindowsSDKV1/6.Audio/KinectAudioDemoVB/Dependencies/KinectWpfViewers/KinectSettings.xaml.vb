Imports Microsoft.Kinect
Imports System.Windows.Threading
Imports System.Threading.Tasks
Imports System.Threading

'------------------------------------------------------------------------------
' <copyright file="KinectSettings.xaml.cs" company="Microsoft">
'     Copyright (c) Microsoft Corporation.  All rights reserved.
' </copyright>
'------------------------------------------------------------------------------

Namespace Microsoft.Samples.Kinect.WpfViewers

	''' <summary>
	''' Interaction logic for KinectSettings.xaml
	''' </summary>
	Partial Friend Class KinectSettings
		Inherits UserControl
		Private ReadOnly debounce As DispatcherTimer = New DispatcherTimer With {.IsEnabled = False, .Interval = TimeSpan.FromMilliseconds(200)}

		Private lastSetSensorAngle As Integer = Integer.MaxValue
		Private userUpdate As Boolean = True
		Private backgroundUpdateInProgress As Boolean

		Public Sub New(ByVal diagViewer As KinectDiagnosticViewer)
			Me.DiagViewer = diagViewer
			InitializeComponent()
			AddHandler Me.debounce.Tick, AddressOf DebounceElapsed
		End Sub

		Public Property DiagViewer() As KinectDiagnosticViewer

		Public Property Kinect() As KinectSensor

		Private Shared ReadOnly Property IsSkeletalViewerAvailable() As Boolean
			Get
				Return KinectSensor.KinectSensors.All(Function(k) ((Not k.IsRunning) OrElse (Not k.SkeletonStream.IsEnabled)))
			End Get
		End Property

		Friend Sub PopulateComboBoxesWithFormatChoices()
			For Each colorImageFormat As ColorImageFormat In System.Enum.GetValues(GetType(ColorImageFormat))
				Select Case colorImageFormat
					Case ColorImageFormat.Undefined
					Case ColorImageFormat.RawYuvResolution640x480Fps15
					' don't add RawYuv to combobox.
					' That colorImageFormat works, but needs YUV->RGB conversion code which this sample doesn't have yet.
					Case Else
						colorFormats.Items.Add(colorImageFormat)
				End Select
			Next colorImageFormat

			For Each depthImageFormat As DepthImageFormat In System.Enum.GetValues(GetType(DepthImageFormat))
				Select Case depthImageFormat
					Case DepthImageFormat.Undefined
					Case Else
						depthFormats.Items.Add(depthImageFormat)
				End Select
			Next depthImageFormat

			For Each trackingMode As TrackingMode In System.Enum.GetValues(GetType(TrackingMode))
				trackingModes.Items.Add(trackingMode)
			Next trackingMode

			For Each depthRange As DepthRange In System.Enum.GetValues(GetType(DepthRange))
				depthRanges.Items.Add(depthRange)
			Next depthRange

			depthRanges.SelectedIndex = 0
		End Sub

		Friend Sub UpdateUiElevationAngleFromSensor()
			If Me.Kinect IsNot Nothing Then
				Me.userUpdate = False

				' If it's never been set, retrieve the value.
				If Me.lastSetSensorAngle = Integer.MaxValue Then
					Me.lastSetSensorAngle = Me.Kinect.ElevationAngle
				End If

				' Use the cache to prevent race conditions with the background thread which may 
				' be in the process of setting this value.
				Me.ElevationAngle.Value = Me.lastSetSensorAngle
				Me.userUpdate = True
			End If
		End Sub

		Private Sub ColorFormatsSelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
			Dim comboBox As ComboBox = TryCast(sender, ComboBox)
			If comboBox Is Nothing Then
				Return
			End If

			If Me.Kinect IsNot Nothing AndAlso Me.Kinect.Status = KinectStatus.Connected AndAlso comboBox.SelectedItem IsNot Nothing Then
				If Me.Kinect.ColorStream.IsEnabled Then
					Me.Kinect.ColorStream.Enable(CType(Me.colorFormats.SelectedItem, ColorImageFormat))
				End If
			End If
		End Sub

		Private Sub DepthFormatsSelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
			Dim comboBox As ComboBox = TryCast(sender, ComboBox)
			If comboBox Is Nothing Then
				Return
			End If

			If Me.Kinect IsNot Nothing AndAlso Me.Kinect.Status = KinectStatus.Connected AndAlso comboBox.SelectedItem IsNot Nothing Then
				If Me.Kinect.DepthStream.IsEnabled Then
					Me.Kinect.DepthStream.Enable(CType(Me.depthFormats.SelectedItem, DepthImageFormat))
				End If
			End If
		End Sub

		Private Sub SkeletonsChecked(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim checkBox As CheckBox = TryCast(sender, CheckBox)
			If checkBox Is Nothing Then
				Return
			End If

			If Me.Kinect IsNot Nothing AndAlso Me.Kinect.Status = KinectStatus.Connected AndAlso checkBox.IsChecked.HasValue Then
				Me.SetSkeletalTracking(checkBox.IsChecked.Value)
				Me.EnableDepthStreamBasedOnDepthOrSkeletonEnabled(Me.Kinect.DepthStream, Me.depthFormats)
			End If
		End Sub

		Private Sub SetSkeletalTracking(ByVal enable As Boolean)
			If enable Then
				If IsSkeletalViewerAvailable Then
					Me.Kinect.SkeletonStream.Enable()
					trackingModes.IsEnabled = True
					Me.DiagViewer.KinectSkeletonViewerOnColor.Visibility = Visibility.Visible
					Me.DiagViewer.KinectSkeletonViewerOnDepth.Visibility = Visibility.Visible
					SkeletonStreamEnable.IsChecked = True
				Else
					SkeletonStreamEnable.IsChecked = False
				End If
			Else
				Me.Kinect.SkeletonStream.Disable()
				trackingModes.IsEnabled = False

				' To ensure that old skeletons aren't displayed when SkeletonTracking
				' is reenabled, we ask SkeletonViewer to hide them all now.
				Me.DiagViewer.KinectSkeletonViewerOnColor.HideAllSkeletons()
				Me.DiagViewer.KinectSkeletonViewerOnDepth.HideAllSkeletons()
				Me.DiagViewer.KinectSkeletonViewerOnColor.Visibility = Visibility.Hidden
				Me.DiagViewer.KinectSkeletonViewerOnDepth.Visibility = Visibility.Hidden
			End If
		End Sub

		Private Sub TrackingModesSelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
			Dim comboBox As ComboBox = TryCast(sender, ComboBox)
			If comboBox Is Nothing Then
				Return
			End If

			If Me.Kinect IsNot Nothing AndAlso Me.Kinect.Status = KinectStatus.Connected AndAlso comboBox.SelectedItem IsNot Nothing Then
				Dim newMode As TrackingMode = CType(comboBox.SelectedItem, TrackingMode)
				Me.Kinect.SkeletonStream.AppChoosesSkeletons = newMode <> TrackingMode.DefaultSystemTracking
				Me.DiagViewer.KinectSkeletonViewerOnColor.TrackingMode = newMode
				Me.DiagViewer.KinectSkeletonViewerOnDepth.TrackingMode = newMode
			End If
		End Sub

		Private Sub DepthRangesSelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
			Dim comboBox As ComboBox = TryCast(sender, ComboBox)
			If comboBox Is Nothing Then
				Return
			End If

			If Me.Kinect IsNot Nothing AndAlso Me.Kinect.Status = KinectStatus.Connected AndAlso comboBox.SelectedItem IsNot Nothing Then
				Try
					Me.Kinect.DepthStream.Range = CType(comboBox.SelectedItem, DepthRange)
				Catch e1 As InvalidOperationException
					comboBox.SelectedIndex = 0
					comboBox.Items.RemoveAt(1)
					comboBox.Items.Add("-- NearMode not supported on this device. See Readme. --")
				Catch e2 As InvalidCastException
					' they chose the error string, switch back
					comboBox.SelectedIndex = 0
				End Try
			End If
		End Sub

		Private Sub ColorStreamEnabled(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim checkBox As CheckBox = CType(sender, CheckBox)
			Me.DisplayColumnBasedOnIsChecked(checkBox, 1, 2)
			Me.DisplayPanelBasedOnIsChecked(checkBox, Me.DiagViewer.colorPanel)
			If Me.Kinect IsNot Nothing Then
				Me.EnableColorImageStreamBasedOnIsChecked(checkBox, Me.Kinect.ColorStream, Me.colorFormats)
			End If
		End Sub

		Private Sub EnableDepthStreamBasedOnDepthOrSkeletonEnabled(ByVal depthImageStream As DepthImageStream, ByVal depthFormatsValue As ComboBox)
			If depthFormatsValue.SelectedItem IsNot Nothing Then
				' SkeletonViewer needs depth. So if DepthViewer or SkeletonViewer is enabled, enabled depthStream.
				If (DepthStreamEnable.IsChecked.HasValue AndAlso DepthStreamEnable.IsChecked.Value) OrElse (SkeletonStreamEnable.IsChecked.HasValue AndAlso SkeletonStreamEnable.IsChecked.Value) Then
					depthImageStream.Enable(CType(depthFormatsValue.SelectedItem, DepthImageFormat))
				Else
					depthImageStream.Disable()
				End If
			End If
		End Sub

		Private Sub EnableColorImageStreamBasedOnIsChecked(ByVal checkBox As CheckBox, ByVal imageStream As ColorImageStream, ByVal colorFormatsValue As ComboBox)
			If checkBox.IsChecked.HasValue AndAlso checkBox.IsChecked.Value Then
				imageStream.Enable(CType(colorFormatsValue.SelectedItem, ColorImageFormat))
			Else
				imageStream.Disable()
			End If
		End Sub

		Private Sub DepthStreamEnabled(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim checkBox As CheckBox = CType(sender, CheckBox)
			Me.DisplayColumnBasedOnIsChecked(checkBox, 2, 1)
			Me.DisplayPanelBasedOnIsChecked(checkBox, Me.DiagViewer.depthPanel)
			If Me.Kinect IsNot Nothing Then
				Me.EnableDepthStreamBasedOnDepthOrSkeletonEnabled(Me.Kinect.DepthStream, Me.depthFormats)
			End If
		End Sub

		Private Sub DisplayPanelBasedOnIsChecked(ByVal checkBox As CheckBox, ByVal panel As Grid)
			' on load of XAML page, panel will be null.
			If panel Is Nothing Then
				Return
			End If

			If checkBox.IsChecked.HasValue AndAlso checkBox.IsChecked.Value Then
				panel.Visibility = Visibility.Visible
			Else
				panel.Visibility = Visibility.Collapsed
			End If
		End Sub

		Private Sub DisplayColumnBasedOnIsChecked(ByVal checkBox As CheckBox, ByVal column As Integer, ByVal stars As Integer)
			If checkBox.IsChecked.HasValue AndAlso checkBox.IsChecked.Value Then
				Me.DiagViewer.LayoutRoot.ColumnDefinitions(column).Width = New GridLength(stars, GridUnitType.Star)
			Else
				Me.DiagViewer.LayoutRoot.ColumnDefinitions(column).Width = New GridLength(0)
			End If
		End Sub

		Private Sub ElevationAngleChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Double))
			If Me.userUpdate Then
				Me.debounce.Stop()
				Me.debounce.Start()
			End If
		End Sub

		Private Sub DebounceElapsed(ByVal sender As Object, ByVal e As EventArgs)
			' The time has elapsed.  We may start it again later.
			Me.debounce.Stop()

			Dim angleToSet As Integer = CInt(Fix(ElevationAngle.Value))

			' Is there an update in progress?
			If Me.backgroundUpdateInProgress Then
				' Try again in a few moments.
				Me.debounce.Start()
			Else
				Me.backgroundUpdateInProgress = True

				Task.Factory.StartNew(Sub()
								' Check for not null and running
									' We must wait at least 1 second, and call no more frequently than 15 times every 20 seconds
									' So, we wait at least 1350ms afterwards before we set backgroundUpdateInProgress to false.
					Try
						If (Me.Kinect IsNot Nothing) AndAlso Me.Kinect.IsRunning Then
							Me.Kinect.ElevationAngle = angleToSet
							Me.lastSetSensorAngle = angleToSet
							Thread.Sleep(1350)
						End If
					Finally
						Me.backgroundUpdateInProgress = False
					End Try
				End Sub).ContinueWith(Sub(results)
									' This can happen if the Kinect transitions from Running to not running
									' after the check above but before setting the ElevationAngle.
					If results.IsFaulted Then
						Dim exception = results.Exception
						Debug.WriteLine("Set Elevation Task failed with exception " & exception)
					End If
End Sub)
			End If
		End Sub
	End Class
End Namespace
