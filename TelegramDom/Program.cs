using System;

using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Globalization;
using Windows.Media.SpeechSynthesis;
using System.Speech.Recognition;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramDom

{
    class Program
    {
        
        static TelegramBotClient bot;

        static void Main(string[] args)
        {

            string token = "";//токен для работы 


            bot = new TelegramBotClient(token);
            bot.OnMessage += MessageListener;
            bot.StartReceiving();//страт для телеграмма 
            Console.ReadKey();
        }
        public static void MessageListener(object sender, MessageEventArgs e)
        { 
            string text = $"{ DateTime.Now.ToLongTimeString()}: {e.Message.Chat.FirstName} {e.Message.Chat.Id} {e.Message.Text}";//общая информация про сообщение 
            Console.WriteLine($"{text} TypeMessage: {e.Message.Type.ToString()}");// вывод в консольку 
            Message(e);//запуск метода с условием сообщения 
        }

        static void Message(MessageEventArgs e)//метод с условиями 
        {
            Telegram.Bot.Types.Message message = e.Message;//переменную с сообщением. Создал для удобства, но использовал только для фотографий 
            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)//если сообщение: документ
            {
                DownLoad(e.Message.Document.FileId, e.Message.Document.FileName);//метод по скачиванию документа 
            }
            if (e.Message.Text == "/help")//команда посказка 
            {
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Привет, этот телеграм бот предназначен для сохранение документов, аудиосообщений и разной другой информации. Тут также есть команда '/start' которая дает пользователю выбор в просмотре файлов");//
            }
            if(e.Message.Text == "/start")//команда для вывода кнопок с выбором просмотра файлов 
            {
                StartDownfile(e);//метод с кнопками 
            }
            if (message.Photo != null && message.Photo.Length > 0)// если сообщение: фотография 
            {
               
                DownPhoto(message);//метод по скачиванию фотографии
             
                
            }
            if (e.Message.Audio != null)//если сообщение: аудио 
            {
                DownAudi(e.Message.Audio.FileId, e.Message.Audio.Title);// метод по скачиванию музыки 
            }
            
        }
        static async void DocumentsDownloads(MessageEventArgs e)//передает все документы , которые есть в телеграмме 
        {
            
                string[] allFoundFiles = Directory.GetFiles(@"C:\Users\poc18\source\repos\TelegramDom\TelegramDom\bin\Debug\net5.0", "*.txt", SearchOption.AllDirectories);//массив с путем к документам 
                foreach (string file in allFoundFiles)//перебор массива 
                {
                Console.WriteLine(file);// путь документа 
                var minifile = file;// переменная для потока 
                FileStream fs = new FileStream(minifile, FileMode.Open);// поток с открытием документа 
                await bot.SendDocumentAsync(e.Message.Chat.Id,  fs);// сообщения бота с документом 
                fs.Close();// закрытие потока 
                fs.Dispose();
                    
                }
            
        }
        static async void AudioDownloads(MessageEventArgs e)//передает все аудио , которые есть в телеграмме 
        {
            string[] allFoundFiles = Directory.GetFiles(@"C:\Users\poc18\source\repos\TelegramDom\TelegramDom\bin\Debug\net5.0", "*.MP3", SearchOption.AllDirectories);//массив с путем к аудио 
            foreach (string file in allFoundFiles)//перебор массива
            {
                Console.WriteLine(file);// путь документа
                var minifile = file;// переменная для потока 
                FileStream fas = new FileStream(file, FileMode.Open);// поток с открытием аудио 
                await bot.SendAudioAsync(e.Message.Chat.Id, fas);// сообщения бота с аудио  
                fas.Close();// закрытие потока 
                fas.Dispose();


            }
        }
        static async void PhotoDownloads(MessageEventArgs e)//передает все фотографии , которые есть в телеграмме
        {
            string[] allfoundFiles = Directory.GetFiles(@"C:\Users\poc18\source\repos\TelegramDom\TelegramDom\bin\Debug\net5.0", "*.jpg", SearchOption.AllDirectories);//массив с путем к фотографиям 
            foreach (string file in allfoundFiles)//перебор массива
            {
                Console.WriteLine(file);// путь документа

                var minifile = file;// переменная для потока
                FileStream fs = new FileStream(minifile, FileMode.Open);// поток с открытием фотографиям
                await bot.SendPhotoAsync(e.Message.Chat.Id, fs);// сообщения бота с фотографиям
                fs.Close();// закрытие потока 
                fs.Dispose();
            }
        }
        private static void DownPhoto(Telegram.Bot.Types.Message message)//метод по скачиванию фотографии
        {
            

            var photo = message.Photo.OrderByDescending(p => p.FileSize).First();//нахождение айди 
            var file = bot.GetFileAsync(photo.FileId).Result;//создание файла 
            if(file != null)//условие с файлом 
            {
                var filename = DownloadFile(file, message.From);
                SaveCaption(filename, message.Caption);
                
            }
        }
        private static string DownloadFile(Telegram.Bot.Types.File file, Telegram.Bot.Types.User from)//скачивание фотографии
        {
            var username = GetUsername(from); 

            var originalPath = file.FilePath;
            string pathToFile = BuildPathToFile(username, originalPath);
            using (var outputFile = File.Create(pathToFile + ".cpy"))// создание потока
            {

                var fileStream = bot.DownloadFileAsync(originalPath, outputFile, new System.Threading.CancellationToken());
                Task.WaitAll(fileStream);
            }
            File.Move(pathToFile + ".cpy", pathToFile);
            return pathToFile;
        }
        private static string BuildPathToFile(string username, string originalPath)//создания пути 
        {

            var filename = originalPath.Substring(originalPath.LastIndexOf("/") + 1);
            var identifier = filename.Substring(1, filename.LastIndexOf("."));
            var extension = filename.Substring(filename.LastIndexOf("."));
            var pathToFile = Path.Combine( string.Join("-", filename, username).Trim('-') + extension);
            return pathToFile;
        }
        private static string GetUsername(Telegram.Bot.Types.User from)//создание имени
        {
            var username = "";

            if (!string.IsNullOrEmpty(from.FirstName))
            {
                username = from.FirstName;
            }

            if (!string.IsNullOrEmpty(from.LastName))
            {
                username = string.Join("_", username, from.LastName).Trim('_');
            }

            if (string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(from.Username))
            {
                username = from.Username;
            }

            return username;
        }
        static async void DownLoad(string fileId, string path)//скачивание документа 
        {
           
            var file = await bot.GetFileAsync(fileId);// файл
            FileStream fs = new FileStream("_" + path + ".txt", FileMode.Create);//поток для скачивания 
            await bot.DownloadFileAsync(file.FilePath, fs);//скачивание для телеграмма 
            fs.Close();// закрытие потока 

            fs.Dispose();
        }
        private static  void SaveCaption(string filename, string caption)//сохранение фотографии 
        {
            var picture = new FileInfo(filename);
            string path = picture.FullName.Replace(picture.Extension, ".jpg");
            File.WriteAllText(path, caption);
        }
        static async void DownAudi(string FileId, string path)//скачивание аудио 
        {
            var file = await bot.GetFileAsync(FileId);//файл
            FileStream stream = new FileStream("+" + path + ".MP3", FileMode.Create );//поток для скачивания
            await bot.DownloadFileAsync(file.FilePath, stream);//скачивание для телеграмма 
            stream.Close();//закрытие потока
            stream.Dispose();
        }
        static async void StartDownfile(MessageEventArgs e)//метод по создание 3х кнопок и каждая кнопка отдает, либо документы, либо фотографии, либо айдио
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]//созадние кнопок
            {
                        // первый ряд
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("просмотр документов", "1"),// первая кнопка. "просмотр документов" - это что будет писаться на кнопке, а 1 это Идентификатор
                            InlineKeyboardButton.WithCallbackData("просмотр музыки", "2"),// вторая кнопка. "просмотр музыки" - это что будет писаться на кнопке, а 2 это Идентификатор
                        },
                        // второй ряд
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Просмотр фотографий", "3"),// третья кнопка. "Просмотр фотографий" - это что будет писаться на кнопке, а 3 это Идентификатор

                        }
            });
            await bot.SendTextMessageAsync(e.Message.Chat.Id, "Нажмите на кнопку:", replyMarkup: inlineKeyboard);//текс над кнопками
            bot.OnCallbackQuery += async (object sc, CallbackQueryEventArgs ev) =>
            {
                var message = ev.CallbackQuery.Message;// переменная для switch
                switch(ev.CallbackQuery.Data)
                {
                    case "1":
                        DocumentsDownloads(e);//метод по передачи документов
                        break;
                    case "2":
                        AudioDownloads(e);//метод по передачи аудио
                        break;
                    case "3":
                        PhotoDownloads(e);//метод по передачи фотографий
                        break;
                    default:
                        break;
                }
            };
        }
    }
}
