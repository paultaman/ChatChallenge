using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatDatabase
{
    class ChatRecord
    {
        [Key]
        public int RecordId { get; set; }

        public string User { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return TimeStamp + "; " + User + ": " + Message;
        }
    }
}
