using MediatR;
using MESS.Application.DTOs.Responses.Users;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Users.Queries.SearchUsers;

public record SearchUsersQuery(string Query, Guid CurrentUserId, int Limit = 20) : IRequest<Result<IEnumerable<UserResponse>>>;
