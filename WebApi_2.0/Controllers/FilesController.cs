using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApi_2._0.Data;
using WebApi_2._0.Models;

namespace WebApi_2._0.Controllers
{
    /// <summary>
    /// Контроллер для загрузки и обработки файлов с данными экспериментов.
    /// </summary>
    [Route("science/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly ExperimentsAPIDbContext _dbContext;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FilesController"/> с указанным контекстом базы данных.
        /// </summary>
        /// <param name="dbContext">Контекст базы данных, используемый для операций, связанных с файлами.</param>
        public FilesController(ExperimentsAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Обработка запроса на загрузку файла с данными экспериментов.
        /// </summary>
        /// <param name="file">Файл, который нужно загрузить и обработать.</param>
        /// <returns>Результат обработки запроса.</returns>
        /// /// <response code="200">Файл успешно загружен и обработан.</response>
        /// <response code="400">Некорректный запрос.</response>
        /// <response code="422">Внутренняя ошибка сервера.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            // Валидация файла
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (file.ContentType != "text/csv")
            {
                return BadRequest("Incorrect file extension");
            }

            try
            {
                // Чтение файла и преобразование его в массив байтов (BLOB)
                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                // Чтение файла построчно и обработка данных
                var values = await ReadAndValidateFileData(file);

                if (values.Count == 0)
                {
                    return BadRequest("No valid records found in the file");
                }

                // Создание нового файла или обновление существующего
                await SaveFile(file.FileName, fileData);

                // Сохранение данных экспериментов в базу данных
                await SaveValues(file.FileName, values);

                // Вычисление и сохранение результатов
                await CalculateAndSaveResults(file.FileName, values);

                return Ok();
            }
            catch (Exception ex)
            {
                // Обработка возможных ошибок
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing the file: " + ex.Message);
            }
    }


        private async Task<List<ValueModel>> ReadAndValidateFileData(IFormFile file)
        {
            var values = new List<ValueModel>();
            int lineCount = 0;

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineCount++;

                    // Проверка на количество строк
                    if (lineCount > 10000)
                    {
                        break; // Прервать чтение файла
                    }

                    // Разбор строки и валидация данных
                    if (TryParseLine(line, out var dateTime, out var duration, out var indicator))
                    {
                        // Валидация даты и времени
                        if (dateTime < new DateTime(2000, 1, 1) || dateTime > DateTime.Now)
                        {
                            continue; // Пропустить некорректную запись
                        }

                        // Валидация времени проведения эксперимента и значения показателя
                        if (duration < 0 || indicator < 0)
                        {
                            continue; // Пропустить некорректную запись
                        }

                        values.Add(new ValueModel
                        {
                            FileName = file.FileName,
                            Date = dateTime,
                            ExperimentTime = duration,
                            Indicators = indicator
                        });
                    }
                }
            }

            return values;
        }

