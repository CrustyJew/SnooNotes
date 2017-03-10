import gulp from 'gulp';
import gulpif from 'gulp-if';
import { log, colors} from 'gulp-util';
import named from 'vinyl-named';
import gulpWebpack from 'webpack-stream';
import plumber from 'gulp-plumber';
import livereload from 'gulp-livereload';
var args = require('./lib/args');
var webpackConfig = require( '../webpack.config');

const ENV = args.production ? 'production' : 'development';

gulp.task('scripts', (cb) => {
    return gulp.src(['app/scripts/background.js','app/scripts/contentscript.js'])
      .pipe(plumber({
          errorHandler: function() {
              // Webpack will log the errors
          }
      }))
      .pipe(named())
      .pipe(gulpWebpack(webpackConfig, require('webpack'), (err, stats) => {
          log(`Finished '${colors.cyan('scripts')}'`, stats.toString({
              chunks: false,
              colors: true,
              cached: false,
              children: false
          }));
      }))
    .pipe(gulp.dest(`dist/${args.vendor}/scripts`))
    .pipe(gulpif(args.watch, livereload()));
});
