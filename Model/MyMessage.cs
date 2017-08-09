using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Office365GmailMigratorChecker
{
    public class MyMessage
    {   
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Rfc822MsgId { get; private set; }
        [Column(TypeName = "VARCHAR(255)")]
        public string GmailId { get; set; }
        [Required]
        [Column(TypeName = "VARCHAR(255)")]
        public string Office365Id { get; private set; }
        public bool IsMigratedToGmail { get; set; }
        [Required]
        public string Subject { get; private set; }
        [Required]
        public DateTime SentDateTime { get; private set; }
        
        [NotMapped]
        [JsonIgnore]
        private Message _outlookMessage;
        [NotMapped]
        public Message OutlookMessage
        {
            get { return _outlookMessage; }
            set { _outlookMessage = value;
                Rfc822MsgId = OutlookMessage.InternetMessageId;
                Subject = OutlookMessage.InternetMessageId;
                Office365Id = OutlookMessage.Id;
                SentDateTime = OutlookMessage.SentDateTime.Value.DateTime;
            }
        }
        

        
    }
}