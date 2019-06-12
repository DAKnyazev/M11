using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

namespace M11.Common
{
    public class GenericDatabase<TEntity> where TEntity : IDatabaseEntity, new()
    {
        private readonly SQLiteAsyncConnection _connection;

        public GenericDatabase(string dbPath)
        {
            _connection = new SQLiteAsyncConnection(dbPath);
            _connection.CreateTableAsync<TEntity>().Wait();
        }

        public async Task<List<TEntity>> GetItemsAsync()
        {
            return await _connection.Table<TEntity>().ToListAsync();
        }

        public async Task<int> SaveItemAsync(TEntity item)
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
            catch (Exception e)
            {
                return 0;
            }
        }

        public async Task<int> ClearAsync()
        {
            return await _connection.DeleteAllAsync<TEntity>();
        }
    }
}
