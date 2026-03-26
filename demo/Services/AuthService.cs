using catalog.Entities;

namespace catalog.Services
{
    // Сервиз за управление на потребители (логин/регистрация)
    public class AuthService : IAuthService
    {
        // Списък с потребители 
        private static List<User> users = new List<User>();

        // Метод за вход
        public User? Login(string username, string password)
        {
            // Търси потребител със съвпадащо име и парола
            return users.FirstOrDefault(u =>
                u.Username == username && u.PasswordHash == password);
        }

        // Метод за регистрация
        public void Register(User user)
        {
            // Генерираме ID
            user.Id = users.Count + 1;

            // Добавяме потребителя в списъка
            users.Add(user);
        }

        // Връща потребител по username
        public User? GetByUsername(string username)
        {
            return users.FirstOrDefault(u => u.Username == username);
        }
    }
}
