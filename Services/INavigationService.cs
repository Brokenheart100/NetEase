using System;
using System.Collections.Generic;
using System.Text;

namespace NetEase.Services
{
    public interface INavigationService
    {
        void Navigate(Type pageType);
    }
}
