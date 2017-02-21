//var path = require('path')
var webpack = require('webpack')

module.exports = {
    output: {
        filename: 'SnooNotes.js'
    },
    module: {
        rules: [
          {
              test: /\.vue$/,
              loader: 'vue-loader',
              options: {
                  loaders: {
                      // Since sass-loader (weirdly) has SCSS as its default parse mode, we map
                      // the "scss" and "sass" values for the lang attribute to the right configs here.
                      // other preprocessors should work out of the box, no loader config like this necessary.
                      'scss': 'vue-style-loader!css-loader!sass-loader',
                      'sass': 'vue-style-loader!css-loader!sass-loader?indentedSyntax',
                      'js': 'vue-ts-loader'
                  },
                  esModule: true,
                  exclude: /node_modules/
                  // other vue-loader options go here
              }
          },
          {
              enforce: 'pre',
              test: /\.js$/,
              loader: "source-map-loader"
          },
          {
              test: /\.ts$/,
              loader: 'vue-ts-loader',
              exclude: /node_modules/
          },
          {
              test: /\.(png|jpg|gif|svg)$/,
              loader: 'file-loader',
              options: {
                  name: '[name].[ext]?[hash]'
              }
          }
        ]
    },
    resolve: {
        extensions: [".tsx", ".ts", ".js"],
        alias: {
            'vue$': 'vue/dist/vue.common.js'
        }
    },
    devtool: 'source-map'
}

//if (process.env.NODE_ENV === 'production') {
//  module.exports.devtool = '#source-map'
//  // http://vue-loader.vuejs.org/en/workflow/production.html
//  module.exports.plugins = (module.exports.plugins || []).concat([
//    new webpack.DefinePlugin({
//      'process.env': {
//        NODE_ENV: '"production"'
//      }
//    }),
//    new webpack.optimize.UglifyJsPlugin({
//      sourceMap: true,
//      compress: {
//        warnings: false
//      }
//    }),
//    new webpack.LoaderOptionsPlugin({
//      minimize: true
//    })
//  ])
//}
