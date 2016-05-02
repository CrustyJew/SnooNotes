var app = angular.module("SnooNotes");
app.controller("AuthCtrl", require("./auth"));
app.controller("HomeCtrl",require("./home"));
app.controller("NavCtrl", require("./nav"));
app.controller("NavSubCtrl", require("./navsub"));
app.controller("SubredditCtrl", require("./subreddit"));