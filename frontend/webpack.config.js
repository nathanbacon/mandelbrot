const HtmlWebpackPlugin = require("html-webpack-plugin");
const InlineSourceWebpackPlugin = require("inline-source-webpack-plugin");

module.exports = {
  entry: "./src/index.ts",
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: "ts-loader",
        exclude: /node_modules/,
      },
      {
        test: /\.(css)$/,
        use: [{ loader: "css-loader" }],
      },
    ],
  },
  resolve: {
    extensions: [".tsx", ".ts", ".js"],
  },
  plugins: [
    new HtmlWebpackPlugin({
      template: "./index.html",
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
  optimization: {
    minimize: false,
  },
};
