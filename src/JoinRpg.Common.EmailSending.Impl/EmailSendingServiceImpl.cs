using JoinRpg.DataModel;
using JoinRpg.Interfaces;
using JoinRpg.Interfaces.Email;
using JoinRpg.Markdown;
using Mailgun.Messages;
using Mailgun.Service;
using Microsoft.Extensions.Options;

namespace JoinRpg.Common.EmailSending.Impl;

public class EmailSendingServiceImpl(IOptions<MailGunOptions> config, IHttpClientFactory httpClientFactory, IOptions<NotificationsOptions> notificationsOptions) : IEmailSendingService
{
    private bool EmailEnabled { get; } = !string.IsNullOrWhiteSpace(config.Value.ApiDomain) && !string.IsNullOrWhiteSpace(config.Value.ApiKey);
    private MessageService MessageService { get; } = new MessageService(config.Value.ApiKey, httpClientFactory);

    public string GetUserDependentValue(string valueKey) => "%recipient." + valueKey + "%";


    public string GetRecepientPlaceholderName() => GetUserDependentValue(Constants.MailGunName);


    public async Task SendEmails(string subject,
        string body,
        string text,
        RecepientData sender,
        IReadOnlyCollection<RecepientData> to)
    {
        if (!to.Any())
        {
            return;
        }

        foreach (var recepientChunk in to.Chunk(Mailgun.Constants.MaximumAllowedRecipients))
        {
            await SendEmailChunkImpl(recepientChunk, subject, text, sender, body);
        }
    }

    public async Task SendEmails(string subject, MarkdownString body, RecepientData sender, IReadOnlyCollection<RecepientData> to)
    {
        if (!to.Any())
        {
            return;
        }

        var html = body.ToHtmlString().ToHtmlString();
        var text = body.ToPlainText().ToString();

        foreach (var recepientChunk in to.Chunk(Mailgun.Constants.MaximumAllowedRecipients))
        {
            await SendEmailChunkImpl(recepientChunk, subject, text, sender, html);
        }
    }

    private async Task SendEmailChunkImpl(IReadOnlyCollection<RecepientData> recipients,
        string subject,
        string text,
        RecepientData sender,
        string html)
    {
        var message = new MessageBuilder().AddUsers(recipients)
            .SetSubject(subject)
            .SetFromAddress(new Recipient()
            {
                DisplayName = sender.DisplayName,
                Email = notificationsOptions.Value.ServiceAccountEmail,
            })
            .SetReplyToAddress(sender.ToMailGunRecepient())
            .SetTextBody(text)
            .SetHtmlBody(html)
            .GetMessage();

        message.Dkim = true;

        message.RecipientVariables = recipients.ToRecipientVariables();
        if (EmailEnabled)
        {
            var response = await MessageService.SendMessageAsync(config.Value.ApiDomain, message);
            if (!response.IsSuccessStatusCode)
            {
                throw new EmailSendFailedException(
                    $"Failed to send email. Response is {response.StatusCode} {response.ReasonPhrase}");
            }
        }
    }
}
