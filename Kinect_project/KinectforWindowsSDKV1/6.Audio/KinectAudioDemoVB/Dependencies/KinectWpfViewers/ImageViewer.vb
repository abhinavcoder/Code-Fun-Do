Imports Microsoft.Kinect
Imports System.ComponentModel

'------------------------------------------------------------------------------
' <copyright file="ImageViewer.cs" company="Microsoft">
'     Copyright (c) Microsoft Corporation.  All rights reserved.
' </copyright>
'------------------------------------------------------------------------------

Namespace Microsoft.Samples.Kinect.WpfViewers

	Public MustInherit Class ImageViewer
		Inherits UserControl
		Implements INotifyPropertyChanged
		Public Shared ReadOnly StretchProperty As DependencyProperty = DependencyProperty.Register("Stretch", GetType(Stretch), GetType(ImageViewer), New UIPropertyMetadata(Stretch.Uniform))

		Public Shared ReadOnly KinectProperty As DependencyProperty = DependencyProperty.Register("Kinect", GetType(KinectSensor), GetType(ImageViewer), New UIPropertyMetadata(Nothing, New PropertyChangedCallback(AddressOf KinectChanged)))

'INSTANT VB NOTE: The variable flipHorizontally was renamed since Visual Basic does not allow class members with the same name:
		Private flipHorizontally_Renamed As Boolean
'INSTANT VB NOTE: The variable horizontalScaleTransform was renamed since Visual Basic does not allow class members with the same name:
		Private horizontalScaleTransform_Renamed As ScaleTransform
'INSTANT VB NOTE: The variable frameRate was renamed since Visual Basic does not allow class members with the same name:
		Private frameRate_Renamed As Integer = -1
'INSTANT VB NOTE: The variable collectFrameRate was renamed since Visual Basic does not allow class members with the same name:
		Private collectFrameRate_Renamed As Boolean
		Private lastTime As Date = Date.MaxValue

		Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

		Public Property Kinect() As KinectSensor
			Get
				Return CType(GetValue(KinectProperty), KinectSensor)
			End Get

			Set(ByVal value As KinectSensor)
				SetValue(KinectProperty, value)
			End Set
		End Property

		Public Property FlipHorizontally() As Boolean
			Get
				Return Me.flipHorizontally_Renamed
			End Get

			Set(ByVal value As Boolean)
				If Me.flipHorizontally_Renamed <> value Then
					Me.flipHorizontally_Renamed = value
					Me.NotifyPropertyChanged("FlipHorizontally")
					Me.horizontalScaleTransform_Renamed = New ScaleTransform With {.ScaleX = If(Me.flipHorizontally_Renamed, -1, 1)}
					Me.NotifyPropertyChanged("HorizontalScaleTransform")
				End If
			End Set
		End Property

		Public ReadOnly Property HorizontalScaleTransform() As ScaleTransform
			Get
				Return Me.horizontalScaleTransform_Renamed
			End Get
		End Property

		Public Property Stretch() As Stretch
			Get
				Return CType(GetValue(StretchProperty), Stretch)
			End Get
			Set(ByVal value As Stretch)
				SetValue(StretchProperty, value)
			End Set
		End Property

		Public Property CollectFrameRate() As Boolean
			Get
				Return Me.collectFrameRate_Renamed
			End Get

			Set(ByVal value As Boolean)
				If value <> Me.collectFrameRate_Renamed Then
					Me.collectFrameRate_Renamed = value
					Me.NotifyPropertyChanged("CollectFrameRate")
				End If
			End Set
		End Property

		Public Property FrameRate() As Integer
			Get
				Return Me.frameRate_Renamed
			End Get

			Private Set(ByVal value As Integer)
				If Me.frameRate_Renamed <> value Then
					Me.frameRate_Renamed = value
					Me.NotifyPropertyChanged("FrameRate")
				End If
			End Set
		End Property

		Protected Property TotalFrames() As Integer

		Protected Property LastFrames() As Integer

		Protected MustOverride Sub OnKinectChanged(ByVal oldKinectSensor As KinectSensor, ByVal newKinectSensor As KinectSensor)

		Protected Sub ResetFrameRateCounters()
			If Me.CollectFrameRate Then
				Me.lastTime = Date.MaxValue
				Me.TotalFrames = 0
				Me.LastFrames = 0
			End If
		End Sub

		Protected Sub UpdateFrameRate()
			If Me.CollectFrameRate Then
				Me.TotalFrames += 1

				Dim cur As Date = Date.Now
				Dim span = cur.Subtract(Me.lastTime)
				If Me.lastTime = Date.MaxValue OrElse span >= TimeSpan.FromSeconds(1) Then
					' A straight cast will truncate the value, leading to chronic under-reporting of framerate.
					' rounding yields a more balanced result
					Me.FrameRate = CInt(Fix(Math.Round((Me.TotalFrames - Me.LastFrames) / span.TotalSeconds)))
					Me.LastFrames = Me.TotalFrames
					Me.lastTime = cur
				End If
			End If
		End Sub

		Protected Sub NotifyPropertyChanged(ByVal info As String)
			RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(info))
		End Sub

		Private Shared Sub KinectChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
			Dim imageViewer As ImageViewer = CType(d, ImageViewer)
			imageViewer.OnKinectChanged(CType(args.OldValue, KinectSensor), CType(args.NewValue, KinectSensor))
		End Sub
	End Class
End Namespace
