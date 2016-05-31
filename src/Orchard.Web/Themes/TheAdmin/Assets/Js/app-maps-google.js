var App = (function () {
  'use strict';

  App.mapsGoogle = function( ){

    //Basic Map
    var map;

    var mapOptions = {
      zoom: 14,
      center: new google.maps.LatLng(-33.877827, 151.188598),
      mapTypeId: google.maps.MapTypeId.ROADMAP
    };
    map = new google.maps.Map(document.getElementById('map_one'), mapOptions);

    //Hybrid Map
    var map2;

    var mapOptions = {
      zoom: 14,
      center: new google.maps.LatLng(-33.877827, 151.188598),
      mapTypeId: google.maps.MapTypeId.HYBRID
    };
    map2 = new google.maps.Map(document.getElementById('map_two'), mapOptions);

     
   //Terrain Map
    var map3;

    var mapOptions = {
      zoom: 14,
      center: new google.maps.LatLng(-33.877827, 151.188598),
      mapTypeId: google.maps.MapTypeId.TERRAIN
    };
    map3 = new google.maps.Map(document.getElementById('map_three'), mapOptions);

  };

  return App;
})(App || {});
