using ChronosAPI.Helpers;
using ChronosAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;


namespace ChronosAPI.Controllers
{
    [Route("api/email")]
    [ApiController]
    public class EmailController : ControllerBase
    {

        private readonly AppSettings _appSettings;

        public EmailController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        public IActionResult sendEmailViaWebApi(EmailModel emailModel)
        {
            try
            {
                
                string emailTo = emailModel.toemail;
                MailMessage mail = new MailMessage();
                SmtpClient client = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(_appSettings.dev_team_email);
                mail.To.Add(emailTo);
                mail.Subject = emailModel.subject;
                mail.Body = emailModel.message;
                client.Port = 587;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(_appSettings.dev_team_email, _appSettings.dev_team_password);
                client.EnableSsl = true;
                client.Send(mail);
                return Ok("Mail Sent");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }
    }
}
