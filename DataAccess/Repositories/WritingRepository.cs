using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories
{
    public class WritingRepository
    {
        private readonly HealmateEnglishContext _context;

        public WritingRepository(HealmateEnglishContext context)
        {
            _context = context;
        }        // Get all writing topics
        public async Task<List<PresetWritingTopic>> GetAllTopicsAsync()
        {
            return await _context.PresetWritingTopics
                .OrderBy(t => t.Title)
                .ToListAsync();
        }

        // Get topics for specific user (admin topics + user's own topics)
        public async Task<List<PresetWritingTopic>> GetTopicsForUserAsync(int userId)
        {
            return await _context.PresetWritingTopics
                .Where(t => t.CreatedBy == 1 || t.CreatedBy == userId) // Admin (1) or user's own topics
                .OrderBy(t => t.Title)
                .ToListAsync();
        }

        // Add new writing session
        public async Task<int> AddWritingSessionAsync(WritingSession session)
        {
            _context.WritingSessions.Add(session);
            await _context.SaveChangesAsync();
            return session.SessionId;
        }

        // Get writing sessions by user
        public async Task<List<WritingSession>> GetSessionsByUserAsync(int userId)
        {
            return await _context.WritingSessions
                .Include(s => s.Topic)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        // Get writing session by ID
        public async Task<WritingSession?> GetSessionByIdAsync(int sessionId)
        {
            return await _context.WritingSessions
                .Include(s => s.Topic)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
        }        // Add new writing topic
        public async Task<int> AddTopicAsync(PresetWritingTopic topic)
        {
            // Validate the topic before saving
            if (topic == null)
                throw new ArgumentNullException(nameof(topic));

            if (string.IsNullOrWhiteSpace(topic.Title))
                throw new ArgumentException("Title cannot be null or empty", nameof(topic));

            if (topic.CreatedBy <= 0)
                throw new ArgumentException("CreatedBy must be a valid user ID", nameof(topic));

            // Verify that the user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == topic.CreatedBy);
            if (!userExists)
                throw new ArgumentException($"User with ID {topic.CreatedBy} does not exist", nameof(topic));

            _context.PresetWritingTopics.Add(topic);
            await _context.SaveChangesAsync();
            return topic.TopicId;
        }

        // Get topics by band level
        public async Task<List<PresetWritingTopic>> GetTopicsByBandAsync(string band)
        {
            return await _context.PresetWritingTopics
                .Where(t => t.Band == band)
                .OrderBy(t => t.Title)
                .ToListAsync();
        }

        // Get user's recent writing sessions (for dashboard)
        public async Task<List<WritingSession>> GetRecentSessionsByUserAsync(int userId, int count = 5)
        {
            return await _context.WritingSessions
                .Include(s => s.Topic)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
