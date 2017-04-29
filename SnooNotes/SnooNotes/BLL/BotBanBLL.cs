using Microsoft.AspNetCore.Identity;
using SnooNotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SnooNotes.BLL
{
    public class BotBanBLL : IBotBanBLL
    {
        private const string AUTOMOD_WIKI_PAGE = "config/automoderator";
        private const string WIKI_SECTION_START_IDENTIFIER = "#***DIRTBAG BOT SECTION***";
        private const string WIKI_SECTION_END_IDENTIFIER = "#***END DIRTBAG BOT SECTION***";

        private DAL.IBotBanDAL bbDAL;
        private UserManager<ApplicationUser> userManager;
        private Utilities.IAuthUtils authUtils;
        private DAL.IYouTubeDAL ytDAL;
        private RedditSharp.RefreshTokenWebAgentPool agentPool;
        public BotBanBLL(DAL.IBotBanDAL botBanDAL, UserManager<ApplicationUser> userManager, Utilities.IAuthUtils authUtils, DAL.IYouTubeDAL youtubeDAL, RedditSharp.RefreshTokenWebAgentPool agentPool )
        {
            bbDAL = botBanDAL;
            this.userManager = userManager;
            this.authUtils = authUtils;
            ytDAL = youtubeDAL;
            this.agentPool = agentPool;
        }

        public async Task BanChannel(Models.BannedEntity channel)
        {
            string vidID = Helpers.YouTubeHelpers.ExtractVideoId(channel.ChannelURL);
            var vidInfo = await ytDAL.GetChannelIDAndName(vidID);
            await bbDAL.BanChannel(channel, vidInfo.Key, vidInfo.Value, VideoProvider.YouTube); //TODO support more vid providers
        }

        public async Task BanUser(Models.BannedEntity user)
        {
            await bbDAL.BanUser(new Models.BannedEntity[] { user });
            var bannedUsers = await bbDAL.GetBannedUsers(user.SubName);
            string reason = $"Banned {user.UserName} : {user.BanReason}";

            //only used if changing to allowing banning of multiple users at a time.
            //if (reason.Length > 255) reason = $"Banned {string.Join(",", userEntities.Select(e => e.EntityString))} for {string.Join(",", userEntities.Select(e => e.BanReason).Distinct())} by {string.Join(",", userEntities.Select(e => e.BannedBy).Distinct())}";
            //if (reason.Length > 255) reason = "Banned lots of things and the summary is too long for the description.. RIP";

            bool done = false;
            int count = 1;

            RedditSharp.Things.Subreddit subreddit;
            var ident = await userManager.FindByNameAsync(user.BannedBy);
            var webAgent = await agentPool.GetOrCreateWebAgentAsync(user.BannedBy, (uname, uagent, rlimit) =>
            {
               return Task.FromResult<RedditSharp.RefreshTokenPoolEntry>(new RedditSharp.RefreshTokenPoolEntry(uname, ident.RefreshToken, rlimit, uagent));
            });
            
            if (!ident.HasWiki) { throw new UnauthorizedAccessException("Need Wiki Permissions"); }
            if (!ident.HasConfig) { throw new UnauthorizedAccessException("Need Config Permissions"); }
            RedditSharp.Reddit rd = new RedditSharp.Reddit(webAgent, true);

            subreddit = await rd.GetSubredditAsync(user.SubName);

            while (!done && count < 5)
            {
                try
                {
                    done = await SaveAutoModConfig(reason, subreddit);
                }
                catch (WebException ex)
                {
                    if((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw;
                    }
                    else count++;
                    await Task.Delay(100);
                }
            }

        }
        public async Task<bool> SaveAutoModConfig(string editReason, RedditSharp.Things.Subreddit subreddit)
        {

            RedditSharp.WikiPage automodWiki;
            string wikiContent = "";
            try
            {
                automodWiki = await subreddit.GetWiki.GetPageAsync(AUTOMOD_WIKI_PAGE);
                wikiContent = WebUtility.HtmlDecode(automodWiki.MarkdownContent);
            }
            catch (WebException ex)
            {
                if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                {
                    wikiContent = WIKI_SECTION_START_IDENTIFIER + DefaultBotConfigSection + WIKI_SECTION_END_IDENTIFIER;
                }
                else
                {
                    throw;
                }
            }
            string botConfigSection = "";
            bool noStart = false;
            bool noEnd = false;
            if (!wikiContent.Contains(WIKI_SECTION_START_IDENTIFIER)) noStart = true;
            if (!wikiContent.Contains(WIKI_SECTION_END_IDENTIFIER)) noEnd = true;

            if (noStart && noEnd)
            {
                wikiContent += $@"
{WIKI_SECTION_START_IDENTIFIER + DefaultBotConfigSection + WIKI_SECTION_END_IDENTIFIER}
";
            }

            else if (noStart || noEnd) throw new Exception("Wiki contains a start or an end section, but not both");

            string updatedWiki = wikiContent;

            int startBotSection = wikiContent.IndexOf(WIKI_SECTION_START_IDENTIFIER) + WIKI_SECTION_START_IDENTIFIER.Length;
            int botSectionLength = wikiContent.IndexOf(WIKI_SECTION_END_IDENTIFIER, startBotSection) - startBotSection;
            if (botSectionLength < 0) { throw new Exception("End section identifier is before the start identifier"); }
            botConfigSection = wikiContent.Substring(startBotSection, botSectionLength);


            var ents = await bbDAL.GetBannedUsers(subreddit.Name);
            string entsString = string.Join(", ", ents.Select(e => "\"" + e.UserName + "\""));
            updatedWiki = updatedWiki.Remove(startBotSection, botSectionLength);
            updatedWiki = updatedWiki.Insert(startBotSection, String.Format(DefaultBotConfigSection, entsString));

            await subreddit.GetWiki.EditPageAsync(AUTOMOD_WIKI_PAGE, updatedWiki, reason: editReason);
            return true;


        }

        private const string DefaultBotConfigSection = @"
---
author:
    name: [{0}]
action: remove
action_reason: ""Codename_Dirtbag Banned Author: {{{{match-1}}}}""
priority: 9001
---
";
    }
}

