using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Windows.Documents;
using GalaSoft.MvvmLight.Command;


namespace IS_lab4
{
    class ViewModel : INotifyPropertyChanged
    {
        // Событие для уведомления об изменении свойств
        public event PropertyChangedEventHandler PropertyChanged;

        // Метод для вызова события
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Конструктор модели представления
        public ViewModel()
        {
            // Инициализируем список цветов
            Students = new BindingList<Student>();
        }

        // Свойство для хранения списка цветов
        public BindingList<Student> Students { get; set; }

        // Свойство для хранения выбранного объекта
        private Student selectedStudent;
        public Student SelectedStudent
        {
            get { return selectedStudent; }
            set
            {
                selectedStudent = value;
                OnPropertyChanged(nameof(SelectedStudent));
            }
        }

        // Свойство для хранения имени
        private string newFname;
        public string NewFname
        {
            get { return newFname; }
            set
            {
                newFname = value;
                OnPropertyChanged(nameof(NewFname));
            }
        }
        // Свойство для хранения фамилия
        private string newLname;
        public string NewLname
        {
            get { return newLname; }
            set
            {
                newLname = value;
                OnPropertyChanged(nameof(NewLname));
            }
        }



        // Свойство для хранения возраста
        private int newAge;
        public int NewAge
        {
            get { return newAge; }
            set
            {
                newAge = value;
                OnPropertyChanged(nameof(NewAge));
            }
        }
        //свойство хранения идентификатора
        private int newId;
        public int NewId
        {
            get { return newId; }
            set
            {
                newId = value;
                OnPropertyChanged(nameof(NewId));
            }
        }
        // Свойство для хранения статуса
        private bool newStatus;
        public bool NewStatus
        {
            get { return newStatus; }
            set
            {
                newStatus = value;
                OnPropertyChanged(nameof(NewStatus));
            }
        }
        //свойство для хранения идентификатора для удаления
        private int deleteId;
        public int DeleteId
        {
            get { return deleteId; }
            set
            {
                deleteId = value;
                OnPropertyChanged(nameof(DeleteId));
            }
        }
        public bool AddCommand
        {
            get
            {
                return false;
            }
            set
            {
                Add();

            }
        }
        public bool DeleteCommand
        {
            get
            {
                return false;
            }
            set
            {
                Delete();

            }
        }

        public bool LoadCommand
        {
            get
            {
                return false;
            }
            set
            {
                Load();

            }
        }
        public bool SaveCommand
        {
            get
            {
                return false;
            }
            set
            {
                Save();

            }
        }
        public bool DeleteIdCommand
        {
            get
            {
                return false;
            }
            set
            {
                DeleteById();

            }
        }
        // Метод для выполнения команды загрузки
        public void Load()
        {
            // Создаем UDP-клиент для отправки запроса на сервер
            UdpClient sender = new UdpClient();
            // Формируем запрос в виде строки с методом GET
            string request = "GET";
            // Преобразуем строку в массив байтов
            byte[] data = Encoding.UTF8.GetBytes(request);
            // Отправляем запрос на сервер по адресу 127.0.0.1:8000
            sender.Send(data, data.Length, "127.0.0.1", 8000);
            // Ожидаем ответ от сервера в течение 5 секунд
            sender.Client.ReceiveTimeout = 5000;
            try
            {
                // Получаем ответ от сервера в виде массива байтов и его удаленный адрес
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data2 = sender.Receive(ref remoteEP);
                // Преобразуем массив байтов в строку
                string response = Encoding.UTF8.GetString(data2);
                // Преобразуем строку в список цветов
                var students = JsonConvert.DeserializeObject<List<Student>>(response);
                // Очищаем текущий список цветов
                Students.Clear();
                // Добавляем полученные цветы в список
                foreach (var student in students)
                {
                    Students.Add(student);
                }
                // Выводим сообщение об успешной загрузке
                MessageBox.Show("Данные загружены с сервера");
            }
            catch (SocketException ex)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show("Не удалось получить ответ от сервера: " + ex.Message);
            }
            // Закрываем соединение
            sender.Close();
        }
        public void Add()
        {
            // Проверяем, что все поля заполнены
            if (NewId <= 0 || string.IsNullOrEmpty(NewFname) || string.IsNullOrEmpty(NewLname) || NewAge <= 0)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show("Пожалуйста, заполните все поля правильно");
                return;
            }

            // Проверяем, что ID студента уникален
            bool isUnique = true;
            foreach (Student student in Students)
            {
                if (student.Id == NewId)
                {
                    isUnique = false;
                    break;
                }
            }

            if (!isUnique)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show("Студент с таким идентификатором уже существует");
                return;
            }

            // Создаем новый объект студента с введенными данными
            Student newStudent = new Student(NewId, NewFname, NewLname, NewAge, NewStatus);

            // Добавляем нового студента в список
            Students.Add(newStudent);

            // Очищаем поля ввода
            NewId = 0;
            NewFname = "";
            NewLname = "";
            NewAge = 0;
            NewStatus = false;

            // Выводим сообщение об успешном добавлении
            MessageBox.Show("Студент добавлен в список");
        }
        public void Delete()
        {
            // Проверяем, что выбран студент для удаления
            if (SelectedStudent == null)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show("Пожалуйста, выберите студента для удаления");
                return;
            }
            // Удаляем выбранного студента из списка
            Students.Remove(SelectedStudent);
            // Выводим сообщение об успешном удалении
            MessageBox.Show("Студент удален из списка");
        }
        public void Save()
        {
            // Создаем UDP-клиент для отправки запроса на сервер
            UdpClient sender = new UdpClient();
            // Формируем запрос в виде строки с методом POST и списком цветов в формате JSON
            string request = "POST" + JsonConvert.SerializeObject(Students);
            // Преобразуем строку в массив байтов
            byte[] data = Encoding.UTF8.GetBytes(request);
            // Отправляем запрос на сервер по адресу 127.0.0.1:8000
            sender.Send(data, data.Length, "127.0.0.1", 8000);
            try
            {
                // Получаем ответ от сервера в виде массива байтов и его удаленный адрес
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data2 = sender.Receive(ref remoteEP);
                // Преобразуем массив байтов в строку
                string response = Encoding.UTF8.GetString(data2);
                // Выводим сообщение об ответе
                MessageBox.Show(response);
            }
            catch (SocketException ex)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show("Не удалось получить ответ от сервера: " + ex.Message);
            }
        }
       public void DeleteById()
        {
            // Проверяем, что введенный идентификатор существует в списке студентов
            bool isExist = false;
            Student deleteStudent = null;
            foreach (Student student in Students)
            {
                if (student.Id == deleteId)
                {
                    isExist = true;
                    deleteStudent = student;
                    break;
                }
            }

            // Если идентификатор не найден, выводим сообщение об ошибке
            if (!isExist)
            {
                MessageBox.Show("Студент с указанным идентификатором не найден");
                return;
            }

            // Удаляем найденного студента из списка
            Students.Remove(deleteStudent);

            // Выводим сообщение об успешном удалении
            MessageBox.Show("Студент удален из списка");
        } 
    }
}
