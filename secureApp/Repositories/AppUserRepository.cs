using Microsoft.EntityFrameworkCore;
using MyCodeFirstProject.Data;
using MyCodeFirstProject.Models;
public class AppUserRepository : IAppUserRepository
{
    private readonly ApplicationDbContext _context;
    public AppUserRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<AppUser>> GetAllAsync() 
        => await _context.AppUsers.OrderByDescending(u => u.Id).ToListAsync();

    public async Task<AppUser?> GetByIdAsync(int id) 
        => await _context.AppUsers.FindAsync(id);

    public async Task InsertAsync(AppUser user) 
    { 
        await _context.AppUsers.AddAsync(user); 
        await _context.SaveChangesAsync(); 
    }

    public async Task UpdateAsync(AppUser user) 
    { 
        _context.AppUsers.Update(user); 
        await _context.SaveChangesAsync(); 
    }

    public async Task DeleteAsync(int id) 
    { 
        var u = await _context.AppUsers.FindAsync(id); 
        if (u != null) { _context.AppUsers.Remove(u); await _context.SaveChangesAsync(); } 
    }
}