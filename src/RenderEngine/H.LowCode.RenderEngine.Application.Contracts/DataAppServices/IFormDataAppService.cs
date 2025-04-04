﻿using Volo.Abp.Http;
using Volo.Abp.Application.Services;

namespace H.LowCode.RenderEngine.Application.Contracts;

public interface IFormDataAppService : IApplicationService
{
    Task<bool> SaveAsync(FormDataDTO dto);

    Task<FormDataDTO> GetAsync(string appId, string pageId, string id);

    Task<bool> DeleteAsync(string appId, string pageId, string id);
}