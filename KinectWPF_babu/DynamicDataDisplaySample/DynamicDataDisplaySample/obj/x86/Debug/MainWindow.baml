   M S B A M L   `   `   `  ����  DynamicDataDisplaySample)    #DynamicDataDisplaySample.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Y UDynamicDataDisplay, Version=0.3.0.0, Culture=neutral, PublicKeyToken=5b7d744a7263923f=d34http://research.microsoft.com/DynamicDataDisplay/1.0  5   	     � Title$  
MainWindow��5   	   $	��900��6   $
��1600��6)   .��5      � $��	-1,0,61,0q�6   $	��542��6   $	��Top=�6+   .+�5   
   8  2Microsoft.Research.DynamicDataDisplay.ChartPlotter  -   5          Name$ plotter��5      $��24,94,801,44q�6+      HorizontalAxis 5      I  CMicrosoft.Research.DynamicDataDisplay.Charts.HorizontalDateTimeAxis  5	      -   5        $��dateAxis��5	   ,   3  -Microsoft.Research.DynamicDataDisplay.Plotter   Children. 5      2  ,Microsoft.Research.DynamicDataDisplay.Header  $��Georgia)�6   $��SQUATTING VS. TIME��6-   =  7Microsoft.Research.DynamicDataDisplay.VerticalAxisTitle  5      $��Georgia)�6#   $%��AVG.SQUAT COUNT PER 0.5-SECONDS��68   ?  9Microsoft.Research.DynamicDataDisplay.HorizontalAxisTitle  5      $��Georgia)�6%   $
��Time��6:   $��370,0,368.2,0q�6I   A  ;Microsoft.Research.DynamicDataDisplay.Charts.HorizontalLine  5      
���  ��$��2��6J   =  7Microsoft.Research.DynamicDataDisplay.Charts.SimpleLine   Value 6    �� *
MaxVoltage+6J     5      
���  ��$��2��6J    6    �� *
MinVoltage+6J   5      �� 5   
   -   5         ��Name$ t1��5      $
��Left=�6   $��23��69   $��257,519,0,0q�6E    �� TextWrapping$
 Wrap=�6Z   $	��Top=�6n   $	��120��6�   �� 5   
   -   5         ��Name$ button1��5      $��Button��6"   $
��Left=�63   $��446,519,0,0q�6N   $	��Top=�6c   $��75��6{   $��23��6�   6�   �� 5   
   -   5         ��Name$ Message��5      $��Center=�6&   $	��Top=�6C   $��30��6[   
��� � �
���  ��$��0,0,0,462.8q�6�   �� 5   
   -   5        $��Image��5      $��780,131,83,-98q�6   �� 5   
   -   5        $��tb��5      $
��Left=�6   $��23��68   $��542,519,0,0q�6D   $
 Wrap=�6Y   $	��Top=�6m   $	��238��6�   6�   �� 5   
   -   5        $	��tb2��5      $
��Left=�6    $��542,488,0,0q�6;   $
��Wrap=�6P   $	��Top=�6d   $��25��6|   $	��238��6�   5      5      