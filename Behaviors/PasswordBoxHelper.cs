using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NetEase.Behaviors
{
    public static class PasswordBoxHelper
    {
        // 1. 定义一个附加属性 BoundPassword
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged)
                {
                    BindsTwoWayByDefault = true // 关键：让这个附加属性支持双向绑定
                });

        // 附加属性的 Get/Set 方法
        public static string GetBoundPassword(DependencyObject d)
        {
            return (string)d.GetValue(BoundPasswordProperty);
        }

        public static void SetBoundPassword(DependencyObject d, string value)
        {
            d.SetValue(BoundPasswordProperty, value);
        }

        // 2. 当附加属性的值改变时，更新 PasswordBox 的 Password
        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                // 防止事件处理程序造成无限循环
                if (GetIsUpdating(passwordBox)) return;

                passwordBox.Password = (string)e.NewValue;
            }
        }

        // 3. 监听 PasswordBox 的 PasswordChanged 事件，以更新附加属性
        //    我们通过另一个附加属性来附加/分离事件处理器
        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false, Attach));

        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

        private static void Attach(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                if ((bool)e.OldValue)
                {
                    passwordBox.PasswordChanged -= HandlePasswordChanged;
                }
                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += HandlePasswordChanged;
                }
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                SetIsUpdating(passwordBox, true);
                SetBoundPassword(passwordBox, passwordBox.Password);
                SetIsUpdating(passwordBox, false);
            }
        }

        // 用于防止无限循环的辅助附加属性
        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxHelper));

        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }
    }
}
