var fs = require("fs"),
    glob = require("glob"),
    path = require("path-posix"),
    merge = require("merge-stream"),
    gulp = require("gulp"),
    gulpif = require("gulp-if"),
    print = require("gulp-print"),
    debug = require("gulp-debug"),
    newer = require("gulp-newer"),
    plumber = require("gulp-plumber"),
    sourcemaps = require("gulp-sourcemaps"),
    less = require("gulp-less"),
    sass = require("gulp-sass"),
    postcss = require("gulp-postcss"),
    autoprefixer = require("autoprefixer"),
    cssnano = require("cssnano"),
    typescript = require("gulp-typescript"),
    uglify = require("gulp-uglify"),
    rename = require("gulp-rename"),
    concat = require("gulp-concat"),
    header = require("gulp-header"),
    eol = require("gulp-eol");

// For compat with older versions of Node.js.
require("es6-promise").polyfill();

// To suppress memory leak warning from gulp.watch().
require("events").EventEmitter.prototype._maxListeners = 100;

/*
** GULP TASKS
*/

// Incremental build (each asset group is built only if one or more inputs are newer than the output).
gulp.task("build", function () {
    var assetGroupTasks = getAssetGroups().map(function (assetGroup) {
        var doRebuild = false;
        return createAssetGroupTask(assetGroup, doRebuild);
    });
    return merge(assetGroupTasks);
});

// Full rebuild (all assets groups are built regardless of timestamps).
gulp.task("rebuild", function () {
    var assetGroupTasks = getAssetGroups().map(function (assetGroup) {
        var doRebuild = true;
        return createAssetGroupTask(assetGroup, doRebuild);
    });
    return merge(assetGroupTasks);
});

// Continuous watch (each asset group is built whenever one of its inputs changes).
gulp.task("watch", function () {
    var pathWin32 = require("path");
    getAssetGroups().forEach(function (assetGroup) {
        var watchPaths = assetGroup.inputPaths.concat(assetGroup.watchPaths);
        var inputWatcher;
        function createWatcher() {
            inputWatcher = gulp.watch(watchPaths, function (event) {
                var isConcat = path.basename(assetGroup.outputFileName, path.extname(assetGroup.outputFileName)) !== "@";
                if (isConcat)
                    console.log("Asset file '" + event.path + "' was " + event.type + ", rebuilding asset group with output '" + assetGroup.outputPath + "'.");
                else
                    console.log("Asset file '" + event.path + "' was " + event.type + ", rebuilding asset group.");
                var doRebuild = true;
                var task = createAssetGroupTask(assetGroup, doRebuild);
            });
        }
        createWatcher();
        gulp.watch(assetGroup.manifestPath, function (event) {
            console.log("Asset manifest file '" + event.path + "' was " + event.type + ", restarting watcher.");
            inputWatcher.remove();
            inputWatcher.end();
            createWatcher();
        });
    });
});

/*
** ASSET GROUPS
*/

function getAssetGroups() {
    var assetManifestPaths = glob.sync("Orchard.Web/{Core,Modules,Themes}/*/Assets.json");
    var assetGroups = [];
    assetManifestPaths.forEach(function (assetManifestPath) {
        var assetManifest = require("./" + assetManifestPath);
        assetManifest.forEach(function (assetGroup) {
            resolveAssetGroupPaths(assetGroup, assetManifestPath);
            assetGroups.push(assetGroup);
        });
    });
    return assetGroups;
}

function resolveAssetGroupPaths(assetGroup, assetManifestPath) {
    assetGroup.manifestPath = assetManifestPath;
    assetGroup.basePath = path.dirname(assetManifestPath);
    assetGroup.inputPaths = assetGroup.inputs.map(function (inputPath) {
        var excludeFile = false;
        if (inputPath.startsWith('!')) {
            inputPath = inputPath.slice(1);
            excludeFile = true;
        }
        var newPath = path.resolve(path.join(assetGroup.basePath, inputPath));
        return (excludeFile ? '!' : '') + newPath;
    });
    assetGroup.watchPaths = [];
    if (assetGroup.watch) {
        assetGroup.watchPaths = assetGroup.watch.map(function (watchPath) {
            return path.resolve(path.join(assetGroup.basePath, watchPath));
        });
    }
    assetGroup.outputPath = path.resolve(path.join(assetGroup.basePath, assetGroup.output));
    assetGroup.outputDir = path.dirname(assetGroup.outputPath);
    assetGroup.outputFileName = path.basename(assetGroup.output);
}

function createAssetGroupTask(assetGroup, doRebuild) {
    var outputExt = path.extname(assetGroup.output).toLowerCase();
    var doConcat = path.basename(assetGroup.outputFileName, outputExt) !== "@";
    switch (outputExt) {
        case ".css":
            return buildCssPipeline(assetGroup, doConcat, doRebuild);
        case ".js":
            return buildJsPipeline(assetGroup, doConcat, doRebuild);
    }
}

/*
** PROCESSING PIPELINES
*/

