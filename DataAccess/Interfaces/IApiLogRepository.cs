using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IApiLogRepository
    {
        Task AddLogAsync(Apilog log);
        Task<List<Apilog>> GetAllLogsAsync();
        Task<List<Apilog>> GetLogsByUserAsync(string username);
        Task<Apilog> GetLogByIdAsync(int id);
    }
}
