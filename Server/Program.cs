using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Server
{
    class Program
    {
        // Создаем объект Logger для логирования операций
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            // Создаем UDP-клиент для прослушивания порта 8000
            UdpClient listener = new UdpClient(8000);
            // Получаем локальный IP-адрес
            string localIP = GetLocalIPAddress();
            // Выводим сообщение о запуске сервера
            Console.WriteLine("Сервер запущен на {0}:8000", localIP);
            // Бесконечный цикл для обработки запросов от клиентов
            while (true)
            {
                // Получаем запрос от клиента в виде массива байтов и его удаленный адрес
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] request = listener.Receive(ref remoteEP);
                // Преобразуем запрос в строку
                string requestString = Encoding.UTF8.GetString(request);
                // Логируем полученное сообщение и адрес клиента
                logger.Info("Получено сообщение '{0}' от {1}", request, remoteEP);
                // Выводим информацию о запросе
                Console.WriteLine("Получен запрос от {0}:{1}", remoteEP.Address, remoteEP.Port);
                Console.WriteLine("Запрос: {0}", requestString);
                // Обрабатываем запрос в отдельном потоке
                Task.Run(() => HandleRequest(requestString, remoteEP, listener));
            }
        }

        // Метод для обработки запроса от клиента
        static async Task HandleRequest(string request, IPEndPoint remoteEP, UdpClient listener)
        {
            string dopString = "";
            if (request.StartsWith("POST"))
            {
                dopString = request.Substring(4);
                request = "POST";
            }
            // Создаем объект для работы с базой данных
            using (StudentsContext db = new StudentsContext())
            {
                // Проверяем, какой метод был использован в запросе
                switch (request)
                {
                    // Если метод GET, то отправляем все записи из базы данных клиенту в формате JSON
                    case "GET":
                        // Получаем список всех цветов из базы данных
                        var students = db.Students.ToList();
                        // Преобразуем список в JSON-строку
                        string responseString = JsonConvert.SerializeObject(students);
                        // Преобразуем строку в массив байтов
                        byte[] response = Encoding.UTF8.GetBytes(responseString);
                        // Отправляем ответ клиенту
                        await listener.SendAsync(response, response.Length, remoteEP);
                        // Логируем отправленный ответ и адрес клиента
                        logger.Info("Отправлен ответ '{0}' на {1}", response, remoteEP);
                        // Выводим информацию об ответе
                        Console.WriteLine("Отправлен ответ клиенту {0}:{1}", remoteEP.Address, remoteEP.Port);
                        Console.WriteLine("Ответ: {0}", responseString);
                        break;
                    // Если метод POST, то читаем пришедшие записи от клиента и перезаписываем базу данных
                    case "POST":
                        // Преобразуем запрос в список 
                        var newStudents = JsonConvert.DeserializeObject<List<Student>>(dopString);
                        // Удаляем все старые записи из базы данных
                        db.Students.RemoveRange(db.Students);
                        //// Добавляем новые записи в базу данных
                        db.Students.AddRange(newStudents);
                        // Сохраняем изменения
                        db.SaveChanges();
                        // Выводим информацию об обновлении базы данных
                        Console.WriteLine("База данных обновлена");
                        // Преобразуем строку в массив байтов
                        response = Encoding.UTF8.GetBytes("Database Updated");
                        // Отправляем ответ клиенту
                        await listener.SendAsync(response, response.Length, remoteEP);
                        // Логируем отправленный ответ и адрес клиента
                        logger.Info("Отправлен ответ '{0}' на {1}", response, remoteEP);
                        break;
                    // Если метод неизвестен, то отправляем сообщение об ошибке
                    default:
                        // Формируем строку с сообщением об ошибке
                        string errorString = "Неверный метод запроса";
                        // Преобразуем строку в массив байтов
                        byte[] error = Encoding.UTF8.GetBytes(errorString);
                        // Отправляем ответ клиенту
                        await listener.SendAsync(error, error.Length, remoteEP);
                        // Выводим информацию об ошибке
                        Console.WriteLine("Отправлено сообщение об ошибке клиенту {0}:{1}", remoteEP.Address, remoteEP.Port);
                        Console.WriteLine("Ошибка: {0}", errorString);
                        break;
                }
            }
        }

        // Метод для получения локального IP-адреса
        static string GetLocalIPAddress()
        {
            // Получаем список всех сетевых интерфейсов
            var interfaces = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            // Ищем первый IPv4-адрес
            foreach (var address in interfaces)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    // Возвращаем адрес в виде строки
                    return address.ToString();
                }
            }
            // Если не нашли, то возвращаем пустую строку
            return "";
        }
    }
}

