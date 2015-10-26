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

  var appDir = 'src/' + _s.capitalize(this.baseName) + '/'
  var appDirApp = 'src/' + _s.capitalize(this.baseName) + '.F/'
  var x64Dir = appDir + 'x64/'
  var x86Dir = appDir + 'x86/'
  var publicDir = appDir + 'Content/'

  this.mkdir(x64Dir);
  this.mkdir(x86Dir);
  this.mkdir(appDir);
  this.mkdir(publicDir);

  this.template('_App.sln', _s.capitalize(this.baseName) + '.sln');


  // WEB PROJECT

  this.copy('src/_App/App.config', appDir + 'App.config');
  this.copy('src/_App/_packages.config', appDir + 'packages.config');
  this.template('src/_App/src/_App.fsproj', appDir + _s.capitalize(this.baseName) + '.fsproj');
  this.template('src/_App/_Main.fs', appDir + 'Main.fs');

  var publicCssDir = publicDir + 'css/';
  var publicJsDir = publicDir + 'app/';
  var publicViewDir = publicDir + 'views/';
  this.mkdir(publicCssDir);
  this.mkdir(publicJsDir);
  this.mkdir(publicViewDir);
  this.template('src/_App/Content/_index.html', publicDir + 'index.html');
  this.copy('src/_App/Content/css/app.css', publicCssDir + 'app.css');
  this.template('src/_App/Content/app/src/_App.js', publicJsDir + 'app.js');
  this.template('src/_App/Content/app/home/_home-controller.js', publicJsDir + 'home/home-controller.js');
  this.template('src/_App/Content/views/home/_home.html', publicViewDir + 'home/home.html');


  // FSHARP PROJECT TEMPLATE

  this.copy('src/_App.F/x64/SQLite.Interop.dll', x64Dir + 'SQLite.Interop.dll');
  this.copy('src/_App.F/x86/SQLite.Interop.dll', x86Dir + 'SQLite.Interop.dll');
  this.copy('src/_App.F/App.config', appDir + 'App.config');
  this.copy('src/_App.F/_packages.config', appDir + 'packages.config');
  this.template('src/_App.F/src/_App.F.fsproj', appDir + _s.capitalize(this.baseName) + '.fsproj');
  this.template('src/_App.F/_Main.fs', appDir + 'Main.fs');
  this.template('src/_App.F/_Domain.fs', appDir + 'Domain.fs');
};

AngularSuaveGenerator.prototype.projectfiles = function projectfiles() {
  this.copy('editorconfig', '.editorconfig');
  this.copy('jshintrc', '.jshintrc');
};
