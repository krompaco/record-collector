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
				rc: ['sans-serif'],
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
