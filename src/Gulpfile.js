var fs = require("fs"),
    glob = require("glob"),
    path = require("path-posix"),
    merge = require("merge-stream"),
    gulpif = require("gulp-if"),
    gulp = require("gulp"),
    newer = require("gulp-newer"),
    plumber = require("gulp-plumber"),
    sourcemaps = require("gulp-sourcemaps"),
    less = require("gulp-less"),
    sass = require("gulp-sass"),
    autoprefixer = require("gulp-autoprefixer"),
    minify = require("gulp-minify-css"),
    typescript = require("gulp-typescript"),
    uglify = require("gulp-uglify"),
    rename = require("gulp-rename"),
    concat = require("gulp-concat"),
    header = require("gulp-header");

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
        return path.resolve(path.join(assetGroup.basePath, inputPath));
    });
    assetGroup.watchPaths = [];
    if (!!assetGroup.watch) {
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
    if (doConcat && !doRebuild) {
        // Force a rebuild of this asset group is the asset manifest file itself is newer than the output.
        var assetManifestStats = fs.statSync(assetGroup.manifestPath);
        var outputStats = fs.existsSync(assetGroup.outputPath) ? fs.statSync(assetGroup.outputPath) : null;
        doRebuild = !outputStats || assetManifestStats.mtime > outputStats.mtime;
    }
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
    return gulp.src(assetGroup.inputPaths)
        .pipe(gulpif(!doRebuild,
            gulpif(doConcat,
                newer(assetGroup.outputPath),
                newer({
                    dest: assetGroup.outputDir,
                    ext: ".css"
                }))))
        .pipe(plumber())
        .pipe(gulpif(generateSourceMaps, sourcemaps.init()))
        .pipe(gulpif("*.less", less()))
        .pipe(gulpif("*.scss", sass({
            precision: 10
        })))
        .pipe(gulpif(doConcat, concat(assetGroup.outputFileName)))
        .pipe(autoprefixer({ browsers: ["last 2 versions"] }))
        .pipe(header(
            "/*\n" +
            "** NOTE: This file is generated by Gulp and should not be edited directly!\n" +
            "** Any changes made directly to this file will be overwritten next time its asset group is processed by Gulp.\n" +
            //"** For more information, see the Readme.txt file in the Gulp solution folder.\n" +
            "*/\n\n"))
        .pipe(gulpif(generateSourceMaps, sourcemaps.write()))
        .pipe(gulp.dest(assetGroup.outputDir))
        .pipe(minify())
        .pipe(rename({
            suffix: ".min"
        }))
        .pipe(gulp.dest(assetGroup.outputDir));
}

function buildJsPipeline(assetGroup, doConcat, doRebuild) {
    assetGroup.inputPaths.forEach(function (inputPath) {
        var ext = path.extname(inputPath).toLowerCase();
        if (ext !== ".ts" && ext !== ".js")
            throw "Input file '" + inputPath + "' is not of a valid type for output file '" + assetGroup.outputPath + "'.";
    });
    var generateSourceMaps = assetGroup.hasOwnProperty("generateSourceMaps") ? assetGroup.generateSourceMaps : true;
    return gulp.src(assetGroup.inputPaths)
        .pipe(gulpif(!doRebuild,
            gulpif(doConcat,
                newer(assetGroup.outputPath),
                newer({
                    dest: assetGroup.outputDir,
                    ext: ".js"
                }))))
        .pipe(plumber())
        .pipe(gulpif(generateSourceMaps, sourcemaps.init()))
        .pipe(gulpif("*.ts", typescript({
            declaration: false,
            noImplicitAny: true,
            noEmitOnError: true,
            sortOutput: true,
        }).js))
        .pipe(gulpif(doConcat, concat(assetGroup.outputFileName)))
        .pipe(header(
            "/*\n" +
            "** NOTE: This file is generated by Gulp and should not be edited directly!\n" +
            "** Any changes made directly to this file will be overwritten next time its asset group is processed by Gulp.\n" +
            //"** For more information, see the Readme.txt file in the Gulp solution folder.\n" +
            "*/\n\n"))
        .pipe(gulpif(generateSourceMaps, sourcemaps.write()))
        .pipe(gulp.dest(assetGroup.outputDir))
        .pipe(uglify())
        .pipe(rename({
            suffix: ".min"
        }))
        .pipe(gulp.dest(assetGroup.outputDir));
}
