using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatDatabase
{
    class ChatContext : DbContext
    {
        public DbSet<ChatRecord> ChatHistory { get; set; }
    }
}
