using System;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.ComponentBase;

public interface ISessionStorageService
{
    Task SetAsync(string key, string value);
    Task<string?> GetAsync(string key);
    Task RemoveAsync(string key);
    Task ClearAsync();
}
