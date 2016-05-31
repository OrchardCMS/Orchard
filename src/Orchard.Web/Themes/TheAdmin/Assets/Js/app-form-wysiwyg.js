var App = (function () {
	'use strict';

  App.textEditors = function( ){

    //Summernote
    $('#editor1').summernote({
      height: 300
    });
    
  };

  return App;
})(App || {});
