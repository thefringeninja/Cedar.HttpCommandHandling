var gulp = require('gulp'),
    karma = require('gulp-karma'),
    uglify = require('gulp-uglify'),
    rename = require('gulp-rename');

gulp.task('jsTests', function () {
    
    return gulp.src(['./fake/*.js'])
        .pipe(karma({
            configFile: 'karma.conf.js',
            action: 'run'
        })).on('error', function (err) {
            console.log(err)
        });
});

gulp.task('default', [ 'jsTests', 'jquery-dist', 'angular-dist']);

gulp.task('jquery-dist', function(){
    gulp.src('./jquery/src/*.js')
    .pipe(uglify())
    .pipe(rename('cedar-jquery.min.js'))
    .pipe(gulp.dest('dist'))
});

gulp.task('angular-dist', function(){
    gulp.src('./angular/src/*.js')
    .pipe(uglify({ mangle: false }))
    .pipe(rename('cedar-angular.min.js'))
    .pipe(gulp.dest('dist'))
});