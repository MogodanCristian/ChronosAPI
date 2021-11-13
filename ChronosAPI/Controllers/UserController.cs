using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using ChronosAPI.Models;

namespace ChronosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]

        public JsonResult GetUsers()
        {
            string query = @" SELECT * from dbo.Users";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("ChronosDBCon");
            SqlDataReader myReader;
            using (SqlConnection myCon=new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand=new SqlCommand(query,myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(table);
        }

        [HttpPost]

        public JsonResult PostUser(User user)
        {
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

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("ChronosDBCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                { 
                    myCommand.Parameters.AddWithValue("@FirstName",user.FirstName);
                    myCommand.Parameters.AddWithValue("@LastName", user.LastName);
                    myCommand.Parameters.AddWithValue("@Email", user.Email);
                    myCommand.Parameters.AddWithValue("@Password", user.Password);
                    myCommand.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth);
                    myCommand.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult("Insert succesfull!!");
        }

        [HttpPut]

        public JsonResult UpdateUser(User user)
        {
            string query = @" UPDATE [dbo].[Users] 
           set
           FirstName=@FirstName,
           LastName=@LastName,
           Email=@Email,
           Password=@Password,
           DateOfBirth=@DateOfBirth,
           CreatedAt= @CreatedAt
        where
           UserID=@UserID";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("ChronosDBCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@UserID", user.UserId);
                    myCommand.Parameters.AddWithValue("@FirstName", user.FirstName);
                    myCommand.Parameters.AddWithValue("@LastName", user.LastName);
                    myCommand.Parameters.AddWithValue("@Email", user.Email);
                    myCommand.Parameters.AddWithValue("@Password", user.Password);
                    myCommand.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth);
                    myCommand.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult("Update succesfull!!");
        }

        [HttpDelete]

        public JsonResult DeleteUser(User user)
        {
            string query = @" DELETE from [dbo].[Users]
            where
           UserID=@UserID";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("ChronosDBCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@UserID",user.UserId);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult("Delete succesfull!!");
        }
    }
}
