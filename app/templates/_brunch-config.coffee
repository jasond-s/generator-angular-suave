module.exports = config:
	modules: wrapper: false
	plugins: uglify: mangle: true
	paths: 
		public: 'src/<%= baseName %>/Content/dist'
		watched: [ 'src/<%= baseName %>/Content/app', 'src/<%= baseName %>/Content/css' ]
	files:
		javascripts: joinTo: 'app.js'
		stylesheets: joinTo: 'app.css'
