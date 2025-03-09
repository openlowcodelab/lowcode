﻿using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IComponentPartsAppService : IApplicationService
{
    Task<List<ComponentPartsListModel>> GetListAsync(string libraryId);

    Task<List<ComponentPartsSchema>> GetAllComponentsAsync(string libraryId);

    Task<ComponentPartsSchema> GetByIdAsync(string libraryId, string componentId);

    Task<bool> SaveAsync(ComponentPartsSchema componentParts);

    Task<bool> DeleteAsync(string libraryId, string componentId);
}
