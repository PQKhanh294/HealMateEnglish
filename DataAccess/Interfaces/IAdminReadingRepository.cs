using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IAdminReadingRepository
    {
        Task<int> AddAdminReadingAsync(PresetReading reading);
        Task AddReadingQuestionsAsync(IEnumerable<ReadingQuestion> questions);
        Task<List<PresetReading>> GetAllAdminReadingsAsync();
        Task<PresetReading> GetAdminReadingByIdAsync(int id);
    }
} 