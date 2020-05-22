import requireDir from 'require-dir';

import gulp from 'gulp';
// Check out the tasks directory
// if you want to modify tasks!
requireDir('./tasks');

gulp.task('styles', gulp.parallel(
    'styles:css',
    'styles:less',
    'styles:sass'
  ));
  
  
gulp.task('build', gulp.series(
  'clean', gulp.parallel(
    'manifest',
    'scripts',
    'styles',
    'pages',
    'images',
    'fonts',
    'chromereload'
  )
)
);

gulp.task('default', gulp.series('build'));



