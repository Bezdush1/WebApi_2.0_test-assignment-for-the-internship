using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApi_2._0.Models
{
    /// <summary>
    /// Класс, представляющий данные, полученные из файла.
    /// </summary>
    public class ValueModel
    {
        /// <summary>
        /// Получает или устанавливает уникальный идентификатор данных.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Получает или устанавливает дату и время начала эксперимента.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Получает или устанавливает время, затраченное на проведение эксперимента в секундах.
        /// </summary>
        [Required]
        public int ExperimentTime { get; set; }

        /// <summary>
        /// Получает или устанавливает показатель эксперимента.
        /// </summary>
        [Required]
        public double Indicators { get; set; }

        /// <summary>
        /// Получает или устанавливает имя файла, к которому относятся данные.
        /// </summary>
        [ForeignKey("File")]
        public string? FileName { get; set; }

        /// <summary>
        /// Получает или устанавливает связанный объект файла, к которому относятся данные.
        /// </summary>
        public FileModel? File { get; set; }
    }
}
