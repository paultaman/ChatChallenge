using System;

namespace ChatDatabase
{
    public class Program
    {
        /// <summary>
        /// Quick sanity check to make sure that the db is functional
        /// Server: (localdb)\mssqllocaldb
        /// </summary>
        static void Main(string[] args)
        {
            using (var db = new ChatContext())
            {
                db.ChatHistory.Add(new ChatRecord() { User = "Bob", Message = "Hi", TimeStamp = DateTime.Now });
                db.SaveChanges();
            }

            foreach (var record in ChatHistoryService.FetchChatHistory("Bob"))
            {
                Console.WriteLine(record.ToString());
            }

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }
    }
}
