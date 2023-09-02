const defaultTheme = require('tailwindcss/defaultTheme')

module.exports = {
	content: [
		'./content/**/*.html',
		'./src/Krompaco.RecordCollector.Web/**/*.razor',
		'./src/stimulus_controllers/**/*.js',
	],
	theme: {
		extend: {
			fontFamily: {
				rc: ['-apple-system', 'BlinkMacSystemFont', '"Avenir Next"' , 'Avenir','"Nimbus Sans L"', 'Roboto', 'Noto', '"Segoe UI"','Arial','Helvetica', '"Helvetica Neue"', 'sans-serif', '"Apple Color Emoji"', '"Segoe UI Emoji"', '"Segoe UI Symbol"'],
				mono: ['Consolas', 'Menlo', 'Monaco', '"Andale Mono"', '"Ubuntu Mono"', 'monospace'],
			},
		},
	},
	plugins: [
		require('@tailwindcss/forms'),
		require('@tailwindcss/typography'),
	],
}
