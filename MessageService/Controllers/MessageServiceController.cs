using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MessageService.Models;
using Microsoft.AspNetCore.Mvc;


namespace MessageService.Controllers
{
    /// <summary>
    /// Контроллер, отвечающий за реализации методов GET и POST.
    /// </summary>
    [Route("/[controller]")]
    public class MessageServiceController : Controller
    {
        private string usersPath = @"../users.json";
        private string messagesPath = @"../messages.json";
        Random random = new();
        private static List<User> users = new();
        private static List<User> orderedUsers = new();
        private static List<string> emails = new();
        private static List<Post> posts = new();

        /// <summary>
        /// Заполняет список пользователей и сортирует его,
        /// заполняет список сообщений, затем сериализует оба списка.
        /// </summary>
        [HttpPost("FillListOfUsers")]
        public IActionResult CreateListsAndJsonFiles()
        {
            ClearAllLists();

            FillListOfUsers();

            try
            {
                orderedUsers = users.OrderBy(user => user.Email).ToList();

                using (FileStream fileWithUsers = new(usersPath, FileMode.OpenOrCreate))
                {
                    fileWithUsers.SetLength(0);
                    JsonSerializer.SerializeAsync(fileWithUsers, orderedUsers);
                }

                FillListOfMessages();

                using (FileStream fileWithMessages = new(messagesPath, FileMode.OpenOrCreate))
                {
                    fileWithMessages.SetLength(0);
                    JsonSerializer.SerializeAsync(fileWithMessages, posts);
                }
                return Ok("Список сформирован и отсортирован.");
            }
            catch
            {
                return BadRequest("Что-то пошло не так...");
            }
        }

        /// <summary>
        /// Делает выборку по пользователям.
        /// </summary>
        /// <param name="limit">Количество шагов.</param>
        /// <param name="offset">Номер, с которого добавляются пользователи (индексация с 0).</param>
        /// <returns>Список выбранных пользователей.</returns>
        [HttpGet("GetSomeUsers")]
        public IActionResult GetSomeUsers(int limit, int offset)
        {
            if (limit <= 0 || offset < 0)
            {
                return BadRequest("Данные введены некорректно.");
            }
            List<User> allUsers;
            List<User> requiredUsers = new();
            try
            {
                allUsers = GetUsers();
                if (allUsers.Count == 0)
                {
                    return NotFound("Список пользователей пуст!");
                }
                if (offset > allUsers.Count - 1)
                {
                    return BadRequest("В списке находится меньше пользователей!");
                }
                for (int i = offset; i < allUsers.Count; i++)
                {
                    if (limit == 0)
                    {
                        return Ok(requiredUsers);
                    }
                    requiredUsers.Add(allUsers[i]);
                    if (i == allUsers.Count - 1)
                    {
                        return Ok(requiredUsers);
                    }
                    limit--;
                }
                return Ok(requiredUsers);
            }
            catch
            {
                return BadRequest("Что-то пошло не так...");
            }
        }

        /// <summary>
        /// Возвращает отсортированный список пользователей.
        /// </summary>
        /// <returns>Отсортированный список пользователей.</returns>
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            try
            {
                using (FileStream file = new FileStream(usersPath, FileMode.OpenOrCreate))
                {
                    if (file.Length == 0)
                    {
                        return NotFound("Список пользователей пуст!");
                    }
                    var usersFromFile = JsonSerializer.DeserializeAsync(file, typeof(List<User>));
                    return Ok(usersFromFile.Result);
                }
            }
            catch
            {
                return BadRequest("Что-то пошло не так...");
            }

        }

        /// <summary>
        /// Поиск пользователя по электронной почте.
        /// </summary>
        /// <param name="email">Электронная почта.</param>
        /// <returns>Возвращает пользователя.</returns>
        [HttpGet("GetUserByEmail")]
        public IActionResult GetUserByEmail(string email)
        {
            List<User> allUsers;
            try
            {
                using (FileStream file = new FileStream(usersPath, FileMode.OpenOrCreate))
                {
                    if (file.Length == 0)
                    {
                        return NotFound("Список пользователей пуст!");
                    }
                    var usersFromFile = JsonSerializer.DeserializeAsync(file, typeof(List<User>));
                    allUsers = usersFromFile.Result as List<User>;
                }
            }
            catch
            {
                return BadRequest("Что-то пошло не так...");
            }
            var userInfo = allUsers.SingleOrDefault(user => user.Email == email);

            if (userInfo == null)
            {
                return NotFound("Пользователь с введенной электронной почтой не найден.");
            }
            return Ok(userInfo);
        }

