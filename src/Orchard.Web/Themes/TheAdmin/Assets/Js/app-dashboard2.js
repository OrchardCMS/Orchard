var App = (function () {
  'use strict';
  
  App.dashboard2 = function( ){

    function randValue() {
      return (Math.floor(Math.random() * (1 + 50 - 20))) + 10;
    }

    //Counter
    function counter(){

      $('[data-toggle="counter"]').each(function(i, e){
        var _el       = $(this);
        var prefix    = '';
        var suffix    = '';
        var start     = 0;
        var end       = 0;
        var decimals  = 0;
        var duration  = 2.5;

        if( _el.data('prefix') ){ prefix = _el.data('prefix'); }

        if( _el.data('suffix') ){ suffix = _el.data('suffix'); }

        if( _el.data('start') ){ start = _el.data('start'); }

        if( _el.data('end') ){ end = _el.data('end'); }

        if( _el.data('decimals') ){ decimals = _el.data('decimals'); }

        if( _el.data('duration') ){ duration = _el.data('duration'); }

        var count = new CountUp(_el.get(0), start, end, decimals, duration, { 
          suffix: suffix,
          prefix: prefix,
        });

        count.start();
      });
    }

    //Line Chart Live
    function widget_linechart_live(){
      var data_com2 = [
        [1, randValue()],
        [2, randValue()],
        [3, 2 + randValue()],
        [4, 3 + randValue()],
        [5, 5 + randValue()],
        [6, 10 + randValue()],
        [7, 15 + randValue()],
        [8, 20 + randValue()],
        [9, 25 + randValue()],
        [10, 30 + randValue()],
        [11, 35 + randValue()],
        [12, 25 + randValue()],
        [13, 15 + randValue()],
        [14, 20 + randValue()],
        [15, 45 + randValue()],
        [16, 50 + randValue()],
        [17, 65 + randValue()],
        [18, 70 + randValue()],
        [19, 85 + randValue()],
        [20, 80 + randValue()],
        [21, 75 + randValue()],
        [22, 80 + randValue()],
        [23, 75 + randValue()]
      ];
      var data_com = [
        [1, randValue()],
        [2, randValue()],
        [3, 10 + randValue()],
        [4, 15 + randValue()],
        [5, 20 + randValue()],
        [6, 25 + randValue()],
        [7, 30 + randValue()],
        [8, 35 + randValue()],
        [9, 40 + randValue()],
        [10, 45 + randValue()],
        [11, 50 + randValue()],
        [12, 55 + randValue()],
        [13, 60 + randValue()],
        [14, 70 + randValue()],
        [15, 75 + randValue()],
        [16, 80 + randValue()],
        [17, 85 + randValue()],
        [18, 90 + randValue()],
        [19, 95 + randValue()],
        [20, 100 + randValue()],
        [21, 110 + randValue()],
        [22, 120 + randValue()],
        [23, 130 + randValue()]
      ];

      var color1 = tinycolor( App.color.primary ).lighten( 22 );
      var color2 = App.color.primary;

      var plot_statistics = $.plot($("#line-chart-live"), [{
        data: data_com, showLabels: true, label: "New Visitors", labelPlacement: "below", canvasRender: true, cColor: "#FFFFFF" 
      },{
        data: data_com2, showLabels: true, label: "Old Visitors", labelPlacement: "below", canvasRender: true, cColor: "#FFFFFF" 
      }
      ], {
        series: {
          lines: {
            show: true,
            lineWidth: 1.5, 
            fill: true,
            fillColor: { colors: [{ opacity: 0.5 }, { opacity: 0.5}] },
          },
          fillColor: "rgba(0, 0, 0, 1)",
          points: {
            show: false,
            fill: true
          },
          shadowSize: 0
        },
        legend:{
          show: false
        },
        grid: {
          show: true,
          margin: {
            top: -20,
            bottom: 0,
            left: 0,
            right: 0,
          },
          labelMargin: 0,
          axisMargin: 0,
          hoverable: true,
          clickable: true,
          tickColor: "rgba(0,0,0,0)",
          borderWidth: 0,
          minBorderMargin: 0
        },
        colors: [color1,color2],
        xaxis: {
          autoscaleMargin: 0,
          ticks: 11,
          tickDecimals: 0
        },
        yaxis: {
          autoscaleMargin: 0.2,
          ticks: 5,
          tickDecimals: 0
        }
      });
    }

    //Line Chart 1
    function widget_linechart1(){
      var color1 = tinycolor( App.color.primary ).lighten( 5 ).toString();

      var plot_statistics = $.plot($("#line-chart1"), [{
        data: [
          [0, 20], [1, 30], [2, 25], [3, 39], [4, 35], [5, 40], [6, 30], [7, 45]
        ],
        label: "Page Views"
      }
      ], {
        series: {
          lines: {
            show: true,
            lineWidth: 2, 
            fill: true,
            fillColor: {
              colors: [{
                opacity: 0.1
              }, {
                opacity: 0.1
              }
              ]
            } 
          },
          points: {
            show: true
          },
          shadowSize: 0
        },
        legend:{
          show: false
        },
        grid: {
          margin: {
            left: -8,
            right: -8,
            top: 0,
            bottom: 0
          },
          show: false,
          labelMargin: 15,
          axisMargin: 500,
          hoverable: true,
          clickable: true,
          tickColor: "rgba(0,0,0,0.15)",
          borderWidth: 0
        },
        colors: [color1, "#95D9F0", "#FFDC7A"],
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

    //Chart Pie 4
    function widget_chartpie4(){
      var data = [
        { label: "Google", data: 45},
        { label: "Dribbble", data: 25},
        { label: "Twitter", data: 20},
        { label: "Facebook", data: 10}
      ];

      var color1 = tinycolor( App.color.primary ).brighten( 9 ).toString();
      var color2 = tinycolor( App.color.primary ).lighten( 13 ).toString();
      var color3 = tinycolor( App.color.primary ).lighten( 20 ).toString();
      var color4 = tinycolor( App.color.primary ).lighten( 27 ).toString();

      $.plot('#pie-chart4', data, {
        series: {
          pie: {
            show: true,
            innerRadius: 0.27,
            shadow:{
              top: 5,
              left: 15,
              alpha:0.3
            },
            stroke:{
              width:0
            },
            label: {
                show: true,
                formatter: function (label, series) {
                    return '<div style="font-size:12px;text-align:center;padding:2px;color:#333;">' + label + '</div>';
                }
            },
            highlight:{
              opacity: 0.08
            }
          }
        },
        grid: {
          hoverable: true,
          clickable: true
        },
        colors: [color1, color2, color3, color4],
        legend: {
          show: false
        }
      });
    }

    //Bar Chart 1
    function widget_barchart1(){

      var color1 = tinycolor( App.color.primary ).lighten( 23 ).toString();
      var color2 = tinycolor( App.color.primary ).brighten( 5 ).toString();

      var plot_statistics = $.plot($("#bar-chart1"), [
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
            barWidth: 0.6, 
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
          shadowSize: 2
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
        colors: [color1, color2],
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

    //Skycons
    function widget_weather(){
      var widgetEl = $(".widget-weather");
      var skycons = new Skycons({"color": "#555555"});
      skycons.add($(".icon1", widgetEl)[0], Skycons.CLEAR_DAY);
      skycons.add($(".icon2", widgetEl)[0], Skycons.PARTLY_CLOUDY_DAY);
      skycons.add($(".icon3", widgetEl)[0], Skycons.RAIN);
      skycons.play();
    }

    //Init countUp
    counter();

    //Row 1
      widget_linechart_live();

    //Row 2
      widget_linechart1();
      widget_chartpie4();
      widget_barchart1();

    //Row 3
      calendar_widget();
      widget_weather();
  };

  return App;
})(App || {});
