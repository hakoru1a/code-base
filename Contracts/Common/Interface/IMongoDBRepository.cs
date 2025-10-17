using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Common.Interface
{
    public interface IMongoDBRepository<T> where T : class
    {
        IMongoCollection<T> FindAll(ReadPreference? readPreference = null);

        Task CreateAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(string id);
    }
}
