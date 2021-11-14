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
    [Route("api/[controller]")]
    [ApiController]
    public class TaskDispatcherController : ControllerBase
    {
        private readonly AppSettings _appSettings;

        public TaskDispatcherController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }


        [HttpGet]
        public JsonResult GetTaskDispatchers()
        {
            JsonResult result = new JsonResult("");

            string query = @" SELECT * from dbo.Task_Dispatcher";
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

        [HttpPost]
        public JsonResult PostTask(TaskDispatcher taskDispatcher)
        {
            JsonResult result = new JsonResult("");

            string query = @"INSERT INTO dbo.Task_Dispatcher
           (TaskID,UserID)
     VALUES
           (@TaskID,@UserID)";
            string sqlDataSource = _appSettings.ChronosDBCon;

            //-----Task and User Handling-----------
            DataTable Task = new DataTable();
            string selectQueryTask = @"SELECT * from dbo.Tasks";
            SqlDataReader taskReader;

            DataTable Users = new DataTable();
            string selectQueryUsers = @"SELECT * from dbo.Users";
            SqlDataReader usersReader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {

                myCon.Open();
                SqlCommand getAllUsers = new SqlCommand(selectQueryUsers, myCon);
                usersReader = getAllUsers.ExecuteReader();
                Users.Load(usersReader);
                bool userExists = Users.AsEnumerable().Any(row => taskDispatcher.UserId == row.Field<int>("UserID"));
                SqlCommand getAllTasks = new SqlCommand(selectQueryTask, myCon);
                taskReader = getAllTasks.ExecuteReader();
                Task.Load(taskReader);
                bool taskExists = Task.AsEnumerable().Any(row => taskDispatcher.TaskId == row.Field<int>("TaskID"));
                myCon.Close();

                if (!userExists)
                {
                    result.StatusCode = 404;
                    result.Value = "User does not exist in Database!!!";
                    return result;
                }
                else if (!taskExists)
                {
                    result.StatusCode = 404;
                    result.Value = "Task does not exist in Database!!!";
                    return result;
                }
                else
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@TaskID", taskDispatcher.TaskId);
                        myCommand.Parameters.AddWithValue("@UserID", taskDispatcher.UserId);

                        int rowsAffected = myCommand.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            result.StatusCode = 400;
                            result.Value = "Failed to Create TaskDispatcher. It's on us...";
                            return result;
                        }
                        myCon.Close();
                    }
                }
            }
                result.StatusCode = 200;
                result.Value = "TaskDispatcher Inserted Successfully!";
                return result;
            }

        [HttpDelete]
        public JsonResult DeleteTaskDispatcher(TaskDispatcher taskDispatcher)
        {
            string query = @" DELETE from dbo.Task_Dispatcher where TaskID=@TaskID and UserID=@UserID";
            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            JsonResult result = new JsonResult("");

            //-------BUCKET AND PLAN DISPATCHER-----------------

            DataTable taskDispatcherTable = new DataTable();
            string selectQueryTaskDispatchers = @"SELECT * from dbo.Task_Dispatcher";
            SqlDataReader taskDispatcherReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                {
                    //CHECK IF USER EXISTS IN USER TABLE

                    myCon.Open();
                    SqlCommand getAllPlanDispatchers = new SqlCommand(selectQueryTaskDispatchers, myCon);
                    taskDispatcherReader = getAllPlanDispatchers.ExecuteReader();
                    taskDispatcherTable.Load(taskDispatcherReader);
                    bool membersExist = taskDispatcherTable.AsEnumerable().Any(row => taskDispatcher.TaskId == row.Field<int>("TaskID") && taskDispatcher.UserId == row.Field<int>("UserID"));
                    myCon.Close();
                    if (!membersExist)
                    {
                        result.StatusCode = 404;
                        result.Value = "TaskDispatcher not exist in the table!";
                        return result;
                    }
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@TaskID", taskDispatcher.TaskId);
                        myCommand.Parameters.AddWithValue("@UserID", taskDispatcher.UserId);
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
