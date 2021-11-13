using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ChronosAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ChronosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanDispatcherController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PlanDispatcherController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]

        public JsonResult GetPlanDispatchers()
        {
            string query = @" SELECT * from dbo.Plan_Dispatcher";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("ChronosDBCon");
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
            return new JsonResult(table);
        }

        [HttpPost]
        public JsonResult PostPlanDispatcher(PlanDispatcher planDispatcher)
        {
            string query = @" INSERT into dbo.Plan_Dispatcher (UserID, PlanID, AssignedAt)
                            VALUES (@UserID, @PlanID, @AssignedAt)";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("ChronosDBCon");
            SqlDataReader myReader;

            //---------------USER HANDLING--------------------------------

            DataTable Users = new DataTable();
            string selectQueryUsers = @"SELECT * from dbo.Users";
            SqlDataReader userReader;

            //---------------PLAN HANDLING--------------------------------

            DataTable Plans = new DataTable();
            string selectQueryPlans = @"SELECT * from dbo.Plans";
            SqlDataReader planReader;

            //-----------------------------------------------------------
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                //CHECK IF USER EXISTS IN USER TABLE

                myCon.Open();
                SqlCommand getAllUsers = new SqlCommand(selectQueryUsers, myCon);
                userReader = getAllUsers.ExecuteReader();
                Users.Load(userReader);
                bool userExists = Users.AsEnumerable().Any(row => planDispatcher.UserId == row.Field<int>("UserID"));
                myCon.Close();

                //CHECK IF PLANS EXIST IN PLAN TABLE

                myCon.Open();
                SqlCommand getAllPlans = new SqlCommand(selectQueryPlans, myCon);
                planReader = getAllPlans.ExecuteReader();
                Plans.Load(planReader);
                bool planExists = Plans.AsEnumerable().Any(row => planDispatcher.PlanId == row.Field<int>("PlanID"));
                myCon.Close();

                //----------------------------------------------------

                if (!userExists)
                {
                    return new JsonResult("User does not exit in Database!!!");
                }
                else if(!planExists)
                {
                    return new JsonResult("Plan does not exit in Database!!!");
                }
                else
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@UserID", planDispatcher.UserId);
                        myCommand.Parameters.AddWithValue("@PlanID", planDispatcher.PlanId);
                        myCommand.Parameters.AddWithValue("@AssignedAt", planDispatcher.AssignedAt);
                        myReader = myCommand.ExecuteReader();
                        table.Load(myReader);
                        myReader.Close();
                        myCon.Close();
                    }
                }
            }
            return new JsonResult("Insert succesfull!!!");
        }
    
        [HttpDelete]

        public JsonResult DeletePlanDispatcher(PlanDispatcher planDispatcher)
        {
            string query = @" DELETE from dbo.Plan_Dispatcher where UserID=@UserID";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("ChronosDBCon");
            SqlDataReader myReader;

            //----------------------PLAN DISPATCHER TABLE--------------------------

            DataTable PlanDispatcherTable = new DataTable();
            string selectQueryPlanDispatchers = @"SELECT * from dbo.Plan_Dispatcher";
            SqlDataReader planDispatcherReader;

            //---------------------------------------------------------------------
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                {
                    //CHECK IF USER EXISTS IN USER TABLE

                    myCon.Open();
                    SqlCommand getAllPlanDispatchers = new SqlCommand(selectQueryPlanDispatchers, myCon);
                    planDispatcherReader = getAllPlanDispatchers.ExecuteReader();
                    PlanDispatcherTable.Load(planDispatcherReader);
                    bool planDispatcherExists = PlanDispatcherTable.AsEnumerable().Any(row => planDispatcher.UserId == row.Field<int>("UserID"));
                    myCon.Close();
                    if(!planDispatcherExists)
                    {
                        return new JsonResult("This user does not have a plan assigned!!");
                    }
                    //-----------------------------------------------------------------
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@UserID", planDispatcher.UserId);
                        myReader = myCommand.ExecuteReader();
                        table.Load(myReader);
                        myReader.Close();
                        myCon.Close();
                    }
                }
            }
            return new JsonResult("Delete succesfull!!!");
        }
    }

}
