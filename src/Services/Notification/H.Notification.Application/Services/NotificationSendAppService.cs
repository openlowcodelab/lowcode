using H.Notification.Application.Contracts;
using H.Notification.Application.Sending;
using H.Util.Base;
using Volo.Abp.Application.Services;

namespace H.Notification.Application;

public class NotificationSendAppService : ApplicationService, INotificationSendAppService
{
    private readonly NotificationDispatcher _dispatcher;

    public NotificationSendAppService(NotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task<BaseOutput<SendNotificationResult>> SendAsync(SendNotificationInput input)
    {
        var result = await _dispatcher.DispatchAsync(input.BusinessCode, input.Level, input.Data, input.RecipientIds, "Api");
        return BaseOutput<SendNotificationResult>.Ok(result);
    }

    public async Task<BaseOutput<SendNotificationResult>> TestSendAsync(TestSendInput input)
    {
        var result = await _dispatcher.DispatchAsync(input.BusinessCode, input.Level, input.Data, input.RecipientIds, "Test");
        return BaseOutput<SendNotificationResult>.Ok(result);
    }
}
