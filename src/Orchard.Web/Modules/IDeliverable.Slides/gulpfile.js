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
    plumber = require("gulp-plumber"),
    sourcemaps = require("gulp-sourcemaps"),
    less = require("gulp-less"),
    autoprefixer = require("gulp-autoprefixer"),
    minify = require("gulp-minify-css"),
    uglify = require("gulp-uglify"),
    rename = require("gulp-rename"),
    concat = require("gulp-concat"),
    merge = require("merge-stream");

/*
 * General tasks.
 */

gulp.task("build", ["buildLess", "buildJs"], function () {
});

gulp.task("watch", ["watchLess", "watchJs"], function () {
});

/*
 * LESS/CSS compilation tasks.
 */

var srcLessAdmin = [
    "Assets/Less/*.less"
];

var srcLessEngineBootstrap = [
    "SlideshowPlayerEngines/Bootstrap/Styles/carousel.less",
    "SlideshowPlayerEngines/Bootstrap/Styles/glyphicons.less",
];

var srcLessEngineJCarousel = [
    "SlideshowPlayerEngines/JCarousel/Styles/engine-jcarousel.less"
];

gulp.task("buildLess", function () {
    return merge([
        lessPipelineFrom(gulp.src(srcLessAdmin), "Styles", "Admin.css"),
        lessPipelineFrom(gulp.src(srcLessEngineBootstrap), "Styles", "Engine.Bootstrap.css"),
        lessPipelineFrom(gulp.src(srcLessEngineJCarousel), "Styles", "Engine.JCarousel.css")
    ]);
});

gulp.task("watchLess", function () {
    var watcher = gulp.watch([srcLessEngineBootstrap, srcLessEngineJCarousel], ["buildLess"]);
    watcher.on("change", function (event) {
        console.log("LESS file " + event.path + " was " + event.type + ", running the 'buildLess' task...");
    });
});

function lessPipelineFrom(inputStream, outputFolder, outputFile) {
    return inputStream
        .pipe(newer(outputFolder + "/" + outputFile))
		.pipe(plumber())
        .pipe(sourcemaps.init())
		.pipe(less())
		.pipe(concat(outputFile))
		.pipe(autoprefixer({ browsers: ["last 2 versions"] }))
        .pipe(sourcemaps.write())
		.pipe(gulp.dest(outputFolder))
		.pipe(minify())
		.pipe(rename({
		    suffix: ".min"
		}))
		.pipe(gulp.dest(outputFolder));
}

/*
 * JavaScript compilation tasks.
 */

var srcJsAdmin = [
    "Assets/JavaScript/*.js"
];

var srcJsEngineBootstrap = [
    "SlideshowPlayerEngines/Bootstrap/Scripts/transition.js",
	"SlideshowPlayerEngines/Bootstrap/Scripts/carousel.js"
];

var srcJsEngineJCarousel = [
    "SlideshowPlayerEngines/JCarousel/Scripts/modernizr.transitions.js",
    "SlideshowPlayerEngines/JCarousel/Scripts/jquery.jcarousel.js",
    "SlideshowPlayerEngines/JCarousel/Scripts/engine-jcarousel.js"
];

gulp.task("buildJs", function () {
    return merge([
        jsPipelineFrom(gulp.src(srcJsAdmin), "Scripts", "Admin.js"),
        jsPipelineFrom(gulp.src(srcJsEngineBootstrap), "Scripts", "Engine.Bootstrap.js"),
        jsPipelineFrom(gulp.src(srcJsEngineJCarousel), "Scripts", "Engine.JCarousel.js"),
    ]);
});

gulp.task("watchJs", function () {
    var watcher = gulp.watch([srcJsEngineBootstrap, srcJsEngineJCarousel], ["buildJs"]);
    watcher.on("change", function (event) {
        console.log("JavaScript file " + event.path + " was " + event.type + ", running the 'buildJs' task...");
    });
});

function jsPipelineFrom(inputStream, outputFolder, outputFile) {
    return inputStream
        .pipe(newer(outputFolder + "/" + outputFile))
		.pipe(plumber())
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
