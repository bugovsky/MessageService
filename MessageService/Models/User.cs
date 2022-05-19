using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MessageService.Models
{
    /// <summary>
    /// Класс, создающий пользователя сервиса сообщений.
    /// </summary>
    public class User
    {
        static Random s_random = new();
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Электронная почта пользователя.
        /// </summary>
        ///
        public string Email { get; set; }

        /// <summary>
        /// Создает объект класса User.
        /// </summary>
        /// <param name="userName">Имя пользователя.</param>
        /// <param name="email">Электронная почта пользователя.</param>
        public User(string userName, string email)
        {
            UserName = userName;
            Email = email;
        }

        /// <summary>
        /// Метод, генерирующий имя пользователя.
        /// </summary>
        /// <returns>Имя пользователя.</returns>
        public static string CreateUserName()
        {
            StringBuilder userName = new();
            int count = s_random.Next(4, 8);
            for (int i = 0; i < count; i++)
            {
                userName.Append((char)s_random.Next(65, 91));
            }
            return userName.ToString();
        }

        /// <summary>
        /// Генерирует электронную почту пользователя.
        /// </summary>
        /// <returns>Электронная почта пользователя.</returns>
        public static string CreateEmail()
        {
            StringBuilder email = new();
            int count = s_random.Next(4, 8);
            for (int i = 0; i < count; i++)
            {
                email.Append((char)s_random.Next(97, 123));
            }
            email.Append("@gmail.com");
            return email.ToString();
        }
    }
}
