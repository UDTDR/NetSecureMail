using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using Cpi.Net.SecureMail;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System.IO;

namespace Tester
{
    class Program
    {

        static public void SendSecureEmail1()
        {
            //set my certificate info up
            string CertificatePath = @"C:\Program Files\OpenSSL-Win64\bin\cert.pem";
            string CertificatePassword = "toto1234";

            string MailServer = "smtp.free.fr";

            string EmailRecipient = "fzefzefezfezc@free.fr";
            string EmailSender = "your@sender.com";
            string EmailSubject = "My first secure email";
            string EmailBody = "This is another secure email";
            bool IsHtmlEmail = false;

            //Load the certificate
            X509Certificate2 EncryptCert =
               new X509Certificate2(CertificatePath, CertificatePassword);

            //Build the body into a string
            StringBuilder Message = new StringBuilder();
            Message.AppendLine("Content-Type: text/" +
                ((IsHtmlEmail) ? "html" : "plain") +
                "; charset=\"iso-8859-1\"");

            Message.AppendLine("Content-Transfer-Encoding: 7bit");
            Message.AppendLine();
            Message.AppendLine(EmailBody);

            //Convert the body to bytes
            byte[] BodyBytes = Encoding.ASCII.GetBytes(Message.ToString());

            //Build the e-mail body bytes into a secure envelope
            EnvelopedCms Envelope = new EnvelopedCms(new ContentInfo(BodyBytes));
            CmsRecipient Recipient = new CmsRecipient(
                SubjectIdentifierType.IssuerAndSerialNumber, EncryptCert);
            Envelope.Encrypt(Recipient);
            byte[] EncryptedBytes = Envelope.Encode();

            //Creat the mail message
            MailMessage Msg = new MailMessage();
            Msg.To.Add(new MailAddress(EmailRecipient));
            Msg.From = new MailAddress(EmailSender);
            Msg.Subject = EmailSubject;

            //Attach the encrypted body to the email as and ALTERNATE VIEW
            MemoryStream ms = new MemoryStream(EncryptedBytes);
            AlternateView av =
                new AlternateView(ms,
                "application/pkcs7-mime; smime-type=signed-data;name=smime.p7m");
            Msg.AlternateViews.Add(av);

            SmtpClient smtp = new SmtpClient(MailServer, 25);
            //send the email                                       
            smtp.Send(Msg);
        }

        static void Main(string[] args)
        {
            SendSecureEmail1();
        }


        static void SendSecurEmail2()
        {
            SecureMailMessage message = new SecureMailMessage();

            // Look up your signing cert by serial number in your cert store
            X509Certificate2 signingCert = CryptoHelper.FindCertificate("1B37D3");
            // Look up your encryption cert the same way
            X509Certificate2 encryptionCert = CryptoHelper.FindCertificate("22C590");

            // Load the recipient's encryption cert from a file.
            X509Certificate2 recipientCert = new X509Certificate2(@"c:\certs\bob.cer");

            message.From = new SecureMailAddress("alice@cynicalpirate.com", "Alice", encryptionCert, signingCert);
            message.To.Add(new SecureMailAddress("fzefzefzefzefzef@free.fr", "Fred (Bob)", recipientCert));

            message.Subject = "This is a signed and encrypted message";

            message.Body = "<h2>Sent from the Cpi.Net.SecureMail library!</h2>";
            message.IsBodyHtml = true;

            message.IsSigned = true;
            message.IsEncrypted = true;

            // Instantiate a good old-fashioned SmtpClient to send your message
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient("smtp.free.fr", 25);

            // If your SMTP server requires you to authenticate, you need to specify your
            // username and password here.
            //client.Credentials = new NetworkCredential("YourSmtpUserName", "YourSmtpPassword");

            client.Send(message);
        }
    }
}
