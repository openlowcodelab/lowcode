using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Services;

namespace H.Testing.Application.Contracts;

public interface IPlaywrightRecorderAppService : IApplicationService
{
    Task<StartRecordingResponse> StartRecordingAsync(string startUrl);

    Task<StopRecordingResponse> StopRecordingAsync(string sessionId);
}
