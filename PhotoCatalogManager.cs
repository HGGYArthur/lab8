using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

#pragma warning disable SYSLIB0011 

namespace PhotoCatalogApp
{
    /// <summary>
    /// Управляет каталогом фотографий, включая загрузку, сохранение,
    /// добавление, удаление и выполнение запросов к данным,
    /// хранящимся в бинарном файле.
    /// </summary>
    public class PhotoCatalogManager
    {
        private List<Photo> _photos;
        private readonly string _filePath;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PhotoCatalogManager"/>.
        /// Загружает данные из указанного файла или создает пустой каталог, если файл не найден.
        /// </summary>
        /// <param name="filePath">Путь к бинарному файлу для хранения данных каталога.</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="filePath"/> равен null.</exception>
        public PhotoCatalogManager(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _photos = LoadData(); // Загрузка данных при инициализации
        }


        /// <summary>
        /// Загружает список фотографий из бинарного файла.
        /// Если файл не найден или пуст, возвращает новый пустой список.
        /// Обрабатывает ошибки чтения файла и десериализации.
        /// </summary>
        /// <returns>Список объектов <see cref="Photo"/>.</returns>
        private List<Photo> LoadData()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"Предупреждение: Файл данных '{_filePath}' не найден. Будет создан новый пустой каталог при первом сохранении.");
                return new List<Photo>();
            }

            if (new FileInfo(_filePath).Length == 0)
            {
                Console.WriteLine($"Предупреждение: Файл данных '{_filePath}' пуст. Загружен пустой каталог.");
                return new List<Photo>();
            }

            try
            {
                using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
                {
                    // BinaryFormatter требует осторожного использования из-за рисков безопасности.
                    BinaryFormatter formatter = new BinaryFormatter();
#pragma warning disable CS8603 // Possible null reference return. - Подавление предупреждения, т.к. каст может вернуть null, но мы ожидаем List<Photo>
                    return (List<Photo>)formatter.Deserialize(fs);
#pragma warning restore CS8603
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Ошибка: Файл данных '{_filePath}' не найден при попытке чтения.");
                return new List<Photo>();
            }
            catch (SerializationException ex)
            {
                Console.WriteLine($"Ошибка десериализации данных из '{_filePath}': {ex.Message}. Возможно, формат файла поврежден или несовместим. Будет создан пустой каталог.");
                return new List<Photo>();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка ввода-вывода при чтении файла '{_filePath}': {ex.Message}.");
                return new List<Photo>();
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Непредвиденная ошибка при загрузке данных из '{_filePath}': {ex.Message}");
                return new List<Photo>();
            }
        }

        /// <summary>
        /// Сохраняет текущее состояние каталога фотографий в бинарный файл.
        /// Перезаписывает существующий файл.
        /// </summary>
        /// <returns>true, если сохранение прошло успешно; иначе false.</returns>
        private bool SaveData()
        {
            try
            {
                // Создаем директорию, если она не существует
                string? directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (FileStream fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, _photos);
                    return true;
                }
            }
            catch (SerializationException ex)
            {
                Console.WriteLine($"Ошибка сериализации данных при сохранении в '{_filePath}': {ex.Message}");
                return false;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка ввода-вывода при сохранении файла '{_filePath}': {ex.Message}");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Ошибка доступа при сохранении файла '{_filePath}': {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Непредвиденная ошибка при сохранении данных в '{_filePath}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Возвращает перечисление всех фотографий в каталоге.
        /// </summary>
        /// <returns>Коллекция <see cref="IEnumerable{Photo}"/>, содержащая все фотографии. Возвращает копию списка.</returns>
        public IEnumerable<Photo> GetAllPhotos()
        {
            return _photos.ToList(); // Возвращаем копию, чтобы предотвратить модификацию исходного списка извне.
        }

        /// <summary>
        /// Удаляет фотографию с указанным идентификатором из каталога.
        /// </summary>
        /// <param name="id">Идентификатор фотографии для удаления.</param>
        /// <returns>true, если фотография была найдена, удалена и данные успешно сохранены; иначе false.</returns>
        public bool DeletePhoto(int id)
        {
            // Используем LINQ для поиска индекса
            int indexToRemove = _photos.FindIndex(p => p.Id == id);

            if (indexToRemove != -1) // Если элемент найден
            {
                _photos.RemoveAt(indexToRemove);
                if (SaveData())
                {
                    Console.WriteLine($"Фотография с ID={id} успешно удалена.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Фотография с ID={id} была удалена из памяти, но не удалось сохранить изменения в файл.");
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"Фотография с ID={id} не найдена.");
                return false;
            }
        }

        /// <summary>
        /// Добавляет новую фотографию в каталог.
        /// </summary>
        /// <param name="newPhoto">Объект <see cref="Photo"/> для добавления. Не должен быть null.</param>
        /// <returns>true, если фотография успешно добавлена (уникальный ID) и данные сохранены; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="newPhoto"/> равен null.</exception>
        public bool AddPhoto(Photo newPhoto)
        {
            if (newPhoto == null)
            {
                throw new ArgumentNullException(nameof(newPhoto), "Нельзя добавить null объект фотографии.");
            }

            // Проверка на уникальность Id с помощью LINQ Any()
            if (_photos.Any(p => p.Id == newPhoto.Id))
            {
                Console.WriteLine($"Ошибка: Фотография с ID={newPhoto.Id} уже существует в каталоге.");
                return false;
            }

            _photos.Add(newPhoto);
            if (SaveData())
            {
                Console.WriteLine($"Фотография '{newPhoto.FileName}' (ID={newPhoto.Id}) успешно добавлена.");
                return true;
            }
            else
            {
                Console.WriteLine($"Фотография '{newPhoto.FileName}' была добавлена в память, но не удалось сохранить изменения в файл.");
                // Удаляем только что добавленный элемент, чтобы сохранить консистентность
                _photos.Remove(newPhoto); // Удаляем по ссылке
                return false;
            }
        }


        /// <summary>
        /// Находит все фотографии с рейтингом не ниже указанного.
        /// </summary>
        /// <param name="minRating">Минимальный допустимый рейтинг (включительно).</param>
        /// <returns>Коллекция <see cref="IEnumerable{Photo}"/> фотографий, удовлетворяющих условию, отсортированная по убыванию рейтинга.</returns>
        public IEnumerable<Photo> GetPhotosByMinRating(int minRating)
        {
            return _photos.Where(p => p.Rating >= minRating)
                          .OrderByDescending(p => p.Rating)
                          .ToList();
        }

        /// <summary>
        /// Находит все фотографии, снятые после указанной даты.
        /// </summary>
        /// <param name="date">Дата, после которой должны быть сняты фотографии.</param>
        /// <returns>Коллекция <see cref="IEnumerable{Photo}"/> фотографий, удовлетворяющих условию, отсортированная по дате съемки.</returns>
        public IEnumerable<Photo> GetPhotosTakenAfter(DateTime date)
        {
            return _photos.Where(p => p.DateTaken > date)
                          .OrderBy(p => p.DateTaken)
                          .ToList();
        }

        /// <summary>
        /// Возвращает общее количество фотографий в каталоге.
        /// </summary>
        /// <returns>Целое число, представляющее количество фотографий.</returns>
        public int GetTotalPhotoCount()
        {
            return _photos.Count();
        }

        /// <summary>
        /// Находит фотографию с самым большим размером файла.
        /// </summary>
        /// <returns>Объект <see cref="Photo"/> с максимальным значением <see cref="Photo.FileSizeMB"/>,
        /// или null, если каталог пуст.</returns>
        public Photo? GetLargestPhoto() 
        {
            if (!_photos.Any())
            {
                return null;
            }
            return _photos.OrderByDescending(p => p.FileSizeMB).FirstOrDefault();
        }


        /// <summary>
        /// Генерирует следующий доступный уникальный идентификатор для новой фотографии.
        /// Находит максимальный существующий ID и возвращает значение на единицу больше.
        /// Если каталог пуст, возвращает 1.
        /// </summary>
        /// <returns>Следующий доступный целочисленный ID.</returns>
        public int GetNextAvailableId()
        {
            if (!_photos.Any())
            {
                return 1; // Начинаем с 1, если список пуст
            }
            // Находим максимальное значение Id среди всех фотографий и добавляем 1
            return _photos.Max(p => p.Id) + 1;
        }
    }
}
#pragma warning restore SYSLIB0011 // Восстанавливаем предупреждение
