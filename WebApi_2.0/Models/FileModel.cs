using System.ComponentModel.DataAnnotations;

namespace WebApi_2._0.Models
{
    /// <summary>
    /// Класс, представляющий модель файла.
    /// </summary>
    public class FileModel
    {
        /// <summary>
        /// Получает или устанавливает имя файла.
        /// </summary>
        [Key]
        public string? FileName { get; set; }

        /// <summary>
        /// Получает или устанавливает массив байтов данных файла.
        /// </summary>
        [Required]
        public byte[]? FileData { get; set; }
    }
}
