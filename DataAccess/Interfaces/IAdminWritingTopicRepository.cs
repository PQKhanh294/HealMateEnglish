using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IAdminWritingTopicRepository
    {
        Task<int> AddAdminWritingTopicAsync(PresetWritingTopic topic);
        Task<List<PresetWritingTopic>> GetAllAdminWritingTopicsAsync();
        Task<PresetWritingTopic> GetAdminWritingTopicByIdAsync(int id);
    }
} 