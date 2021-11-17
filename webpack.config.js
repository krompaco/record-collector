const path = require('path');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const WriteFileWebpackPlugin = require('write-file-webpack-plugin');

module.exports = {
	entry: './index.js',
	output: {
		filename: 'main.js',
		path: path.resolve(__dirname, './src/Krompaco.RecordCollector.Web/wwwroot/dist/')
	},
	mode: 'production',
	watchOptions: {
		ignored: '**/node_modules',
	},
	devtool: 'source-map',
	module: {
		rules: [
			{
				test: /\.js$/,
				exclude: [
					/node_modules/
				],
				use: [
					{
						loader: "babel-loader",
						options: {
							presets: ['@babel/preset-env'],
							plugins: ['@babel/plugin-proposal-class-properties', '@babel/plugin-transform-runtime']
						}
					}
				]
			},
			{
				test: /\.css$/,
				use: [MiniCssExtractPlugin.loader,
				{
					loader: 'css-loader',
					options: {
						importLoaders: 1,
						sourceMap: true,
						url: false
					}
				},
				{
					loader: 'postcss-loader',
					options: {
						sourceMap: true
					}
				}],
			},
			// // {
			// // 	test: /\.(png|woff|woff2|eot|ttf|svg)$/,
			// // 	use: [
			// // 		{
			// // 			loader: 'url-loader',
			// // 			options: {
			// // 				limit: 10000,
			// // 			},
			// // 		},
			// // 	],
			// // }
		]
	},
	plugins: [
		new MiniCssExtractPlugin({
			filename: './styles.css'
		}),
		new WriteFileWebpackPlugin({
			test: /\.css$/,
			useHashIndex: true
		}),
	]
};
