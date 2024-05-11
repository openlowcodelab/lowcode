using System.Reflection;

namespace H.LowCode
{
    public static class LowCodeGlobalVariables
    {
        public static readonly Type LowCodeDefaultLayout = typeof(Admin.Layout.ProAdminLayout);

        public static readonly Assembly[] AdditionalAssemblies =
            [
                typeof(Admin._Imports).Assembly,
                typeof(Theme.AntBlazorPro._Imports).Assembly,
                typeof(DesignEngine._Imports).Assembly
            ];
    }
}
