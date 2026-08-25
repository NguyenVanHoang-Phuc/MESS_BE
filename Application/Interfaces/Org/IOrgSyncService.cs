using System;
using System.Threading;
using System.Threading.Tasks;
using MESS.Application.DTOs.Responses.Org;

namespace MESS.Application.Interfaces.Org;

public interface IOrgSyncService
{
    Task<SyncOrgResultDto> SyncAllOrgGroupsAsync(CancellationToken cancellationToken = default);
    Task OnUserDepartmentOrShiftChangedAsync(Guid userId, Guid? oldDeptId, Guid? newDeptId, Guid? oldShiftId, Guid? newShiftId, CancellationToken cancellationToken = default);
    Task OnUserDeactivatedAsync(Guid userId, CancellationToken cancellationToken = default);
    Task EnsureDepartmentGroupAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task EnsureWorkShiftGroupAsync(Guid workShiftId, CancellationToken cancellationToken = default);
}
