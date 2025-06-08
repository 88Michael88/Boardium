using Boardium.Data;
using Boardium.Models.Game;
using Microsoft.EntityFrameworkCore;

namespace Boardium.Areas.Admin.Services;

public class PublishersService : IPublishersService
{
    private readonly BoardiumContext _context;
    private readonly ILogger<IPublishersService> _logger;
    public PublishersService(BoardiumContext context, ILogger<IPublishersService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Publisher>> GetAllAsync()
    {
        var publishers = await _context.Publishers.ToListAsync();
        return publishers;
    }

    public async Task<Publisher?> GetByIdAsync(int id)
    {
        var publisher = await _context.Publishers
            .FirstOrDefaultAsync(m => m.Id == id);
        return publisher;
    }
    public async Task<bool> CreateAsync(Publisher publisher)
    {
        if (publisher == null) throw new ArgumentNullException(nameof(publisher));

        _context.Add(publisher);
        return await _context.SaveChangesAsync() > 0;
    }
    public async Task<bool> UpdateAsync(Publisher publisher)
    {
        if (publisher == null) throw new ArgumentNullException(nameof(publisher));

        try
        {
            _context.Update(publisher);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ExistsAsync(publisher.Id))
            {
                return false;
            }
            else
            {
                throw;
            }
        }
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var publisher = await _context.Publishers.FindAsync(id);
        if (publisher == null)
        {
            return false;
        }

        _context.Publishers.Remove(publisher);
        return await _context.SaveChangesAsync() > 0;
    }
    public async Task<bool> ExistsAsync(int id)
    {
        return _context.Publishers.Any(e => e.Id == id);
    }
}