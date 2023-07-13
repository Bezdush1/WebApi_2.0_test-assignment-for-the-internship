using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApi_2._0.Models
{
    /// <summary>
    /// Класс, реализующий набор результатов
    /// <param name="Id">id</param>
    /// <param name="FirstExperiment">Время запуска первого эксперимента</param>
    /// <param name="LastExperiment">Время запуска последнего эксперимента</param>
    /// <param name="MaxTimeExperiment">Максимальное время проведения эксперимента</param>
    /// <param name="MinTimeExperiment">Минимальное время проведения эксперимента</param>
    /// <param name="MiddleTimeExperiment">Среднее время проведения эксперимента</param>
    /// <param name="MiddleCountResult">Среднее значение по показателям</param>
    /// <param name="MedianByIndicators">Медиана по показателям</param>
    /// <param name="MaxValueIndicator">Максимальное значение показателя</param>
    /// <param name="MinValueIndicator">Минимальное значение показателя</param>
    /// <param name="ExperimentCount">Количество выполненных экспериментов</param>
    /// </summary>
    public class ResultModel
    {
        [Key]
        [ForeignKey("File")]
        public string FileName { get; set; }
        public FileModel File { get; set; }

        [Required]
        public DateTime FirstExperiment { get; set; }

        [Required]
        public DateTime LastExperiment { get; set; }

        [Required]
        public int MaxTimeExperiment { get; set; }

        [Required]
        public int MinTimeExperiment { get; set; }

        [Required]
        public int MiddleTimeExperiment { get; set; }

        [Required]
        public int MiddleCountResult { get; set; }

        [Required]
        public double MedianByIndicators { get; set; }

        [Required]
        public int MaxValueIndicator { get; set; }

        [Required]
        public int MinValueIndicator { get; set; }

        [Required]
        public int ExperimentCount { get; set; }
    }
}
