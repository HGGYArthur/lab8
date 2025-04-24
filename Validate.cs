using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoCatalogApp
{
    class Validate
    {
        /// <summary>
        /// Считывает строку из консоли, опционально проверяя на пустоту.
        /// </summary>
        /// <param name="prompt">Сообщение для пользователя.</param>
        /// <param name="allowEmpty">Разрешен ли пустой ввод (только пробелы считаются пустым вводом).</param>
        /// <returns>Введенная пользователем строка.</returns>
        public static string ReadString(string prompt, bool allowEmpty = false)
        {
            string? input;
            do
            {
                Console.Write($"{prompt}: ");
                input = Console.ReadLine();
                if (input == null)
                {
                    throw new InvalidOperationException("Ввод не был предоставлен.");
                }

                if (!allowEmpty && string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Ошибка: Ввод не может быть пустым.");
                }
            } while (!allowEmpty && string.IsNullOrWhiteSpace(input));
            return input;
        }

        /// <summary>
        /// Считывает целое число из консоли с валидацией диапазона и возможностью значения по умолчанию.
        /// </summary>
        /// <param name="prompt">Сообщение для пользователя.</param>
        /// <param name="min">Минимально допустимое значение (включительно).</param>
        /// <param name="max">Максимально допустимое значение (включительно).</param>
        /// <param name="defaultValue">Значение, возвращаемое, если пользователь нажмет Enter.</param>
        /// <returns>Введенное пользователем целое число или null, если было введено значение по умолчанию.</returns>
        /// <exception cref="FormatException">Если введенное значение не является целым числом.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Если введенное значение вне диапазона min/max.</exception>
        public static int? ReadInt(string prompt, int? min = null, int? max = null, int? defaultValue = null)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                string? input = Console.ReadLine();

                if (defaultValue.HasValue && string.IsNullOrWhiteSpace(input))
                {
                    // Валидируем значение по умолчанию на всякий случай
                    if (min.HasValue && defaultValue.Value < min.Value) throw new ArgumentOutOfRangeException(nameof(defaultValue), $"Значение по умолчанию {defaultValue.Value} меньше минимального {min.Value}.");
                    if (max.HasValue && defaultValue.Value > max.Value) throw new ArgumentOutOfRangeException(nameof(defaultValue), $"Значение по умолчанию {defaultValue.Value} больше максимального {max.Value}.");
                    return defaultValue.Value;
                }

                if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                {
                    if (min.HasValue && value < min.Value)
                    {
                        Console.WriteLine($"Ошибка: Значение должно быть не меньше {min.Value}.");
                        continue; // Повторить ввод
                    }
                    if (max.HasValue && value > max.Value)
                    {
                        Console.WriteLine($"Ошибка: Значение должно быть не больше {max.Value}.");
                        continue; // Повторить ввод
                    }
                    return value; // Успешный ввод и валидация
                }
                else
                {
                    // Если ввод не пустой и не парсится, это ошибка формата
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        Console.WriteLine("Ошибка: Введите корректное целое число.");
                        // Не выбрасываем исключение, а просим повторить ввод
                    }
                    else if (!defaultValue.HasValue) // Если пустой ввод не разрешен (нет defaultValue)
                    {
                        Console.WriteLine("Ошибка: Ввод не может быть пустым.");
                    }
                    // Если был пустой ввод, но defaultValue нет, цикл просто повторится
                }
            }
        }

        /// <summary>
        /// Считывает число с плавающей точкой (double) из консоли с использованием инвариантной культуры.
        /// </summary>
        /// <param name="prompt">Сообщение для пользователя.</param>
        /// <param name="min">Минимально допустимое значение (включительно).</param>
        /// <returns>Введенное пользователем число.</returns>
        /// <exception cref="FormatException">Если введенное значение не является числом.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Если введенное значение меньше min.</exception>
        public static double ReadDouble(string prompt, double? min = null)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                string? input = Console.ReadLine();

                if (double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double value))
                {
                    if (min.HasValue && value < min.Value)
                    {
                        Console.WriteLine($"Ошибка: Значение должно быть не меньше {min.Value:F2}."); // Форматируем вывод min
                        continue;
                    }
                    return value;
                }
                else
                {
                    Console.WriteLine("Ошибка: Введите корректное число (используйте точку '.' как десятичный разделитель).");
                }
            }
        }

        /// <summary>
        /// Считывает дату и время (или только дату) из консоли.
        /// </summary>
        /// <param name="prompt">Сообщение для пользователя.</param>
        /// <param name="justDate">Если true, ожидается только дата (дд.мм.гггг), иначе дата и время (дд.мм.гггг чч:мм).</param>
        /// <returns>Введенная пользователем дата и время.</returns>
        /// <exception cref="FormatException">Если введенная строка не соответствует ожидаемому формату.</exception>
        public static DateTime ReadDateTime(string prompt, bool justDate = false)
        {
            // Определяем ожидаемые форматы
            string expectedFormat = justDate ? "dd.MM.yyyy" : "dd.MM.yyyy HH:mm";
            // Дополнительно разрешаем ввод только даты, даже если ожидалось время
            string alternativeFormat = justDate ? "" : "dd.MM.yyyy";
            string[] formats = string.IsNullOrEmpty(alternativeFormat)
                ? new[] { expectedFormat }
                : new[] { expectedFormat, alternativeFormat };


            while (true)
            {
                Console.Write($"{prompt} (формат: {expectedFormat}): ");
                string? input = Console.ReadLine();

                if (DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime value))
                {
                    // Если ввели только дату, а требовалось время, время будет 00:00:00
                    return value;
                }
                else
                {
                    Console.WriteLine($"Ошибка: Введите дату{(justDate ? "" : " и время")} в корректном формате ({expectedFormat}).");
                }
            }
        }
    }
}

