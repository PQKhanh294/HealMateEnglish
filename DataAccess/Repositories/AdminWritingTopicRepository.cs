using DataAccess.Interfaces;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class AdminWritingTopicRepository : IAdminWritingTopicRepository
    {
        private readonly HealmateEnglishContext _context;
        public AdminWritingTopicRepository(HealmateEnglishContext context)
        {
            _context = context;
        }

        public async Task<int> AddAdminWritingTopicAsync(PresetWritingTopic topic)
        {
            _context.PresetWritingTopics.Add(topic);
            await _context.SaveChangesAsync();
            return topic.TopicId;
        }

        public async Task<List<PresetWritingTopic>> GetAllAdminWritingTopicsAsync()
        {
            return _context.PresetWritingTopics.ToList();
        }

        public async Task<PresetWritingTopic> GetAdminWritingTopicByIdAsync(int id)
        {
            return _context.PresetWritingTopics.FirstOrDefault(t => t.TopicId == id);
        }
    }
} 