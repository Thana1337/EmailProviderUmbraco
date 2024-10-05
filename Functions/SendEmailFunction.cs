using Azure;
using Azure.Communication.Email;
using EmailProvider.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Mail;

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

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody)!;
            string email = data?.email;

            if (!string.IsNullOrEmpty(email))
            {
                var emailRequest = new EmailRequest()
                {
                    To = email,
                    Subject = "Confirmation",
                    HtmlBody = @"
                        <html>
                            <body>
                                <h1>Thank you for contacting Onatrix!</h1>
                                <p>We have received your request and will contact you shortly.</p>
                                <p>Best regards,<br>Onatrix</p>
                            </body>
                        </html>",
                    PlainText = "Thank you for contacting us."
                };

                bool emailSent = await SendEmailAsync(emailRequest);

                if (emailSent)
                {
                    return new OkObjectResult($"Email sent to {email}");
                }
                else
                {
                    return new BadRequestObjectResult("Email could not be sent.");
                }
            }

            return new BadRequestObjectResult("Please pass a valid email.");
        }

        public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
        {
            try
            {
                var result = await _emailClient.SendAsync(
                    WaitUntil.Completed,

                    senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
                    recipientAddress: emailRequest.To,
                    subject: emailRequest.Subject,
                    htmlContent: emailRequest.HtmlBody,
                    plainTextContent: emailRequest.PlainText);

                if (result.HasCompleted)
                    return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }
    }
}
