/*
 * This gulpfile enables compilation of the TypeScript files in this project. The TypeScript 
 * project properties have been removed, and this glupfile created instead, to remove the requirement 
 * on Visual Studio and TypeScript for compiling/building Orchard while still allowing TypeScript 
 * code in this project to be maintained.
 * 
 * To use this file you will need to:
 * - Install Node.js on your machine
 * - Install the following Node.js modules in this project folder:
 *    npm install gulp
 *    npm install gulp-typescript
 * 
 * NOTE: The above Node.js modules should not be committed to version control. They should only be used
 * locally on your dev machine, while you have the need to edit and compile the TypeScript files in
 * this project.
 * 
 * NOTE: Optionally you can use this gulpfile with the Task Runner Explorer extension in Visual Studio
 * if you want a more integrated/automated workflow.
 */

var gulp = require("gulp"),
    ts = require("gulp-typescript");

gulp.task("default", ["scripts"]);

var project = ts.createProject({
    declarationFiles: false,
    noExternalResolve: true
});

gulp.task("scripts", function() {
    var result = gulp
        .src([
            "Scripts/*.ts",
            "Scripts/typings/*.d.ts"
        ])
        .pipe(ts(project));
    
    return result.js.pipe(gulp.dest("Scripts"));
});