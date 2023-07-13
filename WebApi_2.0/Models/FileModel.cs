using System.ComponentModel.DataAnnotations;

namespace WebApi_2._0.Models
{
/// <summary>
/// Класс, реализующий интерфейс файла
/// <param name="Id">id</param>
/// <param name="FileName">Имя файла</param>
/// </summary>
    public class FileModel
    {
        [Key]
        public string FileName { get; set; }
    }
}
