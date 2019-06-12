using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using M11.Common.Models.BillSummary;
using SQLite;

namespace M11.Common
{
    public class GenericDatabase 
    {
        private readonly SQLiteAsyncConnection _connection;

        public GenericDatabase(string dbPath)
        {
            try
            {
                _connection = new SQLiteAsyncConnection(dbPath);
                _connection.CreateTableAsync<MonthBillSummary>().Wait();
                _connection.CreateTableAsync<MonthBillGroup>().Wait();
                _connection.CreateTableAsync<Bill>().Wait();
            }
            catch
            {
                // ignored
            }
        }

        public async Task<List<TEntity>> GetItemsAsync<TEntity>() where TEntity : IDatabaseEntity, new()
        {
            try
            {
                return await _connection.Table<TEntity>().ToListAsync();
            }
            catch
            {
                return new List<TEntity>();
            }
        }

        public async Task<int> SaveItemAsync<TEntity>(TEntity item) where TEntity : IDatabaseEntity, new()
        {
            try
            {
                var oldItem = _connection.Table<TEntity>().ToListAsync().Result.FirstOrDefault(x => x.Id == item.Id);
                if (oldItem != null)
                {
                    return await _connection.UpdateAsync(item);
                }

                return await _connection.InsertAsync(item);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<int> ClearAsync<TEntity>() where TEntity : IDatabaseEntity, new()
        {
            try
            {
                return await _connection.DeleteAllAsync<TEntity>();
            }
            catch
            {
                return 0;
            }
        }
    }
}
