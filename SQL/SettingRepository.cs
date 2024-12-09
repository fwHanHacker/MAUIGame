using SQLite;
using Games.Models;

namespace Games.SQL;

public class SettingRepository
{
    private string _dbPath;
    private SQLiteAsyncConnection conn;
    private async Task Init()
    {
        if (conn != null)
            return;

        conn = new SQLiteAsyncConnection(_dbPath);
        await conn.CreateTableAsync<Setting>();
    }

    public SettingRepository(string dbPath)
    {
        _dbPath = dbPath;
    }

    public async Task AddNewSetting(int id, int archive,string backPage)
    {
        int result = 0;
        try
        {
            await Init();

            result = await conn.InsertAsync(new Setting { Id = id, Archive = archive ,SettingBackPage=backPage});
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
            var setting = await conn.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
            return setting.Archive;
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
            var setting = await conn.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
            setting.Archive = archive;
            await conn.UpdateAsync(setting);
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
            var setting = await conn.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
            setting.SettingBackPage = backPage;
            await conn.UpdateAsync(setting);
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
            var setting = await conn.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
            return setting.SettingBackPage;
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
            var setting = await conn.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
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
        await Init();
        var setting = await conn.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
        if (setting != null)
        {
            setting.IsMusicEnabled = isEnabled;
            await conn.UpdateAsync(setting);
        }
    }

    public async Task ResetGame(int id)
    {
        await Init();
        var setting = await conn.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
        if (setting != null)
        {
            setting.Archive = 0;
            setting.SettingBackPage = "StartGamePage";
            await conn.UpdateAsync(setting);
        }
    }

    public async Task<Setting> GetSettingById(int id)
    {
        try
        {
            await Init();
            var setting = await conn.Table<Setting>().FirstOrDefaultAsync(x => x.Id == id);
            return setting;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }
}