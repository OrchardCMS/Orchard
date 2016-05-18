var App = (function () {
  'use strict';

  App.pageProfile = function( ){

    //Mini widget 1
    function line_chart2(){

      var data = [
        [1, 20],
        [2, 60],
        [3, 35],
        [4, 70],
        [5, 45]
      ];

      var data2 = [
        [1, 60],
        [2, 20],
        [3, 65],
        [4, 35],
        [5, 65]
      ];

      var plot_statistics = $.plot("#linechart-mini1", 
        [
        {
          data: data, 
          canvasRender: true
        },
        {
          data: data2, 
          canvasRender: true
        }
        ], {
        series: {
          lines: {
            show: true,
            lineWidth: 0, 
            fill: true,
            fillColor: { colors: [{ opacity: 0.7 }, { opacity: 0.7}] }
          },
          fillColor: "rgba(0, 0, 0, 1)",
          shadowSize: 0,
          curvedLines: {
            apply: true,
            active: true,
            monotonicFit: true
          }
        },
        legend:{
          show: false
        },
        grid: {
          show:false,
          hoverable: true,
          clickable: true
        },
        colors: ["#FFDC7A","#FFDC7A"],
        xaxis: {
          autoscaleMargin: 0,
          ticks: 11,
          tickDecimals: 0
        },
        yaxis: {
          autoscaleMargin: 0.5,
          ticks: 5,
          tickDecimals: 0
        }
      });
    }

    //Mini widget 2
    function bar_chart(){

      var plot_statistics = $.plot($("#barchart-mini1"), [
        {
          data: [
            [0, 15], [1, 15], [2, 19], [3, 28], [4, 30], [5, 37], [6, 35], [7, 38], [8, 48], [9, 43], [10, 38], [11, 32], [12, 38]
          ],
          label: "Page Views"
        },
        {
          data: [
            [0, 7], [1, 10], [2, 15], [3, 23], [4, 24], [5, 29], [6, 25], [7, 33], [8, 35], [9, 38], [10, 32], [11, 27], [12, 32]
          ],
          label: "Unique Visitor"
        }
      ], {
        series: {
          bars: {
            align: 'center',
            show: true,
            lineWidth: 1, 
            barWidth: 0.8, 
            fill: true,
            fillColor: {
              colors: [{
                opacity: 1
              }, {
                opacity: 1
              }
              ]
            } 
          },
          shadowSize: 0
        },
        legend:{
          show: false
        },
        grid: {
          margin: 0,
          show: false,
          labelMargin: 10,
          axisMargin: 500,
          hoverable: true,
          clickable: true,
          tickColor: "rgba(0,0,0,0.15)",
          borderWidth: 0
        },
        colors: ["#ADC0D8", "#88A3C6"],
        xaxis: {
          ticks: 11,
          tickDecimals: 0
        },
        yaxis: {
          autoscaleMargin: 0.5,
          ticks: 4,
          tickDecimals: 0
        }
      });
    }

    //Calendar Widget
    function calendar_widget(){
      var widget = $(".widget-calendar");
      var calNotes = $(".cal-notes", widget);
      var calNotesDay = $(".day", calNotes);
      var calNotesDate = $(".date", calNotes);

      //Calculate the weekday name
      var d = new Date();
      var weekday = new Array(7);
      weekday[0]=  "Sunday";
      weekday[1] = "Monday";
      weekday[2] = "Tuesday";
      weekday[3] = "Wednesday";
      weekday[4] = "Thursday";
      weekday[5] = "Friday";
      weekday[6] = "Saturday";

      var weekdayName = weekday[d.getDay()];

      calNotesDay.html( weekdayName );

      //Calculate the month name
      var month = new Array();
      month[0] = "January";
      month[1] = "February";
      month[2] = "March";
      month[3] = "April";
      month[4] = "May";
      month[5] = "June";
      month[6] = "July";
      month[7] = "August";
      month[8] = "September";
      month[9] = "October";
      month[10] = "November";
      month[11] = "December";

      var monthName = month[d.getMonth()];
      var monthDay = d.getDate();

      calNotesDate.html( monthName + " " + monthDay);

      if (typeof jQuery.ui != 'undefined') {
        $( ".ui-datepicker" ).datepicker({
          onSelect: function(s, o){
            var sd = new Date(s);
            var weekdayName = weekday[sd.getDay()];
            var monthName = month[sd.getMonth()];
            var monthDay = sd.getDate();

            calNotesDay.html( weekdayName );
            calNotesDate.html( monthName + " " + monthDay);
          }
        });
      }
    }

    line_chart2();
    bar_chart();
    calendar_widget();
    
  };

  return App;
})(App || {});
