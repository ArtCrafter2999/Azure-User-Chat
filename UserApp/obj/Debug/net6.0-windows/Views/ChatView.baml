   M S B A M L   `   `   `  ????B  >UserApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:UserApp.ViewsUserApp.Views  5      !clr-namespace:UserAppUserApp  5          UserApp.Views.ChatView   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:UserApp.Views     5      &rootclr-namespace:UserApp     5      $??56??5	      $	??250??6+     ? 	Resources  5
         UserApp.BoolToVisibility   VisibilityConverter??%   &	          5   
   6    5      .??5      ? .+?5   
   ?? $??Stretch=?6+   $??Stretch=?6P   ??6   ?? *Click+??6s   ?? *Color+.??5      ? .+?5      ?? $	??Top=?6O   
???????$??15(?6z   $??DemiBold"?6?   $??
10,12,30,0q?6?   $??20??6?   ??6   ??  ?? Path$ ChatModel.Title? ?? Mode$ OneWay=?6?   ?? 5       ?? TextAlignment$ Right=?6q   $??16??6?   $	??Top=?6?   
???????$??12(?6?   $??	10,2,20,0q?6?   ??6   ?? $ ChatModel.LastTime?$ OneWay=? ?? StringFormat$ dd.MM.yy HH:mm??6?   ?? 5      $
 Left=?6O   $??16??6d   $??Bottom=?6p   
???????$??12(?6?   $??	10,0,10,0q?6?   ??6   ?? $ LastMessageView?$ OneWay=?6?   ?? 5      $??15??6p   $??15??6{   $??211,17,20,20q?6?   y?6   ?? $ 
IsUnreaded? ?? 	Converter# [  .+?5      A? $??15??6   $??15??6)   ???#2a4e6c65   ?? 5      
???????$
??Left=?6R    ?? Left$ 1??6m   $??Center=?6}   ??6    ?? $ Unreaded?6}   5      ?? 5      $??8??6n   $??8??6x   $??
0,0,231,37q?6?   y?6   ?? $ IsOnline?# [  .+?5      A? $??8??6   $??8??6(   
???????63   5      5      5      5      5      