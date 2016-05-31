var App = (function () {
  'use strict';

  App.emailCompose = function( ){

    //Select2 Tags
    $(".tags").select2({tags: 0,width: '100%'});

   //Summernote
    $('#email-editor').summernote({
      height: 200
    });
    
  };

  return App;
})(App || {});
