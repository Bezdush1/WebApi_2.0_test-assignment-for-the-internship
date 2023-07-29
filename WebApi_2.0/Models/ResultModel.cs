using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApi_2._0.Models
{
    /// <summary>
    /// Класс, представляющий набор результатов экспериментов.
    /// </summary>
    public class ResultModel
    {
        /// <summary>
        /// Получает или устанавливает уникальный идентификатор набора результатов.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Получает или устанавливает имя файла, к которому относится набор результатов.
        /// </summary>
        [ForeignKey("File")]
        public string? FileName { get; set; }

        /// <summary>
        /// Получает или устанавливает связанный объект файла,
        /// к которому относится набор результатов.
        /// </summary>
        public FileModel? File { get; set; }

        /// <summary>
        /// Получает или устанавливает время запуска первого эксперимента.
        /// </summary>
        [Required]
        public DateTime FirstExperiment { get; set; }

        /// <summary>
        /// Получает или устанавливает время запуска последнего эксперимента.
        /// </summary>
        [Required]
        public DateTime LastExperiment { get; set; }

        /// <summary>
        /// Получает или устанавливает максимальное время проведения эксперимента.
        /// </summary>
        [Required]
        public int MaxTimeExperiment { get; set; }

        /// <summary>
        /// Получает или устанавливает минимальное время проведения эксперимента.
        /// </summary>
        [Required]
        public int MinTimeExperiment { get; set; }

        /// <summary>
        /// Получает или устанавливает среднее время проведения эксперимента.
        /// </summary>
        [Required]
        public int AverageTimeExperiment { get; set; }

        /// <summary>
        /// Получает или устанавливает среднее значение по показателям.
        /// </summary>
        [Required]
        public int AverageIndicator { get; set; }

        /// <summary>
        /// Получает или устанавливает медиану по показателям.
        /// </summary>
        [Required]
        public double MedianIndicator { get; set; }

        /// <summary>
        /// Получает или устанавливает максимальное значение показателя.
        /// </summary>
        [Required]
        public int MaxIndicator { get; set; }

        /// <summary>
        /// Получает или устанавливает минимальное значение показателя.
        /// </summary>
        [Required]
        public int MinIndicator { get; set; }

        /// <summary>
        /// Получает или устанавливает количество выполненных экспериментов.
        /// </summary>
        [Required]
        public int ExperimentCount { get; set; }
    }
}
