var App = (function () {
  'use strict';
  
  App.emailInbox = function( ){
    
    $(".am-select-all input").on('change',function(){
      var checkboxes = $(".email-list").find('input[type="checkbox"]');
      if( $(this).is(':checked') ) {
          checkboxes.prop('checked', true);
      } else {
          checkboxes.prop('checked', false);
      }
    });
    
  };

  return App;
})(App || {});
