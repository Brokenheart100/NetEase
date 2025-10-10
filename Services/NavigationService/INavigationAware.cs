using System;
using System.Collections.Generic;
using System.Text;

namespace NetEase.Services.NavigationService
{
    /// <summary>
    /// 定义一个契约，允许ViewModel在导航到它时接收参数。
    /// </summary>
    public interface INavigationAware
    {
        /// <summary>
        /// 当导航到此ViewModel时，由导航服务调用此方法。
        /// </summary>
        /// <param name="parameter">导航时传递的参数。</param>
        void OnNavigatedTo(object? parameter);
    }
}
