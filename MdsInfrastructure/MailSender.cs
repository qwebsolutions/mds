using Metapsi;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MdsInfrastructure
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
            public string CertificateThumbprint { get; set; }
        }

        public static async Task Send(CommandContext commandContext, State state, Mail mail)
        {
            // For outlook, sometimes works, sometimes doesn't
            //            UseDefaultCredentials = false,
            //            DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
            //            Credentials = new System.Net.NetworkCredential(state.Sender, state.Password),
            //            TargetName = "STARTTLS/smtp.office365.com", // TODO: I don't know what this is, and definitely shouldn't be hardcoded
            //            EnableSsl = true,


            if (!string.IsNullOrEmpty(state.CertificateThumbprint))
            {
                // make sure you give rights for the certificate
                // All taks > Manage private keys > Network service (IIS user)
                using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                {
                    certStore.Open(OpenFlags.ReadOnly);

                    X509Certificate2Collection certCollection = certStore.Certificates.Find(
                                                X509FindType.FindByThumbprint,
                                                state.CertificateThumbprint,
                                                false);
                    X509Certificate2 cert = certCollection.OfType<X509Certificate2>().FirstOrDefault();

                    if (cert is null)
                        throw new System.Exception($"Certificate with thumbprint {state.CertificateThumbprint} was not found");

                    using (System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient()
                    {
                        Host = state.SmtpHostName,
                        UseDefaultCredentials = true,
                        DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                        //Credentials = new System.Net.NetworkCredential(state.Sender, state.Password),
                        EnableSsl = true
                    })
                    {
                        smtpClient.ClientCertificates.Add(cert);
                        using (var message = new System.Net.Mail.MailMessage(state.Sender, mail.ToAddresses, mail.Subject, mail.Body))
                        {
                            await smtpClient.SendMailAsync(message);
                        }
                    }
                }
            }
            else
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

            //commandContext.PostEvent(new Event.MailSent()
            //{
            //    Subject = send.Subject,
            //    ToEmails = send.Emails
            //});
        }

        public static MailSender.State AddMailSender(
            this ApplicationSetup applicationSetup,
            string smtpHostName,
            string sender,
            string password,
            string certificateThumbprint)
        {
            return applicationSetup.AddBusinessState(new MailSender.State()
            {
                SmtpHostName = smtpHostName,
                Sender = sender,
                Password = password,
                CertificateThumbprint = certificateThumbprint
            });
        }
    }
}
