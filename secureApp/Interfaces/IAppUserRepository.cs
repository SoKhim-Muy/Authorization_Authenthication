using MyCodeFirstProject.Models;

public interface IAppUserRepository
{
    Task<IEnumerable<AppUser>> GetAllAsync();
    Task<AppUser?> GetByIdAsync(int id);
    Task InsertAsync(AppUser user);
    Task UpdateAsync(AppUser user);
    Task DeleteAsync(int id);
}