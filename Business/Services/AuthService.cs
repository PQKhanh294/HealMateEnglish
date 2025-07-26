using Models;


namespace Business.Services
{
    public class CustomerService
    {
        public User Login(string Username, string Password)
        {
            try
            {
                using var context = new HealmateEnglishContext();
                return context.Users.FirstOrDefault(c =>
                    c.Username == Username && c.Password == Password);
            }
            catch (Exception ex)
            {
                throw new Exception($"Login error: {ex.Message}");
            }
        }
    }
 }
