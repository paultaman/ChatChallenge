using System;

namespace ChatDatabase
{
    class Program
    {
        /// <summary>
        /// Quick sanity check to make sure that the db is functional
        /// </summary>
        static void Main(string[] args)
        {
            using (var db = new ChatContext())
            {
                db.ChatHistory.Add(new ChatRecord() { User = "Bob", Message = "Hi", TimeStamp = DateTime.Now });
                db.SaveChanges();

                foreach (var record in db.ChatHistory)
                {
                    Console.WriteLine(record.ToString());
                }

                Console.WriteLine("Press a key to exit");
                Console.ReadKey();
            }
        }
    }
}
