   M S B A M L   `   `   `  ????B  >UserApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:UserApp.ViewsUserApp.Views  5      !clr-namespace:UserAppUserApp  5          UserApp.Views.MessageView   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:UserApp.Views     5      &rootclr-namespace:UserApp     5      -   5        $??UserControll??5
        ? 	Resources  5         UserApp.BoolToVisibility   VisibilityConverter??%   &	          5   
   6    5      .??5      ? $??Center=?6    ? ColumnDefinitions 5   
   ?? 5      ?? 5      .+?5   
   ? $??0??5      y?5      ??  ?? Path$ IsOthersMessage? ?? 	Converter# [  ??5      ?? $ 	HalfWidth?.+?5      ?? $??1q?6   $??0??6)   $??2??66    ?? CornerRadius$ 10,10,10,10??5      
??????????#1c34485      ?? 5      $??NoWrap=?6   $??5,0,5,15q?6.   
???????$
??bold"?5      ??5      ?? $ MessageModel.User.Name?5      ?? 5      $??WrapWithOverflow=?6   $??5,10,5,5q?68   
?????????5      ?? $ MessageModel.Text?5       ? 5!   
   $??1??5"      y?5!      ?? $ IsMyMessage?# [  ??5"      ?? $ 	HalfWidth?.+?5$      ?? $??1q?6   $??0??6)   $??2??66   $ 10,10,10,10??5%      
??????????	#FF5A9AE05&      ?? 5'      $??NoWrap=?6   $??5,0,5,15q?6.   
???????$
??bold"?5)      ??5(      ?? $ MessageModel.User.Name?5)      ?? 5*      $??WrapWithOverflow=?6   $??5,10,5,5q?68   
?????????5+      ?? $ MessageModel.Text?5-      5.      5/      