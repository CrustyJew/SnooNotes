using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using System.Data.SqlClient;
using System.Configuration;

namespace SnooNotesAPI.Models
{
    public class NoteType
    {
        public int NoteTypeID { get; set; }
        public string SubName { get; set; }
        public string DisplayName { get; set; }
        public string ColorCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }

        public NoteType()
        {
            NoteTypeID = -1;
        }
        private static string constring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        public static string AddNoteType(NoteType ntype)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "insert into NoteTypes (SubredditID,DisplayName,ColorCode,DisplayOrder,Bold,Italic)" +
                        "values ( (select SubredditID from Subreddits where SubName = @SubName), @DisplayName, @ColorCode, @DisplayOrder, @Bold, @Italic)";
                con.Execute(query, new { ntype.SubName, ntype.DisplayName, ntype.ColorCode, ntype.DisplayOrder, ntype.Bold, ntype.Italic });
                return "Success";
            }
        }

        public static string UpdateNoteType(NoteType ntype)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "update NoteTypes set DisplayName = @DisplayName, ColorCode = @ColorCode, DisplayOrder = @DisplayOrder,Bold = @Bold, Italic = @Italic"
                    + " where NoteTypeID = @NoteTypeID";
                con.Execute(query, new { ntype.DisplayName, ntype.ColorCode, ntype.DisplayOrder, ntype.Bold, ntype.Italic, ntype.NoteTypeID });

                return "Success";
            }
        }

        public static string DeleteNoteType(NoteType ntype)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "delete nt from NoteTypes nt INNER JOIN Subreddits sr on nt.SubredditID = sr.SubredditID where NoteTypeID = @NoteTypeID and sr.SubName = @subname";
                con.Execute(query, new { ntype.NoteTypeID, ntype.SubName });
                return "Success";
            }
        }

        public static NoteType GetNoteType(int id)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "select nt.NoteTypeID,s.SubName,nt.DisplayName,nt.ColorCode,nt.DisplayOrder,nt.Bold,nt.Italic from NoteTypes nt "
                        + " inner join Subreddits s on s.SubredditID = nt.SubredditID"
                        + " where NoteTypeID = @id";
                NoteType ntype = con.Query<NoteType>(query, new { @id }).First();
                return ntype;
            }
        }

        public static IEnumerable<NoteType> GetNoteTypesForSubs(List<string> subredditNames)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "select nt.NoteTypeID,s.SubName,nt.DisplayName,nt.ColorCode,nt.DisplayOrder,nt.Bold,nt.Italic from NoteTypes nt "
                        + " inner join Subreddits s on s.SubredditID = nt.SubredditID"
                        + " where s.SubName in @subs";

                return con.Query<NoteType>(query, new { subs = subredditNames });
            }
        }

        public static void AddMultipleNoteTypes(List<NoteType> ntypes)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "insert into NoteTypes (SubredditID,DisplayName,ColorCode,DisplayOrder,Bold,Italic) " +
                        " values ( (select SubredditID from Subreddits where SubName = @SubName), @DisplayName, @ColorCode, @DisplayOrder, @Bold, @Italic)";
                con.Execute(query, ntypes);
            }
        }
        public static void UpdateMultipleNoteTypes(List<NoteType> ntypes)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "update NoteTypes set DisplayName = @DisplayName , ColorCode = @ColorCode , DisplayOrder = @DisplayOrder , Bold = @Bold , Italic = @Italic " +
                        " where NoteTypeID = @NoteTypeID";
                con.Execute(query, ntypes);
            }
        }

    }
}