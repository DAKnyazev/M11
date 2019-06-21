using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using M11.Common.Extentions;
using M11.Common.Models.BillSummary;
using SQLite;

namespace M11.Common
{
    public class GenericDatabase
    {
        private static GenericDatabase _database;
        private static SQLiteAsyncConnection _connection;

        private GenericDatabase() {}

        public static GenericDatabase GetDatabase(string dbPath)
        {
            if (_database == null)
            {
                _database = new GenericDatabase();
                _connection = new SQLiteAsyncConnection(dbPath);
                AsyncHelpers.RunSync(() => _connection.CreateTablesAsync<MonthBillSummary, MonthBillGroup, Bill>());
            }

            return _database;
        }

        public async Task<List<TEntity>> GetItemsAsync<TEntity>() where TEntity : IDatabaseEntity, new()
        {
            try
            {
                return await _connection.Table<TEntity>().ToListAsync();
            }
            catch (Exception e)
            {
                return new List<TEntity>();
            }
        }

        public async Task<int> SaveItemAsync<TEntity>(TEntity item) where TEntity : IDatabaseEntity, new()
        {
            try
            {
                var oldItem = AsyncHelpers.RunSync(() => _connection.Table<TEntity>().ToListAsync()).FirstOrDefault(x => x.Id == item.Id);
                if (oldItem != null)
                {
                    return await _connection.UpdateAsync(item);
                }

                return await _connection.InsertAsync(item);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public async Task<int> ClearTablesAsync()
        {
            try
            {
                var result = await _connection.DeleteAllAsync<Bill>();
                result += await _connection.DeleteAllAsync<MonthBillGroup>();
                result += await _connection.DeleteAllAsync<MonthBillSummary>();
                return result;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
    }
}
