const defaultTheme = require('tailwindcss/defaultTheme')

module.exports = {
	mode: 'jit',
	purge: {
		content: [
			'./docs/content-record-collector-net/**/*.html',
			'./src/Krompaco.RecordCollector.Web/Views/**/*.cshtml',
			'./src/stimulus_controllers/**/*.js',
		]
	},
	theme: {
		extend: {
			fontFamily: {
				rc: ['-apple-system', 'BlinkMacSystemFont', '"Avenir Next"' , 'Avenir','"Nimbus Sans L"', 'Roboto', 'Noto', '"Segoe UI"','Arial','Helvetica', '"Helvetica Neue"', 'sans-serif', '"Apple Color Emoji"', '"Segoe UI Emoji"', '"Segoe UI Symbol"'],
				mono: ['JetBrainsMono', ...defaultTheme.fontFamily.mono],
			},
		},
	},
	plugins: [
		require('@tailwindcss/forms'),
		require('@tailwindcss/typography'),
		require('@tailwindcss/aspect-ratio'),
	],
}
