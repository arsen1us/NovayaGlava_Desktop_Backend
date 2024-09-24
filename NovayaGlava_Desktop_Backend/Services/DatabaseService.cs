namespace NovayaGlava_Desktop_Backend.Services
{
    public interface IDatabaseService<T> where T : class
    {
        // Получить все записи
        Task<List<T>> FindAsync();
        // Получить запись по id 
        Task<T> FindAsync(string userId);
        // Вставить 1 запись
        Task InsertOneAsync(T obj);
        // Вставить несколько записей
        Task InsertManyAsync(List<T> obj);
        // Обновить запись
        Task UpdateAsync(string id, T obj);
        // Удалить запись
        Task DeleteAsync(string id);
    }

    public class DatabaseService<T> : IDatabaseService<T> where T : class
    {
        public async Task<List<T>> FindAsync()
        {
            try
            {
                throw new Exception("");
            }
            catch(Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        // Получить запись по id 
        public async Task<T> FindAsync(string userId)
        {
            try
            {
                throw new Exception("");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        // Вставить 1 запись
        public async Task InsertOneAsync(T obj)
        {
            try
            {
                throw new Exception("");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        // Вставить несколько записей
        public async Task InsertManyAsync(List<T> obj)
        {
            try
            {
                throw new Exception("");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        // Обновить запись
        public async Task UpdateAsync(string id, T obj)
        {
            try
            {
                throw new Exception("");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        // Удалить запись
        public async Task DeleteAsync(string id)
        {
            try
            {
                throw new Exception("");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
