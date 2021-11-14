using ChronosAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ChronosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlansController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PlansController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]

        public JsonResult GetPlans()
        {
            string query = @"SELECT * from dbo.Plans";
            DataTable table = new DataTable();
            string sqlSource = _configuration.GetConnectionString("ChronosDBCon");
            SqlDataReader reader;
            using (SqlConnection my_connection = new SqlConnection(sqlSource))
            {
                my_connection.Open();
                using (SqlCommand my_command = new SqlCommand(query, my_connection))
                {
                    reader = my_command.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    my_connection.Close();
                }
            }
            return new JsonResult(table);
        }

        public JsonResult PostPlan(Plans plan)
        {
            string query = @"INSERT INTO dbo.Plans
                            ([Title],
                             [CreatedAt],
                             [Description])
                            VALUES
                             (@Title,
                              @CreatedAt,
                              @Description)";

            DataTable table = new DataTable();
            string sqlSource = _configuration.GetConnectionString("ChronosDBCon");
            SqlDataReader reader;
            using (SqlConnection my_connection = new SqlConnection(sqlSource))
            {
                my_connection.Open();
                using (SqlCommand my_command = new SqlCommand(query, my_connection))
                {
                    my_command.Parameters.AddWithValue("@Title", plan.Title);
                    my_command.Parameters.AddWithValue("@CreatedAt", plan.CreatedAt);
                    my_command.Parameters.AddWithValue("@Description", plan.Description);

                    reader = my_command.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    my_connection.Close();
                }
            }
            return new JsonResult("Insert succesfull!");
        }

        [HttpPut]

        public JsonResult UpdatePlan(Plans plan)
        {
            string query = @"UPDATE dbo.Plan
                            set
                            Title=@Title,
                            CreatedAt=@CreatedAt,
                            Description=@Description
                            where PlanID=@PlanID";

            DataTable table = new DataTable();
            string sqlSource = _configuration.GetConnectionString("ChronosDBCon");
            SqlDataReader reader;
            using (SqlConnection my_connection = new SqlConnection(sqlSource))
            {
                my_connection.Open();
                using (SqlCommand my_command = new SqlCommand(query, my_connection))
                {
                    my_command.Parameters.AddWithValue("@PlanID", plan.PlanId);
                    reader = my_command.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    my_connection.Close();
                }
            }
            return new JsonResult("Update succesfull!");
        }

        [HttpDelete]

        public JsonResult DeletePlan(Plans plan)
        {
            string query = @"DELETE FROM dbo.Plans
                            where PlanID=@PlanID";

            DataTable table = new DataTable();
            string sqlSource = _configuration.GetConnectionString("ChronosDBCon");
            SqlDataReader reader;
            using (SqlConnection my_connection = new SqlConnection(sqlSource))
            {
                my_connection.Open();
                using (SqlCommand my_command = new SqlCommand(query, my_connection))
                {
                    my_command.Parameters.AddWithValue("@PlanID", plan.PlanId);
                    reader = my_command.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    my_connection.Close();
                }
            }
            return new JsonResult("Delete succesfull!");
        }
    }
}
