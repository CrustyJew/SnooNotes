# SnooNotes

## Add Note

```javascript
$.ajax({
       url: "https://snoonotes.com/api/note",
       method: "POST",
       datatype: "application/json",
       data: { "NoteTypeID": type, "SubName": sub, "Message": message, "AppliesToUsername": user, "Url": link }
      });
```

## Get NoteTypes
```javascript
$.ajax({
        url: "https://snoonotes.com/restapi/Subreddit" //optional + "/subname" to return only data for 1 subreddit,
        method: "GET"
      });
```

returns data on all modded subreddits activated in SnooNotes, or if given a subname, details on that subreddit

```javascript
[{
  SubredditID : int,
  SubName : string,
  Active : bool,
  BotSettings : {
    DirtbagUrl : string,
    DirtbagUsername : string
  },
  Settings : {
    AccessMask : int,
    NoteTypes : [{
      NoteTypeID : int,
      SubName : string,
      DisplayName : string,
      ColorCode : string,
      DisplayOrder : int,
      Bold : bool,
      Italic : bool
    }],
    PermBanID : int?, // ? == nullable
    TempBanID : int?
  }
}]
```

## Get Notes

```javascript
$.ajax({
        url: "https://snoonotes.com/api/Note/GetNotes",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(["username1","username2"])
      });
```

Returns 

```javascript
[
  "username1":[{
    NoteID : int,
    NoteTypeID : int,
    SubName : string,
    Submitter : string,
    Message : string,
    Url : string,
    TimeStamp : datetime,
    ParentSubreddit : string //used for cabal notes, otherwise null
  }]
]
```
