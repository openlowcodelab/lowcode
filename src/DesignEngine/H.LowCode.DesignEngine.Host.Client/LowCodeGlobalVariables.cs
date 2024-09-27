using System.Reflection;

namespace H.LowCode.DesignEngine.Host.Client;

public static class LowCodeGlobalVariables
{
    public static readonly Type LowCodeDefaultLayout = typeof(Workbench.WorkbenchLayout);

    public static readonly Assembly[] AdditionalAssemblies =
        [
            typeof(Workbench._Imports).Assembly,
            typeof(DesignEngine._Imports).Assembly,
            typeof(PartsDesignEngine._Imports).Assembly,
            typeof(MyApp._Imports).Assembly
        ];
}