function buildCssPipeline(assetGroup, doConcat, doRebuild) {
    assetGroup.inputPaths.forEach(function (inputPath) {
        var ext = path.extname(inputPath).toLowerCase();
        if (ext !== ".less" && ext !== ".scss" && ext !== ".css")
            throw "Input file '" + inputPath + "' is not of a valid type for output file '" + assetGroup.outputPath + "'.";
    });
    var generateSourceMaps = assetGroup.hasOwnProperty("generateSourceMaps") ? assetGroup.generateSourceMaps : true;
    var containsLessOrScss = assetGroup.inputPaths.some(function (inputPath) {
        var ext = path.extname(inputPath).toLowerCase();
        return ext === ".less" || ext === ".scss";
    });
    // Source maps are useless if neither concatenating nor transforming.
    if ((!doConcat || assetGroup.inputPaths.length < 2) && !containsLessOrScss)
        generateSourceMaps = false;
    var minifiedStream = gulp.src(assetGroup.inputPaths) // Minified output, source mapping completely disabled.
        .pipe(gulpif(!doRebuild,
            newer({
                dest: doConcat ? assetGroup.outputPath : assetGroup.outputDir,
                ext: doConcat ? null : ".css",
                extra: assetGroup.manifestPath // Force a rebuild of this asset group is the asset manifest file itself is newer than the output(s).
            })
        ))
        .pipe(plumber())
        .pipe(gulpif("*.less", less()))
        .pipe(gulpif("*.scss", sass({
            precision: 10
        })))
        .pipe(gulpif(doConcat, concat(assetGroup.outputFileName)))
        .pipe(postcss([
            autoprefixer({ browsers: ["last 2 versions"] }),
            cssnano({
                discardComments: { removeAll: true },
                discardUnused: false,
                mergeIdents: false,
                reduceIdents: false,
                zindex: false
            })
        ]))
        .pipe(eol())
        .pipe(rename(function (path) {
            if (assetGroup.flatten)
                path.dirname = "";
            if (assetGroup.separateMinified)
                path.dirname += "/min";
            else
                path.basename += ".min";
        }))
        .pipe(gulp.dest(assetGroup.outputDir));
    var devStream = gulp.src(assetGroup.inputPaths) // Non-minified output, with source mapping.
        .pipe(gulpif(!doRebuild,
            newer({
                dest: doConcat ? assetGroup.outputPath : assetGroup.outputDir,
                ext: doConcat ? null : ".css",
                extra: assetGroup.manifestPath // Force a rebuild of this asset group is the asset manifest file itself is newer than the output(s).
            })
        ))
        .pipe(plumber())
        .pipe(gulpif(generateSourceMaps, sourcemaps.init()))
        .pipe(gulpif("*.less", less()))
        .pipe(gulpif("*.scss", sass({
            precision: 10
        })))
        .pipe(gulpif(doConcat, concat(assetGroup.outputFileName)))
        .pipe(postcss([
            autoprefixer({ browsers: ["last 2 versions"] })
        ]))
        .pipe(header(
            "/*\n" +
            "** NOTE: This file is generated by Gulp and should not be edited directly!\n" +
            "** Any changes made directly to this file will be overwritten next time its asset group is processed by Gulp.\n" +
            "*/\n\n"))
        .pipe(gulpif(generateSourceMaps, sourcemaps.write()))
        .pipe(eol())
        .pipe(rename(function (path) {
            if (assetGroup.flatten)
                path.dirname = "";
        }))
        .pipe(gulp.dest(assetGroup.outputDir));
    return merge([minifiedStream, devStream]);
}

function buildJsPipeline(assetGroup, doConcat, doRebuild) {
    assetGroup.inputPaths.forEach(function (inputPath) {
        var ext = path.extname(inputPath).toLowerCase();
        if (ext !== ".ts" && ext !== ".js")
            throw "Input file '" + inputPath + "' is not of a valid type for output file '" + assetGroup.outputPath + "'.";
    });
    var generateSourceMaps = assetGroup.hasOwnProperty("generateSourceMaps") ? assetGroup.generateSourceMaps : true;
    // Source maps are useless if neither concatenating nor transpiling.
    if ((!doConcat || assetGroup.inputPaths.length < 2) && !assetGroup.inputPaths.some(function (inputPath) { return path.extname(inputPath).toLowerCase() === ".ts"; }))
        generateSourceMaps = false;
    var typeScriptOptions = { allowJs: true, noImplicitAny: true, noEmitOnError: true };
    if (assetGroup.typeScriptOptions)
        typeScriptOptions = Object.assign(typeScriptOptions, assetGroup.typeScriptOptions); // Merge override options from asset group if any.
    if (doConcat)
        typeScriptOptions.outFile = assetGroup.outputFileName;
    return gulp.src(assetGroup.inputPaths)
        .pipe(gulpif(!doRebuild,
            newer({
                dest: doConcat ? assetGroup.outputPath : assetGroup.outputDir,
                ext: doConcat ? null : ".js",
                extra: assetGroup.manifestPath // Force a rebuild of this asset group is the asset manifest file itself is newer than the output(s).
            })
        ))
        .pipe(plumber())
        .pipe(gulpif(generateSourceMaps, sourcemaps.init()))
        .pipe(typescript(typeScriptOptions))
        .pipe(header(
            "/*\n" +
            "** NOTE: This file is generated by Gulp and should not be edited directly!\n" +
            "** Any changes made directly to this file will be overwritten next time its asset group is processed by Gulp.\n" +
            "*/\n\n"))
        .pipe(gulpif(generateSourceMaps, sourcemaps.write()))
        .pipe(eol())
        .pipe(rename(function (path) {
            if (assetGroup.flatten)
                path.dirname = "";
        }))
        .pipe(gulp.dest(assetGroup.outputDir))
        .pipe(uglify())
        .pipe(eol())
        .pipe(rename(function (path) {
            if (assetGroup.separateMinified)
                path.dirname += "/min";
            else
                path.basename += ".min";
        }))
        .pipe(gulp.dest(assetGroup.outputDir));
}
