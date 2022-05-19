using System;
using System.Text;

namespace MessageService.Models
{
    /// <summary>
    /// Класс, создающий сообщения.
    /// </summary>
    public class Post
    {
        static Random s_random = new();
        /// <summary>
        /// Тема сообщения.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Текст сообщения.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Электронная почта отправителя.
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        /// Электронная почта получателя.
        /// </summary>
        public string ReceiverId { get; set; }

        /// <summary>
        /// Создает объект класса Post.
        /// </summary>
        /// <param name="subject">Тема сообщения.</param>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="senderId">Email отправителя.</param>
        /// <param name="receiverId">Email получателя.</param>
        public Post(string subject, string message, string senderId, string receiverId)
        {
            Subject = subject;
            Message = message;
            SenderId = senderId;
            ReceiverId = receiverId;
        }

        /// <summary>
        /// Генирирует случайный текст для темы и текста
        /// сообщения.
        /// </summary>
        /// <returns>Возвращает текст из заглавных английских букв.</returns>
        public static string CreateText()
        {
            StringBuilder text = new();
            int count = s_random.Next(4, 8);
            for (int i = 0; i < count; i++)
            {
                text.Append((char)s_random.Next(65, 91));
            }
            return text.ToString();
        }
    }
}
