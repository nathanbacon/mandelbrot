const HtmlWebpackPlugin = require("html-webpack-plugin");
const InlineSourceWebpackPlugin = require("inline-source-webpack-plugin");

module.exports = {
  entry: "./index.ts",
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: "ts-loader",
        exclude: /node_modules/,
      },
    ],
  },
  resolve: {
    extensions: [".tsx", ".ts", ".js"],
  },
  plugins: [
    new HtmlWebpackPlugin({
      template: "index.html",
      filename: "index.html",
      inject: "body",
    }),
    new InlineSourceWebpackPlugin({
      compress: true,
      rootpath: "./src",
      noAssetMatch: "warn",
    }),
  ],
  output: {
    filename: "bundle.js",
  },
};
