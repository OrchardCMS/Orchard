/**
 * @preserve console-shim 1.0.2
 * https://github.com/kayahr/console-shim
 * Copyright (C) 2011 Klaus Reimer <k@ailis.de>
 * Licensed under the MIT license
 * (See http://www.opensource.org/licenses/mit-license)
 */
 
 
(function(){
"use strict";

/**
 * Returns a function which calls the specified function in the specified
 * scope.
 *
 * @param {Function} func
 *            The function to call.
 * @param {Object} scope
 *            The scope to call the function in.
 * @param {...*} args
 *            Additional arguments to pass to the bound function.
 * @returns {function(...[*]): undefined}
 *            The bound function.
 */
var bind = function(func, scope, args)
{
    var fixedArgs = Array.prototype.slice.call(arguments, 2);
    return function()
    {
        var args = fixedArgs.concat(Array.prototype.slice.call(arguments, 0));
        (/** @type {Function} */ func).apply(scope, args);
    };
};

// Create console if not present
if (!window["console"]) window.console = /** @type {Console} */ ({});
var console = (/** @type {Object} */ window.console);

// Implement console log if needed
if (!console["log"])
{
    // Use log4javascript if present
    if (window["log4javascript"])
    {
        var log = log4javascript.getDefaultLogger();
        console.log = bind(log.info, log);
        console.debug = bind(log.debug, log);
        console.info = bind(log.info, log);
        console.warn = bind(log.warn, log);
        console.error = bind(log.error, log);
    }
    
    // Use empty dummy implementation to ignore logging
    else
    {
        console.log = (/** @param {...*} args */ function(args) {});
    }
}

// Implement other log levels to console.log if missing
if (!console["debug"]) console.debug = console.log;
if (!console["info"]) console.info = console.log;
if (!console["warn"]) console.warn = console.log;
if (!console["error"]) console.error = console.log;

// Wrap the log methods in IE (<=9) because their argument handling is wrong
// This wrapping is also done if the __consoleShimTest__ symbol is set. This
// is needed for unit testing.
if (window["__consoleShimTest__"] != null || 
    eval("/*@cc_on @_jscript_version <= 9@*/"))
{
    /**
     * Wraps the call to a real IE logging method. Modifies the arguments so
     * parameters which are not represented by a placeholder are properly
     * printed with a space character as separator.
     *
     * @param {...*} args
     *            The function arguments. First argument is the log function
     *            to call, the other arguments are the log arguments.
     */
    var wrap = function(args)
    {
        var i, max, match, log;
        
        // Convert argument list to real array
        args = Array.prototype.slice.call(arguments, 0);
        
        // First argument is the log method to call
        log = args.shift();
        
        max = args.length;
        if (max > 1 && window["__consoleShimTest__"] !== false)
        {
            // When first parameter is not a string then add a format string to
            // the argument list so we are able to modify it in the next stop
            if (typeof(args[0]) != "string")
            {
                args.unshift("%o");
                max += 1;
            }
            
            // For each additional parameter which has no placeholder in the
            // format string we add another placeholder separated with a
            // space character.
            match = args[0].match(/%[a-z]/g);
            for (i = match ? match.length + 1 : 1; i < max; i += 1)
            {
                args[0] += " %o";
            }
        }
        Function.apply.call(log, console, args);
    };
    
    // Wrap the native log methods of IE to fix argument output problems
    console.log = bind(wrap, window, console.log);
    console.debug = bind(wrap, window, console.debug);
    console.info = bind(wrap, window, console.info);
    console.warn = bind(wrap, window, console.warn);
    console.error = bind(wrap, window, console.error);
}

// Implement console.assert if missing
if (!console["assert"])
{
    console["assert"] = function()
    {
        var args = Array.prototype.slice.call(arguments, 0);
        var expr = args.shift();
        if (!expr)
        {
            args[0] = "Assertion failed: " + args[0];
            console.error.apply(console, args);
        }
    };
}

// Linking console.dir and console.dirxml to the console.log method if
// missing. Hopefully the browser already logs objects and DOM nodes as a
// tree.
if (!console["dir"]) console["dir"] = console.log;
if (!console["dirxml"]) console["dirxml"] = console.log;

// Linking console.exception to console.error. This is not the same but
// at least some error message is displayed.
if (!console["exception"]) console["exception"] = console.error;

// Implement console.time and console.timeEnd if one of them is missing
if (!console["time"] || !console["timeEnd"])
{
    var timers = {};
    console["time"] = function(id)
    {
        timers[id] = new Date().getTime();
    };
    console["timeEnd"] = function(id)
    {
        var start = timers[id];
        if (start)
        {
            console.log(id + ": " + (new Date().getTime() - start) + "ms");
            delete timers[id];
        }
    };
}

// Implement console.table if missing
if (!console["table"])
{
    console["table"] = function(data, columns)
    {
        var i, iMax, row, j, jMax, k;
        
        // Do nothing if data has wrong type or no data was specified
        if (!data || !(data instanceof Array) || !data.length) return;
        
        // Auto-calculate columns array if not set
        if (!columns || !(columns instanceof Array))
        {
            columns = [];
            for (k in data[0])
            {
                if (!data[0].hasOwnProperty(k)) continue;
                columns.push(k);
            }
        }
        
        for (i = 0, iMax = data.length; i < iMax; i += 1)
        {
            row = [];
            for (j = 0, jMax = columns.length; j < jMax; j += 1)
            {
                row.push(data[i][columns[j]]);
            }
            
            Function.apply.call(console.log, console, row);
        }
    };
}

// Dummy implementations of other console features to prevent error messages
// in browsers not supporting it.
if (!console["clear"]) console["clear"] = function() {};
if (!console["trace"]) console["trace"] = function() {};
if (!console["group"]) console["group"] = function() {};
if (!console["groupCollapsed"]) console["groupCollapsed"] = function() {};
if (!console["groupEnd"]) console["groupEnd"] = function() {};
if (!console["timeStamp"]) console["timeStamp"] = function() {};
if (!console["profile"]) console["profile"] = function() {};
if (!console["profileEnd"]) console["profileEnd"] = function() {};
if (!console["count"]) console["count"] = function() {};

})();

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiIiwic291cmNlcyI6WyJjb25zb2xlLXNoaW0uanMiXSwic291cmNlc0NvbnRlbnQiOlsiLyoqXHJcbiAqIEBwcmVzZXJ2ZSBjb25zb2xlLXNoaW0gMS4wLjJcclxuICogaHR0cHM6Ly9naXRodWIuY29tL2theWFoci9jb25zb2xlLXNoaW1cclxuICogQ29weXJpZ2h0IChDKSAyMDExIEtsYXVzIFJlaW1lciA8a0BhaWxpcy5kZT5cclxuICogTGljZW5zZWQgdW5kZXIgdGhlIE1JVCBsaWNlbnNlXHJcbiAqIChTZWUgaHR0cDovL3d3dy5vcGVuc291cmNlLm9yZy9saWNlbnNlcy9taXQtbGljZW5zZSlcclxuICovXHJcbiBcclxuIFxyXG4oZnVuY3Rpb24oKXtcclxuXCJ1c2Ugc3RyaWN0XCI7XHJcblxyXG4vKipcclxuICogUmV0dXJucyBhIGZ1bmN0aW9uIHdoaWNoIGNhbGxzIHRoZSBzcGVjaWZpZWQgZnVuY3Rpb24gaW4gdGhlIHNwZWNpZmllZFxyXG4gKiBzY29wZS5cclxuICpcclxuICogQHBhcmFtIHtGdW5jdGlvbn0gZnVuY1xyXG4gKiAgICAgICAgICAgIFRoZSBmdW5jdGlvbiB0byBjYWxsLlxyXG4gKiBAcGFyYW0ge09iamVjdH0gc2NvcGVcclxuICogICAgICAgICAgICBUaGUgc2NvcGUgdG8gY2FsbCB0aGUgZnVuY3Rpb24gaW4uXHJcbiAqIEBwYXJhbSB7Li4uKn0gYXJnc1xyXG4gKiAgICAgICAgICAgIEFkZGl0aW9uYWwgYXJndW1lbnRzIHRvIHBhc3MgdG8gdGhlIGJvdW5kIGZ1bmN0aW9uLlxyXG4gKiBAcmV0dXJucyB7ZnVuY3Rpb24oLi4uWypdKTogdW5kZWZpbmVkfVxyXG4gKiAgICAgICAgICAgIFRoZSBib3VuZCBmdW5jdGlvbi5cclxuICovXHJcbnZhciBiaW5kID0gZnVuY3Rpb24oZnVuYywgc2NvcGUsIGFyZ3MpXHJcbntcclxuICAgIHZhciBmaXhlZEFyZ3MgPSBBcnJheS5wcm90b3R5cGUuc2xpY2UuY2FsbChhcmd1bWVudHMsIDIpO1xyXG4gICAgcmV0dXJuIGZ1bmN0aW9uKClcclxuICAgIHtcclxuICAgICAgICB2YXIgYXJncyA9IGZpeGVkQXJncy5jb25jYXQoQXJyYXkucHJvdG90eXBlLnNsaWNlLmNhbGwoYXJndW1lbnRzLCAwKSk7XHJcbiAgICAgICAgKC8qKiBAdHlwZSB7RnVuY3Rpb259ICovIGZ1bmMpLmFwcGx5KHNjb3BlLCBhcmdzKTtcclxuICAgIH07XHJcbn07XHJcblxyXG4vLyBDcmVhdGUgY29uc29sZSBpZiBub3QgcHJlc2VudFxyXG5pZiAoIXdpbmRvd1tcImNvbnNvbGVcIl0pIHdpbmRvdy5jb25zb2xlID0gLyoqIEB0eXBlIHtDb25zb2xlfSAqLyAoe30pO1xyXG52YXIgY29uc29sZSA9ICgvKiogQHR5cGUge09iamVjdH0gKi8gd2luZG93LmNvbnNvbGUpO1xyXG5cclxuLy8gSW1wbGVtZW50IGNvbnNvbGUgbG9nIGlmIG5lZWRlZFxyXG5pZiAoIWNvbnNvbGVbXCJsb2dcIl0pXHJcbntcclxuICAgIC8vIFVzZSBsb2c0amF2YXNjcmlwdCBpZiBwcmVzZW50XHJcbiAgICBpZiAod2luZG93W1wibG9nNGphdmFzY3JpcHRcIl0pXHJcbiAgICB7XHJcbiAgICAgICAgdmFyIGxvZyA9IGxvZzRqYXZhc2NyaXB0LmdldERlZmF1bHRMb2dnZXIoKTtcclxuICAgICAgICBjb25zb2xlLmxvZyA9IGJpbmQobG9nLmluZm8sIGxvZyk7XHJcbiAgICAgICAgY29uc29sZS5kZWJ1ZyA9IGJpbmQobG9nLmRlYnVnLCBsb2cpO1xyXG4gICAgICAgIGNvbnNvbGUuaW5mbyA9IGJpbmQobG9nLmluZm8sIGxvZyk7XHJcbiAgICAgICAgY29uc29sZS53YXJuID0gYmluZChsb2cud2FybiwgbG9nKTtcclxuICAgICAgICBjb25zb2xlLmVycm9yID0gYmluZChsb2cuZXJyb3IsIGxvZyk7XHJcbiAgICB9XHJcbiAgICBcclxuICAgIC8vIFVzZSBlbXB0eSBkdW1teSBpbXBsZW1lbnRhdGlvbiB0byBpZ25vcmUgbG9nZ2luZ1xyXG4gICAgZWxzZVxyXG4gICAge1xyXG4gICAgICAgIGNvbnNvbGUubG9nID0gKC8qKiBAcGFyYW0gey4uLip9IGFyZ3MgKi8gZnVuY3Rpb24oYXJncykge30pO1xyXG4gICAgfVxyXG59XHJcblxyXG4vLyBJbXBsZW1lbnQgb3RoZXIgbG9nIGxldmVscyB0byBjb25zb2xlLmxvZyBpZiBtaXNzaW5nXHJcbmlmICghY29uc29sZVtcImRlYnVnXCJdKSBjb25zb2xlLmRlYnVnID0gY29uc29sZS5sb2c7XHJcbmlmICghY29uc29sZVtcImluZm9cIl0pIGNvbnNvbGUuaW5mbyA9IGNvbnNvbGUubG9nO1xyXG5pZiAoIWNvbnNvbGVbXCJ3YXJuXCJdKSBjb25zb2xlLndhcm4gPSBjb25zb2xlLmxvZztcclxuaWYgKCFjb25zb2xlW1wiZXJyb3JcIl0pIGNvbnNvbGUuZXJyb3IgPSBjb25zb2xlLmxvZztcclxuXHJcbi8vIFdyYXAgdGhlIGxvZyBtZXRob2RzIGluIElFICg8PTkpIGJlY2F1c2UgdGhlaXIgYXJndW1lbnQgaGFuZGxpbmcgaXMgd3JvbmdcclxuLy8gVGhpcyB3cmFwcGluZyBpcyBhbHNvIGRvbmUgaWYgdGhlIF9fY29uc29sZVNoaW1UZXN0X18gc3ltYm9sIGlzIHNldC4gVGhpc1xyXG4vLyBpcyBuZWVkZWQgZm9yIHVuaXQgdGVzdGluZy5cclxuaWYgKHdpbmRvd1tcIl9fY29uc29sZVNoaW1UZXN0X19cIl0gIT0gbnVsbCB8fCBcclxuICAgIGV2YWwoXCIvKkBjY19vbiBAX2pzY3JpcHRfdmVyc2lvbiA8PSA5QCovXCIpKVxyXG57XHJcbiAgICAvKipcclxuICAgICAqIFdyYXBzIHRoZSBjYWxsIHRvIGEgcmVhbCBJRSBsb2dnaW5nIG1ldGhvZC4gTW9kaWZpZXMgdGhlIGFyZ3VtZW50cyBzb1xyXG4gICAgICogcGFyYW1ldGVycyB3aGljaCBhcmUgbm90IHJlcHJlc2VudGVkIGJ5IGEgcGxhY2Vob2xkZXIgYXJlIHByb3Blcmx5XHJcbiAgICAgKiBwcmludGVkIHdpdGggYSBzcGFjZSBjaGFyYWN0ZXIgYXMgc2VwYXJhdG9yLlxyXG4gICAgICpcclxuICAgICAqIEBwYXJhbSB7Li4uKn0gYXJnc1xyXG4gICAgICogICAgICAgICAgICBUaGUgZnVuY3Rpb24gYXJndW1lbnRzLiBGaXJzdCBhcmd1bWVudCBpcyB0aGUgbG9nIGZ1bmN0aW9uXHJcbiAgICAgKiAgICAgICAgICAgIHRvIGNhbGwsIHRoZSBvdGhlciBhcmd1bWVudHMgYXJlIHRoZSBsb2cgYXJndW1lbnRzLlxyXG4gICAgICovXHJcbiAgICB2YXIgd3JhcCA9IGZ1bmN0aW9uKGFyZ3MpXHJcbiAgICB7XHJcbiAgICAgICAgdmFyIGksIG1heCwgbWF0Y2gsIGxvZztcclxuICAgICAgICBcclxuICAgICAgICAvLyBDb252ZXJ0IGFyZ3VtZW50IGxpc3QgdG8gcmVhbCBhcnJheVxyXG4gICAgICAgIGFyZ3MgPSBBcnJheS5wcm90b3R5cGUuc2xpY2UuY2FsbChhcmd1bWVudHMsIDApO1xyXG4gICAgICAgIFxyXG4gICAgICAgIC8vIEZpcnN0IGFyZ3VtZW50IGlzIHRoZSBsb2cgbWV0aG9kIHRvIGNhbGxcclxuICAgICAgICBsb2cgPSBhcmdzLnNoaWZ0KCk7XHJcbiAgICAgICAgXHJcbiAgICAgICAgbWF4ID0gYXJncy5sZW5ndGg7XHJcbiAgICAgICAgaWYgKG1heCA+IDEgJiYgd2luZG93W1wiX19jb25zb2xlU2hpbVRlc3RfX1wiXSAhPT0gZmFsc2UpXHJcbiAgICAgICAge1xyXG4gICAgICAgICAgICAvLyBXaGVuIGZpcnN0IHBhcmFtZXRlciBpcyBub3QgYSBzdHJpbmcgdGhlbiBhZGQgYSBmb3JtYXQgc3RyaW5nIHRvXHJcbiAgICAgICAgICAgIC8vIHRoZSBhcmd1bWVudCBsaXN0IHNvIHdlIGFyZSBhYmxlIHRvIG1vZGlmeSBpdCBpbiB0aGUgbmV4dCBzdG9wXHJcbiAgICAgICAgICAgIGlmICh0eXBlb2YoYXJnc1swXSkgIT0gXCJzdHJpbmdcIilcclxuICAgICAgICAgICAge1xyXG4gICAgICAgICAgICAgICAgYXJncy51bnNoaWZ0KFwiJW9cIik7XHJcbiAgICAgICAgICAgICAgICBtYXggKz0gMTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBcclxuICAgICAgICAgICAgLy8gRm9yIGVhY2ggYWRkaXRpb25hbCBwYXJhbWV0ZXIgd2hpY2ggaGFzIG5vIHBsYWNlaG9sZGVyIGluIHRoZVxyXG4gICAgICAgICAgICAvLyBmb3JtYXQgc3RyaW5nIHdlIGFkZCBhbm90aGVyIHBsYWNlaG9sZGVyIHNlcGFyYXRlZCB3aXRoIGFcclxuICAgICAgICAgICAgLy8gc3BhY2UgY2hhcmFjdGVyLlxyXG4gICAgICAgICAgICBtYXRjaCA9IGFyZ3NbMF0ubWF0Y2goLyVbYS16XS9nKTtcclxuICAgICAgICAgICAgZm9yIChpID0gbWF0Y2ggPyBtYXRjaC5sZW5ndGggKyAxIDogMTsgaSA8IG1heDsgaSArPSAxKVxyXG4gICAgICAgICAgICB7XHJcbiAgICAgICAgICAgICAgICBhcmdzWzBdICs9IFwiICVvXCI7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICAgICAgRnVuY3Rpb24uYXBwbHkuY2FsbChsb2csIGNvbnNvbGUsIGFyZ3MpO1xyXG4gICAgfTtcclxuICAgIFxyXG4gICAgLy8gV3JhcCB0aGUgbmF0aXZlIGxvZyBtZXRob2RzIG9mIElFIHRvIGZpeCBhcmd1bWVudCBvdXRwdXQgcHJvYmxlbXNcclxuICAgIGNvbnNvbGUubG9nID0gYmluZCh3cmFwLCB3aW5kb3csIGNvbnNvbGUubG9nKTtcclxuICAgIGNvbnNvbGUuZGVidWcgPSBiaW5kKHdyYXAsIHdpbmRvdywgY29uc29sZS5kZWJ1Zyk7XHJcbiAgICBjb25zb2xlLmluZm8gPSBiaW5kKHdyYXAsIHdpbmRvdywgY29uc29sZS5pbmZvKTtcclxuICAgIGNvbnNvbGUud2FybiA9IGJpbmQod3JhcCwgd2luZG93LCBjb25zb2xlLndhcm4pO1xyXG4gICAgY29uc29sZS5lcnJvciA9IGJpbmQod3JhcCwgd2luZG93LCBjb25zb2xlLmVycm9yKTtcclxufVxyXG5cclxuLy8gSW1wbGVtZW50IGNvbnNvbGUuYXNzZXJ0IGlmIG1pc3NpbmdcclxuaWYgKCFjb25zb2xlW1wiYXNzZXJ0XCJdKVxyXG57XHJcbiAgICBjb25zb2xlW1wiYXNzZXJ0XCJdID0gZnVuY3Rpb24oKVxyXG4gICAge1xyXG4gICAgICAgIHZhciBhcmdzID0gQXJyYXkucHJvdG90eXBlLnNsaWNlLmNhbGwoYXJndW1lbnRzLCAwKTtcclxuICAgICAgICB2YXIgZXhwciA9IGFyZ3Muc2hpZnQoKTtcclxuICAgICAgICBpZiAoIWV4cHIpXHJcbiAgICAgICAge1xyXG4gICAgICAgICAgICBhcmdzWzBdID0gXCJBc3NlcnRpb24gZmFpbGVkOiBcIiArIGFyZ3NbMF07XHJcbiAgICAgICAgICAgIGNvbnNvbGUuZXJyb3IuYXBwbHkoY29uc29sZSwgYXJncyk7XHJcbiAgICAgICAgfVxyXG4gICAgfTtcclxufVxyXG5cclxuLy8gTGlua2luZyBjb25zb2xlLmRpciBhbmQgY29uc29sZS5kaXJ4bWwgdG8gdGhlIGNvbnNvbGUubG9nIG1ldGhvZCBpZlxyXG4vLyBtaXNzaW5nLiBIb3BlZnVsbHkgdGhlIGJyb3dzZXIgYWxyZWFkeSBsb2dzIG9iamVjdHMgYW5kIERPTSBub2RlcyBhcyBhXHJcbi8vIHRyZWUuXHJcbmlmICghY29uc29sZVtcImRpclwiXSkgY29uc29sZVtcImRpclwiXSA9IGNvbnNvbGUubG9nO1xyXG5pZiAoIWNvbnNvbGVbXCJkaXJ4bWxcIl0pIGNvbnNvbGVbXCJkaXJ4bWxcIl0gPSBjb25zb2xlLmxvZztcclxuXHJcbi8vIExpbmtpbmcgY29uc29sZS5leGNlcHRpb24gdG8gY29uc29sZS5lcnJvci4gVGhpcyBpcyBub3QgdGhlIHNhbWUgYnV0XHJcbi8vIGF0IGxlYXN0IHNvbWUgZXJyb3IgbWVzc2FnZSBpcyBkaXNwbGF5ZWQuXHJcbmlmICghY29uc29sZVtcImV4Y2VwdGlvblwiXSkgY29uc29sZVtcImV4Y2VwdGlvblwiXSA9IGNvbnNvbGUuZXJyb3I7XHJcblxyXG4vLyBJbXBsZW1lbnQgY29uc29sZS50aW1lIGFuZCBjb25zb2xlLnRpbWVFbmQgaWYgb25lIG9mIHRoZW0gaXMgbWlzc2luZ1xyXG5pZiAoIWNvbnNvbGVbXCJ0aW1lXCJdIHx8ICFjb25zb2xlW1widGltZUVuZFwiXSlcclxue1xyXG4gICAgdmFyIHRpbWVycyA9IHt9O1xyXG4gICAgY29uc29sZVtcInRpbWVcIl0gPSBmdW5jdGlvbihpZClcclxuICAgIHtcclxuICAgICAgICB0aW1lcnNbaWRdID0gbmV3IERhdGUoKS5nZXRUaW1lKCk7XHJcbiAgICB9O1xyXG4gICAgY29uc29sZVtcInRpbWVFbmRcIl0gPSBmdW5jdGlvbihpZClcclxuICAgIHtcclxuICAgICAgICB2YXIgc3RhcnQgPSB0aW1lcnNbaWRdO1xyXG4gICAgICAgIGlmIChzdGFydClcclxuICAgICAgICB7XHJcbiAgICAgICAgICAgIGNvbnNvbGUubG9nKGlkICsgXCI6IFwiICsgKG5ldyBEYXRlKCkuZ2V0VGltZSgpIC0gc3RhcnQpICsgXCJtc1wiKTtcclxuICAgICAgICAgICAgZGVsZXRlIHRpbWVyc1tpZF07XHJcbiAgICAgICAgfVxyXG4gICAgfTtcclxufVxyXG5cclxuLy8gSW1wbGVtZW50IGNvbnNvbGUudGFibGUgaWYgbWlzc2luZ1xyXG5pZiAoIWNvbnNvbGVbXCJ0YWJsZVwiXSlcclxue1xyXG4gICAgY29uc29sZVtcInRhYmxlXCJdID0gZnVuY3Rpb24oZGF0YSwgY29sdW1ucylcclxuICAgIHtcclxuICAgICAgICB2YXIgaSwgaU1heCwgcm93LCBqLCBqTWF4LCBrO1xyXG4gICAgICAgIFxyXG4gICAgICAgIC8vIERvIG5vdGhpbmcgaWYgZGF0YSBoYXMgd3JvbmcgdHlwZSBvciBubyBkYXRhIHdhcyBzcGVjaWZpZWRcclxuICAgICAgICBpZiAoIWRhdGEgfHwgIShkYXRhIGluc3RhbmNlb2YgQXJyYXkpIHx8ICFkYXRhLmxlbmd0aCkgcmV0dXJuO1xyXG4gICAgICAgIFxyXG4gICAgICAgIC8vIEF1dG8tY2FsY3VsYXRlIGNvbHVtbnMgYXJyYXkgaWYgbm90IHNldFxyXG4gICAgICAgIGlmICghY29sdW1ucyB8fCAhKGNvbHVtbnMgaW5zdGFuY2VvZiBBcnJheSkpXHJcbiAgICAgICAge1xyXG4gICAgICAgICAgICBjb2x1bW5zID0gW107XHJcbiAgICAgICAgICAgIGZvciAoayBpbiBkYXRhWzBdKVxyXG4gICAgICAgICAgICB7XHJcbiAgICAgICAgICAgICAgICBpZiAoIWRhdGFbMF0uaGFzT3duUHJvcGVydHkoaykpIGNvbnRpbnVlO1xyXG4gICAgICAgICAgICAgICAgY29sdW1ucy5wdXNoKGspO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG4gICAgICAgIFxyXG4gICAgICAgIGZvciAoaSA9IDAsIGlNYXggPSBkYXRhLmxlbmd0aDsgaSA8IGlNYXg7IGkgKz0gMSlcclxuICAgICAgICB7XHJcbiAgICAgICAgICAgIHJvdyA9IFtdO1xyXG4gICAgICAgICAgICBmb3IgKGogPSAwLCBqTWF4ID0gY29sdW1ucy5sZW5ndGg7IGogPCBqTWF4OyBqICs9IDEpXHJcbiAgICAgICAgICAgIHtcclxuICAgICAgICAgICAgICAgIHJvdy5wdXNoKGRhdGFbaV1bY29sdW1uc1tqXV0pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIFxyXG4gICAgICAgICAgICBGdW5jdGlvbi5hcHBseS5jYWxsKGNvbnNvbGUubG9nLCBjb25zb2xlLCByb3cpO1xyXG4gICAgICAgIH1cclxuICAgIH07XHJcbn1cclxuXHJcbi8vIER1bW15IGltcGxlbWVudGF0aW9ucyBvZiBvdGhlciBjb25zb2xlIGZlYXR1cmVzIHRvIHByZXZlbnQgZXJyb3IgbWVzc2FnZXNcclxuLy8gaW4gYnJvd3NlcnMgbm90IHN1cHBvcnRpbmcgaXQuXHJcbmlmICghY29uc29sZVtcImNsZWFyXCJdKSBjb25zb2xlW1wiY2xlYXJcIl0gPSBmdW5jdGlvbigpIHt9O1xyXG5pZiAoIWNvbnNvbGVbXCJ0cmFjZVwiXSkgY29uc29sZVtcInRyYWNlXCJdID0gZnVuY3Rpb24oKSB7fTtcclxuaWYgKCFjb25zb2xlW1wiZ3JvdXBcIl0pIGNvbnNvbGVbXCJncm91cFwiXSA9IGZ1bmN0aW9uKCkge307XHJcbmlmICghY29uc29sZVtcImdyb3VwQ29sbGFwc2VkXCJdKSBjb25zb2xlW1wiZ3JvdXBDb2xsYXBzZWRcIl0gPSBmdW5jdGlvbigpIHt9O1xyXG5pZiAoIWNvbnNvbGVbXCJncm91cEVuZFwiXSkgY29uc29sZVtcImdyb3VwRW5kXCJdID0gZnVuY3Rpb24oKSB7fTtcclxuaWYgKCFjb25zb2xlW1widGltZVN0YW1wXCJdKSBjb25zb2xlW1widGltZVN0YW1wXCJdID0gZnVuY3Rpb24oKSB7fTtcclxuaWYgKCFjb25zb2xlW1wicHJvZmlsZVwiXSkgY29uc29sZVtcInByb2ZpbGVcIl0gPSBmdW5jdGlvbigpIHt9O1xyXG5pZiAoIWNvbnNvbGVbXCJwcm9maWxlRW5kXCJdKSBjb25zb2xlW1wicHJvZmlsZUVuZFwiXSA9IGZ1bmN0aW9uKCkge307XHJcbmlmICghY29uc29sZVtcImNvdW50XCJdKSBjb25zb2xlW1wiY291bnRcIl0gPSBmdW5jdGlvbigpIHt9O1xyXG5cclxufSkoKTtcclxuIl0sImZpbGUiOiJjb25zb2xlLXNoaW0uanMiLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==