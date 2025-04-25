using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO; 
using System.Linq;
using System.Text; 

namespace PhotoCatalogApp
{
    /// <summary>
    /// Главный класс приложения, отвечающий за взаимодействие с пользователем
    /// через консольный интерфейс для управления каталогом фотографий.
    /// </summary>
    internal static class Program 
    {
        /// <summary>
        /// Имя файла для хранения данных каталога.
        /// </summary>
        private const string DataFileName = "photocatalog.bin";

        /// <summary>
        /// Менеджер каталога, инкапсулирующий логику работы с данными.
        /// Инициализируется в методе Main.
        /// </summary>
        private static PhotoCatalogManager _catalogManager = null!;

        /// <summary>
        /// Точка входа в приложение.
        /// Инициализирует менеджер каталога, настраивает консоль
        /// и запускает главный цикл обработки команд пользователя.
        /// </summary>
        /// <param name="args">Аргументы командной строки (не используются).</param>
        static void Main(string[] args)
        {
            // Настройка кодировки консоли для корректного отображения кириллицы
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            // Установка инвариантной культуры для парсинга чисел и дат
            // Это гарантирует, что точка '.' будет разделителем дробной части
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            Console.WriteLine("Добро пожаловать в Менеджер Каталога Фотографий!");

            // Получаем полный путь к файлу данных в директории приложения
            string dataFilePath = Path.Combine(AppContext.BaseDirectory, DataFileName);
            _catalogManager = new PhotoCatalogManager(dataFilePath);

            bool running = true;
            while (running)
            {
                DisplayMainMenu();
                Console.Write("Введите номер опции: ");
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ViewCatalog();
                        break;
                    case "2":
                        AddPhotoUI();
                        break;
                    case "3":
                        DeletePhotoUI();
                        break;
                    case "4":
                        RunQueriesUI();
                        break;
                    case "0":
                        running = false;
                        Console.WriteLine("Завершение работы приложения...");
                        break;
                    default:
                        Console.WriteLine("Ошибка: Неверный ввод. Пожалуйста, выберите опцию из меню (0-4).");
                        break;
                }

                if (running)
                {
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                }
            }

            Console.WriteLine("Работа программы завершена.");
        }


        /// <summary>
        /// Отображает главное меню приложения в консоли.
        /// </summary>
        private static void DisplayMainMenu()
        {
            Console.Clear(); // Очистка экрана перед отображением меню
            Console.WriteLine("======= Главное Меню =======");
            Console.WriteLine("1. Просмотреть весь каталог");
            Console.WriteLine("2. Добавить новую фотографию");
            Console.WriteLine("3. Удалить фотографию по ID");
            Console.WriteLine("4. Выполнить запросы к каталогу");
            Console.WriteLine("----------------------------");
            Console.WriteLine("0. Выход");
            Console.WriteLine("============================");
        }

        /// <summary>
        /// Отображает все фотографии из каталога в консоли.
        /// </summary>
        private static void ViewCatalog()
        {
            Console.Clear();
            Console.WriteLine("======= Каталог Фотографий =======");
            IEnumerable<Photo> photos = _catalogManager.GetAllPhotos();

            if (!photos.Any()) // Проверка на наличие элементов с помощью LINQ
            {
                Console.WriteLine("Каталог пуст.");
            }
            else
            {
                foreach (var photo in photos)
                {
                    Console.WriteLine(photo);
                }
            }
            Console.WriteLine("=================================");
        }

        /// <summary>
        /// Обрабатывает пользовательский ввод для добавления новой фотографии в каталог.
        /// Включает валидацию вводимых данных.
        /// </summary>
        private static void AddPhotoUI()
        {
            Console.Clear();
            Console.WriteLine("======= Добавление Новой Фотографии =======");
            try
            {

                int id = _catalogManager.GetNextAvailableId();

                string fileName = Validate.ReadString("Введите имя файла: ");
                string description = Validate.ReadString("Введите описание (можно оставить пустым): ", allowEmpty: true);
                DateTime dateTaken = Validate.ReadDateTime("Введите дату и время съемки"); // Использует формат по умолчанию
                double fileSizeMB = Validate.ReadDouble("Введите размер файла (МБ): ", min: 0.0); // Размер не может быть отрицательным
                int rating = Validate.ReadInt("Введите рейтинг (1-5): ", min: 1, max: 5) ?? 0; // Валидация диапазона

                Photo newPhoto = new Photo(id, fileName, description, dateTaken, fileSizeMB, rating);

                _catalogManager.AddPhoto(newPhoto);

            }
            catch (FormatException ex) // Ошибка парсинга числа или даты
            {
                Console.WriteLine($"Ошибка формата ввода: {ex.Message}");
            }
            catch (ArgumentException ex) // Ошибки из конструктора Photo или методов Read*
            {
                Console.WriteLine($"Ошибка ввода данных: {ex.Message}");
            }
            catch (Exception ex) // Другие ошибки
            {
                Console.WriteLine($"Произошла непредвиденная ошибка при добавлении: {ex.Message}");
            }
            Console.WriteLine("==========================================");
        }

