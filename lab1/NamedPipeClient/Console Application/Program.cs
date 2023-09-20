using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
public struct UserInfo
{
    public string Name { get; set; }
    public int Age { get; set; }

    public UserInfo(string name, int age)
    {
        Name = name;
        Age = age;
    }
}



namespace NamedPipeClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "MyPipe", PipeDirection.InOut))
            {
                Console.WriteLine("Подключение к серверу...");
                pipeClient.Connect();

                Console.Write("Введите имя: ");
                string name = Console.ReadLine();

                Console.Write("Введите возраст: ");
                int age = int.Parse(Console.ReadLine());

                UserInfo user = new UserInfo { Name = name, Age = age };
                string jsonUserData = JsonSerializer.Serialize(user);
                byte[] buffer = Encoding.UTF8.GetBytes(jsonUserData);
                pipeClient.Write(buffer, 0, buffer.Length);
                Console.WriteLine("Данные отправлены серверу.");

                buffer = new byte[1024];
                int bytesRead = pipeClient.Read(buffer, 0, buffer.Length);
                string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                UserInfo responseUser = JsonSerializer.Deserialize<UserInfo>(jsonResponse);
                Console.WriteLine($"Ответ от сервера: Имя: {responseUser.Name}, Возраст: {responseUser.Age}");

                pipeClient.Close();
            }

            Console.ReadLine();
        }
    }
}