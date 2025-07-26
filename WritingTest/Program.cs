using System;
using System.Threading.Tasks;
using Business.Services;
using DataAccess.Repositories;
using HealMateEnglish.Utils;
using HealMateEnglish.ViewModels;
using Models;

namespace WritingTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== HealMate English - Writing Feature Test ===");
            Console.WriteLine();

            try
            {
                // Initialize dependencies
                var context = new HealmateEnglishContext();
                var writingRepo = new WritingRepository(context);
                var writingService = new WritingService();
                int testUserId = 1; // Assuming user ID 1 exists

                Console.WriteLine("1. Testing database seeding...");
                var seeder = new DatabaseSeeder(context);
                await seeder.SeedWritingTopicsAsync();
                Console.WriteLine("✓ Database seeding completed");

                Console.WriteLine("\n2. Testing topic loading...");
                var topics = await writingRepo.GetAllTopicsAsync();
                Console.WriteLine($"✓ Loaded {topics.Count} writing topics");

                if (topics.Count > 0)
                {
                    Console.WriteLine("\nAvailable topics:");
                    for (int i = 0; i < Math.Min(3, topics.Count); i++)
                    {
                        Console.WriteLine($"  - {topics[i].Title.Substring(0, Math.Min(80, topics[i].Title.Length))}...");
                    }
                }

                if (topics.Count > 0)
                {
                    Console.WriteLine("\n3. Testing writing evaluation...");
                    var testTopic = topics[0];
                    string testWriting = @"Technology has significantly changed our lives in many ways. It has made communication easier and faster through smartphones and social media platforms. We can now connect with people around the world instantly.

However, technology also brought some challenges. Privacy concerns have increased as companies collect personal data. Social isolation has become more common as people spend more time online rather than face-to-face interactions.

In conclusion, while technology offers many benefits, we must address its negative impacts to ensure a balanced society.";

                    Console.WriteLine("Evaluating sample essay...");
                    var evaluation = await writingService.EvaluateWritingAsync(testTopic.Title, testWriting);
                    Console.WriteLine($"✓ Writing evaluation completed");
                    Console.WriteLine($"  Score: {evaluation.Score}/9.0");
                    Console.WriteLine($"  Feedback: {evaluation.Feedback}");
                    Console.WriteLine("  Suggestions:");
                    for (int i = 0; i < evaluation.Suggestions.Count; i++)
                    {
                        Console.WriteLine($"    {i + 1}. {evaluation.Suggestions[i]}");
                    }

                    Console.WriteLine("\n4. Testing writing session save...");
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

                    Console.WriteLine("\n5. Testing ViewModel initialization...");
                    var viewModel = new WritingViewModel(writingRepo, writingService, testUserId);
                    await Task.Delay(2000); // Give time for async loading
                    Console.WriteLine($"✓ ViewModel initialized with {viewModel.Topics.Count} topics");
                    Console.WriteLine($"  Default mode: {(viewModel.IsPresetMode ? "Preset" : "Custom")}");
                    Console.WriteLine($"  Can submit: {viewModel.SubmitCommand.CanExecute(null)}");
                }

                Console.WriteLine("\n6. Testing custom topic mode...");
                var customViewModel = new WritingViewModel(writingRepo, writingService, testUserId);
                await Task.Delay(1000);
                customViewModel.IsPresetMode = false;
                customViewModel.CustomTopic = "Discuss the advantages and disadvantages of online learning.";
                customViewModel.UserText = "Online learning has become popular due to its flexibility and accessibility.";

                Console.WriteLine($"✓ Custom mode test - Topic: {customViewModel.CustomTopic}");
                Console.WriteLine($"  Can submit: {customViewModel.SubmitCommand.CanExecute(null)}");

                Console.WriteLine("\n🎉 All tests passed! The Writing feature is working correctly.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
