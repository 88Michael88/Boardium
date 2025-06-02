using Boardium.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Boardium.Services.Background;

public class ReminderService : BackgroundService
{
    private readonly ILogger<ReminderService> _logger;
    private readonly EmailService _emailService;
    private readonly ReminderServiceOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public ReminderService(ILogger<ReminderService> logger, EmailService emailService,
        IServiceScopeFactory scopeFactory,
        IOptions<ReminderServiceOptions> options)
    {
        _logger = logger;
        _emailService = emailService;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SendingEmailService is starting.");
        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay;
            if (_options.IsTestMode)
            {
                delay = TimeSpan.FromMinutes(1);
                _logger.LogInformation("Testing email service.");
            }
            else
            {
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(1).AddHours(18);
                delay = nextRun - now;

                if (delay < TimeSpan.Zero)
                {
                    _logger.LogInformation("Next run already passed, sending immediately.");
                    delay = TimeSpan.Zero;
                }
                else
                {
                    _logger.LogInformation($"Production mode: waiting until {nextRun} to send reminders.");
                }
            }
            await Task.Delay(delay, stoppingToken);
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<BoardiumContext>();
                var tomorrow = DateTime.Today.AddDays(1);
                var dayAfterTomorrow = tomorrow.AddDays(1);
                var dueRentals = await context.Rentals
                    .Include(r => r.ApplicationUser)
                    .Include(r=>r.GameCopy)
                    .ThenInclude(gc => gc.Game)
                    .Where(r => r.DueDate >= tomorrow && r.DueDate < dayAfterTomorrow &&
                                r.ReturnedAt == null)
                    .ToListAsync(stoppingToken);
                foreach (var dueRental in dueRentals)
                {
                    _logger.LogInformation($"Sending reminder for {dueRental.ApplicationUser.UserName}");
                    var toEmail = dueRental.ApplicationUser!.Email!;
                    var userName = dueRental.ApplicationUser.FirstName;
                    var dueDate = dueRental.DueDate.ToString("dd/MM/yyyy");
                    var gameTitle = dueRental.GameCopy!.Game.Title;
                    await _emailService.SendReminderAsync(toEmail,userName,dueDate,gameTitle);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending emails.");
            }
            
        }

        _logger.LogInformation("SendingEmailService is stopping.");
    }
}