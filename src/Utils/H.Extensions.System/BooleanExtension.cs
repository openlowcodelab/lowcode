namespace System;

public static class BooleanExtension
{
    public static bool IsTrue(this bool? bol)
    {
        if (bol == null)
            return false;
        return bol.Value;
    }
}
