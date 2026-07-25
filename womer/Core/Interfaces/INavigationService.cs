using System;
using System.Collections.Generic;
using System.Text;

namespace womer.Core.Interfaces
{
    public interface INavigationService
    {
        Task<bool> GoToAsync(string route, bool animate = true);
    }
}
