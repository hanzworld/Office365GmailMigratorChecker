using Microsoft.Graph;
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
        public string Rfc822MsgId { get; set; }
        [Column(TypeName = "VARCHAR(255)")]
        public string GmailId { get; set; }
        [Required]
        [Column(TypeName = "VARCHAR(255)")]
        public string Office365Id { get; set; }
        public bool IsMigratedToGmail { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public DateTime SentDateTime { get; set; }
        [NotMapped]
        public Message OutlookMessage { get; set; }
    }
}