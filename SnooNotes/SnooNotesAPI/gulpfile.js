/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require("gulp"),
    del = require("del"),
    browserify = require('browserify'),
    source = require('vinyl-source-stream'),
    buffer = require('vinyl-buffer'),
    vinylPaths = require('vinyl-paths'),
    ngAnnotate = require('gulp-ng-annotate'),
    sourcemaps = require('gulp-sourcemaps'),
    sass = require('gulp-sass'),
    mainBowerFiles = require('main-bower-files'),
    concat = require("gulp-concat"),
    templateCache = require("gulp-angular-templatecache"),
    uglify = require("gulp-uglify"),
    ngAnnotate = require('browserify-ngannotate');
//
//cssmin = require("gulp-cssmin"),
//uglify = require("gulp-uglify");

var CacheBuster = require('gulp-cachebust');
var cachebust = new CacheBuster();


var config = {};

config.buildDir = "./build/"; //dont change this unless you change .csproj build reference as well
config.sourceDir = "./app/";
gulp.task("clean", function () {
    del.sync([config.buildDir + '**', '!' + config.buildDir]);

});
gulp.task("Debug", ["clean", "css", "templates", "browserify-debug"], function () {

});

gulp.task("Release", ["clean", "css", "templates", "browserify-min"], function () {

});

gulp.task("browserify-debug", function () {
    return browserify(config.sourceDir + 'app.js', { insertGlobals: true, debug: true })
        .bundle()
        .pipe(source('app.js'))
        .pipe(gulp.dest(config.buildDir));
});

gulp.task("browserify-min", function () {
    return browserify(config.sourceDir + 'app.js', { insertGlobals: true, debug: true, transform: [ngAnnotate] })
        .bundle()
        .pipe(source('app.js'))
        .pipe(buffer())
        .pipe(sourcemaps.init({ loadMaps: true }))
        .pipe(uglify())
        .pipe(sourcemaps.write('./maps'))
        .pipe(gulp.dest(config.buildDir));
})

gulp.task("css", function () {
    return gulp.src(config.sourceDir + "site.scss")
        .pipe(sourcemaps.init())
        .pipe(sass())
        //.pipe(cachebust.resources())
        .pipe(sourcemaps.write("./maps"))
        .pipe(gulp.dest(config.buildDir));
});

gulp.task("templates", function () {
    return gulp.src(config.sourceDir + "templates/**/*.html")
        .pipe(templateCache("templates.js", { module: 'templates', standalone: true }))
        .pipe(gulp.dest(config.buildDir));
});


gulp.task("bower", function () {
    return gulp.src(mainBowerFiles({
        debugging: true,
        paths: {
            bowerDirectory: 'vendor'
        }
    }))
        .pipe(concat('vendor.js'))
        .pipe(gulp.dest(config.buildDir));
})

gulp.task("watch", function () {
    gulp.watch(config.sourceDir + "templates/**/*.html", ["templates"]);
    gulp.watch(config.sourceDir + "**/*.js", ["browserify"]);
    gulp.watch(config.sourceDir + "**/*.scss", ["css"]);
});
