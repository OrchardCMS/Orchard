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
	merge = require("merge-stream")

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

var srcLessLib = [
    "Styles/Lib/Bootstrap/bootstrap.less",
    "Styles/Lib/FontAwesome/font-awesome.less"
];

var srcLessLayoutEditor = [
    "Styles/LayoutEditor/Editor.less",
    "Styles/LayoutEditor/Element.less",
    "Styles/LayoutEditor/Container.less",
    "Styles/LayoutEditor/Canvas.less",
    "Styles/LayoutEditor/Row.less",
    "Styles/LayoutEditor/Column.less",
    "Styles/LayoutEditor/Content.less",
    "Styles/LayoutEditor/Toolbox.less",
    "Styles/LayoutEditor/Popup.less"
];

gulp.task("buildLess", function () {
    return merge([
        lessPipelineFrom(gulp.src(srcLessLib), "Styles", "Lib.css"),
        lessPipelineFrom(gulp.src(srcLessLayoutEditor), "Styles", "LayoutEditor.css")
    ]);
});

gulp.task("watchLess", function () {
    var watcher = gulp.watch([srcLessLib, srcLessLayoutEditor], ["buildLess"]);
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

var srcJsLib = [
	"Scripts/Lib/underscore.js",
	"Scripts/Lib/angular.js",
	"Scripts/Lib/angular-sanitize.js",
	"Scripts/Lib/angular-resource.js",
	"Scripts/Lib/sortable.js"
];

var srcJsLayoutEditor = [
    "Scripts/LayoutEditor/Module.js",
    "Scripts/LayoutEditor/Services/Clipboard.js",
    "Scripts/LayoutEditor/Services/ScopeConfigurator.js",
    "Scripts/LayoutEditor/Directives/Editor.js",
    "Scripts/LayoutEditor/Directives/Canvas.js",
    "Scripts/LayoutEditor/Directives/Child.js",
    "Scripts/LayoutEditor/Directives/Column.js",
    "Scripts/LayoutEditor/Directives/Content.js",
    "Scripts/LayoutEditor/Directives/Html.js",
    "Scripts/LayoutEditor/Directives/Grid.js",
    "Scripts/LayoutEditor/Directives/Row.js",
    "Scripts/LayoutEditor/Directives/Popup.js",
    "Scripts/LayoutEditor/Directives/Toolbox.js",
    "Scripts/LayoutEditor/Directives/ToolboxGroup.js"
];

var srcJsModels = [
    "Scripts/Models/Helpers.js",
    "Scripts/Models/Editor.js",
    "Scripts/Models/Element.js",
    "Scripts/Models/Container.js",
    "Scripts/Models/Canvas.js",
    "Scripts/Models/Grid.js",
    "Scripts/Models/Row.js",
    "Scripts/Models/Column.js",
    "Scripts/Models/Content.js",
    "Scripts/Models/Html.js"
];

gulp.task("buildJs", function () {
    return merge([
        jsPipelineFrom(gulp.src(srcJsLib), "Scripts", "Lib.js"),
        jsPipelineFrom(gulp.src(srcJsLayoutEditor), "Scripts", "LayoutEditor.js"),
        jsPipelineFrom(gulp.src(srcJsModels), "Scripts", "Models.js")
    ]);
});

gulp.task("watchJs", function () {
    var watcher = gulp.watch([srcJsLib, srcJsLayoutEditor, srcJsModels], ["buildJs"]);
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