        /// <summary>
        /// Получает все сообщения по идентификационным номерам
        /// отправителя и получателя.
        /// </summary>
        /// <param name="senderId">Email отправителя.</param>
        /// <param name="receiverId">Email получателя.</param>
        /// <returns>Все сообщения, отправленные одним пользователем другому.</returns>
        [HttpGet("GetMessagesBySenderIdAndReceiverId")]
        public IActionResult GetMessagesBySenderIdAndReceiverId(string senderId, string receiverId)
        {
            List<Post> requiredPosts;
            try
            {
                using FileStream messageFile = new(messagesPath, FileMode.OpenOrCreate);

                if (messageFile.Length == 0)
                {
                    return NotFound("Список сообщений пуст!");
                }
                if (senderId == receiverId)
                {
                    return BadRequest("Отправлять сообщения самому себе запрещено.");
                }
                requiredPosts = FillListOfRequiredMessage(messageFile, senderId, receiverId);
                if (senderId == null || receiverId == null || !emails.Contains(senderId) || !emails.Contains(receiverId))
                {
                    return BadRequest("Пользователь(и) с таким(и) Email не найден(ы).");
                }
            }
            catch
            {
                return BadRequest("Что-то пошло не так...");
            }

            if (requiredPosts.Count == 0)
            {
                return NotFound($"Пользователь с Email {senderId} не отправил ни одного" +
                    $" сообщения пользователю с Email {receiverId}");
            }
            return Ok(requiredPosts);
        }

        /// <summary>
        /// Возвращает все соообщения, отправленные каким-либо
        /// пользователем.
        /// </summary>
        /// <param name="senderId">Email пользователя.</param>
        /// <returns>Список сообщений,отправленных пользователем.</returns>
        [HttpGet("GetMessagesBySenderId")]
        public IActionResult GetMessagesBySenderId(string senderId)
        {
            List<Post> requiredPosts;
            try
            {
                using FileStream messageFile = new(messagesPath, FileMode.OpenOrCreate);

                if (messageFile.Length == 0)
                {
                    return NotFound("Список сообщений пуст!");
                }
                requiredPosts = FillListOfRequiredMessage(messageFile, senderId);
                if (senderId == null || !emails.Contains(senderId))
                {
                    return BadRequest("Пользователь с таким Email не найден.");
                }
            }
            catch
            {
                return BadRequest("Что-то пошло не так...");
            }

            if (requiredPosts.Count == 0)
            {
                return NotFound($"Пользователь с Email {senderId} не отправил ни одного" +
                    $" сообщения");
            }
            return Ok(requiredPosts);
        }

        /// <summary>
        /// Возвращает список сообщений, полученных каким-либо
        /// пользователем.
        /// </summary>
        /// <param name="receiverId">Email получателя.</param>
        /// <returns>Список сообщений, полученных каким-либо пользователем.</returns>
        [HttpGet("GetMessagesByReceiverId")]
        public IActionResult GetMessagesByReceiverId(string receiverId)
        {
            List<Post> requiredPosts;
            try
            {
                using FileStream messageFile = new(messagesPath, FileMode.OpenOrCreate);

                if (messageFile.Length == 0)
                {
                    return NotFound("Список сообщений пуст!");
                }
                requiredPosts = FillListOfRequiredMessage(receiverId, messageFile);
                if (receiverId == null || !emails.Contains(receiverId))
                {
                    return BadRequest("Пользователь с таким Email не найден.");
                }
            }
            catch
            {
                return BadRequest("Что-то пошло не так...");
            }

            if (requiredPosts.Count == 0)
            {
                return NotFound($"Пользователь с Email {receiverId} не получил ни одного" +
                    $" сообщения");
            }
            return Ok(requiredPosts);
        }

        /// <summary>
        /// Очищает все списки.
        /// </summary>
        private void ClearAllLists()
        {
            users.Clear();
            orderedUsers.Clear();
            emails.Clear();
            posts.Clear();
        }

