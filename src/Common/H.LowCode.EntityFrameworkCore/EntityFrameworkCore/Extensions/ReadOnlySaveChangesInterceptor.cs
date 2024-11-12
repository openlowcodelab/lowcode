using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.EntityFrameworkCore;

public class ReadOnlySaveChangesInterceptor : SaveChangesInterceptor
{
    private void DetectReadOnly(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries().Where(e => IsReadOnly(context, e.Entity) && (e.State == EntityState.Deleted || e.State == EntityState.Modified || e.State == EntityState.Added)))
        {
            throw new InvalidOperationException($"Entity {entry.Entity.GetType()} is marked as read-only.");
        }
    }

    private bool IsReadOnly(DbContext context, object entity)
    {
        if (context.Entry(entity).Metadata.FindAnnotation("Custom:ReadOnly")?.Value is bool readOnly)
        {
            return readOnly;
        }
        return false;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        DetectReadOnly(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        DetectReadOnly(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}