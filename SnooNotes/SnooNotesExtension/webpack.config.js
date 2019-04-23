var args = require('./tasks/lib/args');
var webpack = require('webpack');
var path = require("path");
const VueLoaderPlugin = require('vue-loader/lib/plugin')

const ENV = args.production ? 'production' : 'development';

var config = {
  devtool: args.sourcemaps ? 'inline-source-map' : false,
  watch: args.watch,
  mode: ENV,
  plugins: [
    new webpack.DefinePlugin({
      'process.env': {
        'NODE_ENV': JSON.stringify(ENV)
      },
      '__ENV__': JSON.stringify(ENV),
      '__VENDOR__': JSON.stringify(args.vendor)
    }),
    new VueLoaderPlugin()
  ],
  module: {
    rules: [{
        test: /\.js$/,
        // excluding some local linked packages.
        // for normal use cases only node_modules is needed.
        exclude: /node_modules|vue\/src|vue-router\//,
        loader: 'babel-loader',
        options: {
            presets: ['@babel/preset-env']
          }
      },
      {
        test: /\.scss$/,
        loaders: ['vue-style-loader', 'css-loader', 'sass-loader']
      },
      {
        test: /\.vue$/,
        loader: 'vue-loader',
        /*options: {
          loaders: {
            // Since sass-loader (weirdly) has SCSS as its default parse mode, we map
            // the "scss" and "sass" values for the lang attribute to the right configs here.
            // other preprocessors should work out of the box, no loader config like this necessary.
            'scss': 'vue-style-loader!css-loader!sass-loader',
            'sass': 'vue-style-loader!css-loader!sass-loader?indentedSyntax'
          }
          // other vue-loader options go here
        }*/
      },
      {
        test: /\.css$/,
        use: [
          'vue-style-loader',
          'css-loader'
        ]
      }
    ]
  },
  resolve: {
    alias: {
      'vue$': 'vue/dist/vue.esm.js',
      'styles': path.resolve(__dirname, './app/styles')
    }
  },
};

module.exports = config;
