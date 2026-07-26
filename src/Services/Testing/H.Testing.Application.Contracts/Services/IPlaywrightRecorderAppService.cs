using System;
using System.Collections.Generic;
using System.Text;
using H.Abp.Application.Contracts;

namespace H.Testing.Application.Contracts;

public interface IPlaywrightRecorderAppService : IAppService
{
    Task<StartRecordingResponse> StartRecordingAsync(string startUrl);

    Task<StopRecordingResponse> StopRecordingAsync(string sessionId);
}
