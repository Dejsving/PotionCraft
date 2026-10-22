using PotionCraft.Server.Services.Email;

namespace PotionCraft.Tests.Services.Email;

/// <summary>
/// Тесты для SmtpEmailSender.BuildMessage: формирование письма перед отправкой.
/// </summary>
public class SmtpEmailSenderTests
{
    /// <summary>
    /// Проверяет, что BuildMessage заполняет отправителя, получателя, тему и текст с кириллицей.
    /// </summary>
    [Fact]
    public void BuildMessage_SetsFromToSubjectAndBody()
    {
        var options = new EmailOptions { From = "PotionCraft <noreply@example.com>" };
        const string to = "player@example.com";
        const string subject = "Код подтверждения";
        const string body = "Ваш код: 123456. Никому его не сообщайте.";

        var message = SmtpEmailSender.BuildMessage(options, to, subject, body);

        var from = Assert.Single(message.From.Mailboxes);
        Assert.Equal("noreply@example.com", from.Address);
        Assert.Equal("PotionCraft", from.Name);

        var recipient = Assert.Single(message.To.Mailboxes);
        Assert.Equal(to, recipient.Address);

        Assert.Equal(subject, message.Subject);

        var textPart = Assert.IsType<MimeKit.TextPart>(message.Body);
        Assert.Equal(MimeKit.Text.TextFormat.Plain, textPart.Format);
        Assert.Equal(body, textPart.Text);
    }
}
