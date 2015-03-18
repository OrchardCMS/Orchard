/// <vs AfterBuild='scripts' />

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
 * NOTE: If you install the Task Runner Explorer extension in Visual Studio the "typescript" task in this
 * gulpfile will execute automatically on build for a more integrated/automated workflow. That's the 
 * purpose of the <vs> comment element at the top.
 */

var gulp = require("gulp"),
    ts = require("gulp-typescript");

gulp.task("default", ["typescript"]);

var project = ts.createProject({
    declarationFiles: false,
    noExternalResolve: true
});

gulp.task("typescript", function () {
    var result = gulp
        .src([
            "Scripts/*.ts",
            "Scripts/typings/*.d.ts"
        ])
        .pipe(ts(project));
    
    return result.js.pipe(gulp.dest("Scripts"));
});