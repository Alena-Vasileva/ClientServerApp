using System;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.Json;
using System.IO;
using System.Threading;

namespace Server
{
    internal class Program
    {
        static readonly List<Item> items;
        private static string local = "http://localhost:8080";

        static Program()
        {
            items = new List<Item>();
            items.Add(new Item { Id = Guid.NewGuid().ToString(), Text = "It WORKS", Description = "It fucking works." });
        }

        static void Main(string[] args)
        {
            var httpListener = new HttpListener();
            // ngrok http 8080 -host-header="localhost:8080
            // http://2c8f-149-154-117-63.ngrok.io/get/
            httpListener.Prefixes.Add(local + "/get/");
            httpListener.Prefixes.Add(local + "/add/");
            httpListener.Prefixes.Add(local + "/update/");
            httpListener.Prefixes.Add(local + "/delete/");
            httpListener.Start();
            Debug.WriteLine("started");
            while (true)
            {
                var requestContext = httpListener.GetContext();
                requestContext.Response.StatusCode = 200;
                Debug.WriteLine("point");
                if (requestContext.Request.Url.AbsolutePath.Contains("get"))
                {
                    Debug.WriteLine("get");
                    var streamGet = requestContext.Response.OutputStream;
                    var text = JsonSerializer.Serialize<IEnumerable<Item>>(items);
                    var bytes = Encoding.UTF8.GetBytes(text);
                    streamGet.Write(bytes, 0, bytes.Length);
                    requestContext.Response.Close();
                }
                if (requestContext.Request.Url.AbsolutePath.Contains("add"))
                {
                    Debug.WriteLine("add");
                    var streamAdd = new StreamReader(requestContext.Request.InputStream, requestContext.Request.ContentEncoding);
                    var newItem = JsonSerializer.Deserialize<Item>(streamAdd.ReadToEnd());
                    items.Add(newItem);
                    requestContext.Response.Close();
                }
                if (requestContext.Request.Url.AbsolutePath.Contains("update"))
                {
                    Debug.WriteLine("update");
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
                    var streamDelete = new StreamReader(requestContext.Request.InputStream, requestContext.Request.ContentEncoding);
                    var deleteId = JsonSerializer.Deserialize<string>(streamDelete.ReadToEnd());
                    var deadItem = items.Where((Item arg) => arg.Id == deleteId).FirstOrDefault();
                    items.Remove(deadItem);
                    requestContext.Response.Close();
                }
            }
        }
    }
}