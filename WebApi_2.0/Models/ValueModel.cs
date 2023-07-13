using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApi_2._0.Models
{
    /// <summary>
    /// Класс, реализующий интерфейс данных, полученных из файла 
    /// <param name="Id"></param>
    /// <param name="Date">Дата и время начала эксперимента</param>
    /// <param name="ExperimentTime">Затраченное время на проведение эксперимента в секундах</param>
    /// <param name="Indicators">Показатель эксперимента</param>
    /// </summary>
    public class ValueModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int ExperimentTime { get; set; }

        [Required]
        public double Indicators { get; set; }

        [ForeignKey("FileName")]
        public string FileName { get; set; }
        public FileModel File { get; set; }
    }
}
