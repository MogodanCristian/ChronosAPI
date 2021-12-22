using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ChronosAPI.Helpers;
using ChronosAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ChronosAPI.Controllers
{
    [Authorize]
    [Route("api/Tasks")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly AppSettings _appSettings;

        public TaskController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        [HttpGet]
        public JsonResult GetTasks()
        {
            JsonResult result = new JsonResult("");

            string query = @" SELECT * from dbo.Tasks";
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
            if (table.Rows.Count == 0)
            {
                result.StatusCode = 404;
                result.Value = "Table is empty!";
                return result;
            }
            result.StatusCode = 200;
            result.Value = table;
            return result;
        }

        [HttpGet("{PlanId:int}")]
        public JsonResult GetPlansForUser(int PlanId)
        {
            JsonResult result = new JsonResult("");
            string query = @"SELECT  T.TaskID, T.Title, T.Description, T.CreatedAt, T.StartDate, T.EndDate, T.Progress, T.Priority, T.FinishedBy,  B.BucketID, P.PlanID
                             FROM Tasks AS T
                             JOIN Task_Dispatcher AS TD
                             ON T.TaskID = TD.TaskID
                             JOIN Buckets AS B
                             ON TD.BucketID = B.BucketID
							 JOIN Bucket_Dispatcher AS BD
							 ON B.BucketID = BD.BucketID
							 JOIN Plans AS P
							 ON BD.PlanID = P.PlanID
							 WHERE P.PlanID = @PlanId";
            DataTable table = new DataTable();
            string sqlSource = _appSettings.ChronosDBCon;
            SqlDataReader reader;
            using (SqlConnection my_connection = new SqlConnection(sqlSource))
            {
                my_connection.Open();
                using (SqlCommand my_command = new SqlCommand(query, my_connection))
                {
                    my_command.Parameters.AddWithValue("@PlanId", PlanId);
                    reader = my_command.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    my_connection.Close();
                }
            }
            if (table.Rows.Count == 0)
            {
                result.StatusCode = 404;
                result.Value = "No tasks found for this plan";
                return result;
            }
            result.StatusCode = 200;
            result.Value = table;
            return result;
        }

        [HttpPost]
        public JsonResult CreateTaskToBucket(TaskRequestModel task)
        {
            JsonResult result = new JsonResult("");
            string addToBucketProcedure = "dbo.AddTaskToBucket";
            string sqlDataSource = _appSettings.ChronosDBCon;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(addToBucketProcedure, myCon))
                {
                    myCommand.CommandType = CommandType.StoredProcedure;
                    myCommand.Parameters.AddWithValue("@Title", task.Title);
                    myCommand.Parameters.AddWithValue("@Description", ((object)task.Description) ?? DBNull.Value);
                    myCommand.Parameters.AddWithValue("@CreatedAt", ((object)task.CreatedAt) ?? DateTime.Now);
                    myCommand.Parameters.AddWithValue("@StartDate", ((object)task.StartDate) ?? DateTime.Now.Date);
                    myCommand.Parameters.AddWithValue("@EndDate", ((object)task.EndDate) ?? DBNull.Value);
                    myCommand.Parameters.AddWithValue("@Progress", task.Progress);
                    myCommand.Parameters.AddWithValue("@Priority", task.Priority);
                    myCommand.Parameters.AddWithValue("@BucketId", task.BucketId);
                    myCommand.Parameters.AddWithValue("@UserId", task.UserId);
                    int rowsAffected = myCommand.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        result.StatusCode = 400;
                        result.Value = "Failed to Create Task And assign to Bucket. It's on us...";
                        myCon.Close();
                        return result;
                    }
                    myCon.Close();
                }
            }
            result.StatusCode = 200;
            result.Value = task;
            return result;
        }

        [HttpPut]
        public JsonResult UpdateTask(TaskModel task)
        {
            JsonResult result = new JsonResult("");

            string query = @"UPDATE dbo.Tasks SET Title = @Title,
                                                    Description=@Description,
                                                    CreatedAt=@CreatedAt,
                                                    StartDate=@StartDate,
                                                    EndDate=@EndDate,
                                                    Progress=@Progress,
                                                    Priority=@Priority,
                                                    FinishedBy=@FinishedBy
                            WHERE TaskID=@TaskID";
            string sqlDataSource = _appSettings.ChronosDBCon;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@Title", task.Title);
                    myCommand.Parameters.AddWithValue("@Description", ((object)task.Description) ?? DBNull.Value);
                    myCommand.Parameters.AddWithValue("@CreatedAt", ((object)task.CreatedAt) ?? DateTime.Now);
                    myCommand.Parameters.AddWithValue("@StartDate", ((object)task.StartDate) ?? DateTime.Now.Date);
                    myCommand.Parameters.AddWithValue("@EndDate", ((object)task.EndDate) ?? DBNull.Value);
                    myCommand.Parameters.AddWithValue("@Progress", task.Progress);
                    myCommand.Parameters.AddWithValue("@Priority", task.Priority);
                    myCommand.Parameters.AddWithValue("@FinishedBy", ((object)task.FinishedBy) ?? DBNull.Value);
                    myCommand.Parameters.AddWithValue("@TaskID", task.TaskId);
                    int rowsAffected = myCommand.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        result.StatusCode = 404;
                        result.Value = "No Task with this Id exists.";
                        myCon.Close();
                        return result;
                    }
                    myCon.Close();
                }
            }

            result.StatusCode = 200;
            result.Value = task;
            return result;
        }

        [HttpDelete]

        public JsonResult DeleteTask(TaskModel task)
        {
            string query = @" DELETE from dbo.Tasks where TaskID=@TaskID";
            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            JsonResult result = new JsonResult("");

            DataTable taskTable = new DataTable();
            string selectQueryBucketDispatchers = @"SELECT * from dbo.Tasks";
            SqlDataReader taskReader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                {
                    myCon.Open();
                    SqlCommand getAllPlanDispatchers = new SqlCommand(selectQueryBucketDispatchers, myCon);
                    taskReader = getAllPlanDispatchers.ExecuteReader();
                    taskTable.Load(taskReader);
                    bool taskExists = taskTable.AsEnumerable().Any(row => task.TaskId == row.Field<int>("TaskID"));
                    myCon.Close();
                    if (!taskExists)
                    {
                        result.StatusCode = 404;
                        result.Value = "Task does not exist in the table!";
                        myCon.Close();
                        return result;
                    }

                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@TaskID", task.TaskId);
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