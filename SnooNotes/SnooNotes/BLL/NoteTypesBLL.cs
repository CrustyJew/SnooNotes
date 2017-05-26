using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SnooNotes.Models;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace SnooNotes.BLL {
    public class NoteTypesBLL : INoteTypesBLL {
        private DAL.INoteTypesDAL noteTypesDAL;
        private Signalr.ISnooNoteUpdates snUpdates;
        public NoteTypesBLL(DAL.INoteTypesDAL noteTypesDAL, Signalr.ISnooNoteUpdates snooNoteUpdates ) {
            this.noteTypesDAL = noteTypesDAL;
            snUpdates = snooNoteUpdates;
        }
        public async Task<Dictionary<string, IEnumerable<BasicNoteType>>> GetNoteTypesForSubs( IEnumerable<string> subs ) {
            Dictionary<string, IEnumerable<BasicNoteType>> toReturn = new Dictionary<string, IEnumerable<BasicNoteType>>();
            var notetypes = await noteTypesDAL.GetNoteTypesForSubs( subs );
            foreach ( string sub in subs ) {
                var basicNoteTypesForSub = notetypes.Where( t => t.SubName.ToLower() == sub ).Select( t => new BasicNoteType() { Bold = t.Bold, ColorCode = t.ColorCode, DisplayName = t.DisplayName, DisplayOrder = t.DisplayOrder, Italic = t.Italic, NoteTypeID = t.NoteTypeID, IconString = t.IconString } ).OrderBy( bt => bt.DisplayOrder );
                toReturn.Add( sub, basicNoteTypesForSub );
            }
            return toReturn;
        }

        public Task<NoteType> GetNoteType( int id ) {
            return noteTypesDAL.GetNoteType( id );
        }

        public async Task<IEnumerable<NoteType>> AddMultipleNoteTypes( IEnumerable<NoteType> values, string name, ClaimsPrincipal user ) {
            foreach ( Models.NoteType nt in values ) {
                if ( !await ValidateNoteType( nt, user ) ) {
                    throw new Exception("bad request"); //TODO
                }
            }

            var ret = await noteTypesDAL.AddMultipleNoteTypes( values, name );
            snUpdates.RefreshNoteTypes( values.Select( nt => nt.SubName ).Distinct() );
            return ret;
        }

        //TODO refactor out ClaimsPrincipal
        public async Task<IEnumerable<int>> DeleteMultipleNoteTypes( NoteType[] values, string name, ClaimsPrincipal user ) {
            if ( !await noteTypesDAL.ValidateNoteTypesInSubs( values ) ) {
                throw new Exception(  "You gone and changed a NoteType to a different Subreddit ya goof!" );
            }
            foreach ( NoteType nt in values ) {

                if ( !user.HasClaim("uri:snoonotes:admin", nt.SubName.ToLower()) )
                    throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );

            }
            await noteTypesDAL.DeleteMultipleNoteTypes( values, name );
            snUpdates.RefreshNoteTypes( values.Select( nt => nt.SubName ).Distinct() );
            return values.Select( nt => nt.NoteTypeID );
        }

        public async Task<IEnumerable<Models.NoteType>> UpdateMultipleNoteTypes( NoteType[] values, string name, ClaimsPrincipal user ) {
            foreach ( Models.NoteType nt in values ) {
                if ( !await ValidateNoteType( nt, user ) ) {
                    throw new Exception( "Bad Request" ); //TODO
                }

            }
            await noteTypesDAL.UpdateMultipleNoteTypes( values, name );
            snUpdates.RefreshNoteTypes( values.Select( nt => nt.SubName ).Distinct() );
            return values;
        }

        private async Task<bool> ValidateNoteType( Models.NoteType ntype, ClaimsPrincipal user ) {

            if ( String.IsNullOrEmpty( ntype.SubName ) || !user.IsInRole( ntype.SubName.ToLower() ) ) {
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