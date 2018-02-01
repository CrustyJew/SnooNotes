var app = angular.module("SnooNotes");
app.controller("AuthCtrl", require("./auth"));
app.controller("HomeCtrl",require("./home"));
app.controller("NavCtrl", require("./nav"));
app.controller("NavSubCtrl", require("./navsub"));
app.controller("SubredditSettingsCtrl", require("./subredditSettings"));
app.controller("SubredditCtrl", require("./subreddit"));
app.controller("BannedEntitiesCtrl", require("./bannedEntities"));
app.controller("UserKeyCtrl", require("./userKey"));