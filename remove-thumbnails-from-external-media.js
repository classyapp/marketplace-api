db.classifieds.find({"ExternalMedia.Thumbnails":{$exists:true}})
.forEach( function(doc) {
  var arr = doc.ExternalMedia;
  var length = arr.length;
  for (var i = 0; i < length; i++) {
    delete arr[i]["Thumbnails"];
  }
  db.classifieds.save(doc);
});

db.profiles.update({"Avatar.Thumbnails":{$exists:true}},
{$unset:{"Avatar.Thumbnails":1}}, {multi:true});