        /// <summary>
        /// Обрабатывает пользовательский ввод для удаления фотографии по ID.
        /// </summary>
        private static void DeletePhotoUI()
        {
            Console.Clear();
            Console.WriteLine("======= Удаление Фотографии =======");
            try
            {
                int idToDelete = Validate.ReadInt("Введите ID фотографии для удаления: ") ?? 0;
                _catalogManager.DeletePhoto(idToDelete);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Ошибка формата ввода: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка при удалении: {ex.Message}");
            }
            Console.WriteLine("===================================");
        }

        /// <summary>
        /// Отображает меню запросов и обрабатывает выбор пользователя.
        /// </summary>
        private static void RunQueriesUI()
        {
            bool queryMenuRunning = true;
            while (queryMenuRunning)
            {
                Console.Clear();
                Console.WriteLine("========== Меню Запросов ==========");
                Console.WriteLine("1. Показать фото с рейтингом N или выше");
                Console.WriteLine("2. Показать фото, снятые после даты D");
                Console.WriteLine("3. Показать общее количество фото");
                Console.WriteLine("4. Показать самую 'тяжелую' фотографию");
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("0. Назад в главное меню");
                Console.WriteLine("===================================");
                Console.Write("Выберите запрос: ");
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        QueryByRating();
                        break;
                    case "2":
                        QueryByDate();
                        break;
                    case "3":
                        QueryTotalCount();
                        break;
                    case "4":
                        QueryLargestPhoto();
                        break;
                    case "0":
                        queryMenuRunning = false;
                        break;
                    default:
                        Console.WriteLine("Ошибка: Неверный выбор. Пожалуйста, выберите опцию из меню (0-4).");
                        break;
                }
                if (queryMenuRunning)
                {
                    Console.WriteLine("\nНажмите Enter для возврата в меню запросов...");
                    Console.ReadLine();
                }
            }
        }


        /// <summary>
        /// Выполняет и отображает запрос на поиск фотографий по минимальному рейтингу.
        /// </summary>
        private static void QueryByRating()
        {
            Console.WriteLine("\n--- Запрос: Фотографии по рейтингу ---");
            try
            {
                int minRating = Validate.ReadInt("Введите минимальный рейтинг (1-5): ", min: 1, max: 5) ?? 0;
                IEnumerable<Photo> results = _catalogManager.GetPhotosByMinRating(minRating);
                DisplayQueryResults(results, $"Фотографии с рейтингом {minRating} или выше:");
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Ошибка формата ввода: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при выполнении запроса: {ex.Message}");
            }
        }

        /// <summary>
        /// Выполняет и отображает запрос на поиск фотографий, снятых после указанной даты.
        /// </summary>
        private static void QueryByDate()
        {
            Console.WriteLine("\n--- Запрос: Фотографии после даты ---");
            try
            {
                DateTime date = Validate.ReadDateTime("Введите дату", justDate: true); // Запрос только даты
                IEnumerable<Photo> results = _catalogManager.GetPhotosTakenAfter(date);
                DisplayQueryResults(results, $"Фотографии, снятые после {date:dd.MM.yyyy}:");
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Ошибка формата ввода: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при выполнении запроса: {ex.Message}");
            }
        }

        /// <summary>
        /// Выполняет и отображает запрос на получение общего количества фотографий.
        /// </summary>
        private static void QueryTotalCount()
        {
            Console.WriteLine("\n--- Запрос: Общее количество фотографий ---");
            try
            {
                int count = _catalogManager.GetTotalPhotoCount();
                Console.WriteLine($"Всего фотографий в каталоге: {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при подсчете: {ex.Message}");
            }
            Console.WriteLine("------------------------------------------");
        }

        /// <summary>
        /// Выполняет и отображает запрос на поиск фотографии с наибольшим размером файла.
        /// </summary>
        private static void QueryLargestPhoto()
        {
            Console.WriteLine("\n--- Запрос: Самая 'тяжелая' фотография ---");
            try
            {
                Photo? largest = _catalogManager.GetLargestPhoto();
                if (largest != null)
                {
                    Console.WriteLine("Найдена фотография с максимальным размером:");
                    Console.WriteLine(largest);
                }
                else
                {
                    Console.WriteLine("Каталог пуст, нечего искать.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при поиске: {ex.Message}");
            }
            Console.WriteLine("------------------------------------------");
        }

        /// <summary>
        /// Отображает результаты запроса (список фотографий) в консоли.
        /// </summary>
        /// <param name="results">Коллекция фотографий для отображения.</param>
        /// <param name="title">Заголовок для результатов запроса.</param>
        private static void DisplayQueryResults(IEnumerable<Photo> results, string title)
        {
            Console.WriteLine($"\n--- {title} ---");
            // Проверяем, что results не null перед использованием Any()
            if (results != null && results.Any())
            {
                foreach (var photo in results)
                {
                    Console.WriteLine(photo);
                }
            }
            else
            {
                Console.WriteLine("По вашему запросу ничего не найдено.");
            }
            Console.WriteLine("-----------------------------------");
        }

    }       
}
