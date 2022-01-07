using System;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.Json;
using System.IO;

namespace Server
{
    internal class Program
    {
        // Хранилище заметок, одно для всех клиентов
        // TO DO: заменить поле на таблицу в SQLite
        private static readonly List<Item> items;
        // Порт для подключение на локальном компьютере
        private static string local = "http://localhost:8080";
        // Команда для подключения переадресации
        // ngrok http 8080 -host-header="localhost:8080

        static Program()
        {
            items = new List<Item>();
            // Пробная заметка, которая появляется при подключении сервера
            items.Add(new Item { Id = Guid.NewGuid().ToString(), Text = "It WORKS", Description = "It really works." });
        }

        static void Main(string[] args)
        {
            // Инициализация слушателя
            var httpListener = new HttpListener();
            // Типы http-запросов, которые обрабатывает сервер
            httpListener.Prefixes.Add(local + "/get/");
            httpListener.Prefixes.Add(local + "/add/");
            httpListener.Prefixes.Add(local + "/update/");
            httpListener.Prefixes.Add(local + "/delete/");
            httpListener.Start();
            Debug.WriteLine("started");
            // Цикл прослушивания
            while (true)
            {
                // Получение нового запроса
                var requestContext = httpListener.GetContext();
                // Код успешного ответа
                requestContext.Response.StatusCode = 200;
                // Определение типа запроса "грубой силой"
                // TO DO: найти более симпатичный вариант
                if (requestContext.Request.Url.AbsolutePath.Contains("get"))
                {
                    Debug.WriteLine("get");
                    // Обработка get-запроса:
                    // Запись в выходной поток списка заметок в формате JSON
                    var streamGet = requestContext.Response.OutputStream;
                    var text = JsonSerializer.Serialize<IEnumerable<Item>>(items);
                    var bytes = Encoding.UTF8.GetBytes(text);
                    streamGet.Write(bytes, 0, bytes.Length);
                    requestContext.Response.Close();
                }
                if (requestContext.Request.Url.AbsolutePath.Contains("add"))
                {
                    Debug.WriteLine("add");
                    // Обработка add-запроса:
                    // Получение новой заметки в формате JSON и добавление её в список заметок
                    var streamAdd = new StreamReader(requestContext.Request.InputStream, requestContext.Request.ContentEncoding);
                    var newItem = JsonSerializer.Deserialize<Item>(streamAdd.ReadToEnd());
                    items.Add(newItem);
                    requestContext.Response.Close();
                }
                if (requestContext.Request.Url.AbsolutePath.Contains("update"))
                {
                    Debug.WriteLine("update");
                    // Обработка update-запроса:
                    // Получение обновлённой заметки в формате JSON, поиск заметки с таким же Id в списке
                    // и замена старой на новую
                    var streamUpdate = new StreamReader(requestContext.Request.InputStream, requestContext.Request.ContentEncoding);
                    var updateItem = JsonSerializer.Deserialize<Item>(streamUpdate.ReadToEnd());
                    var oldItem = items.Where((Item arg) => arg.Id == updateItem.Id).FirstOrDefault();
                    items.Remove(oldItem);
                    items.Add(updateItem);
                    requestContext.Response.Close();
                }
                if (requestContext.Request.Url.AbsolutePath.Contains("delete"))
                {
                    Debug.WriteLine("delete");
                    // Обработка delete-запроса:
                    // Получение Id заметки, которую надо удалить, и удаление её
                    var streamDelete = new StreamReader(requestContext.Request.InputStream, requestContext.Request.ContentEncoding);
                    var deleteId = JsonSerializer.Deserialize<string>(streamDelete.ReadToEnd());
                    var deleteItem = items.Where((Item arg) => arg.Id == deleteId).FirstOrDefault();
                    items.Remove(deleteItem);
                    requestContext.Response.Close();
                }
            }
        }
    }
}