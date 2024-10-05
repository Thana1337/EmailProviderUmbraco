using Azure;
using Azure.Communication.Email;
using EmailProvider.Dtos;
using EmailProvider.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace EmailProvider.Functions
{
    public class SendEmailFunction
    {
        private readonly EmailClient _emailClient;
        private readonly ILogger<SendEmailFunction> _logger;

        public SendEmailFunction(ILogger<SendEmailFunction> logger, EmailClient emailClient)
        {
            _emailClient = emailClient;
            _logger = logger;
        }

        [Function(nameof(SendEmailFunction))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Processing request to send email.");

            // Read the request body and deserialize it into an EmailDto object
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var emailData = JsonConvert.DeserializeObject<EmailDto>(requestBody);

            // Validate the incoming data: Check only required fields (like Email)
            if (emailData == null || string.IsNullOrEmpty(emailData.Email))
            {
                _logger.LogWarning("Invalid request: Missing required email field.");
                return new BadRequestObjectResult("Please provide a valid email address.");
            }

            // Optional fields can be null or empty and should be handled accordingly.
            var emailRequest = new EmailRequest()
            {
                To = emailData.Email,
                Subject = "Confirmation",
                HtmlBody = $@"
                <html>
                    <body>
                        <h1>Thank you for contacting Onatrix!</h1>
                        <p>We will contact you back at {emailData.Email} {(string.IsNullOrEmpty(emailData.Phone) ? "" : $"or {emailData.Phone}")}.</p>
                        <p>{(!string.IsNullOrEmpty(emailData.Message) ? $"You mentioned: \"{emailData.Message}\"" : "")}</p>
                        <p>{(!string.IsNullOrEmpty(emailData.Service) ? $"We will assist you with: \"{emailData.Service}\"" : "")}</p>
                        <p>Best regards,<br>Onatrix</p>
                    </body>
                </html>",
                PlainText = $@"
        Thank you for contacting Onatrix!
        We will contact you back at {emailData.Email} {(string.IsNullOrEmpty(emailData.Phone) ? "" : $"or {emailData.Phone}")}.
        {(!string.IsNullOrEmpty(emailData.Message) ? $"You mentioned: \"{emailData.Message}\"" : "")}
        {(!string.IsNullOrEmpty(emailData.Service) ? $"We will assist you with: \"{emailData.Service}\"" : "")}
        Best regards,
        Onatrix"
            };

            // Send the email
            bool emailSent = await SendEmailAsync(emailRequest);

            if (emailSent)
            {
                return new OkObjectResult($"Email sent to {emailData.Email}");
            }
            else
            {
                return new BadRequestObjectResult("Email could not be sent.");
            }
        }


        public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
        {
            try
            {
                // Build the email message
                var emailMessage = new EmailMessage(
                    senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
                    content: new EmailContent(emailRequest.Subject)
                    {
                        Html = emailRequest.HtmlBody,
                        PlainText = emailRequest.PlainText
                    },
                    recipients: new EmailRecipients(new List<EmailAddress>
                    {
                new EmailAddress(emailRequest.To) // Recipient email address
                    })
                );

                // Send the email
                var result = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);

                if (result.HasCompleted)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
            }

            return false;
        }

    }
}
