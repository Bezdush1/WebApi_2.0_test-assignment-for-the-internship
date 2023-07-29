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
    /// Контроллер для получения данных из файла.
    /// </summary>
    [Route("science/values")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ExperimentsAPIDbContext _dbContext;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ValuesController"/> с указанным контекстом базы данных.
        /// </summary>
        /// <param name="dbContext">Контекст базы данных, используемый для операций, связанных с значениями.</param>
        public ValuesController(ExperimentsAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Получение данных экспериментов из файла по его имени.
        /// </summary>
        /// <param name="fileName">Имя файла, для которого нужно получить данные.</param>
        /// <returns>Список данных экспериментов из указанного файла.</returns>
        /// <response code="200">Успешный запрос. Возвращает список данных экспериментов.</response>
        /// <response code="404">Файл не найден. Если данные для указанного файла отсутствуют.</response>
        [HttpGet("{fileName}")]
        [ProducesResponseType(typeof(List<ValueModel>), 200)] 
        [ProducesResponseType(404)] 
        public async Task<IActionResult> GetValues(string fileName)
        {
            var values = await _dbContext.Values
                .Where(v => v.FileName == fileName)
                .ToListAsync();

            if (values.Count == 0)
            {
                return NotFound("No values found for the specified file name");
            }

            return Ok(values);
        }
    }
}
