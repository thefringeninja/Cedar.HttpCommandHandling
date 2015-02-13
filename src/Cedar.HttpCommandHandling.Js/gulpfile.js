var gulp = require('gulp'),
    karma = require('gulp-karma');

gulp.task('jsTests', function () {
    
    return gulp.src(['./fake/*.js'])
        .pipe(karma({
            configFile: 'karma.conf.js',
            action: 'run'
        })).on('error', function (err) {
            console.log(err)
        });
});

gulp.task('default', [ 'jsTests']);