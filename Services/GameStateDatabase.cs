using SQLite;
using Games.Models;

namespace Games.Services;

public class GameStateDatabase
{
    private SQLiteAsyncConnection _database;

    public GameStateDatabase()
    {
    }

    async Task Init()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await _database.CreateTableAsync<GameState>();
    }

    public async Task<bool> GetStateAsync(string stateName)
    {
        await Init();
        var state = await _database.Table<GameState>()
            .Where(s => s.StateName == stateName)
            .FirstOrDefaultAsync();
        return state?.Value ?? false;
    }

    public async Task SaveStateAsync(string stateName, bool value)
    {
        await Init();
        var state = await _database.Table<GameState>()
            .Where(s => s.StateName == stateName)
            .FirstOrDefaultAsync();

        if (state == null)
        {
            state = new GameState
            {
                StateName = stateName,
                Value = value
            };
            await _database.InsertAsync(state);
        }
        else
        {
            state.Value = value;
            await _database.UpdateAsync(state);
        }
    }

    public async Task ResetGameStatesAsync()
    {
        await Init();
        await _database.DeleteAllAsync<GameState>();
    }
} 