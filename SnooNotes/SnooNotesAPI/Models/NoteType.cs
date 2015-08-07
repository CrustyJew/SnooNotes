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

        public static bool DeleteMultipleNoteTypes(IEnumerable<NoteType> ntypes)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "delete nt from NoteTypes nt INNER JOIN Subreddits sr on nt.SubredditID = sr.SubredditID where NoteTypeID = @NoteTypeID and sr.SubName = @subname";
                con.Execute(query, ntypes);
                return true;
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
                        + " where s.SubName in @subs and nt.Disabled = 0";

                return con.Query<NoteType>(query, new { subs = subredditNames });
            }
        }

        public static IEnumerable<NoteType> AddMultipleNoteTypes(IEnumerable<NoteType> ntypes)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "insert into NoteTypes (SubredditID,DisplayName,ColorCode,DisplayOrder,Bold,Italic) " +
                        "values ( (select SubredditID from Subreddits where SubName = @SubName), @DisplayName, @ColorCode, @DisplayOrder, @Bold, @Italic) " +
                        "select nt.NoteTypeID,s.SubName,nt.DisplayName,nt.ColorCode,nt.DisplayOrder,nt.Bold,nt.Italic from NoteTypes nt "
                        + " inner join Subreddits s on s.SubredditID = nt.SubredditID"
                        + " where NoteTypeID = cast(SCOPE_IDENTITY() as int)";
                List<NoteType> ret = new List<NoteType>();
                foreach (NoteType nt in ntypes)
                {
                    ret.Add(con.Query<NoteType>(query, new { nt.SubName,nt.DisplayName,nt.ColorCode,nt.DisplayOrder,nt.Bold,nt.Italic}).First());
                }
                return ret;
            }
        }
        public static void UpdateMultipleNoteTypes(NoteType[] ntypes)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "update NoteTypes set DisplayName = @DisplayName , ColorCode = @ColorCode , DisplayOrder = @DisplayOrder , Bold = @Bold , Italic = @Italic " +
                        " where NoteTypeID = @NoteTypeID";
                con.Execute(query, ntypes);
            }
        }

        public static bool ValidateNoteTypesInSubs(IEnumerable<NoteType> ntypes)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "select count(*) from "+ 
                    "NoteTypes nt inner join Subreddits s on s.SubredditID = nt.SubredditID "+
                    "where nt.NoteTypeID = @NoteTypeID and s.SubName = @SubName";
                int count = 0;
                foreach (NoteType nt in ntypes)
                {
                    count += con.Query<int>(query, new {nt.NoteTypeID,nt.SubName }).First();
                }
                return count == ntypes.Count();
            }
        }
        

    }
}