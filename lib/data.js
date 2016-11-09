var fs = require('fs');

function saveData(dataFile, data, done) {
  fs.writeFile(dataFile, JSON.stringify(data, null, 4), {
    encoding: 'utf8',
  }, function (err) {
    return done(err);
  });
}

function loadData(dataFile, done) {
  fs.readFile(dataFile, 'utf-8', function (err, content) {
    if (err) {
      return done(err);
    }

    try {
      return done(null, JSON.parse(content));
    } catch (err) {
      return done(err);
    }
  })
}


module.exports = {
  saveData: saveData,
  loadData: loadData,
}
