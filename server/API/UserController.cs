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
    
    
}