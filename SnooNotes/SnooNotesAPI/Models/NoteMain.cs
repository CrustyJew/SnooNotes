using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using System.Data.SqlClient;
using System.Configuration;

namespace SnooNotesAPI.Models
{
    public class NoteMain
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());

        public IEnumerable<Note> GetNotesForUsers(List<string> usernames, string subname)
        {
            string query = "select n.NoteID, n.NoteTypeID, s.SubName, n.Submitter, n.Message, n.AppliesToUsername "
                    + " from Notes n inner join Subreddits s on s.SubredditID = n.SubredditID "
                    + " where n.AppliesToUsername in @usernames and s.SubName = @subname";

            return con.Query<Note>(query, new {usernames, subname });
        }

        public string AddNoteForUser(Note anote)
        {
            string query = "insert into Notes(NoteTypeID,SubredditID,Submitter,Message,AppliesToUsername) "
                + " values (@NoteTypeID,(select SubredditID from Subreddits where SubName = @SubName),@Submitter,@Message,@AppliesToUsername) ";
            con.Execute(query, new { anote.NoteTypeID, anote.SubName, anote.Submitter, anote.Message, anote.AppliesToUsername });

            return "Success";
        }

        public string DeleteNoteForUser(int id)
        {
            string query = "delete from Notes where NoteID = @id";
            con.Execute(query, new { id });
            return "Success";
        }
    }
}