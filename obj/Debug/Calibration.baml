   M S B A M L   `   `   `  ����D  @Eyeplayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:Eyeplayer	Eyeplayer  5   	       Eyeplayer.Calibration   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   )localclr-namespace:Eyeplayer     5   	   -   5        $��MainWindowInstance��5   	     � Title$  Calibration��5   	   ���	#FF383838 � WindowStyle$
 None=�5   	    � 
ResizeMode$ NoResize=�5	   	    � WindowState$ 	Maximized=�5
   	    � Topmost .  J� InputBindings 5      �� 5   
   
 �� Key$ Enter��69    �� Command 6   �� *ContinueCommand+69   �� 5   
   $ Escape��65    6   �� *ExitCommand+65    � 	Resources 5       $   ShrinkingCalibrationDotAnimation	 W� From	
 W� To g� Duration g� AutoReverse W� EasingFunction-  'System.Windows.Media.Animation.SineEase7  1System.Windows.Media.Animation.EasingFunctionBase   
EasingMode   Eyeplayer.ScalingConverter 
 Scaler(  "Eyeplayer.EnumDisplayNameConverter  PositioningStatusConverter   Items#  Eyeplayer.EnumDisplayNameItem!   Eyeplayer.PositioningStatus   TooClose   Value   DisplayName   TooFarOrNotDetected   
PositionOk  InitializingStageTemplate �� TextAlignment ! PositioningGuideStageTemplate �� Orientation �� 
TargetType �� Property �� Value `� 	Converter �� Path �� ElementName �� 	Converter  CalibrationStageTemplate @� RadiusX @� RadiusY  @� Center % !ComputingCalibrationStageTemplate " CalibrationFailedStageTemplate  FinishedStageTemplate 	 ErrorStageTemplate! �� Mode��%�  &	        &	 �     &	 �     &	 U    &	 �    7  7  7  &	 g    7  7  &	 �    &	 �	    &	 /
    &		 �
    W� 5   
   -   5        $		 3.0X�5      $

 25.0X�5      $ 0:0:2D�5       .   5        5      $ 	EaseInOut=�6   5        5    
   6!     5#   
   . 5$        # Z 6(   $ 	Too Close��6\     5%      # Z 6(   $ Too Far��6g     5&      # Z 6(   $ please wait...��6^   5'      �� 5*   
   .R�5+      �� $ Center=�6   $��Stretch=�6/   $��Center=�6M   Initializing, please wait...6�   5,      �� 5/   
   .R�50      �� $ Vertical=�6   $��Center=�60   .+�51      �� $	��800��6D   $	��600��6P   ���#101010��6   �� *EyePositions+��52      �� 53      .R�54      �� $��PositioningGuideCanvas��6%   $��10 10 30 30q�6C   55      ��57      �� 58      " ��6    .�59      ��  �@��Left 5:   "   `� 5;   &   8   .1�5<   *   �� $ X�62   �� 5=   *   $ PositioningGuideCanvas��62   $ ActualWidth�6W   5>   '   5@      �� 5A       �@��Top 5B   "   `� 5C   &   8  .1�5D   *   �� $ Y�62   �� 5E   *   $ PositioningGuideCanvas��62   $ ActualHeight�6W   5F   '   5H      5I      ��5K      �� 5L      .R�5M      A� $��50��6&   $��50��61   
�������6=   5N      5P      �� 5Q      $��10q�6v   $��Center=�6�   $��Center=�6�   ��6   �� *PositioningStatus+8  6�   5R      5S      �� 5V   
   .R�5W      �� $��CalibrationCanvas��6   .+�5X      F� 
�������$��Stretch=�6$   $��Stretch=�6B   ��5Y      @� 5Z      Ellipse�� 6;   �� $ MainWindowInstance��$ CalibrationDotRadius� 6�   �� $ MainWindowInstance��$ CalibrationDotRadius�  5[      `� 5\   "   8   .1�5]   &   �� $ CalibrationDotPosition�6.   �� 5^   &   $ CalibrationCanvas��6.   $ ActualWidth�6N   �� 5_   &   $ CalibrationCanvas��6.   $ ActualHeight�6N   5`   #   5b      5d      F� 5e      
���   �$��Stretch=�6$   $��Stretch=�6B   ��5f      @� 5g      $ 3Q�6*   $ 3Q�66     5h      `� 5i   "   8  .1�5j   &   �� $ CalibrationDotPosition�6.   �� 5k   &   $ CalibrationCanvas��6.   $ ActualWidth�6N   �� 5l   &   $ CalibrationCanvas��6.   $ ActualHeight�6N   5m   #   5o      5q      5r      5s      �� 5v   
   .R�5w      �� $ Center=�6   $��Stretch=�6/   $��Center=�6M   '%Computing calibration, please wait...6�   5x      �� 5{   
   .R�5|      �� $ Center=�6   $��Stretch=�6/   $��Center=�6M   B@Calibration failed. Press Enter to try again, or escape to exit.6�   5}      �� 5�   
   .R�5�      �� $ Center=�6   $��Stretch=�6/   $��Center=�6M   Done! Press Enter to exit.6�   5�      �� 5�   
   .R�5�      �� $ Center=�6   $��Stretch=�6/   $��Center=�6M   �� 5�      �6   �� *ErrorMessage+$! OneWay=� Press Enter to exit.5�      5�      5�      .��5�      �� ��6   �� ��5�   
   �� 5�      " ��6   " �� Triggers" 5�      �� 5�      # �� Binding# 6"   �� *Stage+$ �� Value$ Initializing6<   .a�5�      ��  � ��# [ 65   5�      �� 5�      # 6"   �� *Stage+$ PositioningGuide6<   .a�5�      ��  � ��# [ 65   5�      �� 5�      # 6"   �� *Stage+$ Calibration6<   .a�5�      ��  � ��# [ 65   5�      �� 5�      # 6"   �� *Stage+$ ComputingCalibration6<   .a�5�      ��  � ��# [ 65   5�      �� 5�      # 6"   �� *Stage+$ CalibrationFailed6<   .a�5�      ��  � ��# [ 65   5�      �� 5�      # 6"   �� *Stage+$ Finished6<   .a�5�      ��  � ��# [ 65   5�      �� 5�      # 6"   �� *Stage+	$ Error6<   .a�5�      ��  � ��# [	 65   5�      5�      5�      5�      