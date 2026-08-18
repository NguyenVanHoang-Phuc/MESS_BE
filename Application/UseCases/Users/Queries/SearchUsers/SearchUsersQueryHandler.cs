using AutoMapper;
using MediatR;
using MESS.Application.DTOs.Responses.Users;
using MESS.Domain.Interfaces;
using MESS.Domain.Shared;

namespace MESS.Application.UseCases.Users.Queries.SearchUsers;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, Result<IEnumerable<UserResponse>>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public SearchUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<UserResponse>>> Handle(
        SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.SearchUsersAsync(request.Query, request.CurrentUserId, request.Limit);
        var response = _mapper.Map<IEnumerable<UserResponse>>(users);
        return Result<IEnumerable<UserResponse>>.Success(response);
    }
}
