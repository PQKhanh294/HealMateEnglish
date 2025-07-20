using DataAccess.Interfaces;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class AdminReadingRepository : IAdminReadingRepository
    {
        private readonly HealmateEnglishContext _context;
        public AdminReadingRepository(HealmateEnglishContext context)
        {
            _context = context;
        }

        public async Task<int> AddAdminReadingAsync(PresetReading reading)
        {
            _context.PresetReadings.Add(reading);
            await _context.SaveChangesAsync();
            return reading.PresetId;
        }

        public async Task AddReadingQuestionsAsync(IEnumerable<ReadingQuestion> questions)
        {
            _context.ReadingQuestions.AddRange(questions);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PresetReading>> GetAllAdminReadingsAsync()
        {
            return _context.PresetReadings.ToList();
        }

        public async Task<PresetReading> GetAdminReadingByIdAsync(int id)
        {
            return _context.PresetReadings.FirstOrDefault(r => r.PresetId == id);
        }
    }
} 