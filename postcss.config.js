// Minification of CSS
const cssnano = require("cssnano")({
	preset: "default"
});

module.exports = {
	plugins: [
		require('tailwindcss'),
		require('autoprefixer'),
		...([cssnano])]
};
