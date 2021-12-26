using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronosAPI.Helpers;
using ChronosAPI.Models;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using ChronosAPI.Exceptions;

namespace ChronosAPI.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        RegisterResponse Register(User user);
        User GetUserById(int id);

        List<UserMemberInfo> GetUsers();

        JsonResult UpdateUserPassword(ResetPasswordModel resetPasswordModel);

        AuthenticateResponse AuthenticateJwt(string jwtToken);
    }
    public class UserService : IUserService
    {
        //private List<User> _users = new List<User>(); // populate users from db.
        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            if(string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                throw new CredentialsEmptyException();
            }

            // check if email exists in db, get user.
            string query = @" SELECT * from dbo.Users WHERE Email = @Email";

            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@Email", model.Email);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            if(table.Rows.Count == 0)
            {
                throw new UserEmailNotFoundException();
            }

            var userRow = table.AsEnumerable().SingleOrDefault(row => model.Email == row.Field<string>("Email"));
            User user = new User();
            user.UserId = Convert.ToInt32(userRow.Field<int>("UserID"));
            user.FirstName = userRow.Field<string>("FirstName");
            user.LastName = userRow.Field<string>("LastName");
            user.Email = userRow.Field<string>("Email").ToString();
            user.DateOfBirth = userRow.Field<DateTime>("DateOfBirth");
            user.CreatedAt = userRow.Field<DateTime>("CreatedAt");
            user.Password = userRow.Field<string>("Password");

 
            if (!(BCrypt.Net.BCrypt.Verify(model.Password, user.Password)))
            {
                throw new UserInvalidPasswordException();
            }

            // authentication successful so generate jwt token
            var token = GenerateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        public RegisterResponse Register(User user)
        {
            // Check if credentials are sent.
            if(user.FirstName == null || user.LastName == null || user.Email == null || user.Password == null || user.DateOfBirth == null )
            {
                throw new CredentialsEmptyException();
            }

            // validate user exists in the DB.

            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            string queryUserExists = @" SELECT * from dbo.Users WHERE Email = @Email";
            SqlDataReader userExistsReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(queryUserExists, myCon))
                {
                    myCommand.Parameters.AddWithValue("@Email", user.Email);
                    userExistsReader = myCommand.ExecuteReader();
                    table.Load(userExistsReader);
                    userExistsReader.Close();
                    myCon.Close();
                }
            }

            if (table.Rows.Count > 0)
            {
                throw new UserAlreadyRegisteredException();
            }

            // hash password
            string oldPassword = user.Password;
            user.Password = BCrypt.Net.BCrypt.HashPassword(oldPassword);

            string query = @" INSERT INTO [dbo].[Users]
           ([FirstName]
           ,[LastName]
           ,[Email]
           ,[Password]
           ,[DateOfBirth]
           ,[CreatedAt])
     VALUES
           (@FirstName,
           @LastName,
           @Email,
           @Password,
           @DateOfBirth,
           @CreatedAt)";

            table = new DataTable();
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@FirstName", user.FirstName);
                    myCommand.Parameters.AddWithValue("@LastName", user.LastName);
                    myCommand.Parameters.AddWithValue("@Email", user.Email);
                    myCommand.Parameters.AddWithValue("@Password", user.Password);
                    myCommand.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth.Date);
                    myCommand.Parameters.AddWithValue("@CreatedAt", ((object)user.CreatedAt) ?? DateTime.Now);

                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new RegisterResponse(user);
        }

        // util methods

        private string GenerateJwtToken(User user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.UserId.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public User GetUserById(int id)
        {
            string query = @" SELECT * from dbo.Users WHERE UserID = @UserId";

            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@UserId", id);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            if (table.Rows.Count == 0)
            {
                return null; // USER WITH THAT id WAS NOT FOUND
            }

            var userRow = table.AsEnumerable().SingleOrDefault(row => id == row.Field<int>("UserID"));
            User user = new User();
            user.UserId = Convert.ToInt32(userRow.Field<int>("UserID"));
            user.FirstName = userRow.Field<string>("FirstName");
            user.LastName = userRow.Field<string>("LastName");
            user.Email = userRow.Field<string>("Email").ToString();
            user.DateOfBirth = userRow.Field<DateTime>("DateOfBirth");
            user.CreatedAt = userRow.Field<DateTime>("CreatedAt");
            user.Password = userRow.Field<string>("Password");

            return user;           
        
        }

        public AuthenticateResponse AuthenticateJwt(string jwtTokenParam)
        {
            if (string.IsNullOrEmpty(jwtTokenParam))
            {
                throw new CredentialsEmptyException();
            }
            // get USER ID BY JWT.!!!!!!!!!
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                tokenHandler.ValidateToken(jwtTokenParam, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,

                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                int userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                User user = GetUserById(userId);
                return new AuthenticateResponse(user, jwtTokenParam);
            }
            catch
            {
                throw new UserBadJwtException();
            }
        }

        public List<UserMemberInfo> GetUsers()
        {
            string query = @"SELECT UserID
                            ,FirstName
                            ,LastName
                            ,Email
                            FROM dbo.Users";

            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            var tableList = table.AsEnumerable();
            List<UserMemberInfo> listOfUsers = new List<UserMemberInfo>();
            foreach(var x in tableList)
            {
                listOfUsers.Add(new UserMemberInfo
                {
                    UserId = x.Field<int>("UserId"),
                    FirstName = x.Field<string>("FirstName"),
                    LastName = x.Field<string>("LastName"),
                    Email = x.Field<string>("Email")
                });
            }

            return listOfUsers;
        }

        public JsonResult UpdateUserPassword(ResetPasswordModel resetPasswordModel)
        {

            // validate user exists in the DB.
            JsonResult result = new JsonResult("");

            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            string queryUserExists = @" SELECT * from dbo.Users WHERE Email = @Email";
            SqlDataReader userExistsReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(queryUserExists, myCon))
                {
                    myCommand.Parameters.AddWithValue("@Email", resetPasswordModel.email);
                    userExistsReader = myCommand.ExecuteReader();
                    table.Load(userExistsReader);
                    userExistsReader.Close();
                    myCon.Close();
                }
            }

            if (table.Rows.Count == 0)
            {
                result.Value = "User not found!";
                result.StatusCode = 404;
                return result;
            }

            // hash password
            string oldPassword = resetPasswordModel.newPassword;
            resetPasswordModel.newPassword = BCrypt.Net.BCrypt.HashPassword(oldPassword);

            string query = @"UPDATE [dbo].[Users] SET Password = @Password WHERE Email = @Email";

            table = new DataTable();
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@Email", resetPasswordModel.email);
                    myCommand.Parameters.AddWithValue("@Password", resetPasswordModel.newPassword);

                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            result.Value = "Successfully updated user's password";
            result.StatusCode = 200;
            return result;
        }
    }
}
