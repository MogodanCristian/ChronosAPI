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
    [Route("api/[controller]")]
    [ApiController]
    public class BucketDispatcherController : ControllerBase
    {
        private readonly AppSettings _appSettings;

        public BucketDispatcherController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        [HttpGet]

        public JsonResult GetBucketDispatchers(BucketDispatcher bucketdispatcher)
        {
            string query = @"SELECT * from dbo.Bucket_Dispatcher";
            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            JsonResult result = new JsonResult("");

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
                result.Value = "Data table is empty!";
                return result;
            }
            result.StatusCode = 200;
            result.Value = table;
            return result;
        }

        [HttpPost]

        public JsonResult PostBucketDispatcher(BucketDispatcher bucketDispatcher)
        {
            string query = @"INSERT into dbo.Bucket_Dispatcher(BucketID, PlanID)
            VALUES (@BucketID, @PlanID)";
            string sqlDataSource = _appSettings.ChronosDBCon;
            JsonResult result = new JsonResult("");

            //-----Bucket and Plan Handling-----------
            DataTable Bucket = new DataTable();
            string selectQueryBuckets = @"SELECT * from dbo.Buckets";
            SqlDataReader bucketReader;

            DataTable Plans = new DataTable();
            string selectQueryPlans = @"SELECT * from dbo.Plans";
            SqlDataReader planReader;

            //-----------------------------------------

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                //CHECK IF BUCKET EXISTS IN USER TABLE

                myCon.Open();
                SqlCommand getAllBuckets = new SqlCommand(selectQueryBuckets, myCon);
                bucketReader = getAllBuckets.ExecuteReader();
                Bucket.Load(bucketReader);
                bool bucketExists = Bucket.AsEnumerable().Any(row => bucketDispatcher.BucketId == row.Field<int>("BucketID"));
                myCon.Close();

                //CHECK IF PLANS EXIST IN PLAN TABLE

                myCon.Open();
                SqlCommand getAllPlans = new SqlCommand(selectQueryPlans, myCon);
                planReader = getAllPlans.ExecuteReader();
                Plans.Load(planReader);
                bool planExists = Plans.AsEnumerable().Any(row => bucketDispatcher.PlanId == row.Field<int>("PlanID"));
                myCon.Close();

                //----------------------------------------------------

                if (!bucketExists)
                {
                    result.StatusCode = 404;
                    result.Value = "Bucket does not exist in Database!!!";
                    return result;
                }
                else if (!planExists)
                {
                    result.StatusCode = 404;
                    result.Value = "Plan does not exist in Database!!!";
                    return result;
                }
                else
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@BucketID", bucketDispatcher.BucketId);
                        myCommand.Parameters.AddWithValue("@PlanID", bucketDispatcher.PlanId);
                        int rowsAffected = myCommand.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            result.StatusCode = 400;
                            result.Value = "Insert failed!";
                            return result;
                        }
                        myCon.Close();
                    }
                }
            }
            result.StatusCode = 200;
            result.Value = "Insert successful!";
            return result;
        }

        [HttpDelete]

        public JsonResult DeleteBucketDispatcher(BucketDispatcher bucketDispatcher)
        {
            string query = @" DELETE from dbo.Bucket_Dispatcher where BucketID=@BucketID and PlanID=@PlanID ";
            DataTable table = new DataTable();
            string sqlDataSource = _appSettings.ChronosDBCon;
            SqlDataReader myReader;
            JsonResult result = new JsonResult("");

            //-------BUCKET AND PLAN DISPATCHER-----------------

            DataTable BucketDispatcherTable = new DataTable();
            string selectQueryBucketDispatchers = @"SELECT * from dbo.Bucket_Dispatcher";
            SqlDataReader bucketDispatcherReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                {
                    //CHECK IF USER EXISTS IN USER TABLE

                    myCon.Open();
                    SqlCommand getAllPlanDispatchers = new SqlCommand(selectQueryBucketDispatchers, myCon);
                    bucketDispatcherReader = getAllPlanDispatchers.ExecuteReader();
                    BucketDispatcherTable.Load(bucketDispatcherReader);
                    bool bucketExists = BucketDispatcherTable.AsEnumerable().Any(row => bucketDispatcher.BucketId == row.Field<int>("BucketID") && bucketDispatcher.PlanId == row.Field<int>("PlanID"));
                    myCon.Close();
                    if (!bucketExists)
                    {
                        result.StatusCode = 404;
                        result.Value = "Bucket does not exist in the table!";
                        return result;
                    }
                    //-----------------------------------------------------------------
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@BucketID", bucketDispatcher.BucketId);
                        myCommand.Parameters.AddWithValue("@PlanID", bucketDispatcher.PlanId);
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