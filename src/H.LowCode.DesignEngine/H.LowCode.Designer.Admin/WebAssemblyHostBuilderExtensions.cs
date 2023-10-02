using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Designer.Admin
{
    public static class WebAssemblyHostBuilderExtensions
    {
        public static void AddDesignerAdmin(this WebAssemblyHostBuilder builder)
        {
            builder.Services.AddAntDesign();
        }
    }
}
