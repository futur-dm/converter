   M S B A M L   `   `   `  ����  	converter/    )CurrencyConverter.Views.BankDetailsWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Адреса банков��5   	   $	��500��6   $	��600��6,   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5	      $
��Auto �6   .+�5   
   �� $��0��6   
������$��10q�66    �� Padding$ 10q�6B    �� CornerRadius$ 5��6O   ._�5      �� .+�5      �� $
��Bold"�6?   $��16(�6Q   ��6   �� *BestRate.BankName+6Q   � 5       � ColumnDefinitions 5      �� 5      $��* �6+   �� 5      $��* �6+   .+�5      �� $��0��6!   .+�5      �� $��Доллар США (USD)��6$   $
��Bold"�6<   �� 5      ��6$   �� *BestRate.UsdBuy+ �� StringFormat$$ Покупка: {0:N2} руб.���� 5      ��6$   �� *BestRate.UsdSell+$$ Продажа: {0:N2} руб.��5      �� 5      $��1��6!   .+�5      �� $��Евро (EUR)��6$   $
��Bold"�66   �� 5      ��6$   �� *BestRate.EurBuy+$$ Покупка: {0:N2} руб.���� 5      ��6$   �� *BestRate.EurSell+$$ Продажа: {0:N2} руб.��5      5      5       5!      �� 5$   
   $��1��6   $��10q�6$   .��5%      �� ��6   �� *FilteredAddresses+��5&      �� 5'      .R�5(      �� 
�������$��1q�64   $	��0,5q�6H   $ 10q�6U   $ 5��6b   ._�5)      �� .+�5*   "   �� $
��Bold"�6F   $��14(�6X   ��6,   �� *
BankName+6X   �� 5+   "   $
��10,0q�6Q   ��6/   �� *	Addresses+��5,   &   �� 5-   *   .R�5.   .   �� $	��0,2q�6I   ��68   �� 6I   5/   +   51   #   52      53      54      56      57      �� 5:   
   $��2��6   $ ��Назад к курсам��6   $��10q�5;      $��5q�6   $��Center=�5<      ��5=      �� *BackCommand+5>      5?      