        private async Task SaveFile(string fileName, byte[] fileData)
        {
            var existingFile = await _dbContext.Files.FirstOrDefaultAsync(f => f.FileName == fileName);
            if (existingFile != null)
            {
                // Если файл с таким именем существует, обновите его данные
                existingFile.FileData = fileData;
            }
            else
            {
                // Если файла с таким именем нет, создайте новую запись
                var newFile = new FileModel
                {
                    FileName = fileName,
                    FileData = fileData
                };
                _dbContext.Files.Add(newFile);
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task SaveValues(string fileName, List<ValueModel> values)
        {
            // Удалить старые записи с таким же именем файла, если они существуют
            var existingValues = await _dbContext.Values.Where(v => v.FileName == fileName).ToListAsync();
            if (existingValues.Any())
            {
                _dbContext.Values.RemoveRange(existingValues);
                await _dbContext.SaveChangesAsync();
            }

            // Добавить новые записи
            foreach (var value in values)
            {
                _dbContext.Values.Add(value);
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task CalculateAndSaveResults(string fileName, List<ValueModel> values)
        {
            var existingResult = await _dbContext.Results.FirstOrDefaultAsync(r => r.FileName == fileName);

            if (existingResult != null)
            {
                // Обновление существующей записи
                FillResultModel(existingResult, values);
            }
            else
            {
                // Создание новой записи
                var newResult = CreateNewResult(fileName, values);
                _dbContext.Results.Add(newResult);
            }

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Разбирает строку файла и возвращает данные эксперимента, если строка корректна.
        /// </summary>
        /// <param name="line">Строка файла для разбора.</param>
        /// <param name="dateTime">Дата и время начала эксперимента.</param>
        /// <param name="duration">Затраченное время на проведение эксперимента в секундах.</param>
        /// <param name="indicator">Показатель эксперимента.</param>
        /// <returns>True, если строка была успешно разобрана; в противном случае - false.</returns>
        internal static bool TryParseLine(string line, out DateTime dateTime, out int duration, out double indicator)
        {
            dateTime = DateTime.MinValue;
            duration = 0;
            indicator = 0.0;

            var parts = line.Split(';');
            if (parts.Length != 3)
            {
                return false; // Некорректное количество частей в строке
            }

            if (!DateTime.TryParseExact(parts[0], "yyyy-MM-dd_HH-mm-ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                return false; // Не удалось разобрать дату и время
            }

            if (!int.TryParse(parts[1], out duration))
            {
                return false; // Не удалось разобрать время проведения эксперимента
            }

            // Используем неизменяемую культуру для разбора значения показателя с запятой в качестве разделителя дробной части
            if (!double.TryParse(parts[2], NumberStyles.Float, new CultureInfo("ru-RU"), out indicator))
            {
                return false; // Не удалось разобрать показатель
            }

            return true; // Успешный разбор строки
        }


        private void FillResultModel(ResultModel result, List<ValueModel> values)
        {
            //Вычисление времени запуска первого эксперимента
            result.FirstExperiment = values.Min(v => v.Date);

            //Вычисление времени запуска последнего эксперимента
            result.LastExperiment = values.Max(v => v.Date);

            //Вычисление максимального времи проведения эксперимента
            result.MaxTimeExperiment = values.Max(v => v.ExperimentTime);

            //Вычисление минимального времи проведения эксперимента
            result.MinTimeExperiment = values.Min(v => v.ExperimentTime);

            //Вычисление среднего времени проведения эксперимента
            result.AverageTimeExperiment = (double)values.Average(v => v.ExperimentTime);

            //Вычисление среднего значения по показателям
            result.AverageIndicator = (double)values.Average(v => v.Indicators);

            //Вычисление медианы по показателям
            result.MedianIndicator = CalculateMedian(values.Select(v => v.Indicators));

            //Вычисление максимального значения показателя
            result.MaxIndicator = (double)values.Max(v => v.Indicators);

            //Вычисление минимального значения показателя
            result.MinIndicator = (double)values.Min(v => v.Indicators);

            //Вычисление количества выполненных экспериментов
            result.ExperimentCount = values.Count;
        }

        private ResultModel CreateNewResult(string fileName, List<ValueModel> values)
        {
            var newResult = new ResultModel
            {
                FileName = fileName
            };

            FillResultModel(newResult, values);

            return newResult;
        }


        /// <summary>
        /// Вычисляет медиану для набора значений.
        /// </summary>
        /// <param name="values">Набор значений для вычисления медианы.</param>
        /// <returns>Медиана набора значений.</returns>
        internal double CalculateMedian(IEnumerable<double> values)
        {
            var sortedValues = values.OrderBy(v => v).ToList();
            int count = sortedValues.Count;

            if (count % 2 == 0)
            {
                return (sortedValues[(count / 2) - 1] + sortedValues[count / 2]) / 2;
            }
            else
            {
                return sortedValues[count / 2];
            }
        }
    }
}
