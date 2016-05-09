using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading.Tasks;
using SnooNotesAPI.Models;

namespace SnooNotesAPI.DAL {
    public class NotesDAL {
        private static string constring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

        public async Task<IEnumerable<Note>> GetNotesForUsers( string subname, IEnumerable<string> usernames ) {
            using ( SqlConnection con = new SqlConnection( constring ) ) {
                string query = "select n.NoteID, n.NoteTypeID, s.SubName, n.Submitter, n.Message, n.AppliesToUsername, n.Url, n.Timestamp "
                        + " from Notes n inner join Subreddits s on s.SubredditID = n.SubredditID "
                        + " where n.AppliesToUsername in @usernames and s.SubName = @subname";

                return await con.QueryAsync<Note>( query, new { usernames, subname } );
            }
        }
        public async Task<IEnumerable<Note>> GetNotesForUsers( IEnumerable<string> subnames, IEnumerable<string> usernames ) {
            using ( SqlConnection con = new SqlConnection( constring ) ) {
                string query = "select n.NoteID, n.NoteTypeID, s.SubName, n.Submitter, n.Message, n.AppliesToUsername "
                        + " from Notes n inner join Subreddits s on s.SubredditID = n.SubredditID "
                        + " where n.AppliesToUsername in @usernames and s.SubName in @subnames";

                return await con.QueryAsync<Note>( query, new { usernames, subnames } );
            }
        }
        public async Task<IEnumerable<string>> GetUsersWithNotes( IEnumerable<string> subnames ) {
            using ( SqlConnection con = new SqlConnection( constring ) ) {
                string query = "select distinct n.AppliesToUsername "
                       + " from Notes n inner join Subreddits s on s.SubredditID = n.SubredditID "
                       + " where s.SubName in @subnames";

                return await con.QueryAsync<string>( query, new { subnames } );
            }
        }
        public async Task<IEnumerable<Note>> GetNotesForSubs( IEnumerable<string> subnames ) {
            using ( SqlConnection con = new SqlConnection( constring ) ) {
                string query = "select n.NoteID, n.NoteTypeID, s.SubName, n.Submitter, n.Message, n.AppliesToUsername, n.Url, n.Timestamp "
                        + " from Notes n inner join Subreddits s on s.SubredditID = n.SubredditID "
                        + " where s.SubName in @subnames";

                return await con.QueryAsync<Note>( query, new { subnames } );
            }
        }
        public async Task<Note> GetNoteByID( int id ) {
            using ( SqlConnection con = new SqlConnection( constring ) ) {
                string query = "select n.NoteID, n.NoteTypeID, s.SubName, n.Submitter, n.Message, n.AppliesToUsername, n.Url, n.Timestamp "
                        + " from Notes n inner join Subreddits s on s.SubredditID = n.SubredditID "
                        + " where n.NoteID = @noteid";

                return ( await con.QueryAsync<Note>( query, new { noteid = id } ) ).Single();
            }
        }
        public async Task<Note> AddNoteForUser( Note anote ) {
            anote.AppliesToUsername = anote.AppliesToUsername.ToLower();
            using ( SqlConnection con = new SqlConnection( constring ) ) {
                string query = "insert into Notes(NoteTypeID,SubredditID,Submitter,Message,AppliesToUsername, n.Url, n.Timestamp) "
                    + " values (@NoteTypeID,(select SubredditID from Subreddits where SubName = @SubName),@Submitter,@Message,@AppliesToUsername, @Url, @Timestamp);"
                    + " select n.NoteID, n.NoteTypeID, s.SubName, n.Submitter, n.Message, n.AppliesToUsername, n.Url, n.Timestamp "
                        + " from Notes n inner join Subreddits s on s.SubredditID = n.SubredditID "
                        + " where n.NoteID = cast(SCOPE_IDENTITY() as int) ";
                Note insertedNote = ( await con.QueryAsync<Note>( query, new { anote.NoteTypeID, anote.SubName, anote.Submitter, anote.Message, anote.AppliesToUsername, anote.Url, anote.Timestamp } ) ).Single();

                return insertedNote;
            }
        }

        public async Task<string> DeleteNoteForUser( Note anote, string uname ) {
            using ( SqlConnection con = new SqlConnection( constring ) ) {
                string query = "delete n " +
                    "OUTPUT GETUTCDATE() as 'HistTimestamp','D' as 'HistAction',@uname as 'HistUser', DELETED.NoteID, DELETED.NoteTypeID, DELETED.SubRedditID,DELETED.Submitter,DELETED.Message,DELETED.AppliesToUsername,DELETED.URL,DELETED.TimeStamp into Notes_History " +
                    "from Notes n INNER JOIN Subreddits sr on n.SubredditID = sr.SubredditID " +
                    "where NoteID = @noteid and sr.SubName = @subname";
                await con.ExecuteAsync( query, new { anote.NoteID, anote.SubName, uname } );
                return "Success";
            }
        }

        public async Task<int> AddNewToolBoxNotes( List<Note> tbNotes ) {
            using ( SqlConnection con = new SqlConnection( constring ) ) {
                string query = "insert into Notes(NoteTypeID,SubredditID,Submitter,Message,AppliesToUsername, Url, Timestamp) "
                    + "select @NoteTypeID,(select SubredditID from Subreddits where SubName = @SubName),@Submitter,@Message,@AppliesToUsername, @Url, @Timestamp "
                    //+ "where not exists(select * from Notes n inner join Subreddits s on s.SubredditID = n.SubredditID where s.SubName = @SubName and n.AppliesToUsername = @AppliesToUsername and HASHBYTES('SHA2_256',Lower(s.SubName + n.Submitter + n.AppliesToUsername + CONVERT(VARCHAR,n.Timestamp,120) + n.Url)) = HASHBYTES('SHA2_256',Lower(@SubName + @Submitter + @AppliesToUsername + CONVERT(VARCHAR,@TimeStamp,120) + @Url)))";
                    + "where not exists (select * from Notes n inner join Subreddits s on s.SubredditID = n.SubredditID where s.SubName = @SubName and n.AppliesToUsername = @AppliesToUsername and n.Submitter = @Submitter and n.Url = @Url and CONVERT(VARCHAR,n.Timestamp,120) = CONVERT(VARCHAR,@TimeStamp,120))";
                int rowsEffected = await con.ExecuteAsync( query, tbNotes );
                return rowsEffected;

                //HASHBYTES('SHA2_256',Lower(s.SubName + n.Submitter + n.AppliesToUsername + CONVERT(VARCHAR,n.Timestamp,120) + n.Url))
            }
        }
    }
}