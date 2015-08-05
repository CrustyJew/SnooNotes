using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Security.Claims;
namespace SnooNotesAPI.Controllers
{
    [Authorize]
    public class NoteTypeController : ApiController
    {
        
        // GET: api/NoteType
        public Dictionary<string, IEnumerable<Models.BasicNoteType>> Get()
        {
            List<string> roles = new List<string>();
            ClaimsIdentity id = (User.Identity as ClaimsIdentity);
            roles = id.Claims.Where(c => c.Type == id.RoleClaimType).Select(c => c.Value).ToList();
            var notetypes = Models.NoteType.GetNoteTypesForSubs(roles);
            Dictionary<string, IEnumerable<Models.BasicNoteType>> toReturn = new Dictionary<string, IEnumerable<Models.BasicNoteType>>();
            foreach(string sub in roles){
                var basicNoteTypesForSub = notetypes.Where(t => t.SubName.ToLower() == sub).Select(t => new Models.BasicNoteType(){ Bold = t.Bold, ColorCode = t.ColorCode, DisplayName = t.DisplayName, DisplayOrder= t.DisplayOrder, Italic = t.Italic, NoteTypeID = t.NoteTypeID}).OrderBy(bt => bt.DisplayOrder);
                toReturn.Add(sub,basicNoteTypesForSub);
            }
            return toReturn;
        }

        // GET: api/NoteType/5
        public Models.NoteType Get(int id)
        {
            Models.NoteType ntype = Models.NoteType.GetNoteType(id);
            if (ntype != null && ClaimsPrincipal.Current.IsInRole(ntype.SubName))
            {
                return ntype;
            }
            return null;
        }

        // POST: api/NoteType
        public void Post([FromBody]Models.NoteType value)
        {
            if (User.IsInRole(value.SubName.ToLower()))
            {
                
                Models.NoteType.AddNoteType(value);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
        }

        // PUT: api/NoteType/5
        public void Put([FromBody]Models.NoteType[] values)
        {
            List<Models.NoteType> toAdd = new List<Models.NoteType>();
            List<Models.NoteType> toUpdate = new List<Models.NoteType>();
            foreach (Models.NoteType nt in values)
            {
                if(ValidateNoteType(nt))
                {
                    if (nt.NoteTypeID == -1)
                    {
                        toAdd.Add(nt);
                    }
                    else
                    {
                        toUpdate.Add(nt);
                    }
                }
                else
                {
                    BadRequest();
                }
            }
            
        //if (User.IsInRole(value.SubName.ToLower()))
        //{
        //    Models.NoteType.UpdateNoteType(value);
        //}
        //else
        //{
        //    throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
        //}
    }

        // DELETE: api/NoteType/5
        public void Delete([FromBody]Models.NoteType value)
        {
            if (User.IsInRole(value.SubName.ToLower()))
            {
                Models.NoteType.DeleteNoteType(value);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
        }


        private bool ValidateNoteType(Models.NoteType ntype)
        {
            
            if (String.IsNullOrEmpty(ntype.SubName) || !ClaimsPrincipal.Current.IsInRole(ntype.SubName.ToLower()))
            {
                return false; //doesn't mod sub or empty/null sub, insta FAIL
            }
            if (ntype.NoteTypeID == -1)
            {
                //adding new note
            }
            else
            {
                var toModNT = Models.NoteType.GetNoteType(ntype.NoteTypeID);
                if(toModNT == null)
                {
                    return false; //NoteTypeID doesn't exist, FAIL
                }
                if(toModNT.SubName.ToLower() != ntype.SubName.ToLower())
                {
                    return false; //Subreddit name changed, FAIL
                }
                
            }
            if (String.IsNullOrEmpty(ntype.ColorCode))
            {
                return false; //No color code, FAIL
            }
            else if (ntype.ColorCode.Length != 3 && ntype.ColorCode.Length != 6)
            {
                return false; //Color code wrong length, FAIL
            }
            else if(!System.Text.RegularExpressions.Regex.IsMatch(ntype.ColorCode, @"\A\b[0-9a-fA-F]+\b\Z"))
            {
                return false; //Color code not valid hex, FAIL
            }
            if (String.IsNullOrEmpty(ntype.DisplayName))
            {
                return false; //Null or empty display name, FAIL
            }
            else if(ntype.DisplayName.Length > 20)
            {
                return false; //Displayname too long, FAIL
            }
            

            return true;
        }
    }
}
