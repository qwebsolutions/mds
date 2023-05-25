using Metapsi;
using System.Threading.Tasks;

namespace MdsBuildManager
{
    public static partial class MailSender
    {
        public partial class Mail : IData
        {
            public string Subject { get; set; } = System.String.Empty;
            public string ToAddresses { get; set; } = System.String.Empty;
            public string Body { get; set; } = System.String.Empty;
        }

        public class State
        {
            public string SmtpHostName { get; set; }
            public string Sender { get; set; }
            public string Password { get; set; }
        }

        public static async Task Send(CommandContext commandContext, State state, Mail mail)
        {
            using (System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient()
            {
                Host = state.SmtpHostName,
                Port = 587,
                UseDefaultCredentials = false,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                Credentials = new System.Net.NetworkCredential(state.Sender, state.Password),
                EnableSsl = false
            })
            {
                using (var message = new System.Net.Mail.MailMessage(state.Sender, mail.ToAddresses, mail.Subject, mail.Body))
                {
                    await smtpClient.SendMailAsync(message);
                }
            }
        }

        public static MailSender.State AddMailSender(
            this ApplicationSetup applicationSetup,
            string smtpHostName,
            string sender,
            string password)
        {
            return applicationSetup.AddBusinessState(new MailSender.State()
            {
                SmtpHostName = smtpHostName,
                Sender = sender,
                Password = password
            });
        }
    }
}
