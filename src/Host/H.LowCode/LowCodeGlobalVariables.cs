using System.Reflection;

namespace H.LowCode;

public static class LowCodeGlobalVariables
{
    public static readonly Type LowCodeDefaultLayout = typeof(Workbench.WorkbenchLayout);

    public static readonly Assembly[] AdditionalAssemblies =
        [
            typeof(Workbench._Imports).Assembly,
            typeof(DesignEngine._Imports).Assembly,
            typeof(DesignEngine.Admin._Imports).Assembly,
            typeof(ThemeParts.AntBlazor._Imports).Assembly
        ];
}
