using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronosAPI.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using ChronosAPI.Services;
using ChronosAPI.Helpers;
using ChronosAPI.Exceptions;

namespace ChronosAPI.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [Route("api/auth/login/jwt")]
        [HttpPost]
        public IActionResult JwtLogin(AuthenticateJwtRequest authJwtReq)
        {
            try
            {

                var response = _userService.AuthenticateJwt(authJwtReq.jwt);
                if (response == null)
                {
                    return BadRequest(new { message = "Login failed... It's on us." });
                }
                return Ok(response);
            }
            catch(CredentialsEmptyException)
            {
                return BadRequest(new { message = "Credentials Empty. No Jwt token found in request body." });
            }
            catch (UserBadJwtException)
            {
                return BadRequest(new { message = "Invalid JWT. Login again." });
            }
        }

        [Route("api/auth/login")]
        [HttpPost]
        public IActionResult Login(AuthenticateRequest req)
        {
            try
            {
                var response = _userService.Authenticate(req);

                if (response == null)
                {
                    return BadRequest(new { message = "Login failed... It's on us." });
                }

                return Ok(response);
            }
            catch(CredentialsEmptyException)
            {
                return BadRequest(new { message = "Credentials Empty. Fill all the boxes." });
            }
            catch(UserEmailNotFoundException)
            {
                return BadRequest(new { message = "Wrong Email. Check again." });
            }
            catch (UserInvalidPasswordException)
            {
                return BadRequest(new { message = "Invalid password. Check again." });
            }
        }


        [Route("api/auth/register")]
        [HttpPost]
        public IActionResult Register(User user)
        {
            try
            {
                var response = _userService.Register(user);
                if (response == null)
                {
                    return BadRequest(new { message = "Register failed... It's on us." });
                }

                return Ok(response);
            }
            catch(CredentialsEmptyException)
            {
                return BadRequest(new { message = "Credentials Empty.Fill all the boxes." });

            }
            catch (UserAlreadyRegisteredException)
            {
                return BadRequest(new { message = "User already registered. Login instead." });
            }
        }
    }
}