        /// <summary>
        /// Заполняет списки пользователей.
        /// </summary>
        private void FillListOfUsers()
        {
            int count = random.Next(10, 26);
            for (int i = 0; i < count; i++)
            {
                User user = new User(Models.User.CreateUserName(), Models.User.CreateEmail());
                while (emails.Contains(user.Email))
                {
                    user.Email = Models.User.CreateEmail();
                }
                emails.Add(user.Email);
                users.Add(user);
            }
        }

        /// <summary>
        /// Заполняет список сообщений.
        /// </summary>
        private void FillListOfMessages()
        {
            for (int i = 0; i < users.Count; i++)
            {
                Post post = new Post(Post.CreateText(), Post.CreateText(),
                   orderedUsers[random.Next(0, orderedUsers.Count)].Email,
                   orderedUsers[random.Next(0, orderedUsers.Count)].Email);
                while (post.SenderId == post.ReceiverId)
                {
                    post.ReceiverId = orderedUsers[random.Next(0, orderedUsers.Count)].Email;
                }
                posts.Add(post);
            }
        }

        /// <summary>
        /// Заполняет список с сообщениями от определенного получателя
        /// к определенному адресату.
        /// </summary>
        /// <param name="messageFile">Файловый поток.</param>
        /// <param name="senderId">Email отправителя.</param>
        /// <param name="receiverId">Email получателя.</param>
        /// <returns>Список нужных сообщений.</returns>
        private List<Post> FillListOfRequiredMessage(FileStream messageFile, string senderId, string receiverId)
        {
            List<Post> requiredPosts = new();
            var messagesFromFile = JsonSerializer.DeserializeAsync(messageFile, typeof(List<Post>));
            for (int i = 0; i < (messagesFromFile.Result as List<Post>).Count; i++)
            {
                if ((messagesFromFile.Result as List<Post>)[i].SenderId == senderId
                    && (messagesFromFile.Result as List<Post>)[i].ReceiverId == receiverId)
                {
                    requiredPosts.Add((messagesFromFile.Result as List<Post>)[i]);
                }
            }
            return requiredPosts;
        }

        /// <summary>
        /// Заполняет список сообщениями отправителя по его Email.
        /// </summary>
        /// <param name="messageFile">Файловый поток.</param>
        /// <param name="senderId">Email отправителя.</param>
        /// <returns>Список отправленных сообщений.</returns>
        private List<Post> FillListOfRequiredMessage(FileStream messageFile, string senderId)
        {
            List<Post> requiredPosts = new();
            var messagesFromFile = JsonSerializer.DeserializeAsync(messageFile, typeof(List<Post>));
            for (int i = 0; i < (messagesFromFile.Result as List<Post>).Count; i++)
            {
                if ((messagesFromFile.Result as List<Post>)[i].SenderId == senderId)
                {
                    requiredPosts.Add((messagesFromFile.Result as List<Post>)[i]);
                }
            }
            return requiredPosts;
        }

        /// <summary>
        /// Заполняет список сообщениями получателя по его Email.
        /// </summary>
        /// <param name="receiverId">Email получателя.</param>
        /// <param name="messageFile">Файловый поток.</param>
        /// <returns>Список сообщений получателя.</returns>
        private List<Post> FillListOfRequiredMessage(string receiverId, FileStream messageFile)
        {
            List<Post> requiredPosts = new();
            var messagesFromFile = JsonSerializer.DeserializeAsync(messageFile, typeof(List<Post>));
            for (int i = 0; i < (messagesFromFile.Result as List<Post>).Count; i++)
            {
                if ((messagesFromFile.Result as List<Post>)[i].ReceiverId == receiverId)
                {
                    requiredPosts.Add((messagesFromFile.Result as List<Post>)[i]);
                }
            }
            return requiredPosts;
        }

        /// <summary>
        /// Возвращает список пользователей.
        /// </summary>
        /// <returns>Список пользователей.</returns>
        private List<User> GetUsers()
        {
            List<User> allUsers;
            try
            {
                using (FileStream file = new FileStream(usersPath, FileMode.OpenOrCreate))
                {
                    var usersFromFile = JsonSerializer.DeserializeAsync(file, typeof(List<User>));
                    allUsers = usersFromFile.Result as List<User>;
                    return allUsers;
                }
            }
            catch
            {
                return orderedUsers;
            }
        }
    }
}
