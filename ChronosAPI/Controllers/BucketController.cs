using ChronosAPI.Helpers;
using ChronosAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    [Route("api/buckets")]
    [ApiController]
    public class BucketController : ControllerBase
    {
        private readonly AppSettings _appSettings;

        public BucketController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        [HttpGet]
        public JsonResult GetBuckets()
        {
            JsonResult result = new JsonResult("");

            string query = @" SELECT * from dbo.Buckets";
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
                result.Value = "No Buckets created!";
                return result;
            }
            result.StatusCode = 200;
            result.Value = table;
            return result;
        }

        [HttpPost("{PlanId:int}")]
        public JsonResult PostBucketAtPlan([FromRoute()]int PlanId, [FromBody()] object Title)
        {

            JsonResult result = new JsonResult("");
            string titleFinal = Title.ToString().Trim().Split(":")[1].Split("\"")[1].Trim('"');



            string addToBucketProcedure = "dbo.AddBucketToPlan";
            string sqlDataSource = _appSettings.ChronosDBCon;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(addToBucketProcedure, myCon))
                {
                    myCommand.CommandType = CommandType.StoredProcedure;
                    myCommand.Parameters.AddWithValue("@Title", titleFinal);
                    myCommand.Parameters.AddWithValue("@PlanId", PlanId);
                    int rowsAffected = myCommand.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        result.StatusCode = 400;
                        result.Value = "Failed to Create Bucket And assign to Plan. It's on us...";
                        myCon.Close();
                        return result;
                    }
                    myCon.Close();
                }
            }
            result.StatusCode = 200;
            result.Value = titleFinal;
            return result;
        }

        [HttpGet("{PlanId:int}")]
        public JsonResult GetBucketsForPlan([FromRoute()]int PlanId)
        {
            JsonResult result = new JsonResult("");
            string query = @"SELECT B.BucketID, B.Title
                             FROM Buckets AS B
                             JOIN Bucket_Dispatcher as BD
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
                result.Value = "No buckets found for this plan";
                return result;
            }
            result.StatusCode = 200;
            result.Value = table;
            return result;
        }


        [HttpPost]
        public JsonResult PostBucket(Bucket bucket)
        {
            JsonResult result = new JsonResult("");

            string query = @"INSERT INTO dbo.Buckets (Title) VALUES (@Title)";
            string sqlDataSource = _appSettings.ChronosDBCon;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@Title", bucket.Title);
                    int rowsAffected = myCommand.ExecuteNonQuery();
                    if(rowsAffected == 0)
                    {
                        result.StatusCode = 400;
                        result.Value = "Failed to Create Bucket. It's on us...";
                        myCon.Close();
                        return result;
                    }
                    myCon.Close();
                }
            }
            result.StatusCode = 200;
            result.Value = bucket;
            return result;
        }

        [HttpPut]
        public JsonResult UpdateBucketTitle(Bucket bucketToUpdate)
        {
            JsonResult result = new JsonResult("");

            string query = @"UPDATE dbo.Buckets SET Title = @newTitle WHERE BucketID = @BucketId";
            string sqlDataSource = _appSettings.ChronosDBCon;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@BucketId", bucketToUpdate.BucketId);
                    myCommand.Parameters.AddWithValue("@newTitle", bucketToUpdate.Title);
                    int rowsAffected = myCommand.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        result.StatusCode = 404;
                        result.Value = "No Bucket with this Id exists.";
                        myCon.Close();
                        return result;
                    }
                    myCon.Close();
                }
            }

            result.StatusCode = 200;
            result.Value = bucketToUpdate;
            return result;
        }

        [HttpDelete]
        public JsonResult DeleteBucket(Bucket bucketToDelete)
        {
            string query = @" DELETE from dbo.Buckets where BucketID=@BucketId";
            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            JsonResult result = new JsonResult("");

            DataTable taskTable = new DataTable();
            string selectQueryBucket = @"SELECT * from dbo.Buckets";
            SqlDataReader taskReader;

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                {
                    myCon.Open();
                    SqlCommand getAllPlanDispatchers = new SqlCommand(selectQueryBucket, myCon);
                    taskReader = getAllPlanDispatchers.ExecuteReader();
                    taskTable.Load(taskReader);
                    bool taskExists = taskTable.AsEnumerable().Any(row => bucketToDelete.BucketId == row.Field<int>("BucketID"));
                    myCon.Close();
                    if (!taskExists)
                    {
                        result.StatusCode = 404;
                        result.Value = "Bucket does not exist in the table!";
                        return result;
                    }

                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@BucketId", bucketToDelete.BucketId);
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
