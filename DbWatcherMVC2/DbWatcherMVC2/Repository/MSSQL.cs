using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DbWatcherMVC2.Repository
{
    public class MSSQL : IRepository
    {
        string cnnString = "";

        public MSSQL(IConfiguration config)
        {
            cnnString = config.GetConnectionString("DefaultConnection");
        }

        public List<Person> GetAllPersons()
        {
            var persons = new List<Person>();

            using (var cnn = new SqlConnection(cnnString))
            {
                cnn.Open();

                SqlDependency.Start(cnnString);

                var cmdText = "SELECT Id, FirstName, LastName FROM dbo.Persons";

                var cmd = new SqlCommand(cmdText, cnn);

                var dependency = new SqlDependency(cmd);

                dependency.OnChange += new OnChangeEventHandler(dbChangeNotification);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var person = new Person
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString()
                    };

                    persons.Add(person);
                }
            }

            return persons;
        }

        private void dbChangeNotification(object sender, SqlNotificationEventArgs e)
        {
            using (var writer = new StreamWriter(Directory.GetCurrentDirectory() + "test.txt", true))
            {
                writer.WriteLine($"{DateTime.UtcNow} - Something changed");
            }
        }
    }
}
