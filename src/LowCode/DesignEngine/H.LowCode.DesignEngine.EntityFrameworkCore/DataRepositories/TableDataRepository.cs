using H.LowCode.DesignEngine.Domain;

namespace H.LowCode.DesignEngine.EntityFrameworkCore;

public class TableDataRepository : ITableDataRepository
{
    public bool? IsChangeTrackingEnabled => true;

    public string? EntityName { get; set; }

    public string ProviderName => throw new NotImplementedException();

    public TableDataRepository(DesignEngineDbContext dbContext)
    {
    }
}
