using ChronosAPI.Helpers;
using ChronosAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ChronosAPI.Controllers
{
    [Authorize]
    [Route("api/Plans")]
    [ApiController]
    public class PlansController : ControllerBase
    {
        private readonly AppSettings _appSettings;

        public PlansController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        [HttpGet]

        public JsonResult GetPlans()
        {
            JsonResult result = new JsonResult("");
            string query = @"SELECT * from dbo.Plans";
            DataTable table = new DataTable();
            string sqlSource = _appSettings.ChronosDBCon;
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
            if (table.Rows.Count == 0)
            {
                result.StatusCode = 404;
                result.Value = "Plans table is empty!";
                return result;
            }
            result.StatusCode = 200;
            result.Value = table;
            return result;
        }

        [HttpGet("{UserId:int}")]
        public JsonResult GetPlansForUser(int UserId)
        {
            JsonResult result = new JsonResult("");
            string query = @"SELECT Pl.PlanID, Pl.Title, Pl.Description, Pl.CreatedAt
                            FROM Plan_Dispatcher as PD
                            JOIN Users AS U
                            ON PD.UserID = U.UserID
                            JOIN Plans AS Pl
                            ON PD.PlanID = Pl.PlanID
                            WHERE U.UserID = @UserId";
            DataTable table = new DataTable();
            string sqlSource = _appSettings.ChronosDBCon;
            SqlDataReader reader;
            using (SqlConnection my_connection = new SqlConnection(sqlSource))
            {
                my_connection.Open();
                using (SqlCommand my_command = new SqlCommand(query, my_connection))
                {
                    my_command.Parameters.AddWithValue("@UserId", UserId);
                    reader = my_command.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    my_connection.Close();
                }
            }
            if (table.Rows.Count == 0)
            {
                result.StatusCode = 404;
                result.Value = "No plans found for this user";
                return result;
            }
            result.StatusCode = 200;
            result.Value = table;
            return result;
        }


        [HttpPost("{UserId:int}")]
        public JsonResult CreatePlanForUser([FromRoute()]int UserId, [FromBody()] Plan plan)
        {
            JsonResult result = new JsonResult("");
            string addToBucketProcedure = "dbo.AddPlanToUser";
            string sqlDataSource = _appSettings.ChronosDBCon;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(addToBucketProcedure, myCon))
                {
                    myCommand.CommandType = CommandType.StoredProcedure;
                    myCommand.Parameters.AddWithValue("@Title", plan.Title);
                    myCommand.Parameters.AddWithValue("@Description", plan.Description);
                    myCommand.Parameters.AddWithValue("@UserId", UserId);
                    int rowsAffected = myCommand.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        result.StatusCode = 400;
                        result.Value = "Failed to Create Plan And assign to User. It's on us...";
                        myCon.Close();
                        return result;
                    }
                    myCon.Close();
                }
            }
            result.StatusCode = 200;
            result.Value = plan;
            return result;
        }

        public JsonResult PostPlan(Plan plan)
        {
            JsonResult result = new JsonResult("");

            string query = @"INSERT INTO dbo.Plans
                            ([Title],
                             [CreatedAt],
                             [Description])
                            VALUES
                             (@Title,
                              @CreatedAt,
                              @Description)";

            string sqlSource = _appSettings.ChronosDBCon;
            using (SqlConnection my_connection = new SqlConnection(sqlSource))
            {
                my_connection.Open();
                using (SqlCommand my_command = new SqlCommand(query, my_connection))
                {
                    my_command.Parameters.AddWithValue("@Title", plan.Title);
                    my_command.Parameters.AddWithValue("@CreatedAt", ((object)plan.CreatedAt) ?? DateTime.Now);
                    my_command.Parameters.AddWithValue("@Description", ((object)plan.Description ?? DBNull.Value));

                    int rowsAffected = my_command.ExecuteNonQuery();
                    if(rowsAffected == 0)
                    {
                        result.StatusCode = 400;
                        result.Value = "Failed to create plan... It's on us.";
                        my_connection.Close();
                        return result;
                    }
                    my_connection.Close();
                }
            }

            result.StatusCode = 200;
            result.Value = plan;
            return result;
        }

        [HttpPut]

        public JsonResult UpdatePlan(Plan plan)
        {
            JsonResult result = new JsonResult("");

            string query = @"UPDATE dbo.Plans
                            set
                            Title=@Title,
                            CreatedAt=@CreatedAt,
                            Description=@Description
                            where PlanID=@PlanID";


            string sqlSource = _appSettings.ChronosDBCon;
            using (SqlConnection my_connection = new SqlConnection(sqlSource))
            {
                my_connection.Open();
                using (SqlCommand my_command = new SqlCommand(query, my_connection))
                {
                    my_command.Parameters.AddWithValue("@PlanID", plan.PlanId);
                    my_command.Parameters.AddWithValue("@Title", plan.Title);
                    my_command.Parameters.AddWithValue("@CreatedAt", ((object)plan.CreatedAt) ?? DateTime.Now);
                    my_command.Parameters.AddWithValue("@Description", ((object)plan.Description) ?? DBNull.Value);
                    
                    int rowsAffected = my_command.ExecuteNonQuery();
                    if(rowsAffected == 0)
                    {
                        result.StatusCode = 400;
                        result.Value = "Couldn't update the plan. It's on us...";
                        my_connection.Close();
                        return result;
                    }
                    my_connection.Close();
                }
            }

            result.StatusCode = 200;
            result.Value = plan;
            return result;
        }

        [HttpDelete]

        public JsonResult DeletePlan(Plan plan)
        {
            string query = @" DELETE from dbo.Plans where PlanID=@PlanID";
            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            JsonResult result = new JsonResult("");

            DataTable taskTable = new DataTable();
            string selectQueryBucketDispatchers = @"SELECT * from dbo.Plans";
            SqlDataReader taskReader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                {
                    myCon.Open();
                    SqlCommand getAllPlanDispatchers = new SqlCommand(selectQueryBucketDispatchers, myCon);
                    taskReader = getAllPlanDispatchers.ExecuteReader();
                    taskTable.Load(taskReader);
                    bool taskExists = taskTable.AsEnumerable().Any(row => plan.PlanId == row.Field<int>("PlanID"));
                    myCon.Close();
                    if (!taskExists)
                    {
                        result.StatusCode = 404;
                        result.Value = "Plan does not exist in the table!";
                        myCon.Close();
                        return result;
                    }

                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@PlanID", plan.PlanId);
                        myReader = myCommand.ExecuteReader();
                        table.Load(myReader);
                        myReader.Close();
                        myCon.Close();
                    }
                }
            }
            result.StatusCode = 200;
            result.Value = "Delete successful!";
            return result;
        }
    }
}