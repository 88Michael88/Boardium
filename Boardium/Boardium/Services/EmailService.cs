using Boardium.Models.ServiceModels;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Boardium.Services;
public class EmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly EmailTemplateRenderer _emailTemplateRenderer;

    public EmailService(IOptions<EmailSettings> emailSettings,EmailTemplateRenderer emailTemplateRenderer)
    {
        _emailSettings = emailSettings.Value;
        _emailTemplateRenderer = emailTemplateRenderer;
    }

    public async Task SendConfirmationAsync(string toEmail, string userFirstName, string gameTitle, string qrUrl,
        string pickupCode)
    {
        var body = _emailTemplateRenderer.Render("Confirmation", new()

        {
            {"Name",userFirstName},
            {"GameTitle", gameTitle},
            {"QrUrl", qrUrl},
            {"PickupCode", pickupCode}
        });
        await SendEmailAsync(toEmail,"Potwierdzenie rezerwacji", body);
    }
    public async Task SendReminderAsync(string toEmail, string userFirstName, string date, string gameTitle)
    {
        var body = _emailTemplateRenderer.Render("Reminder", new()
        {
            { "Name", userFirstName },
            { "Date", date },
            { "GameTitle", gameTitle }
        });

        await SendEmailAsync(toEmail, "Przypomnienie o oddaniu gry", body);
    }
    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Boardium", _emailSettings.From));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html")
        {
            Text = body
        };
        
        using var client = new MailKit.Net.Smtp.SmtpClient();
        try
        {
            await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, _emailSettings.EnableSsl);
            await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
            await client.SendAsync(message);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to send email", ex);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}