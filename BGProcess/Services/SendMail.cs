using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using SendGrid;
using BGProcess.Interface;
using BGProcess.Models;

namespace BGProcess.Services
{
    public class SendMail : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ISendGridClient _sendGridClient;
        private readonly ILogger<SendMail> _logger;
        private readonly IEmailQueue _emailQueue;

        public SendMail(IConfiguration configuration, ISendGridClient sendGridClient, ILogger<SendMail> logger, IEmailQueue emailQueue)
        {
            _sendGridClient = sendGridClient;
            _configuration = configuration;
            _logger = logger;
            _emailQueue = emailQueue;

            // Validasi konfigurasi pada saat startup
            string apiKey = _configuration["SendGrid:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("SendGrid API key is not configured.");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email processing loop has started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"QUEUE COUNT: {_emailQueue.Queue.Count}");
                if (_emailQueue.Queue.Count > 0)
                {
                    await ProcessEmails(stoppingToken);
                }
                else
                {
                    await Task.Delay(10000, stoppingToken); // Tunggu 10 detik sebelum memeriksa lagi
                }
            }

            _logger.LogInformation("Email Service Stopped");
        }

        private async Task ProcessEmails(CancellationToken stoppingToken)
        {
            while (_emailQueue.Queue.Count > 0 && !stoppingToken.IsCancellationRequested)
            {
                if (_emailQueue.Queue.TryDequeue(out var emailMessage))
                {
                    _logger.LogInformation($"Processing email for {emailMessage.ToEmail}...");
                    await SendEmail(emailMessage, stoppingToken);
                }
            }
        }

        private async Task SendEmail(EmailMessage emailMessage, CancellationToken stoppingToken)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("accsylz@gmail.com", "MyDataApp");
            var to = new EmailAddress(emailMessage.ToEmail);
            var plainTextContent = emailMessage.Message;
            var htmlContent = $"<strong>{System.Net.WebUtility.HtmlEncode(emailMessage.Message)}</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, emailMessage.Subject, plainTextContent, htmlContent);

            try
            {
                var response = await client.SendEmailAsync(msg, stoppingToken);

                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    _logger.LogError($"Failed to send email to {emailMessage.ToEmail}. Status Code: {response.StatusCode}");
                }
                else
                {
                    _logger.LogInformation($"Email sent to {emailMessage.ToEmail} with subject: {emailMessage.Subject}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred while sending email to {emailMessage.ToEmail}: {ex.Message}");
            }
        }
    }
}
