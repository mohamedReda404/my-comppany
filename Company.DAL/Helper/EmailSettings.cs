using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Company.Ali.DAL.Helper
{
    public static class EmailSettings
    {

        public static bool SendEmail(Email email)
        {

            // Mail Server : Gmail
            // SMTP

              try
              {
                var client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential("altir1781@gmail.com", "ffopkwfsnmltsavh"); // Sender

                client.Send("altir1781@gmail.com", email.To, email.Subject, email.Body);

                // ffopkwfsnmltsavh

                return true;
            }
            catch (Exception ex) 
            {
                return false;
            }
          
        }



    }
}
