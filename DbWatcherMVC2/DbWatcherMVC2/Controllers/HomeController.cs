using DbWatcherMVC2.Models;
using DbWatcherMVC2.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DbWatcherMVC2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository _repo;

        string cnnString = "";

        public HomeController(ILogger<HomeController> logger, IRepository repo, IConfiguration config)
        {
            _logger = logger;
            _repo = repo;
            cnnString = config.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            //_repo.GetAllPersons();

            Task.Run(() => {
                DbChecker(); 
            });

            return View();
        }


        private void DbChecker()
        {
            using (var cnn = new SqlConnection(cnnString))
            {
                cnn.Open();

                SqlDependency.Start(cnnString);

                var cmdText = "SELECT Id, FirstName, LastName FROM dbo.Persons";

                var cmd = new SqlCommand(cmdText, cnn);

                var dependency = new SqlDependency(cmd);

                dependency.OnChange += new OnChangeEventHandler(dbChangeNotification);

                var reader = cmd.ExecuteReader();
            }
        }

        private void dbChangeNotification(object sender, SqlNotificationEventArgs e)
        {
            using (var writer = new StreamWriter(Directory.GetCurrentDirectory() + "test.txt", true))
            {
                writer.WriteLine($"{DateTime.UtcNow} - Something changed");

                DbChecker();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
