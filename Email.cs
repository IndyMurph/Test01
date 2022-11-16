using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;

namespace PIAdaptMRP
{
    internal struct EmailNode
    {
        internal String EmailAdd;
        internal String EmailNm;
    }

    internal static class Email
    {
        internal static LinkedList<EmailNode> EmailList = new LinkedList<EmailNode>();

        /// <summary>
        ///     Load the Email list linked to the service requiring setup data
        /// </summary>
        internal static void LoadList()
        {
            EmailList.Clear();

            using (var emailReader = XmlReader.Create(Setup.SetUpXmlFileName))
            {
                emailReader.Read();
                while (emailReader.ReadToFollowing("Pager"))
                {
                    emailReader.ReadToFollowing("SetUpName");
                    if (String.Compare(Setup.SetUpServiceName, emailReader.ReadString(), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        emailReader.ReadToFollowing("Name");
                        EmailNode emailNode;
                        emailNode.EmailNm = emailReader.ReadString();

                        emailReader.ReadToFollowing("PagerAddress");
                        emailNode.EmailAdd = emailReader.ReadString();

                        EmailList.AddLast(emailNode);
                    }
                }
            }
        }

        /// <summary>
        ///  Send message to Email list
        /// </summary>
        /// <param name="emailBitMask">Debug Level</param>
        /// <param name="from">From address</param>
        /// <param name="emailList">Email List</param>
        /// <param name="subject">Subject</param>
        /// <param name="body">Content</param>
        internal static void SendMsgToList(int emailBitMask, String from, LinkedList<EmailNode> emailList,
            String subject, String body)
        {
            if (!Setup.PagerBitVector32[emailBitMask]) return;

            var email = emailList.First;

            while (email != null)
            {
                Send(@from, email.Value.EmailAdd, subject, body);
                email = email.Next;
            }
        }

        /// <summary>
        ///   Send Email
        /// </summary>
        /// <param name="from">Origin Email add</param>
        /// <param name="to">Destination Email add</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email content</param>
        private static void Send(String from, String to, String subject, String body)
        {
            var workerThreadObject = new EmailThread(@from,
                to,
                subject,
                body, Setup.EmailServer);

            var workerCaller = new Thread(workerThreadObject.SendEmail);
            workerCaller.Start();
        }
    }
}