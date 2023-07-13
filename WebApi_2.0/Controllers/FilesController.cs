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
using static System.Net.WebRequestMethods;

namespace WebApi_2._0.Controllers
{
    [Route("science/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly ExperimentsAPIDbContext _dbContext;

        public FilesController(ExperimentsAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            // Валидация файла
            if (file == null || file.Length == 0 )
            {
                return BadRequest("No file uploaded");
            }

            if (file.ContentType != "text/csv")
            {
                return BadRequest("Incorrect file extension");
            }

            // Чтение файла построчно и обработка данных
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string line;
                int lineCount = 0;
                var values = new List<ValueModel>();

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
                            Date = dateTime,
                            ExperimentTime = duration,
                            Indicators = indicator
                        });
                    }
                }

                if (values.Count == 0)
                {
                    return BadRequest("No valid records found in the file");
                }

                // Создание нового файла или обновление существующего
                var fileModel = await SaveFile(file.FileName);
                await SaveValues(fileModel.FileName, values);
                await CalculateAndSaveResults(fileModel.FileName);
            }

            return Ok();
        }

        private async Task<FileModel> SaveFile(string fileName)
        {
            var existingFile = await _dbContext.Files.FindAsync(fileName);

            if (existingFile == null)
            {
                var newFile = new FileModel { FileName = fileName };
                _dbContext.Files.Add(newFile);
                await _dbContext.SaveChangesAsync();
                return newFile;
            }
            else
            {
                return existingFile;
            }
        }

        private async Task SaveValues(string fileName, List<ValueModel> values)
        {
            foreach (var value in values)
            {
                value.FileName = fileName;
                _dbContext.Values.Add(value);
            }

            await _dbContext.SaveChangesAsync();
        }


        private async Task CalculateAndSaveResults(string fileName)
        {
            var values = await _dbContext.Values.Where(v => v.FileName == fileName).ToListAsync();

            var existingResult = await _dbContext.Results.FirstOrDefaultAsync(r => r.FileName == fileName);

            if (existingResult != null)
            {
                // Обновление существующей записи
                existingResult.FirstExperiment = values.Min(v => v.Date);
                existingResult.LastExperiment = values.Max(v => v.Date);
                existingResult.MaxTimeExperiment = values.Max(v => v.ExperimentTime);
                existingResult.MinTimeExperiment = values.Min(v => v.ExperimentTime);
                existingResult.MiddleTimeExperiment = (int)values.Average(v => v.ExperimentTime);
                existingResult.MiddleCountResult = (int)values.Average(v => v.Indicators);
                existingResult.MedianByIndicators = CalculateMedian(values.Select(v => v.Indicators));
                existingResult.MaxValueIndicator = (int)values.Max(v => v.Indicators);
                existingResult.MinValueIndicator = (int)values.Min(v => v.Indicators);
                existingResult.ExperimentCount = values.Count;

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                // Создание новой записи
                var results = new ResultModel
                {
                    FileName = fileName,
                    FirstExperiment = values.Min(v => v.Date),
                    LastExperiment = values.Max(v => v.Date),
                    MaxTimeExperiment = values.Max(v => v.ExperimentTime),
                    MinTimeExperiment = values.Min(v => v.ExperimentTime),
                    MiddleTimeExperiment = (int)values.Average(v => v.ExperimentTime),
                    MiddleCountResult = (int)values.Average(v => v.Indicators),
                    MedianByIndicators = CalculateMedian(values.Select(v => v.Indicators)),
                    MaxValueIndicator = (int)values.Max(v => v.Indicators),
                    MinValueIndicator = (int)values.Min(v => v.Indicators),
                    ExperimentCount = values.Count
                };

                _dbContext.Results.Add(results);
                await _dbContext.SaveChangesAsync();
            }
        }


        private double CalculateMedian(IEnumerable<double> values)
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

        private bool TryParseLine(string line, out DateTime dateTime, out int duration, out double indicator)
        {
            dateTime = DateTime.MinValue;
            duration = 0;
            indicator = 0.0;

            var parts = line.Split(';');
            if (parts.Length != 3)
            {
                return false; // Некорректное количество частей в строке
            }


            if (!DateTime.TryParseExact(parts[0], "yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                return false; // Не удалось разобрать дату и время
            }

            if (!int.TryParse(parts[1], out duration))
            {
                return false; // Не удалось разобрать время проведения эксперимента
            }

            if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out indicator))
            {
                return false; // Не удалось разобрать показатель
            }

            return true; // Успешный разбор строки
        }
    }
}
