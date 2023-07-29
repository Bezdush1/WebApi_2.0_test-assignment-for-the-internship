using Microsoft.EntityFrameworkCore;
using WebApi_2._0.Models;

namespace WebApi_2._0.Data
{
    /// <summary>
    /// Класс, представляющий контекст базы данных для API экспериментов.
    /// </summary>
    public class ExperimentsAPIDbContext : DbContext
    {
        /// <summary>
        /// Конструктор контекста базы данных.
        /// </summary>
        /// <param name="options">Опции для настройки контекста.</param>
        public ExperimentsAPIDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        /// <summary>
        /// Получает или устанавливает набор данных о файлах.
        /// </summary>
        public DbSet<FileModel> Files { get; set; }

        /// <summary>
        /// Получает или устанавливает набор данных об экспериментах.
        /// </summary>
        public DbSet<ValueModel> Values { get; set; }

        /// <summary>
        /// Получает или устанавливает набор результатов экспериментов.
        /// </summary>
        public DbSet<ResultModel> Results { get; set; }
    }
}
