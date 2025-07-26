using System.Windows;
using DataAccess.Repositories;
using HealMateEnglish.Views;
using HealMateEnglish.Utils;
using Models;

namespace HealMateEnglish
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static HealmateEnglishContext? DbContext { get; private set; }        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set shutdown mode to manual - don't shutdown when windows close
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Khởi tạo DbContext với connection string
            DbContext = new HealmateEnglishContext();

            // Seed database with sample writing topics
            _ = SeedDatabaseAsync();

            // Show login window and handle result
            ShowLoginWindow();
        }

        private async System.Threading.Tasks.Task SeedDatabaseAsync()
        {
            try
            {
                if (DbContext != null)
                {
                    var seeder = new DatabaseSeeder(DbContext);
                    await seeder.SeedWritingTopicsAsync();
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Database seeding failed: {ex.Message}");
            }
        }
        private void ShowLoginWindow()
        {
            var loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Login dialog result: {result}");

            if (result == true)
            {
                // Login successful, open dashboard
                var userId = loginWindow.LoggedInUserId;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Login successful, userId: {userId}"); if (userId > 0)
                {
                    // Create dashboard window
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Creating DashboardWindow...");

                    try
                    {
                        var dashboardWindow = new DashboardWindow(userId);
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] DashboardWindow created successfully");

                        // Set as main window
                        this.MainWindow = dashboardWindow;

                        // Change shutdown mode to close with main window
                        this.ShutdownMode = ShutdownMode.OnMainWindowClose;

                        dashboardWindow.Show();

                        System.Diagnostics.Debug.WriteLine($"[DEBUG] DashboardWindow shown successfully");
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERROR] Exception creating dashboard: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                        this.Shutdown();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Invalid userId: {userId}, shutting down");
                    this.Shutdown();
                }
            }
            else
            {
                // Login cancelled or failed, exit application
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Login cancelled or failed, shutting down");
                this.Shutdown();
            }
        }
    }
}
