using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API;

public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }
    
    /*[HttpPost(nameof(AddUser))]
    public async Task<UserSummary> AddUser([FromBody] CreateUserDAO newUser)
    {
        var user = await _userService.CreateUserAsync(
            newUser.UserName,
            newUser.Password,
            newUser.Role,
            //todo add CancellationToken);

        return user;
    }*/
    
    /*[HttpGet(nameof(GetUsers))]
    public async Task<List<UserSummary>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync(//todo add CancellationToken);
        return users;
    }*/
    /*[HttpDelete(nameof(DeleteUser))]
    public async Task<bool> DeleteUser([FromBody]string id)
    {
       return await _userService.DeleteUserAsync(id, //todo add CancellationToken);
    }*/
    /*[HttpPut(nameof(UpdateUser))]
    public async Task<bool> UpdateUser([FromBody] string id, UsersDB user)
    {
        return await _userService.UpdateUserAsync(id, user, //todo add CancellationToken);
    }*/
}