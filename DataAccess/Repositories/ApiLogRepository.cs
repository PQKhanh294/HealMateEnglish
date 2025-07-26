using Models;
using System;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class ApiLogRepository
    {
        private readonly HealmateEnglishContext _context;

        public ApiLogRepository(HealmateEnglishContext context)
        {
            _context = context;
        }

        public async Task LogApiCallAsync(int userId, string requestType, string responseStatus)
        {
            var apiLog = new Apilog
            {
                UserId = userId,
                RequestType = requestType,
                ApiResponseStatus = responseStatus,
                Timestamp = DateTime.Now
            };

            _context.Apilogs.Add(apiLog);
            await _context.SaveChangesAsync();
        }
    }
}
