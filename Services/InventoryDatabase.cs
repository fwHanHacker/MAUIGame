using SQLite;
using Games.Models;

namespace Games.Services;

public class InventoryDatabase
{
    private SQLiteAsyncConnection _database;

    public InventoryDatabase()
    {
    }

    async Task Init()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await _database.CreateTableAsync<InventoryItem>();
    }

    public async Task<List<InventoryItem>> GetInventoryItemsAsync()
    {
        await Init();
        return await _database.Table<InventoryItem>().ToListAsync();
    }

    public async Task<InventoryItem> GetInventoryItemAsync(string itemName)
    {
        await Init();
        return await _database.Table<InventoryItem>()
            .Where(i => i.ItemName == itemName)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveInventoryItemAsync(InventoryItem item)
    {
        await Init();
        if (item.Id != 0)
        {
            return await _database.UpdateAsync(item);
        }
        else
        {
            return await _database.InsertAsync(item);
        }
    }

    public async Task<int> DeleteInventoryItemAsync(InventoryItem item)
    {
        await Init();
        return await _database.DeleteAsync(item);
    }

    public async Task ResetInventoryAsync()
    {
        await Init();
        await _database.DeleteAllAsync<InventoryItem>();
    }
} 