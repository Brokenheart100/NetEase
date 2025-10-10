using NetEase.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetEase.Services.NavigationService
{
    public interface INavigationService
    {
        BaseViewModel CurrentView { get; }

        /// <summary>
        /// 检查是否可以执行后退操作
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// 导航到一个新的 ViewModel 实例 (通过泛型)
        /// </summary>
        void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;

        /// <summary>
        /// 导航到一个新的 ViewModel 实例 (通过 Type)
        /// </summary>
        void NavigateTo(Type viewModelType);

        /// <summary>
        /// 导航到一个已经存在的、配置好的 ViewModel 实例
        /// </summary>
        /// <param name="viewModel">要导航到的 ViewModel 实例</param>
        void NavigateToViewModel(BaseViewModel viewModel);

        /// <summary>
        /// 执行后退操作
        /// </summary>
        void GoBack();
        /// <summary>
        /// 导航到一个新的 ViewModel 实例，并传递一个参数。
        /// </summary>
        /// <typeparam name="TViewModel">目标 ViewModel 的类型。</typeparam>
        /// <param name="parameter">要传递给 ViewModel 的参数。</param>
        void NavigateTo<TViewModel>(object? parameter) where TViewModel : BaseViewModel;

        /// <summary>
        /// 导航到一个新的 ViewModel 实例 (通过 Type)，并传递一个参数。
        /// </summary>
        /// <param name="viewModelType">目标 ViewModel 的类型。</param>
        /// <param name="parameter">要传递给 ViewModel 的参数。</param>
        void NavigateTo(Type viewModelType, object? parameter);
    }
}
