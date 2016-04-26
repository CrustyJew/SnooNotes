using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SnooNotesAPI.Models;
using System.Web.Http;
using System.Security.Claims;

namespace SnooNotesAPI.BLL {
    public class NoteTypesBLL {
        private DAL.NoteTypesDAL noteTypesDAL;
        public NoteTypesBLL() {
            noteTypesDAL = new DAL.NoteTypesDAL();
        }
        public async Task<Dictionary<string, IEnumerable<BasicNoteType>>> GetNoteTypesForSubs(IEnumerable<string> subs) {
            Dictionary<string, IEnumerable<BasicNoteType>> toReturn = new Dictionary<string, IEnumerable<BasicNoteType>>();
            var notetypes = await noteTypesDAL.GetNoteTypesForSubs( subs );
            foreach ( string sub in subs ) {
                var basicNoteTypesForSub = notetypes.Where( t => t.SubName.ToLower() == sub ).Select( t => new BasicNoteType() { Bold = t.Bold, ColorCode = t.ColorCode, DisplayName = t.DisplayName, DisplayOrder = t.DisplayOrder, Italic = t.Italic, NoteTypeID = t.NoteTypeID } ).OrderBy( bt => bt.DisplayOrder );
                toReturn.Add( sub, basicNoteTypesForSub );
            }
            return toReturn;
        }

        public Task<NoteType> GetNoteType( int id ) {
            return noteTypesDAL.GetNoteType( id );
        }

        public async Task<IEnumerable<NoteType>> AddMultipleNoteTypes( IEnumerable<NoteType> values, string name ) {
            foreach ( Models.NoteType nt in values ) {
                if ( ! await ValidateNoteType( nt ) ) {
                    throw new HttpResponseException( System.Net.HttpStatusCode.BadRequest );
                }
            }

            var ret = await noteTypesDAL.AddMultipleNoteTypes( values, name );
            Signalr.SnooNoteUpdates.Instance.RefreshNoteTypes( values.Select( nt => nt.SubName ).Distinct() );
            return ret;
        }

        public async Task<IEnumerable<int>> DeleteMultipleNoteTypes( NoteType[] values, string name ) {
            if ( ! await noteTypesDAL.ValidateNoteTypesInSubs( values ) ) {
                throw new HttpResponseException( new System.Net.Http.HttpResponseMessage( System.Net.HttpStatusCode.BadRequest ) { ReasonPhrase = "You gone and changed a NoteType to a different Subreddit ya goof!" } );
            }
            foreach ( NoteType nt in values ) {
                if ( !ClaimsPrincipal.Current.IsInRole( nt.SubName.ToLower() ) ) {
                    throw new UnauthorizedAccessException( "You are not a moderator of that subreddit!" );
                }
            }
            await noteTypesDAL.DeleteMultipleNoteTypes( values, name );
            Signalr.SnooNoteUpdates.Instance.RefreshNoteTypes( values.Select( nt => nt.SubName ).Distinct() );
            return values.Select( nt => nt.NoteTypeID );
        }

        public async Task<IEnumerable<Models.NoteType>> UpdateMultipleNoteTypes( NoteType[] values, string name ) {
            foreach ( Models.NoteType nt in values ) {
                if ( ! await ValidateNoteType( nt ) ) {
                    throw new HttpResponseException( System.Net.HttpStatusCode.BadRequest );
                }

            }
            await noteTypesDAL.UpdateMultipleNoteTypes( values, name );
            Signalr.SnooNoteUpdates.Instance.RefreshNoteTypes( values.Select( nt => nt.SubName ).Distinct() );
            return values;
        }

        private async Task<bool> ValidateNoteType( Models.NoteType ntype ) {

            if ( String.IsNullOrEmpty( ntype.SubName ) || !ClaimsPrincipal.Current.IsInRole( ntype.SubName.ToLower() ) ) {
                return false; //doesn't mod sub or empty/null sub, insta FAIL
            }
            if ( ntype.NoteTypeID == -1 ) {
                //adding new note
            }
            else {
                var toModNT = await GetNoteType( ntype.NoteTypeID );
                if ( toModNT == null ) {
                    return false; //NoteTypeID doesn't exist, FAIL
                }
                if ( toModNT.SubName.ToLower() != ntype.SubName.ToLower() ) {
                    return false; //Subreddit name changed, FAIL
                }

            }
            if ( String.IsNullOrEmpty( ntype.ColorCode ) ) {
                return false; //No color code, FAIL
            }
            else if ( ntype.ColorCode.Length != 3 && ntype.ColorCode.Length != 6 ) {
                return false; //Color code wrong length, FAIL
            }
            else if ( !System.Text.RegularExpressions.Regex.IsMatch( ntype.ColorCode, @"\A\b[0-9a-fA-F]+\b\Z" ) ) {
                return false; //Color code not valid hex, FAIL
            }
            if ( String.IsNullOrEmpty( ntype.DisplayName ) ) {
                return false; //Null or empty display name, FAIL
            }
            else if ( ntype.DisplayName.Length > 20 ) {
                return false; //Displayname too long, FAIL
            }


            return true;
        }
    }
}