using DataAccess.Interfaces;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class ApiLogRepository : IApiLogRepository
    {
        private readonly HealmateEnglishContext _context;
        public ApiLogRepository(HealmateEnglishContext context)
        {
            _context = context;
        }

        public async Task AddLogAsync(Apilog log)
        {
            _context.Apilogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Apilog>> GetAllLogsAsync()
        {
            return _context.Apilogs.ToList();
        }

        public async Task<List<Apilog>> GetLogsByUserAsync(string username)
        {
            return _context.Apilogs.Where(l => l.User != null && l.User.Username == username).ToList();
        }

        public async Task<Apilog> GetLogByIdAsync(int id)
        {
            return _context.Apilogs.FirstOrDefault(l => l.LogId == id);
        }
    }
}
