using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using System.Data.SqlClient;
using System.Configuration;

namespace SnooNotesAPI.Models
{
    public class Note
    {
        public int NoteID { get; set; }
        public int NoteTypeID { get; set; }
        public string SubName { get; set; }
        public string Submitter { get; set; }
        public string Message { get; set; }
        public string AppliesToUsername { get; set; }

        private static SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());

        public static IEnumerable<Note> GetNotesForUsers(string subname, IEnumerable<string> usernames)
        {
            string query = "select n.NoteID, n.NoteTypeID, s.SubName, n.Submitter, n.Message, n.AppliesToUsername "
                    + " from Notes n inner join Subreddits s on s.SubredditID = n.SubredditID "
                    + " where n.AppliesToUsername in @usernames and s.SubName = @subname";

            return con.Query<Note>(query, new { usernames, subname });
        }
        public static IEnumerable<Note> GetNotesForUsers(IEnumerable<string> subnames, IEnumerable<string> usernames)
        {
            string query = "select n.NoteID, n.NoteTypeID, s.SubName, n.Submitter, n.Message, n.AppliesToUsername "
                    + " from Notes n inner join Subreddits s on s.SubredditID = n.SubredditID "
                    + " where n.AppliesToUsername in @usernames and s.SubName in @subnames";

            return con.Query<Note>(query, new { usernames, subnames });
        }
        public static IEnumerable<string> GetUsersWithNotes(IEnumerable<string> subnames)
        {
            string query = "select n.AppliesToUsername "
                   + " from Notes n inner join Subreddits s on s.SubredditID = n.SubredditID "
                   + " where s.SubName in @subnames";

            return con.Query<string>(query, new { subnames });
        }
        public static IEnumerable<Note> GetNotesForSubs(IEnumerable<string> subnames)
        {
            string query = "select n.NoteID, n.NoteTypeID, s.SubName, n.Submitter, n.Message, n.AppliesToUsername "
                    + " from Notes n inner join Subreddits s on s.SubredditID = n.SubredditID "
                    + " where s.SubName in @subnames";

            return con.Query<Note>(query, new { subnames });
        }

        public static string AddNoteForUser(Note anote)
        {
            string query = "insert into Notes(NoteTypeID,SubredditID,Submitter,Message,AppliesToUsername) "
                + " values (@NoteTypeID,(select SubredditID from Subreddits where SubName = @SubName),@Submitter,@Message,@AppliesToUsername) ";
            con.Execute(query, new { anote.NoteTypeID, anote.SubName, anote.Submitter, anote.Message, anote.AppliesToUsername });

            return "Success";
        }

        public static string DeleteNoteForUser(Note anote)
        {
            string query = "delete n from Notes n INNER JOIN Subreddits sr on n.SubredditID = sr.SubredditID where NoteID = @id and sr.SubName = @subname";
            con.Execute(query, new { anote.NoteID, anote.SubName });
            return "Success";
        }
    }
}