using SQLite;
using Games.Models;

namespace Games.Services;

public class SettingDatabase
{
    private SQLiteAsyncConnection _database;

    public SettingDatabase()
    {
    }

    private async Task Init()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await _database.CreateTableAsync<Setting>();
    }

    public async Task AddNewSetting(int id, int archive, string backPage)
    {
        try
        {
            await Init();
            await _database.InsertAsync(new Setting { Id = id, Archive = archive, SettingBackPage = backPage });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task<int> GetArchiveById(int id)
    {
        try
        {
            await Init();
            var setting = await _database.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
            return setting?.Archive ?? 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 0;
        }
    }

    public async Task UpdateArchive(int id, int archive)
    {
        try
        {
            await Init();
            var setting = await _database.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
            if (setting != null)
            {
                setting.Archive = archive;
                await _database.UpdateAsync(setting);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task UpdateBackPage(int id, string backPage)
    {
        try
        {
            await Init();
            var setting = await _database.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
            if (setting != null)
            {
                setting.SettingBackPage = backPage;
                await _database.UpdateAsync(setting);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task<string> GetBackPageById(int id)
    {
        try
        {
            await Init();
            var setting = await _database.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
            return setting?.SettingBackPage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> IsSettingExists(int id)
    {
        try
        {
            await Init();
            var setting = await _database.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
            return setting != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public async Task UpdateMusicSetting(int id, bool isEnabled)
    {
        try
        {
            await Init();
            var setting = await _database.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
            if (setting != null)
            {
                setting.IsMusicEnabled = isEnabled;
                await _database.UpdateAsync(setting);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task ResetGame(int id)
    {
        try
        {
            await Init();
            var setting = await _database.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
            if (setting != null)
            {
                setting.Archive = 0;
                setting.SettingBackPage = "StartGamePage";
                await _database.UpdateAsync(setting);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task<Setting> GetSettingById(int id)
    {
        try
        {
            await Init();
            return await _database.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }
} 