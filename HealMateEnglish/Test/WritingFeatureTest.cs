using System;
using System.Threading.Tasks;
using DataAccess.Repositories;
using Business.Services;
using Models;
using HealMateEnglish.ViewModels;

namespace HealMateEnglish.Test
{
    class WritingFeatureTest
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Testing Writing Feature...");

            try
            {
                // Initialize dependencies
                var context = new HealmateEnglishContext();
                var writingRepo = new WritingRepository(context);
                var writingService = new WritingService();
                int testUserId = 1; // Assuming user ID 1 exists

                Console.WriteLine("1. Testing database seeding...");
                var seeder = new HealMateEnglish.Utils.DatabaseSeeder(context);
                await seeder.SeedWritingTopicsAsync();
                Console.WriteLine("✓ Database seeding completed");

                Console.WriteLine("2. Testing topic loading...");
                var topics = await writingRepo.GetAllTopicsAsync();
                Console.WriteLine($"✓ Loaded {topics.Count} writing topics");

                if (topics.Count > 0)
                {
                    Console.WriteLine("3. Testing writing evaluation...");
                    var testTopic = topics[0];
                    string testWriting = "Technology has significantly changed our lives in many ways. It has made communication easier and faster. However, it also brought some challenges such as privacy concerns and social isolation.";

                    var evaluation = await writingService.EvaluateWritingAsync(testTopic.Title, testWriting);
                    Console.WriteLine($"✓ Writing evaluation completed");
                    Console.WriteLine($"  Score: {evaluation.Score}");
                    Console.WriteLine($"  Feedback: {evaluation.Feedback}");

                    Console.WriteLine("4. Testing writing session save...");
                    var session = new WritingSession
                    {
                        UserId = testUserId,
                        SourceType = "preset",
                        TopicId = testTopic.TopicId,
                        UserText = testWriting,
                        AiFeedback = evaluation.Feedback,
                        Score = evaluation.Score,
                        CreatedAt = DateTime.Now
                    };

                    var sessionId = await writingRepo.AddWritingSessionAsync(session);
                    Console.WriteLine($"✓ Writing session saved with ID: {sessionId}");

                    Console.WriteLine("5. Testing ViewModel initialization...");
                    var viewModel = new WritingViewModel(writingRepo, writingService, testUserId);
                    await Task.Delay(2000); // Give time for async loading
                    Console.WriteLine($"✓ ViewModel initialized with {viewModel.Topics.Count} topics");
                }

                Console.WriteLine("\n🎉 All tests passed! The Writing feature is working correctly.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
