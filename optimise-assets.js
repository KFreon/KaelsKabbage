// Get all processable files, except those already processed
// Video = ones (avi?) without postfixes
// Pictures = non-webms

// pictures - convert to webm - need to figure out what the settings for that are
// Videos - AV1, x264, and VP9

// Move the original somewhere gitignored?
// Can probably do video and movies in parallel


var fs = require('fs');

fs.readdir(path, function(err, items) {
    console.log(items);
 
    for (var i=0; i<items.length; i++) {
        console.log(items[i]);
    }
});