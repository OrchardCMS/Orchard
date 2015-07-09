/// <binding BeforeBuild='build' ProjectOpened='watch' />

/*
 * This gulpfile provides an automated build pipeline for client-side assets in this project.
 * 
 * To use this file you will need to:
 * - Install Node.js on your machine
 * - Run "npm install" in this folder (either via command line or a Visual Studio extension) to install dependency packages from package.json
 * 
 * NOTE: If you install the Task Runner Explorer extension in Visual Studio the tasks in this
 * gulpfile will execute automatically on VS events for a more integrated/automated workflow. That's the 
 * purpose of the <binding> comment element at the top.
 */

var gulp = require("gulp"),
    newer = require("gulp-newer"),
	minify = require("gulp-minify-css"),
	uglify = require("gulp-uglify"),
	rename = require("gulp-rename"),
	concat = require("gulp-concat"),
    sourcemaps = require("gulp-sourcemaps"),
	merge = require("merge-stream");

/*
 * General tasks.
 */

gulp.task("build", ["buildCss", "buildJs"], function () {
});

gulp.task("watch", ["watchCss", "watchJs"], function () {
});

/*
 * LESS/CSS compilation tasks.
 */

var srcCss = [
    "Styles/forms-admin.css",
    "Styles/forms.css",
    "Styles/menu.dynamicforms-admin.css"
];

gulp.task("buildCss", function () {
    return gulp.src(srcCss)
		.pipe(minify())
		.pipe(rename({
		    suffix: ".min"
		}))
		.pipe(gulp.dest("Styles"));
});

gulp.task("watchCss", function () {
    var watcher = gulp.watch(srcCss, ["buildCss"]);
    watcher.on("change", function (event) {
        console.log("CSS file " + event.path + " was " + event.type + ", running the 'buildCss' task...");
    });
});

/*
 * JavaScript compilation tasks.
 */

var srcJsLib = [
	"Scripts/Lib/jquery.validate.js",
	"Scripts/Lib/jquery.validate.unobtrusive.additional.js",
	"Scripts/Lib/jquery.validate.unobtrusive.js"
];

var srcJsLayoutEditor = [
    "Scripts/LayoutEditor/Models/*.js",
    "Scripts/LayoutEditor/Directives/*.js"
];

gulp.task("buildJs", function () {
    return merge([
        jsPipelineFrom(gulp.src(srcJsLib), "Scripts", "Lib.js"),
        jsPipelineFrom(gulp.src(srcJsLayoutEditor), "Scripts", "LayoutEditor.js")
    ]);
});

gulp.task("watchJs", function () {
    var watcher = gulp.watch([srcJsLib, srcJsLayoutEditor], ["buildJs"]);
    watcher.on("change", function (event) {
        console.log("JavaScript file " + event.path + " was " + event.type + ", running the 'buildJs' task...");
    });
});

function jsPipelineFrom(inputStream, outputFolder, outputFile) {
    return inputStream
        .pipe(newer(outputFolder + "/" + outputFile))
        .pipe(sourcemaps.init())
		.pipe(concat(outputFile))
        .pipe(sourcemaps.write())
		.pipe(gulp.dest(outputFolder))
		.pipe(uglify())
		.pipe(rename({
		    suffix: ".min"
		}))
		.pipe(gulp.dest(outputFolder));
}