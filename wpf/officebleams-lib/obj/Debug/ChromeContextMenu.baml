   M S B A M L   `   `   `  ����K  Gofficebleams-lib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null(    "officebleams_lib.ChromeContextMenu   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -   5          >�Name$  chromeContextMenu��5       � 	Resources 5	         onHighlight �� TargetProperty �� 
TargetName8  2System.Windows.Media.Animation.EasingColorKeyFrame �� KeyTime �� Value   EasingFunction.  (System.Windows.Media.Animation.CubicEase7  1System.Windows.Media.Animation.EasingFunctionBase   
EasingMode  offHighlight  onHighlight2  offHighlight2��%�  &	        &	 j    &	 f    &	 �    �� 5
   
   .��5      �� $4 .(TextBlock.Background).(SolidColorBrush.Color)�6+   $ minimizeButton��5   +   .g�5        $ 0��6&   $ Transparent��5   &    5        5      $ EaseOut=�6$   5        5      $ 0:0:0.5��6&   $ 	#19FFFFFF��5   &    5        5      $ EaseOut=�6$   5      5      5      �� 5   
   .��5      �� $4 .(TextBlock.Background).(SolidColorBrush.Color)�6+   $ minimizeButton��5   +   .g�5      �� $ 0��6&   $ 	#19FFFFFF��5   &   �� 5       $ 0:0:0.2��6&   $ Transparent��5!   &   5"      5#      �� 5$   
   .��5%      �� $4 .(TextBlock.Background).(SolidColorBrush.Color)�6+   $ closeButton��5&   +   .g�5'        $ 0��6&   $ Transparent��5(   &    5)        5*      $ EaseOut=�6$   5,        5-      $ 0:0:0.5��6&   $ 	#19FFFFFF��5.   &    5/        50      $ EaseOut=�6$   52      53      54      �� 55   
   .��56      �� $4 .(TextBlock.Background).(SolidColorBrush.Color)�6+   $ closeButton��57   +   .g�58      �� $ 0��6&   $ 	#19FFFFFF��59   &   �� 5:      $ 0:0:0.2��6&   $ Transparent��5;   &   5<      5=      5>       � Triggers 5?      :� 5@   
   	 :� RoutedEvent$	 UIElement.MouseEnter��6   
 :� 
SourceName$
 minimizeButton��6:   .[�5A      �� -   5         ��Name$! onHighlight_BeginStoryboard��5A      #��[  5B      5C      :� 5D   
   $	 UIElement.MouseLeave��6   $
 minimizeButton��6:   .[�5E      �� -   5        $" offHighlight_BeginStoryboard��5E      #��[ 5F      5G      :� 5H   
   $	 UIElement.MouseEnter��6   $
 closeButton��6:   .[�5I      �� -   5        $" onHighlight2_BeginStoryboard��5I      #��[ 5J      5K      :� 5L   
   $	 UIElement.MouseLeave��6   $
 closeButton��6:   .[�5M      �� -   5        $# offHighlight2_BeginStoryboard��5M      #��[ 5N      5O      .��5Q      �   baseChromeBrush#��[ 6   $��80��5R      $��10q�5S       J� Effect 5T   
   3  -System.Windows.Media.Effects.DropShadowEffect  5U         
BlurRadius$ 10Q�6      	Direction$ 0Q�5V         ShadowDepth$ 0Q�5W         RenderingBias$ Quality=�5X      .+�5Z   
   �� $	��0,5q�6   .+�5[      �� -   5         ��Name$ minimizeButton��5[        textControlStyle#��[ 5\      $��Minimize��5]      $��13(�5^      
������  �� Padding$ 10,10q�5`      5a      �� 5b      -   5        $ closeButton��5b      #��[ 5c      $��Close��5d      $��13(�5e      
������ $ 10,10q�5g      5h      5i      5j      5k      