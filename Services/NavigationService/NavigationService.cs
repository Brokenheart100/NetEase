// File: NetEase\Services\NavigationService\NavigationService.cs
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using NetEase.ViewModels;
using System;
using System.Collections.Generic;

namespace NetEase.Services.NavigationService
{
    public class NavigationService : ObservableObject, INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Stack<BaseViewModel> _history = new();
        private BaseViewModel _currentView;

        public BaseViewModel CurrentView
        {
            get => _currentView;
            private set
            {
                if (SetProperty(ref _currentView, value))
                {
                    OnPropertyChanged(nameof(CanGoBack));
                }
            }
        }

        public bool CanGoBack => _history.Count > 0;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private void SetCurrentView(BaseViewModel viewModel)
        {
            if (CurrentView != null)
            {
                _history.Push(CurrentView);
            }
            CurrentView = viewModel;
        }

        // --- 【核心修改】实现新的导航逻辑 ---
        private void NavigateTo(Type viewModelType, object? parameter)
        {
            if (viewModelType == null || !typeof(BaseViewModel).IsAssignableFrom(viewModelType))
                return;

            // 1. 从DI容器创建ViewModel实例
            var nextViewModel = (BaseViewModel)_serviceProvider.GetRequiredService(viewModelType);

            // 2. 检查ViewModel是否能接收参数
            if (nextViewModel is INavigationAware navigationAwareViewModel)
            {
                // 3. 如果能，就调用接口方法传递参数
                navigationAwareViewModel.OnNavigatedTo(parameter);
            }

            // 4. 执行实际的视图切换
            SetCurrentView(nextViewModel);
        }

        // --- 【修改】更新所有公开的导航方法 ---
        public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            NavigateTo(typeof(TViewModel), null); // 调用新逻辑，参数为null
        }

        // 【新增】泛型带参版本
        public void NavigateTo<TViewModel>(object? parameter) where TViewModel : BaseViewModel
        {
            NavigateTo(typeof(TViewModel), parameter);
        }

        public void NavigateTo(Type viewModelType)
        {
            NavigateTo(viewModelType, null); // 调用新逻辑，参数为null
        }

        void INavigationService.NavigateTo(Type viewModelType, object? parameter)
        {
            NavigateTo(viewModelType, parameter);
        }

        public void NavigateToViewModel(BaseViewModel viewModel)
        {
            if (viewModel == null) return;
            SetCurrentView(viewModel);
        }

        public void GoBack()
        {
            if (_history.Count > 0)
            {
                CurrentView = _history.Pop();
            }
        }
    }
}