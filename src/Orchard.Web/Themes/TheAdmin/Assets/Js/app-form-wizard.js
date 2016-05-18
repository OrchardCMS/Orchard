var App = (function () {
  'use strict';
  
  App.wizard = function( ){

    //Fuel UX
    $('.wizard-ux').wizard();

    $('.wizard-ux').on('changed.fu.wizard',function(){
      //Bootstrap Slider
      $('.bslider').slider();
    });
    
    $(".wizard-next").click(function(e){
      var id = $(this).data("wizard");
      $(id).wizard('next');
      e.preventDefault();
    });
    
    $(".wizard-previous").click(function(e){
      var id = $(this).data("wizard");
      $(id).wizard('previous');
      e.preventDefault();
    });

   //Select2
    $(".select2").select2({
      width: '100%'
    });
    
    //Select2 tags
    $(".tags").select2({tags: true, width: '100%'});

    $("#credit_slider").slider().on("slide",function(e){
      $("#credits").html("$" + e.value);
    });

    $("#rate_slider").slider().on("slide",function(e){
      $("#rate").html(e.value + "%");
    });
    
  };

  return App;
})(App || {});
