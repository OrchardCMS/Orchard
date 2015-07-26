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

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiIiwic291cmNlcyI6WyJjb25zb2xlLXNoaW0uanMiXSwic291cmNlc0NvbnRlbnQiOlsiLyoqXG4gKiBAcHJlc2VydmUgY29uc29sZS1zaGltIDEuMC4yXG4gKiBodHRwczovL2dpdGh1Yi5jb20va2F5YWhyL2NvbnNvbGUtc2hpbVxuICogQ29weXJpZ2h0IChDKSAyMDExIEtsYXVzIFJlaW1lciA8a0BhaWxpcy5kZT5cbiAqIExpY2Vuc2VkIHVuZGVyIHRoZSBNSVQgbGljZW5zZVxuICogKFNlZSBodHRwOi8vd3d3Lm9wZW5zb3VyY2Uub3JnL2xpY2Vuc2VzL21pdC1saWNlbnNlKVxuICovXG4gXG4gXG4oZnVuY3Rpb24oKXtcblwidXNlIHN0cmljdFwiO1xuXG4vKipcbiAqIFJldHVybnMgYSBmdW5jdGlvbiB3aGljaCBjYWxscyB0aGUgc3BlY2lmaWVkIGZ1bmN0aW9uIGluIHRoZSBzcGVjaWZpZWRcbiAqIHNjb3BlLlxuICpcbiAqIEBwYXJhbSB7RnVuY3Rpb259IGZ1bmNcbiAqICAgICAgICAgICAgVGhlIGZ1bmN0aW9uIHRvIGNhbGwuXG4gKiBAcGFyYW0ge09iamVjdH0gc2NvcGVcbiAqICAgICAgICAgICAgVGhlIHNjb3BlIHRvIGNhbGwgdGhlIGZ1bmN0aW9uIGluLlxuICogQHBhcmFtIHsuLi4qfSBhcmdzXG4gKiAgICAgICAgICAgIEFkZGl0aW9uYWwgYXJndW1lbnRzIHRvIHBhc3MgdG8gdGhlIGJvdW5kIGZ1bmN0aW9uLlxuICogQHJldHVybnMge2Z1bmN0aW9uKC4uLlsqXSk6IHVuZGVmaW5lZH1cbiAqICAgICAgICAgICAgVGhlIGJvdW5kIGZ1bmN0aW9uLlxuICovXG52YXIgYmluZCA9IGZ1bmN0aW9uKGZ1bmMsIHNjb3BlLCBhcmdzKVxue1xuICAgIHZhciBmaXhlZEFyZ3MgPSBBcnJheS5wcm90b3R5cGUuc2xpY2UuY2FsbChhcmd1bWVudHMsIDIpO1xuICAgIHJldHVybiBmdW5jdGlvbigpXG4gICAge1xuICAgICAgICB2YXIgYXJncyA9IGZpeGVkQXJncy5jb25jYXQoQXJyYXkucHJvdG90eXBlLnNsaWNlLmNhbGwoYXJndW1lbnRzLCAwKSk7XG4gICAgICAgICgvKiogQHR5cGUge0Z1bmN0aW9ufSAqLyBmdW5jKS5hcHBseShzY29wZSwgYXJncyk7XG4gICAgfTtcbn07XG5cbi8vIENyZWF0ZSBjb25zb2xlIGlmIG5vdCBwcmVzZW50XG5pZiAoIXdpbmRvd1tcImNvbnNvbGVcIl0pIHdpbmRvdy5jb25zb2xlID0gLyoqIEB0eXBlIHtDb25zb2xlfSAqLyAoe30pO1xudmFyIGNvbnNvbGUgPSAoLyoqIEB0eXBlIHtPYmplY3R9ICovIHdpbmRvdy5jb25zb2xlKTtcblxuLy8gSW1wbGVtZW50IGNvbnNvbGUgbG9nIGlmIG5lZWRlZFxuaWYgKCFjb25zb2xlW1wibG9nXCJdKVxue1xuICAgIC8vIFVzZSBsb2c0amF2YXNjcmlwdCBpZiBwcmVzZW50XG4gICAgaWYgKHdpbmRvd1tcImxvZzRqYXZhc2NyaXB0XCJdKVxuICAgIHtcbiAgICAgICAgdmFyIGxvZyA9IGxvZzRqYXZhc2NyaXB0LmdldERlZmF1bHRMb2dnZXIoKTtcbiAgICAgICAgY29uc29sZS5sb2cgPSBiaW5kKGxvZy5pbmZvLCBsb2cpO1xuICAgICAgICBjb25zb2xlLmRlYnVnID0gYmluZChsb2cuZGVidWcsIGxvZyk7XG4gICAgICAgIGNvbnNvbGUuaW5mbyA9IGJpbmQobG9nLmluZm8sIGxvZyk7XG4gICAgICAgIGNvbnNvbGUud2FybiA9IGJpbmQobG9nLndhcm4sIGxvZyk7XG4gICAgICAgIGNvbnNvbGUuZXJyb3IgPSBiaW5kKGxvZy5lcnJvciwgbG9nKTtcbiAgICB9XG4gICAgXG4gICAgLy8gVXNlIGVtcHR5IGR1bW15IGltcGxlbWVudGF0aW9uIHRvIGlnbm9yZSBsb2dnaW5nXG4gICAgZWxzZVxuICAgIHtcbiAgICAgICAgY29uc29sZS5sb2cgPSAoLyoqIEBwYXJhbSB7Li4uKn0gYXJncyAqLyBmdW5jdGlvbihhcmdzKSB7fSk7XG4gICAgfVxufVxuXG4vLyBJbXBsZW1lbnQgb3RoZXIgbG9nIGxldmVscyB0byBjb25zb2xlLmxvZyBpZiBtaXNzaW5nXG5pZiAoIWNvbnNvbGVbXCJkZWJ1Z1wiXSkgY29uc29sZS5kZWJ1ZyA9IGNvbnNvbGUubG9nO1xuaWYgKCFjb25zb2xlW1wiaW5mb1wiXSkgY29uc29sZS5pbmZvID0gY29uc29sZS5sb2c7XG5pZiAoIWNvbnNvbGVbXCJ3YXJuXCJdKSBjb25zb2xlLndhcm4gPSBjb25zb2xlLmxvZztcbmlmICghY29uc29sZVtcImVycm9yXCJdKSBjb25zb2xlLmVycm9yID0gY29uc29sZS5sb2c7XG5cbi8vIFdyYXAgdGhlIGxvZyBtZXRob2RzIGluIElFICg8PTkpIGJlY2F1c2UgdGhlaXIgYXJndW1lbnQgaGFuZGxpbmcgaXMgd3Jvbmdcbi8vIFRoaXMgd3JhcHBpbmcgaXMgYWxzbyBkb25lIGlmIHRoZSBfX2NvbnNvbGVTaGltVGVzdF9fIHN5bWJvbCBpcyBzZXQuIFRoaXNcbi8vIGlzIG5lZWRlZCBmb3IgdW5pdCB0ZXN0aW5nLlxuaWYgKHdpbmRvd1tcIl9fY29uc29sZVNoaW1UZXN0X19cIl0gIT0gbnVsbCB8fCBcbiAgICBldmFsKFwiLypAY2Nfb24gQF9qc2NyaXB0X3ZlcnNpb24gPD0gOUAqL1wiKSlcbntcbiAgICAvKipcbiAgICAgKiBXcmFwcyB0aGUgY2FsbCB0byBhIHJlYWwgSUUgbG9nZ2luZyBtZXRob2QuIE1vZGlmaWVzIHRoZSBhcmd1bWVudHMgc29cbiAgICAgKiBwYXJhbWV0ZXJzIHdoaWNoIGFyZSBub3QgcmVwcmVzZW50ZWQgYnkgYSBwbGFjZWhvbGRlciBhcmUgcHJvcGVybHlcbiAgICAgKiBwcmludGVkIHdpdGggYSBzcGFjZSBjaGFyYWN0ZXIgYXMgc2VwYXJhdG9yLlxuICAgICAqXG4gICAgICogQHBhcmFtIHsuLi4qfSBhcmdzXG4gICAgICogICAgICAgICAgICBUaGUgZnVuY3Rpb24gYXJndW1lbnRzLiBGaXJzdCBhcmd1bWVudCBpcyB0aGUgbG9nIGZ1bmN0aW9uXG4gICAgICogICAgICAgICAgICB0byBjYWxsLCB0aGUgb3RoZXIgYXJndW1lbnRzIGFyZSB0aGUgbG9nIGFyZ3VtZW50cy5cbiAgICAgKi9cbiAgICB2YXIgd3JhcCA9IGZ1bmN0aW9uKGFyZ3MpXG4gICAge1xuICAgICAgICB2YXIgaSwgbWF4LCBtYXRjaCwgbG9nO1xuICAgICAgICBcbiAgICAgICAgLy8gQ29udmVydCBhcmd1bWVudCBsaXN0IHRvIHJlYWwgYXJyYXlcbiAgICAgICAgYXJncyA9IEFycmF5LnByb3RvdHlwZS5zbGljZS5jYWxsKGFyZ3VtZW50cywgMCk7XG4gICAgICAgIFxuICAgICAgICAvLyBGaXJzdCBhcmd1bWVudCBpcyB0aGUgbG9nIG1ldGhvZCB0byBjYWxsXG4gICAgICAgIGxvZyA9IGFyZ3Muc2hpZnQoKTtcbiAgICAgICAgXG4gICAgICAgIG1heCA9IGFyZ3MubGVuZ3RoO1xuICAgICAgICBpZiAobWF4ID4gMSAmJiB3aW5kb3dbXCJfX2NvbnNvbGVTaGltVGVzdF9fXCJdICE9PSBmYWxzZSlcbiAgICAgICAge1xuICAgICAgICAgICAgLy8gV2hlbiBmaXJzdCBwYXJhbWV0ZXIgaXMgbm90IGEgc3RyaW5nIHRoZW4gYWRkIGEgZm9ybWF0IHN0cmluZyB0b1xuICAgICAgICAgICAgLy8gdGhlIGFyZ3VtZW50IGxpc3Qgc28gd2UgYXJlIGFibGUgdG8gbW9kaWZ5IGl0IGluIHRoZSBuZXh0IHN0b3BcbiAgICAgICAgICAgIGlmICh0eXBlb2YoYXJnc1swXSkgIT0gXCJzdHJpbmdcIilcbiAgICAgICAgICAgIHtcbiAgICAgICAgICAgICAgICBhcmdzLnVuc2hpZnQoXCIlb1wiKTtcbiAgICAgICAgICAgICAgICBtYXggKz0gMTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgICAgIFxuICAgICAgICAgICAgLy8gRm9yIGVhY2ggYWRkaXRpb25hbCBwYXJhbWV0ZXIgd2hpY2ggaGFzIG5vIHBsYWNlaG9sZGVyIGluIHRoZVxuICAgICAgICAgICAgLy8gZm9ybWF0IHN0cmluZyB3ZSBhZGQgYW5vdGhlciBwbGFjZWhvbGRlciBzZXBhcmF0ZWQgd2l0aCBhXG4gICAgICAgICAgICAvLyBzcGFjZSBjaGFyYWN0ZXIuXG4gICAgICAgICAgICBtYXRjaCA9IGFyZ3NbMF0ubWF0Y2goLyVbYS16XS9nKTtcbiAgICAgICAgICAgIGZvciAoaSA9IG1hdGNoID8gbWF0Y2gubGVuZ3RoICsgMSA6IDE7IGkgPCBtYXg7IGkgKz0gMSlcbiAgICAgICAgICAgIHtcbiAgICAgICAgICAgICAgICBhcmdzWzBdICs9IFwiICVvXCI7XG4gICAgICAgICAgICB9XG4gICAgICAgIH1cbiAgICAgICAgRnVuY3Rpb24uYXBwbHkuY2FsbChsb2csIGNvbnNvbGUsIGFyZ3MpO1xuICAgIH07XG4gICAgXG4gICAgLy8gV3JhcCB0aGUgbmF0aXZlIGxvZyBtZXRob2RzIG9mIElFIHRvIGZpeCBhcmd1bWVudCBvdXRwdXQgcHJvYmxlbXNcbiAgICBjb25zb2xlLmxvZyA9IGJpbmQod3JhcCwgd2luZG93LCBjb25zb2xlLmxvZyk7XG4gICAgY29uc29sZS5kZWJ1ZyA9IGJpbmQod3JhcCwgd2luZG93LCBjb25zb2xlLmRlYnVnKTtcbiAgICBjb25zb2xlLmluZm8gPSBiaW5kKHdyYXAsIHdpbmRvdywgY29uc29sZS5pbmZvKTtcbiAgICBjb25zb2xlLndhcm4gPSBiaW5kKHdyYXAsIHdpbmRvdywgY29uc29sZS53YXJuKTtcbiAgICBjb25zb2xlLmVycm9yID0gYmluZCh3cmFwLCB3aW5kb3csIGNvbnNvbGUuZXJyb3IpO1xufVxuXG4vLyBJbXBsZW1lbnQgY29uc29sZS5hc3NlcnQgaWYgbWlzc2luZ1xuaWYgKCFjb25zb2xlW1wiYXNzZXJ0XCJdKVxue1xuICAgIGNvbnNvbGVbXCJhc3NlcnRcIl0gPSBmdW5jdGlvbigpXG4gICAge1xuICAgICAgICB2YXIgYXJncyA9IEFycmF5LnByb3RvdHlwZS5zbGljZS5jYWxsKGFyZ3VtZW50cywgMCk7XG4gICAgICAgIHZhciBleHByID0gYXJncy5zaGlmdCgpO1xuICAgICAgICBpZiAoIWV4cHIpXG4gICAgICAgIHtcbiAgICAgICAgICAgIGFyZ3NbMF0gPSBcIkFzc2VydGlvbiBmYWlsZWQ6IFwiICsgYXJnc1swXTtcbiAgICAgICAgICAgIGNvbnNvbGUuZXJyb3IuYXBwbHkoY29uc29sZSwgYXJncyk7XG4gICAgICAgIH1cbiAgICB9O1xufVxuXG4vLyBMaW5raW5nIGNvbnNvbGUuZGlyIGFuZCBjb25zb2xlLmRpcnhtbCB0byB0aGUgY29uc29sZS5sb2cgbWV0aG9kIGlmXG4vLyBtaXNzaW5nLiBIb3BlZnVsbHkgdGhlIGJyb3dzZXIgYWxyZWFkeSBsb2dzIG9iamVjdHMgYW5kIERPTSBub2RlcyBhcyBhXG4vLyB0cmVlLlxuaWYgKCFjb25zb2xlW1wiZGlyXCJdKSBjb25zb2xlW1wiZGlyXCJdID0gY29uc29sZS5sb2c7XG5pZiAoIWNvbnNvbGVbXCJkaXJ4bWxcIl0pIGNvbnNvbGVbXCJkaXJ4bWxcIl0gPSBjb25zb2xlLmxvZztcblxuLy8gTGlua2luZyBjb25zb2xlLmV4Y2VwdGlvbiB0byBjb25zb2xlLmVycm9yLiBUaGlzIGlzIG5vdCB0aGUgc2FtZSBidXRcbi8vIGF0IGxlYXN0IHNvbWUgZXJyb3IgbWVzc2FnZSBpcyBkaXNwbGF5ZWQuXG5pZiAoIWNvbnNvbGVbXCJleGNlcHRpb25cIl0pIGNvbnNvbGVbXCJleGNlcHRpb25cIl0gPSBjb25zb2xlLmVycm9yO1xuXG4vLyBJbXBsZW1lbnQgY29uc29sZS50aW1lIGFuZCBjb25zb2xlLnRpbWVFbmQgaWYgb25lIG9mIHRoZW0gaXMgbWlzc2luZ1xuaWYgKCFjb25zb2xlW1widGltZVwiXSB8fCAhY29uc29sZVtcInRpbWVFbmRcIl0pXG57XG4gICAgdmFyIHRpbWVycyA9IHt9O1xuICAgIGNvbnNvbGVbXCJ0aW1lXCJdID0gZnVuY3Rpb24oaWQpXG4gICAge1xuICAgICAgICB0aW1lcnNbaWRdID0gbmV3IERhdGUoKS5nZXRUaW1lKCk7XG4gICAgfTtcbiAgICBjb25zb2xlW1widGltZUVuZFwiXSA9IGZ1bmN0aW9uKGlkKVxuICAgIHtcbiAgICAgICAgdmFyIHN0YXJ0ID0gdGltZXJzW2lkXTtcbiAgICAgICAgaWYgKHN0YXJ0KVxuICAgICAgICB7XG4gICAgICAgICAgICBjb25zb2xlLmxvZyhpZCArIFwiOiBcIiArIChuZXcgRGF0ZSgpLmdldFRpbWUoKSAtIHN0YXJ0KSArIFwibXNcIik7XG4gICAgICAgICAgICBkZWxldGUgdGltZXJzW2lkXTtcbiAgICAgICAgfVxuICAgIH07XG59XG5cbi8vIEltcGxlbWVudCBjb25zb2xlLnRhYmxlIGlmIG1pc3NpbmdcbmlmICghY29uc29sZVtcInRhYmxlXCJdKVxue1xuICAgIGNvbnNvbGVbXCJ0YWJsZVwiXSA9IGZ1bmN0aW9uKGRhdGEsIGNvbHVtbnMpXG4gICAge1xuICAgICAgICB2YXIgaSwgaU1heCwgcm93LCBqLCBqTWF4LCBrO1xuICAgICAgICBcbiAgICAgICAgLy8gRG8gbm90aGluZyBpZiBkYXRhIGhhcyB3cm9uZyB0eXBlIG9yIG5vIGRhdGEgd2FzIHNwZWNpZmllZFxuICAgICAgICBpZiAoIWRhdGEgfHwgIShkYXRhIGluc3RhbmNlb2YgQXJyYXkpIHx8ICFkYXRhLmxlbmd0aCkgcmV0dXJuO1xuICAgICAgICBcbiAgICAgICAgLy8gQXV0by1jYWxjdWxhdGUgY29sdW1ucyBhcnJheSBpZiBub3Qgc2V0XG4gICAgICAgIGlmICghY29sdW1ucyB8fCAhKGNvbHVtbnMgaW5zdGFuY2VvZiBBcnJheSkpXG4gICAgICAgIHtcbiAgICAgICAgICAgIGNvbHVtbnMgPSBbXTtcbiAgICAgICAgICAgIGZvciAoayBpbiBkYXRhWzBdKVxuICAgICAgICAgICAge1xuICAgICAgICAgICAgICAgIGlmICghZGF0YVswXS5oYXNPd25Qcm9wZXJ0eShrKSkgY29udGludWU7XG4gICAgICAgICAgICAgICAgY29sdW1ucy5wdXNoKGspO1xuICAgICAgICAgICAgfVxuICAgICAgICB9XG4gICAgICAgIFxuICAgICAgICBmb3IgKGkgPSAwLCBpTWF4ID0gZGF0YS5sZW5ndGg7IGkgPCBpTWF4OyBpICs9IDEpXG4gICAgICAgIHtcbiAgICAgICAgICAgIHJvdyA9IFtdO1xuICAgICAgICAgICAgZm9yIChqID0gMCwgak1heCA9IGNvbHVtbnMubGVuZ3RoOyBqIDwgak1heDsgaiArPSAxKVxuICAgICAgICAgICAge1xuICAgICAgICAgICAgICAgIHJvdy5wdXNoKGRhdGFbaV1bY29sdW1uc1tqXV0pO1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgXG4gICAgICAgICAgICBGdW5jdGlvbi5hcHBseS5jYWxsKGNvbnNvbGUubG9nLCBjb25zb2xlLCByb3cpO1xuICAgICAgICB9XG4gICAgfTtcbn1cblxuLy8gRHVtbXkgaW1wbGVtZW50YXRpb25zIG9mIG90aGVyIGNvbnNvbGUgZmVhdHVyZXMgdG8gcHJldmVudCBlcnJvciBtZXNzYWdlc1xuLy8gaW4gYnJvd3NlcnMgbm90IHN1cHBvcnRpbmcgaXQuXG5pZiAoIWNvbnNvbGVbXCJjbGVhclwiXSkgY29uc29sZVtcImNsZWFyXCJdID0gZnVuY3Rpb24oKSB7fTtcbmlmICghY29uc29sZVtcInRyYWNlXCJdKSBjb25zb2xlW1widHJhY2VcIl0gPSBmdW5jdGlvbigpIHt9O1xuaWYgKCFjb25zb2xlW1wiZ3JvdXBcIl0pIGNvbnNvbGVbXCJncm91cFwiXSA9IGZ1bmN0aW9uKCkge307XG5pZiAoIWNvbnNvbGVbXCJncm91cENvbGxhcHNlZFwiXSkgY29uc29sZVtcImdyb3VwQ29sbGFwc2VkXCJdID0gZnVuY3Rpb24oKSB7fTtcbmlmICghY29uc29sZVtcImdyb3VwRW5kXCJdKSBjb25zb2xlW1wiZ3JvdXBFbmRcIl0gPSBmdW5jdGlvbigpIHt9O1xuaWYgKCFjb25zb2xlW1widGltZVN0YW1wXCJdKSBjb25zb2xlW1widGltZVN0YW1wXCJdID0gZnVuY3Rpb24oKSB7fTtcbmlmICghY29uc29sZVtcInByb2ZpbGVcIl0pIGNvbnNvbGVbXCJwcm9maWxlXCJdID0gZnVuY3Rpb24oKSB7fTtcbmlmICghY29uc29sZVtcInByb2ZpbGVFbmRcIl0pIGNvbnNvbGVbXCJwcm9maWxlRW5kXCJdID0gZnVuY3Rpb24oKSB7fTtcbmlmICghY29uc29sZVtcImNvdW50XCJdKSBjb25zb2xlW1wiY291bnRcIl0gPSBmdW5jdGlvbigpIHt9O1xuXG59KSgpO1xuIl0sImZpbGUiOiJjb25zb2xlLXNoaW0uanMiLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==