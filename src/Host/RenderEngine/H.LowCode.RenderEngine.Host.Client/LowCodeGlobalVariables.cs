using H.LowCode.RenderEngine;
using System.Reflection;

namespace H.LowCode.RenderEngine.Host.Client;

public static class LowCodeGlobalVariables
{
    public static readonly Type LowCodeDefaultLayout = typeof(AntBlazorThemeLayout);

    public static readonly Assembly[] AdditionalAssemblies =
        [
            typeof(H.LowCode.RenderEngine._Imports).Assembly
        ];
}
