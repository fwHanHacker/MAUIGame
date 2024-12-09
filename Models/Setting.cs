using SQLite;

namespace Games.Models
{
    [Table("Setting")]
    public class Setting
    {
        [PrimaryKey, Unique]
        public int Id { get; set; }
        
        public int Archive { get; set; }

        [MaxLength(255)]
        public string SettingBackPage { get; set; }

        public bool IsMusicEnabled { get; set; } = true;
    }
}