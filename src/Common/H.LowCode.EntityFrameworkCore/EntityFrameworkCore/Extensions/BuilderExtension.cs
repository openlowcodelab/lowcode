using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.EntityFrameworkCore;

public static class BuilderExtensions
{
    public static EntityTypeBuilder IsReadOnly(this EntityTypeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        var metadata = builder.Metadata;
        metadata.AddAnnotation("Custom:ReadOnly", false);
        return builder;
    }

    public static EntityTypeBuilder IsReadOnly(this EntityTypeBuilder builder, bool readOnly = true)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        var metadata = builder.Metadata;
        var props = metadata.GetProperties();

        foreach (var prop in props)
        {
            prop.SetAfterSaveBehavior(readOnly ? PropertySaveBehavior.Throw : PropertySaveBehavior.Save);
        }

        return builder;
    }

    public static PropertyBuilder IsReadOnly(this PropertyBuilder prop, bool readOnly = true)
    {
        ArgumentNullException.ThrowIfNull(prop, nameof(prop));
        prop.Metadata.SetAfterSaveBehavior(readOnly ? PropertySaveBehavior.Throw : PropertySaveBehavior.Save);
        return prop;
    }
}