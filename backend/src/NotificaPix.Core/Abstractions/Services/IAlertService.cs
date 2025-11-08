using NotificaPix.Core.Contracts.Requests;
using NotificaPix.Core.Contracts.Responses;

namespace NotificaPix.Core.Abstractions.Services;

public interface IAlertService
{
    Task<AlertDto> DispatchTestAlertAsync(Guid organizationId, AlertTestRequest request, CancellationToken cancellationToken);
}
