using MediatR;
using MESS.Application.DTOs.Responses.Users;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Users.Queries.GetAllUsers;

public class GetAllUsersQuery : IRequest<Result<IEnumerable<UserResponse>>>
{
}
