using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatDatabase
{
    public class ChatHistoryService
    {
        public static List<ChatRecord> FetchChatHistory(string user)
        {
            using (var db = new ChatContext())
            {
                return db.ChatHistory.Where(cr => cr.User == user).ToList();
            }
        }

        public static void LogMessage(string user, string message)
        {
            using (var db = new ChatContext())
            {
                db.ChatHistory.Add(new ChatRecord() { User = user, Message = message, TimeStamp = DateTime.Now });
                db.SaveChanges();
            }
        }
    }
}
