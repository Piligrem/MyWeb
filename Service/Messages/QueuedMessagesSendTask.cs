﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InSearch.Core.Email;
using InSearch.Core.Logging;
using InSearch.Services.Tasks;
using InSearch.Core.Domain.Messages;

namespace InSearch.Services.Messages
{
    /// <summary>
    /// Represents a task for sending queued message 
    /// </summary>
    public partial class QueuedMessagesSendTask : ITask
    {
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailSender _emailSender;
        private readonly EmailAccountSettings _emailAccountSettings;

        public QueuedMessagesSendTask(IQueuedEmailService queuedEmailService, IEmailSender emailSender, EmailAccountSettings emailAccountSettings)
        {
            this._queuedEmailService = queuedEmailService;
            this._emailSender = emailSender;
            this._emailAccountSettings = emailAccountSettings;
			Logger = NullLogger.Instance;
        }

		public ILogger Logger { get; set; }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            var queuedEmails = _queuedEmailService.SearchEmails(null, null, null, null, true, _emailAccountSettings.MaximumTries, false, 0, 10000);

            foreach (var qe in queuedEmails)
            {
                var bcc = String.IsNullOrWhiteSpace(qe.Bcc) 
                            ? null 
                            : qe.Bcc.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var cc = String.IsNullOrWhiteSpace(qe.CC) 
                            ? null 
                            : qe.CC.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                try
                {
					var smtpContext = new SmtpContext(qe.EmailAccount);

					var msg = new EmailMessage(
						new EmailAddress(qe.To, qe.ToName),
						qe.Subject,
						qe.Body,
						new EmailAddress(qe.From, qe.FromName));

					if (qe.ReplyTo.HasValue()) 
					{
						msg.ReplyTo.Add(new EmailAddress(qe.ReplyTo, qe.ReplyToName));
					}

					if (cc != null)
						msg.Cc.AddRange(cc.Where(x => x.HasValue()).Select(x => new EmailAddress(x)));

					if (bcc != null)
						msg.Bcc.AddRange(bcc.Where(x => x.HasValue()).Select(x => new EmailAddress(x)));

					_emailSender.SendEmail(smtpContext, msg);

                    qe.SentOnUtc = DateTime.UtcNow;
                }
                catch (Exception exc)
                {
					Logger.Error(string.Format("Error sending e-mail: {0}", exc.Message), exc);
                }
                finally
                {
                    qe.SentTries = qe.SentTries + 1;
                    _queuedEmailService.UpdateQueuedEmail(qe);
                }
            }
        }
    }
}
