using ChronosAPI.Helpers;
using ChronosAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChronosAPI.Controllers
{
    [Authorize]
    [ApiController]
    public class UsersMembersController : ControllerBase
    {
        private IUserService _userService;

        public UsersMembersController(IUserService userService)
        {
            _userService = userService;
        }

        [Route("api/users/memberinfo")]
        [HttpGet]
        public IActionResult GetUsers()
        {
            try
            {
                var response = _userService.GetUsers();
                if (response == null)
                {
                    return BadRequest(new { message = "Users Fetch Failed... It's on us." });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message});
            }
        }


    }
}
