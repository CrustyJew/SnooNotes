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

        public static bool DeleteMultipleNoteTypes(IEnumerable<NoteType> ntypes,string uname)
        {
            List<Dictionary<string, object>> ntypeParams = new List<Dictionary<string, object>>();
            foreach (NoteType nt in ntypes)
            {
                ntypeParams.Add(new Dictionary<string, object>() { { "NoteTypeID", nt.NoteTypeID }, { "uname", uname } });
            }
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "update NoteTypes set Disabled = 1 " +
                    " OUTPUT GETUTCDATE() as 'HistTimestamp','U' as 'HistAction',@uname as 'HistUser',INSERTED.NoteTypeID,INSERTED.SubredditID,INSERTED.DisplayName,INSERTED.ColorCode,INSERTED.DisplayOrder,INSERTED.Bold,INSERTED.Italic,INSERTED.Disabled INTO " +
                        "NoteTypes_History(HistTimestamp,HistAction,HistUser,NoteTypeID,SubredditID,DisplayName,ColorCode,DisplayOrder,Bold,Italic,Disabled) " +
                    " where NoteTypeID = @NoteTypeID";
                con.Execute(query, ntypeParams);
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

        public static IEnumerable<NoteType> AddMultipleNoteTypes(IEnumerable<NoteType> ntypes,string uname)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "declare @newNote as Table([NoteTypeID] INT, [SubredditID] INT, [DisplayName] NVARCHAR(25), [ColorCode] NVARCHAR(6), [DisplayOrder] INT, [Bold] BIT, [Italic] BIT, [Disabled] BIT) " +
                        "insert into NoteTypes (SubredditID,DisplayName,ColorCode,DisplayOrder,Bold,Italic) " +
                        "OUTPUT INSERTED.NoteTypeID,INSERTED.SubredditID,INSERTED.DisplayName,INSERTED.ColorCode,INSERTED.DisplayOrder,INSERTED.Bold,INSERTED.Italic, INSERTED.Disabled INTO @newNote(NoteTypeID,SubredditID,DisplayName,ColorCode,DisplayOrder,Bold,Italic,Disabled) " +
                        "values ( (select SubredditID from Subreddits where SubName = @SubName), @DisplayName, @ColorCode, @DisplayOrder, @Bold, @Italic) " +
                        "insert into NoteTypes_History(HistTimestamp,HistAction,HistUser,NoteTypeID,SubredditID,DisplayName,ColorCode,DisplayOrder,Bold,Italic) " +
                        "select GETUTCDATE() as 'HistTimestamp','C' as 'HistAction',@uname as 'HistUser',NoteTypeID,SubredditID,DisplayName,ColorCode,DisplayOrder,Bold,Italic from @newNote " +
                        "select nt.NoteTypeID,s.SubName,nt.DisplayName,nt.ColorCode,nt.DisplayOrder,nt.Bold,nt.Italic " +
                        "from @newNote nt "
                        + " inner join Subreddits s on s.SubredditID = nt.SubredditID";
                        //+ " where NoteTypeID = cast(SCOPE_IDENTITY() as int)";
                List<NoteType> ret = new List<NoteType>();
                foreach (NoteType nt in ntypes)
                {
                    ret.Add(con.Query<NoteType>(query, new { nt.SubName,nt.DisplayName,nt.ColorCode,nt.DisplayOrder,nt.Bold,nt.Italic,uname}).First());
                }
                return ret;
            }
        }
        public static void UpdateMultipleNoteTypes(NoteType[] ntypes,string uname)
        {
            List<Dictionary<string, object>> ntypeParams = new List<Dictionary<string, object>>();
            foreach(NoteType nt in ntypes)
            {
                ntypeParams.Add(new Dictionary<string, object>() { { "NoteTypeID", nt.NoteTypeID }, { "SubName", nt.SubName }, { "DisplayName", nt.DisplayName }, { "ColorCode", nt.ColorCode }, { "DisplayOrder", nt.DisplayOrder }, { "Bold", nt.Bold }, { "Italic", nt.Italic }, { "uname", uname } });
            }
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "update NoteTypes set DisplayName = @DisplayName , ColorCode = @ColorCode , DisplayOrder = @DisplayOrder , Bold = @Bold , Italic = @Italic " +
                    " OUTPUT GETUTCDATE() as 'HistTimestamp','U' as 'HistAction',@uname as 'HistUser',INSERTED.NoteTypeID,INSERTED.SubredditID,INSERTED.DisplayName,INSERTED.ColorCode,INSERTED.DisplayOrder,INSERTED.Bold,INSERTED.Italic,INSERTED.Disabled INTO " +
                        "NoteTypes_History(HistTimestamp,HistAction,HistUser,NoteTypeID,SubredditID,DisplayName,ColorCode,DisplayOrder,Bold,Italic,Disabled) " + 
                    " where NoteTypeID = @NoteTypeID";
                con.Execute(query, ntypeParams);
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