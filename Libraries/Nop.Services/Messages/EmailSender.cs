using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Nop.Core.Domain.Messages;
using Nop.Services.Media;
using System.Text;
using System.Web;
using Nop.Services.Logging;
using System.Xml;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Email sender
    /// </summary>
    public partial class EmailSender : IEmailSender
    {
        private readonly IDownloadService _downloadService;


        public EmailSender(IDownloadService downloadService)
        {
            this._downloadService = downloadService;
        }

        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="emailAccount">Email account to use</param>
        /// <param name="subject">Subject</param>
        /// <param name="body">Body</param>
        /// <param name="fromAddress">From address</param>
        /// <param name="fromName">From display name</param>
        /// <param name="toAddress">To address</param>
        /// <param name="toName">To display name</param>
        /// <param name="replyTo">ReplyTo address</param>
        /// <param name="replyToName">ReplyTo display name</param>
        /// <param name="bcc">BCC addresses list</param>
        /// <param name="cc">CC addresses list</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <param name="attachedDownloadId">Attachment download ID (another attachedment)</param>
        /// <param name="headers">Headers</param>
        public virtual void SendEmail(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
             string replyTo = null, string replyToName = null,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null,
            int attachedDownloadId = 0, IDictionary<string, string> headers = null)
        {
            var message = new MailMessage();
            //from, to, reply to
            message.From = new MailAddress(fromAddress, fromName);
            message.To.Add(new MailAddress(toAddress, toName));
            if (!String.IsNullOrEmpty(replyTo))
            {
                message.ReplyToList.Add(new MailAddress(replyTo, replyToName));
            }

            //BCC
            if (bcc != null)
            {
                foreach (var address in bcc.Where(bccValue => !String.IsNullOrWhiteSpace(bccValue)))
                {
                    message.Bcc.Add(address.Trim());
                }
            }

            //CC
            if (cc != null)
            {
                foreach (var address in cc.Where(ccValue => !String.IsNullOrWhiteSpace(ccValue)))
                {
                    message.CC.Add(address.Trim());
                }
            }

            //content
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            //headers
            if (headers != null)
                foreach (var header in headers)
                {
                    message.Headers.Add(header.Key, header.Value);
                }

            //create the file attachment for this e-mail message
            if (!String.IsNullOrEmpty(attachmentFilePath) &&
                File.Exists(attachmentFilePath))
            {
                var attachment = new Attachment(attachmentFilePath);
                attachment.ContentDisposition.CreationDate = File.GetCreationTime(attachmentFilePath);
                attachment.ContentDisposition.ModificationDate = File.GetLastWriteTime(attachmentFilePath);
                attachment.ContentDisposition.ReadDate = File.GetLastAccessTime(attachmentFilePath);
                if (!String.IsNullOrEmpty(attachmentFileName))
                {
                    attachment.Name = attachmentFileName;
                }
                message.Attachments.Add(attachment);
            }
            //another attachment?
            if (attachedDownloadId > 0)
            {
                var download = _downloadService.GetDownloadById(attachedDownloadId);
                if (download != null)
                {
                    //we do not support URLs as attachments
                    if (!download.UseDownloadUrl)
                    {
                        string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : download.Id.ToString();
                        fileName += download.Extension;


                        var ms = new MemoryStream(download.DownloadBinary);
                        var attachment = new Attachment(ms, fileName);
                        //string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
                        //var attachment = new Attachment(ms, fileName, contentType);
                        attachment.ContentDisposition.CreationDate = DateTime.UtcNow;
                        attachment.ContentDisposition.ModificationDate = DateTime.UtcNow;
                        attachment.ContentDisposition.ReadDate = DateTime.UtcNow;
                        message.Attachments.Add(attachment);
                    }
                }
            }

            //send email
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.UseDefaultCredentials = emailAccount.UseDefaultCredentials;
                smtpClient.Host = emailAccount.Host;
                smtpClient.Port = emailAccount.Port;
                smtpClient.EnableSsl = emailAccount.EnableSsl;
                smtpClient.Credentials = emailAccount.UseDefaultCredentials ?
                    CredentialCache.DefaultNetworkCredentials :
                    new NetworkCredential(emailAccount.Username, emailAccount.Password);
                smtpClient.Send(message);
            }
        }


        public void SendSmS(string PhoneList, string Msg, string userName, string password, string timeToSend, string sender, string smsRemovalLink = null)
        {



            string msg3 = Msg;
            if (!string.IsNullOrWhiteSpace(smsRemovalLink))
            {
                 msg3  += "\n" + smsRemovalLink;
            }

           


            string messageText = System.Security.SecurityElement.Escape(msg3);
            string messageInterval = "0";



            StringBuilder sbXml = new StringBuilder();
            sbXml.Append("<Inforu>");
            sbXml.Append("<User>");
            sbXml.Append("<Username>" + userName + "</Username>");
            sbXml.Append("<Password>" + password + "</Password>");
            sbXml.Append("</User>");
            sbXml.Append("<Content Type=\"sms\">");
            sbXml.Append("<Message>" + messageText + "</Message>");
            sbXml.Append("</Content>");
            sbXml.Append("<Recipients>");
            sbXml.Append("<PhoneNumber>" + PhoneList + "</PhoneNumber>");
            sbXml.Append("</Recipients>");
            sbXml.Append("<Settings>");
            sbXml.Append("<Sender>" + sender + "</Sender>");
            sbXml.Append("<MessageInterval>" + messageInterval + "</MessageInterval>");
            sbXml.Append("<TimeToSend>" + timeToSend + "</TimeToSend>");
            sbXml.Append("</Settings>");
            sbXml.Append("</Inforu >");
            string strXML = HttpUtility.UrlEncode(sbXml.ToString(), System.Text.Encoding.UTF8);
            string result = PostDataToURL("http://api.inforu.co.il/SendMessageXml.ashx", "InforuXML=" + strXML);

        }

        string PostDataToURL(string szUrl, string szData)
        {
            //Setup the web request
            string szResult = string.Empty;
            WebRequest Request = WebRequest.Create(szUrl);
            Request.Timeout = 30000;
            Request.Method = "POST";
            Request.ContentType = "application/x-www-form-urlencoded";
            byte[] PostBuffer;
            try
            {
                // replacing " " with "+" according to Http post RPC
                szData = szData.Replace(" ", "+");
                //Specify the length of the buffer
                PostBuffer = Encoding.UTF8.GetBytes(szData);
                Request.ContentLength = PostBuffer.Length;
                //Open up a request stream
                Stream RequestStream = Request.GetRequestStream();
                //Write the POST data
                RequestStream.Write(PostBuffer, 0, PostBuffer.Length);
                //Close the stream
                RequestStream.Close();
                //Create the Response object
                WebResponse Response;
                Response = Request.GetResponse();
                //Create the reader for the response
                StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.UTF8);
                //Read the response
                szResult = sr.ReadToEnd();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(szResult);

                XmlNodeList statusNodes = doc.DocumentElement.SelectNodes("/Result/status");

                foreach (XmlNode statusNode in statusNodes)
                {
                    string statusMessage = statusNode.InnerText;
                    if (statusMessage != "1")
                    {

                        XmlNode errorNode = doc.DocumentElement.SelectSingleNode("/Result/status/Description");
                        //_logger.Error(string.Format("Error sending SmS. {0} - {1}", statusMessage, errorNode.InnerText));
                    }

                }


                //Close the reader, and response
                sr.Close();
                Response.Close();



                return szResult;
            }
            catch (XmlException e)
            {
               // _logger.Error(string.Format("Error sending SmS xml response xml error"));
                return szResult;
            }
            catch (Exception e)
            {
               // _logger.Error(string.Format("Error sending SmS - connection error"));
                return szResult;
            }

        }



    }
    }

