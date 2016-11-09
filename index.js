process.chdir(__dirname);
var express = require('express')
var expressHandlebars  = require('express-handlebars');
var multer  = require('multer')
var upload = multer({ dest: './uploads/' })
var dataLib = require('./lib/data');
var path = require('path');
var fs = require('fs');
var async = require('async');

var app = express();

app.engine('handlebars', expressHandlebars({
  defaultLayout: 'main',
  helpers: {
    select: function(value, options) {
    return options.fn(this)
      .split('\n')
      .map(function(v) {
        var t = 'value="' + value + '"'
        return ! RegExp(t).test(v) ? v : v.replace(t, t + ' selected="selected"')
      })
      .join('\n')
    }
  }
}));
app.set('view engine', 'handlebars');

// serwowanie plikow statycznych

app.use('/public', express.static('public'));
app.use('/images', express.static('uploads'));

app.get('/', function (req, res) {
  dataLib.loadData('data.json', function (err, data) {
    if (err) {
      return res.status(500).end();
    }

    var users = data.users.map(function (user) {
      return user.name;
    });

    var defaultUser = users[0];
    var currentUser = req.query.user;

    if (!currentUser || users.indexOf(currentUser) === -1) {
      currentUser = defaultUser;
    }

    res.render('home', {
      users: users,
      currentUser: currentUser,
      images: data.files.filter(function (file) {
        return file.user === currentUser;
      }).map(function (file) {
        return file.file;
      })
    });
  })
})

app.post('/upload', upload.array('images'), function (req, res) {
  if (!req.files.length) {
    return res.render('no-files');
  }

  async.waterfall([
    function (done) {
      dataLib.loadData('data.json', function(err, data) {
        if (err) {
          return done(err);
        }

        return done(null, data);
      });
    },
    function (data, done) {
      async.mapSeries(req.files, function (file, done) {
        var ext = path.extname(file.originalname);
        var basename = path.basename(file.path);

        fs.rename(file.path, path.join(file.destination, file.filename + ext), function (err) {
          return done(err, file.filename + ext);
        });
      }, function (err, mapped) {
        if (err) {
          return done(err);
        }

        var newFiles = data.files.filter(function (file) {
          return file.user !== req.body.user;
        });

        mapped.forEach(function (file) {
          newFiles.push({
            user: req.body.user,
            file: file,
          });
        })

        data.files = newFiles;

        return dataLib.saveData('data.json', data, done);
      })
    },
  ], function (err) {
    if (err) {
      return res.render('error');
    }

    return res.redirect('/?user=' + req.body.user);
  })
})

app.post('/user-images/:user', function (req, res) {
  dataLib.loadData('data.json', function (err, data) {
    if (err) {
      return res.status(500).json({
        error: 'Nie udało się odczytać danych o uzytkownikach.',
      });
    }

    if (!req.params.user || !data.users.some(function(user) {
      return user.name === req.params.user;
    })) {
      return res.status(500).json({
        error: 'Nie ma takiego uzytkownika.',
      });
    }

    var imagesLinks = data.files.filter(function (file) {
      return file.user === req.params.user;
    }).map(function (file) {
      return req.protocol + '://' + req.get('host') + '/images/' + file.file;
    });

    return res.json({
      images: imagesLinks,
    });
  });
})


app.listen(3000, function () {
  console.log('Example app listening on port 3000!')
})
