/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp');
var webpackGulp = require('webpack-stream');
var webpack = require('webpack');
var config = require('./webpack.config.js');
var del = require('del');
const debug = require('gulp-debug');

var entry = './src/content/main';
//config.entry = entry;

gulp.task('default', function () {
    // place code for your default task here
});

gulp.task('build', ['clean'], function () {
    gulp.src(['src/**/*.ts','src/**/*.html','src/**/*.scss'])
    .pipe(webpackGulp(config,webpack))
    .pipe(gulp.dest('./dist/'));

    gulp.src('./src/manifest.json')
    .pipe(gulp.dest('./dist/'));
});

gulp.task('clean', function () {
    return del(['./dist/']);
})