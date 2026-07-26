using H.Notification.Application.Contracts;
using H.Notification.Application.Sending;
using Volo.Abp.Application.Services;

namespace H.Notification.Application;

public class NotificationSendAppService : ApplicationService, INotificationSendAppService
{
    private readonly NotificationDispatcher _dispatcher;

    public NotificationSendAppService(NotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public Task<SendNotificationResult> SendAsync(SendNotificationInput input)
    {
        return _dispatcher.DispatchAsync(input.BusinessCode, input.Level, input.Data, input.RecipientIds, "Api");
    }

    public Task<SendNotificationResult> TestSendAsync(TestSendInput input)
    {
        return _dispatcher.DispatchAsync(input.BusinessCode, input.Level, input.Data, input.RecipientIds, "Test");
    }
}
