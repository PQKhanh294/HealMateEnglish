using System;
using System.Linq;
using System.Threading.Tasks;
using Models;
using DataAccess.Repositories;

namespace HealMateEnglish.Utils
{
    public class DatabaseSeeder
    {
        private readonly HealmateEnglishContext _context;

        public DatabaseSeeder(HealmateEnglishContext context)
        {
            _context = context;
        }

        public async Task SeedWritingTopicsAsync()
        {
            // Check if we already have writing topics
            if (_context.PresetWritingTopics.Any())
            {
                return; // Already seeded
            }

            // Get a user to be the creator (assuming user ID 1 exists)
            var user = _context.Users.FirstOrDefault();
            if (user == null)
            {
                Console.WriteLine("No users found in database. Please create a user first.");
                return;
            }            var topics = new[]
            {
                new PresetWritingTopic
                {
                    Title = "Some people believe that technology has made our lives easier, while others think it has made them more complicated. Discuss both views and give your opinion.",
                    Band = "6.5-7.0",
                    CreatedBy = 1, // Admin user
                    CreatedAt = DateTime.Now
                },
                new PresetWritingTopic
                {
                    Title = "In many countries, the cost of living is rising rapidly. What are the causes of this problem and what solutions can be implemented?",
                    Band = "6.0-6.5",
                    CreatedBy = 1, // Admin user
                    CreatedAt = DateTime.Now
                },
                new PresetWritingTopic
                {
                    Title = "Some people think that the best way to learn a foreign language is to study it in the country where it is spoken. Others believe it can be learned just as effectively in one's home country. Discuss both views and give your opinion.",
                    Band = "7.0-7.5",
                    CreatedBy = 1, // Admin user
                    CreatedAt = DateTime.Now
                },
                new PresetWritingTopic
                {
                    Title = "Many cities around the world are facing problems with traffic congestion. What are the causes of this issue and what measures can be taken to solve it?",
                    Band = "6.0-6.5",
                    CreatedBy = 1, // Admin user
                    CreatedAt = DateTime.Now
                },
                new PresetWritingTopic
                {
                    Title = "Some people argue that the internet has brought people closer together, while others believe it has made us more isolated. Discuss both sides and give your own opinion.",
                    Band = "6.5-7.0",
                    CreatedBy = 1, // Admin user
                    CreatedAt = DateTime.Now
                },
                new PresetWritingTopic
                {
                    Title = "In some countries, young people are encouraged to work or travel for a year between finishing high school and starting university studies. Discuss the advantages and disadvantages for young people who decide to do this.",
                    Band = "7.0-7.5",
                    CreatedBy = 1, // Admin user
                    CreatedAt = DateTime.Now
                },
                new PresetWritingTopic
                {
                    Title = "Environmental protection is one of the most important issues facing the world today. What are the main environmental problems and what can individuals and governments do to address them?",
                    Band = "6.5-7.0",
                    CreatedBy = 1, // Admin user
                    CreatedAt = DateTime.Now
                },
                new PresetWritingTopic
                {
                    Title = "Some people believe that children should start learning a foreign language at primary school rather than secondary school. Do the advantages of this outweigh the disadvantages?",
                    Band = "6.0-6.5",
                    CreatedBy = 1, // Admin user
                    CreatedAt = DateTime.Now
                }
            };

            _context.PresetWritingTopics.AddRange(topics);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Successfully seeded {topics.Length} writing topics.");
        }
    }
}
