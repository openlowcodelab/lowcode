﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace H.LowCode.RenderEngine.Domain;

public class TableDataDomainService : DomainService, ITableDataDomainService
{
    public TableDataDomainService(ITableDataRepository tableDataRepository)
    {

    }
}
