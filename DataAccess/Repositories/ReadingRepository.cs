using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories
{
    public class ReadingRepository
    {
        private readonly HealmateEnglishContext _context;

        public ReadingRepository(HealmateEnglishContext context)
        {
            _context = context;
        }

        // Add method to fetch presets
        public async Task<List<PresetReading>> GetAllPresetReadingsAsync()
        {
            return await _context.PresetReadings.ToListAsync();
        }

        // Fetch questions by presetId (session concept removed)
        public async Task<List<ReadingQuestion>> GetQuestionsBySessionIdAsync(int sessionId)
        {
            // sessionId is treated as presetId now
            return await GetQuestionsByPresetIdAsync(sessionId);
        }

        public async Task AddReadingQuestionAsync(ReadingQuestion question)
        {
            _context.ReadingQuestions.Add(question);
            await _context.SaveChangesAsync();
        }

        // Add new method to add session
        public async Task<int> AddReadingSessionAsync(ReadingSession session)
        {
            _context.ReadingSessions.Add(session);
            await _context.SaveChangesAsync();
            return session.SessionId;
        }

        // Add new method to add preset reading (for custom passages)
        public async Task<int> AddPresetReadingAsync(PresetReading preset)
        {
            _context.PresetReadings.Add(preset);
            await _context.SaveChangesAsync();
            return preset.PresetId;
        }

        // Get the latest session for a preset
        public async Task<ReadingSession?> GetLatestSessionByPresetAsync(int presetId)
        {
            return await _context.ReadingSessions
                .Where(s => s.PresetId == presetId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // Add method to fetch all sessions by preset
        public async Task<List<ReadingSession>> GetSessionsByPresetAsync(int presetId)
        {
            return await _context.ReadingSessions
                .Where(s => s.PresetId == presetId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        // Count questions by presetId
        public async Task<int> GetQuestionsCountBySessionAsync(int sessionId)
        {
            return await _context.ReadingQuestions.CountAsync(q => q.PresetId == sessionId);
        }        // Get questions by preset directly
        public async Task<List<ReadingQuestion>> GetQuestionsByPresetIdAsync(int presetId)
        {
            return await _context.ReadingQuestions
                .Include(q => q.ReadingOptions)
                .Where(q => q.PresetId == presetId)
                .ToListAsync();
        }

        // Update a question to be a multiple-choice question
        public async Task UpdateQuestionToMultipleChoiceAsync(int questionId, bool isMultipleChoice)
        {
            var question = await _context.ReadingQuestions.FindAsync(questionId);
            if (question != null)
            {
                question.IsMultipleChoice = isMultipleChoice;
                await _context.SaveChangesAsync();
            }
        }

        // Check questions that have multiple correct options and mark them as multiple-choice
        public async Task MarkQuestionsWithMultipleCorrectOptionsAsync()
        {
            var questions = await _context.ReadingQuestions
                .Include(q => q.ReadingOptions)
                .ToListAsync();

            foreach (var question in questions)
            {
                // If a question has more than one correct option, mark it as multiple-choice
                int correctOptionsCount = question.ReadingOptions.Count(o => o.IsCorrect == true);
                if (correctOptionsCount > 1 && !question.IsMultipleChoice)
                {
                    question.IsMultipleChoice = true;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
