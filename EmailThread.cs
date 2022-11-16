using System;
using System.Net.Mail;

namespace PIAdaptMRP
{
    internal class EmailThread
    {
        private readonly String _body;
        private readonly String _fromAddress;
        private readonly String _subject;
        private readonly String _toAddress;
        private readonly String _emailServer;

        internal EmailThread(String incomingFromAddress, String incomingToAddress, String incomingSubject, String incomingBody, String emailServer)
        {
            _fromAddress = incomingFromAddress;
            _toAddress = incomingToAddress;
            _subject = incomingSubject;
            _body = incomingBody;
            _emailServer = emailServer;
        }

        internal void SendEmail()
        {
            var message = new MailMessage(_fromAddress, _toAddress, _subject, _body);

            var client = new SmtpClient(_emailServer);

            client.Send(message);
        }
    }
}