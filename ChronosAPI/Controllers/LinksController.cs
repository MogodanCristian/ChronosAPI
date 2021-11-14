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
    public class LinksController : ControllerBase
    {
        private readonly AppSettings _appSettings;

        public LinksController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        [HttpGet]
        public JsonResult GetTasks()
        {
            JsonResult result = new JsonResult("");

            string query = @" SELECT * from dbo.Links";
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
        /*[HttpPost]
        public JsonResult PostTask(Links links)
        {
            JsonResult result = new JsonResult("");

            string query = @"INSERT INTO dbo.Tasks
           (Title,Description,CreatedAt,StartDate,EndDate,Progress,Priority,FinishedBy)
     VALUES
           (@Title,@Description, @CreatedAt,@StartDate,@EndDate,@Progress,@Priority, @FinishedBy)";
            string sqlDataSource = _appSettings.ChronosDBCon;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@Title", task.Title);
                    myCommand.Parameters.AddWithValue("@Description", ((object)task.Description) ?? DBNull.Value);
                    myCommand.Parameters.AddWithValue("@CreatedAt", ((object)task.CreatedAt) ?? DBNull.Value);
                    myCommand.Parameters.AddWithValue("@StartDate", ((object)task.StartDate) ?? DBNull.Value);
                    myCommand.Parameters.AddWithValue("@EndDate", ((object)task.EndDate) ?? DBNull.Value);
                    myCommand.Parameters.AddWithValue("@Progress", task.Progress);
                    myCommand.Parameters.AddWithValue("@Priority", task.Priority);
                    myCommand.Parameters.AddWithValue("@FinishedBy", ((object)task.FinishedBy) ?? DBNull.Value);

                    int rowsAffected = myCommand.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        result.StatusCode = 400;
                        result.Value = "Failed to Create Task. It's on us...";
                        return result;
                    }
                    myCon.Close();
                }
            }
            result.StatusCode = 200;
            result.Value = "Task Inserted Successfully!";
            return result;
        }*/
    }
}
