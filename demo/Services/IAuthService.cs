using catalog.Entities;

namespace catalog.Services
    {
        // Интерфейс, който описва какво трябва да може AuthService
        public interface IAuthService
        {
            User? Login(string username, string password); // Вход
            void Register(User user); // Регистрация
            User? GetByUsername(string username); // Търсене на потребител
        }
    }