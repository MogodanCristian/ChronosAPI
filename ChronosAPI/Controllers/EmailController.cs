using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [HttpPost]
        public IActionResult sendEmailViaWebApi()
        {
            try
            {
                string subject = "Email Subject";
                string body = "Email body";
                string FromMail = "steelparrot.inc@gmail.com";
                string emailTo = "georgeandronache.cpp@gmail.com";
                MailMessage mail = new MailMessage();
                SmtpClient client = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(FromMail);
                mail.To.Add(emailTo);
                mail.Subject = subject;
                mail.Body = body;
                client.Port = 587;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential("steelparrot.inc@gmail.com", "papagaluu1$");
                client.EnableSsl = true;
                client.Send(mail);
                return Ok("Mail Sent");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }

        private string createEmailBody(string userName, string message)
        {
            string body = message;
            return body;
        }
    }
}
