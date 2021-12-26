using ChronosAPI.Helpers;
using ChronosAPI.Models;
using ChronosAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChronosAPI.Controllers
{
    [ApiController]
    public class UsersMembersController : ControllerBase
    {
        private IUserService _userService;

        public UsersMembersController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
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

        [Route("api/users/reset-password")]
        [HttpPost]
        public IActionResult ResetUserPassword(ResetPasswordModel resetPasswordModel)
        {
            try
            {
                var response = _userService.UpdateUserPassword(resetPasswordModel);
                if (response == null)
                {
                    return BadRequest(new { message = "User Reset Password Failed... It's on us." });
                }
                if(response.StatusCode == 404)
                {
                    return BadRequest(new { message = "NO User with this email! Check for typos" });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
