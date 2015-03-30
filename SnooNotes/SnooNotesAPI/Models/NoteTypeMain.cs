using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using System.Data.SqlClient;
using System.Configuration;
namespace SnooNotesAPI.Models
{
    public class NoteTypeMain
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());
        public string AddNoteType(NoteType ntype){
            string query = "insert into NoteTypes (SubredditID,DisplayName,ColorCode,DisplayOrder,Bold,Italic)" +
                    "values ( (select SubredditID from Subreddits where SubName = @SubName), @DisplayName, @ColorCode, @DisplayOrder, @Bold, @Italic)";
            con.Execute(query, new { ntype.SubName, ntype.DisplayName, ntype.ColorCode, ntype.DisplayOrder, ntype.Bold, ntype.Italic });
            return "Success";
        }

        public string UpdateNoteType(NoteType ntype)
        {
            string query = "update NoteTypes set DisplayName = @DisplayName, ColorCode = @ColorCode, DisplayOrder = @DisplayOrder,Bold = @Bold, Italic = @Italic"
                + " where NoteTypeID = @NoteTypeID";
            con.Execute(query, new { ntype.DisplayName, ntype.ColorCode, ntype.DisplayOrder, ntype.Bold, ntype.Italic, ntype.NoteTypeID });

            return "Success";
        }

        public string DeleteNoteType(NoteType ntype)
        {
            string query = "delete nt from NoteTypes nt INNER JOIN Subreddits sr on nt.SubredditID = sr.SubredditID where NoteTypeID = @NoteTypeID and sr.SubName = @subname";
            con.Execute(query, new { ntype.NoteTypeID, ntype.SubName });
            return "Success";
        }

        public NoteType GetNoteType(int id)
        {
            string query = "select nt.NoteTypeID,s.SubName,nt.DisplayName,nt.ColorCode,nt.DisplayOrder,nt.Bold,nt.Italic from NoteTypes nt "
                    + " inner join Subreddits s on s.SubredditID = nt.SubredditID"
                    + " where NoteTypeID = @id";
            NoteType ntype = con.Query<NoteType>(query, new { @id }).First();
            return ntype;
        }

        public IEnumerable<NoteType> GetNoteTypesForSubs(List<string> subredditNames)
        {
            string query = "select nt.NoteTypeID,s.SubName,nt.DisplayName,nt.ColorCode,nt.DisplayOrder,nt.Bold,nt.Italic from NoteTypes nt "
                    + " inner join Subreddits s on s.SubredditID = nt.SubredditID"
                    + " where s.SubName in @subs";

            return con.Query<NoteType>(query, new { subs = subredditNames });
        }
    }
}