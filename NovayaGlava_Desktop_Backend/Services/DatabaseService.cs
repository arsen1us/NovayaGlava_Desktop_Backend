using ClassLibForNovayaGlava_Desktop.UserModel;

namespace NovayaGlava_Desktop_Backend.Services
{
    public interface IDatabaseService<T> where T : class
    {
        // Получить все записи
        Task<List<T>> FindAllAsync();
        // Получить запись по id 
        Task<T> FindByIdAsync(string userId);
        // Вставить 1 запись
        Task InsertOneAsync(T obj);
        // Вставить несколько записей
        Task InsertManyAsync(List<T> obj);
        // Обновить запись
        Task UpdateAsync(string id, T obj);
        // Удалить запись
        Task DeleteAsync(string id);
    }
}
