using SQLite;

namespace Games.Models;

public class GameState
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string StateName { get; set; }
    public bool Value { get; set; }
} 