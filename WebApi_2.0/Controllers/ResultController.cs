using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi_2._0.Data;
using WebApi_2._0.Models;

namespace WebApi_2._0.Controllers
{
    /// <summary>
    /// Контроллер для получения результатов экспериментов.
    /// </summary>
    [Route("science/results")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly ExperimentsAPIDbContext _dbContext;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ResultsController"/> с указанным контекстом базы данных.
        /// </summary>
        /// <param name="dbContext">Контекст базы данных, используемый для операций, связанных с результатами экспериментов.</param>
        public ResultsController(ExperimentsAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Получение результатов экспериментов с возможностью фильтрации по различным критериям.
        /// </summary>
        /// <param name="fileName">Имя файла для фильтрации.</param>
        /// <param name="minAverageIndicator">Минимальное значение среднего показателя для фильтрации.</param>
        /// <param name="maxAverageIndicator">Максимальное значение среднего показателя для фильтрации.</param>
        /// <param name="minAverageTime">Минимальное значение среднего времени для фильтрации.</param>
        /// <param name="maxAverageTime">Максимальное значение среднего времени для фильтрации.</param>
        /// <returns>Список результатов экспериментов, удовлетворяющих заданным критериям.</returns>
        /// <response code="200">Успешный запрос. Возвращает список результатов экспериментов.</response>
        /// <response code="400">Некорректный запрос.</response>
        /// <response code="404">Ресурс не найден (если применимо).</response>
        /// <response code="422">Некорректные данные запроса (если применимо).</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<ResultModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> GetResults(string? fileName,
            [FromQuery] double? minAverageIndicator, [FromQuery] double? maxAverageIndicator,
            [FromQuery] int? minAverageTime, [FromQuery] int? maxAverageTime)
        {
            try
            {
                IQueryable<ResultModel> query = _dbContext.Results.AsQueryable();

                if (!string.IsNullOrEmpty(fileName))
                {
                    // Применение фильтра по имени файла
                    query = query.Where(r => r.FileName == fileName);
                }

                if (minAverageIndicator.HasValue && maxAverageIndicator.HasValue)
                {
                    // Применение фильтра по среднему показателю (в диапазоне)
                    query = query.Where(r => r.AverageIndicator >= minAverageIndicator &&
                    r.AverageIndicator <= maxAverageIndicator);
                }
                else if (minAverageIndicator.HasValue)
                {
                    query = query.Where(r => r.AverageIndicator >= minAverageIndicator);
                }
                else if (maxAverageIndicator.HasValue)
                {
                    query = query.Where(r => r.AverageIndicator <= maxAverageIndicator);
                }

                if (minAverageTime.HasValue && maxAverageTime.HasValue)
                {
                    // Применение фильтра по среднему времени (в диапазоне)
                    query = query.Where(r => r.AverageTimeExperiment >= minAverageTime &&
                    r.AverageTimeExperiment <= maxAverageTime);
                }
                else if (minAverageTime.HasValue)
                {
                    query = query.Where(r => r.AverageTimeExperiment >= minAverageTime);
                }
                else if (maxAverageTime.HasValue)
                {
                    query = query.Where(r => r.AverageTimeExperiment <= maxAverageTime);
                }

                var results = await query.ToListAsync();
                if (results.Count == 0)
                {
                    return NotFound("No results found matching the criteria.");
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                // Обработка возможных исключений и ошибок
                return StatusCode(StatusCodes.Status422UnprocessableEntity,
                    "An error occurred while processing the request: " + ex.Message);
            }
        }
    }
    }
