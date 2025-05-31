using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Thrift.Data;
using Thrift.Models;

namespace Thrift.Controllers
{
    public class DatabaseController : Controller
    {
        private readonly MongoDbContext _mongoContext;

        public DatabaseController(MongoDbContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public IActionResult Index()
        {
            ViewBag.IsConnected = _mongoContext.IsConnected;
            return View();
        }

        public IActionResult TestConnection()
        {
            bool isConnected = _mongoContext.IsConnected;
            var result = new { success = isConnected, message = isConnected ? "Connected to MongoDB" : "Failed to connect to MongoDB" };
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestLoan()
        {
            try
            {
                if (!_mongoContext.IsConnected)
                {
                    return Json(new { success = false, message = "Database is not connected" });
                }

                // Get the Loans collection
                var loansCollection = _mongoContext.GetCollection<Loan>("Loans");
                  // Create a test loan
                var testLoan = new Loan
                {
                    Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                    UserId = "test-user",
                    LoanName = "Test Loan " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    LoanType = LoanType.PersonalLoan,
                    PrincipalAmount = 1000,
                    InterestRate = 5.0m,
                    LoanTermYears = 1,
                    LoanTermMonths = 0,
                    Status = LoanStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                // Insert the loan
                await loansCollection.InsertOneAsync(testLoan);
                
                return Json(new { success = true, message = "Test loan created successfully", loanId = testLoan.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error creating test loan: " + ex.Message });
            }
        }
    }
}
