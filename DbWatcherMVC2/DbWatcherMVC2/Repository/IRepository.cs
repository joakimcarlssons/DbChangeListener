using System.Collections.Generic;

namespace DbWatcherMVC2.Repository
{
    public interface IRepository
    {
        List<Person> GetAllPersons();
    }
}