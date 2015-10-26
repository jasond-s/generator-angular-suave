'use strict';
var util = require('util'),
    path = require('path'),
    yeoman = require('yeoman-generator'),
    _ = require('lodash'),
    _s = require('underscore.string'),
    pluralize = require('pluralize'),
    asciify = require('asciify');

var AngularSuaveGenerator = module.exports = function AngularSuaveGenerator(args, options, config) {
  yeoman.generators.Base.apply(this, arguments);

  this.on('end', function () {
    this.installDependencies({ skipInstall: options['skip-install'] });
  });

  this.pkg = JSON.parse(this.readFileAsString(path.join(__dirname, '../package.json')));
};

util.inherits(AngularSuaveGenerator, yeoman.generators.Base);

AngularSuaveGenerator.prototype.askFor = function askFor() {

  var cb = this.async();

  console.log('\n' +
    '+-+-+-+-+-+-+-+ +-+-+-+-+-+ +-+-+-+-+-+-+-+-+-+-+-+-+\n' +
    '|a|n|g|u|l|a|r| |s|u|a|v|e| |g|e|n|e|r|a|t|o|r| |v|2|\n' +
    '+-+-+-+-+-+-+-+ +-+-+-+-+-+ +-+-+-+-+-+-+-+-+-+-+-+-+\n' +
    '\n');

  var prompts = [{
    type: 'input',
    name: 'baseName',
    message: 'What is the name of your application?',
    default: 'myapp'
  },
  {
    type: 'list',
    name: 'platform',
    message: 'Which platform would you like to target?',
    choices: ['Mono', 'Windows'],
    default: 'Windows'
  }];

  this.prompt(prompts, function (props) {
    this.baseName = props.baseName;
    this.platform = props.platform;

    cb();
  }.bind(this));
};

AngularSuaveGenerator.prototype.app = function app() {

  this.entities = [];
  this.resources = [];
  this.generatorConfig = {
    "baseName": this.baseName,
    "platform": this.platform,
    "entities": this.entities,
    "resources": this.resources
  };
  this.generatorConfigStr = JSON.stringify(this.generatorConfig, null, '\t');


  // SOLUTION

  this.template('_generator.json', 'generator.json');
  this.template('_package.json', 'package.json');
  this.template('_bower.json', 'bower.json');
  this.template('_brunch-config.coffee', 'brunch-config.coffee');
  this.template('bowerrc', '.bowerrc');
  this.copy('gitignore', '.gitignore');

  var appDir = 'src/' + _s.capitalize(this.baseName) + '/';
  var x64Dir = appDir + 'x64/';
  var x86Dir = appDir + 'x86/';
  var publicDir = appDir + 'Content/';

  var appDirApp = 'src/' + _s.capitalize(this.baseName) + '.F/';
  var x64DirApp = appDirApp + 'x64/';
  var x86DirApp = appDirApp + 'x86/';

  var appDirB = 'src/_App/';
  var x64DirB = appDirB + 'x64/';
  var x86DirB = appDirB + 'x86/';
  var publicDirB = appDirB + 'Content/';

  var appDirAppB = 'src/_App.F/';
  var x64DirAppB = appDirAppB + 'x64/';
  var x86DirAppB = appDirAppB + 'x86/';


  this.mkdir(appDir);
  this.mkdir(x64Dir);
  this.mkdir(x86Dir);
  this.mkdir(publicDir);

  this.mkdir(appDirApp);
  this.mkdir(x64DirApp);
  this.mkdir(x86DirApp);

  this.copy('src/.paket/paket.targets', 'src/.paket/paket.targets');
  this.copy('src/.paket/paket.exe', 'src/.paket/paket.exe');
  this.copy('src/.paket/paket.bootstrapper.exe', 'src/.paket/paket.bootstrapper.exe');

  this.template('src/_App.sln', 'src/' + _s.capitalize(this.baseName) + '.sln');
  this.copy('src/paket.dependencies', 'src/paket.dependencies');
  this.copy('src/paket.lock', 'src/paket.lock');


  // WEB PROJECT

  this.copy(appDirB + 'App.config', appDir + 'App.config');
  this.copy(appDirB + 'paket.references', appDir + 'paket.references');
  this.template(appDirB + '_App.fsproj', appDir + _s.capitalize(this.baseName) + '.fsproj');
  this.template(appDirB + '_Main.fs', appDir + 'Main.fs');

  var publicCssDir = publicDir + 'css/';
  var publicJsDir = publicDir + 'app/';
  var publicViewDir = publicDir + 'views/';
  var publicCssDirB = publicDirB + 'css/';
  var publicJsDirB = publicDirB + 'app/';
  var publicViewDirB = publicDirB + 'views/';
  this.mkdir(publicCssDir);
  this.mkdir(publicJsDir);
  this.mkdir(publicViewDir);

  this.template(publicDirB +'_index.html', publicDir + 'index.html');
  this.copy(publicCssDirB + 'app.css', publicCssDir + 'app.css');
  this.template(publicJsDirB + '_App.js', publicJsDir + 'app.js');
  this.template(publicJsDirB + '/home/_home-controller.js', publicJsDir + 'home/home-controller.js');
  this.template(publicViewDirB + '/home/_home.html', publicViewDir + 'home/home.html');


  // FSHARP PROJECT TEMPLATE

  this.copy(x64DirAppB + 'SQLite.Interop.dll', x64DirApp + 'SQLite.Interop.dll');
  this.copy(x64DirAppB + 'SQLite.Interop.dll', x86DirApp + 'SQLite.Interop.dll');
  this.copy(appDirAppB + 'App.config', appDirApp + 'App.config');
  this.copy(appDirAppB + 'paket.references', appDirApp + 'paket.references');
  this.template(appDirAppB + '_App.F.fsproj', appDirApp + _s.capitalize(this.baseName) + '.F.fsproj');
  this.template(appDirAppB + '_Main.fs', appDirApp + 'Main.fs');
  this.template(appDirAppB + '_Domain.fs', appDirApp + 'Domain.fs');
};

AngularSuaveGenerator.prototype.projectfiles = function projectfiles() {
  this.copy('editorconfig', '.editorconfig');
  this.copy('jshintrc', '.jshintrc');
};
