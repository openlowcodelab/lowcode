using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

public interface IPlaywrightRecorderAppService : IAppService
{
    Task<BaseOutput<StartRecordingResponse>> StartRecordingAsync(string startUrl);

    Task<BaseOutput<StopRecordingResponse>> StopRecordingAsync(string sessionId);
}
