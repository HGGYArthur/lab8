using System;
using System.Text; 

namespace PhotoCatalogApp
{
    /// <summary>
    /// Представляет фотографию в каталоге.
    /// Содержит основную информацию о файле фотографии.
    /// </summary>
    [Serializable]
    public class Photo
    {
        private string _fileName = string.Empty;
        private string _description = string.Empty;
        private double _fileSizeMB;
        private int _rating;

        /// <summary>
        /// Получает или задает уникальный идентификатор фотографии (ключ).
        /// </summary>
        /// <value>Целочисленный идентификатор.</value>
        /// <remarks>Уникальность ID должна обеспечиваться вызывающим кодом (например, PhotoCatalogManager).</remarks>
        public int Id { get; set; } 

        /// <summary>
        /// Получает или задает имя файла фотографии.
        /// Имя файла не может быть null или пустым.
        /// </summary>
        /// <value>Строка с именем файла.</value>
        /// <exception cref="ArgumentNullException">Выбрасывается, если присваивается null.</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если присваивается пустая строка или строка из пробелов.</exception>
        public string FileName
        {
            get => _fileName;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Имя файла не может быть null.");
                }
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Имя файла не может быть пустым или состоять только из пробелов.", nameof(value));
                }
                _fileName = value;
            }
        }

        /// <summary>
        /// Получает или задает описание или заметки к фотографии.
        /// Может быть пустым, но не null.
        /// </summary>
        /// <value>Строка с описанием. Если присваивается null, устанавливается пустая строка.</value>
        public string Description
        {
            get => _description;
            set
            {
                _description = value ?? string.Empty;
            }
        }

        /// <summary>
        /// Получает или задает дату и время съемки фотографии.
        /// </summary>
        /// <value>Объект <see cref="DateTime"/>, представляющий дату и время съемки.</value>
        public DateTime DateTaken { get; set; } 

        /// <summary>
        /// Получает или задает размер файла фотографии в мегабайтах.
        /// Размер файла не может быть отрицательным.
        /// </summary>
        /// <value>Значение типа double, представляющее размер файла в МБ. Должно быть >= 0.</value>
        /// <exception cref="ArgumentOutOfRangeException">Выбрасывается, если присваивается отрицательное значение.</exception>
        public double FileSizeMB
        {
            get => _fileSizeMB;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Размер файла не может быть отрицательным.");
                }
                _fileSizeMB = value;
            }
        }

        /// <summary>
        /// Получает или задает рейтинг фотографии (например, от 1 до 5).
        /// Рейтинг должен быть в диапазоне от 1 до 5 включительно.
        /// </summary>
        /// <value>Целочисленный рейтинг. Должен быть в диапазоне [1, 5].</value>
        /// <exception cref="ArgumentOutOfRangeException">Выбрасывается, если присваиваемое значение вне диапазона [1, 5].</exception>
        public int Rating
        {
            get => _rating;
            set
            {
                if (value < 1 || value > 5)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Рейтинг должен быть в диапазоне от 1 до 5.");
                }
                _rating = value;
            }
        }

        // --- Конструкторы ---

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Photo"/> значениями по умолчанию.
        /// Устанавливает безопасные значения для свойств.
        /// </summary>
        /// <remarks>
        /// Требуется для некоторых операций сериализации/десериализации.
        /// Устанавливает значения, которые проходят валидацию сеттеров.
        /// </remarks>
        public Photo()
        {
            Id = 0;
            FileName = "DefaultFileName.jpg"; 
            Description = string.Empty; 
            DateTaken = DateTime.MinValue; 
            FileSizeMB = 0.0; 
            Rating = 1;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Photo"/> с указанными параметрами.
        /// Использует сеттеры свойств для выполнения валидации входных данных.
        /// </summary>
        /// <param name="id">Уникальный идентификатор фотографии.</param>
        /// <param name="fileName">Имя файла фотографии. Не может быть null или пустым.</param>
        /// <param name="description">Описание фотографии. Может быть null или пустым.</param>
        /// <param name="dateTaken">Дата и время съемки.</param>
        /// <param name="fileSizeMB">Размер файла в мегабайтах. Не может быть отрицательным.</param>
        /// <param name="rating">Рейтинг фотографии (от 1 до 5).</param>
        /// <exception cref="ArgumentNullException">Выбрасывается сеттером <see cref="FileName"/>, если <paramref name="fileName"/> равен null.</exception>
        /// <exception cref="ArgumentException">Выбрасывается сеттером <see cref="FileName"/>, если <paramref name="fileName"/> пустой.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Выбрасывается сеттером <see cref="FileSizeMB"/>, если <paramref name="fileSizeMB"/> меньше 0.
        /// Выбрасывается сеттером <see cref="Rating"/>, если <paramref name="rating"/> вне диапазона [1, 5].
        /// </exception>
        public Photo(int id, string fileName, string description, DateTime dateTaken, double fileSizeMB, int rating)
        {
            Id = id; 
            FileName = fileName; 
            Description = description; 
            DateTaken = dateTaken; 
            FileSizeMB = fileSizeMB;
            Rating = rating; 
        }


        /// <summary>
        /// Возвращает строковое представление объекта <see cref="Photo"/>.
        /// </summary>
        /// <returns>Отформатированная строка с информацией о фотографии.</returns>
        public override string ToString()
        {
            // Используем System.Text.StringBuilder для более эффективной конкатенации строк
            var builder = new StringBuilder();
            builder.AppendLine($"ID: {Id}");
            builder.AppendLine($"  Имя файла:   {FileName}");
            builder.AppendLine($"  Описание:    {Description}");
            builder.AppendLine($"  Дата съемки: {DateTaken:dd.MM.yyyy HH:mm}"); // Явный формат даты/времени
            builder.AppendLine($"  Размер (МБ): {FileSizeMB:F2}"); // Форматирование до 2 знаков после запятой
            builder.AppendLine($"  Рейтинг:     {Rating}/5");
            builder.Append("-----------------------------------");
            return builder.ToString();
        }
    }
}
