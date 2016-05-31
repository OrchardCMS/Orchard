var App = (function () {
  'use strict';
  
  App.dashboard = function( ){

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

    //Top pie widget 1
    function widget_top_1(){
	    
	    var data = [
	    	{ label: "Premium Purchases", data: 15 },
	    	{ label: "Standard Plans", data: 25 },
	    	{ label: "Services", data: 60 }
	    ];

      var color1 = tinycolor( App.color.primary ).lighten( 5 ).toString();
      var color2 = App.color.alt2;
      var color3 = App.color.alt1;

      var legendContainer = $("#widget-top-1").parent().next().find(".legend");

	    $.plot('#widget-top-1', data, {
		    series: {
					pie: {
						show: true,
						highlight: {
							opacity: 0.1
						}
		      }
		    },
		    grid:{
		    	hoverable: true
		    },
		    legend:{
		    	container: legendContainer
		    },
        colors: [ color1, color2, color3]  
			});
    }

    //Top pie widget 2
    function widget_top_2(){
	    
	    var data = [
	    	{ label: "Direct Access", data: 20 },
	    	{ label: "Advertisment", data: 15 },
	    	{ label: "Facebook", data: 15 },
	    	{ label: "Twitter", data: 30 },
	    	{ label: "Google Plus", data: 20 }
	    ];

      var color1 = App.color.alt2;
      var color2 = App.color.alt4;
      var color3 = App.color.alt3;
      var color4 = App.color.alt1;
      var color5 = tinycolor( App.color.primary ).lighten( 5 ).toString();

	    var legendContainer = $("#widget-top-2").parent().next().find(".legend");

	    $.plot('#widget-top-2', data, {
		    series: {
	        pie: {
	          innerRadius: 0.5,
	          show: true,
	          highlight: {
							opacity: 0.1
						}
		      }
		    },
		    grid:{
		    	hoverable: true
		    },
		    legend:{
		    	container: legendContainer
		    },
		    colors: [color1, color2, color3, color4, color5]
			});
    }

    //Top pie widget 3
    function widget_top_3(){
	    
	    var data = [
	    	{ label: "Google Ads", data: 70 },
	    	{ label: "Facebook", data: 30 }
	    ];

      var color1 = App.color.alt3;
      var color2 = tinycolor( App.color.alt4 ).lighten( 6.5 ).toString();

	    $.plot('#widget-top-3', data, {
		    series: {
					pie: {
						show: true,
						label: {
							show: false
						},
						highlight: {
							opacity: 0.1
						}
		      }
		    },
		    grid:{
		    	hoverable: true
		    },
		    legend:{
		    	show: false
		    },
		    colors: [color1, color2]
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

    //Fullwidth line chart 1
    function line_chart1(){

      var chartEl = $("#line-chart1");
      var data = [
        [1, 10],
        [2, 30],
        [3, 10 + 45],
        [4, 15 + 21],
        [5, 57],
        [6, 80],
        [7, 65],
        [8, 50],
        [9, 80],
        [10, 70],
        [11, 90],
        [12, 55 + 12],
        [12, 55 + 12]
      ];

      var color1 = App.color.alt3;

 			var plot_statistics = $.plot("#line-chart1", 
 				[{
        	data: data, 
        	showLabels: true, 
        	label: "New Visitors", 
        	labelPlacement: "below", 
        	canvasRender: true, 
        	cColor: "#FFFFFF" 
      	}
      	], {
        series: {
          lines: {
            show: true,
            lineWidth: 2, 
            fill: true,
            fillColor: { colors: [{ opacity: 0.6 }, { opacity: 0.6}] }
          },
          fillColor: "rgba(0, 0, 0, 1)",
          points: {
            show: true,
            fill: true,
            fillColor: color1
          },
          shadowSize: 0
        },
        legend:{
          show: false
        },
        grid: {
          show:false,
          margin: {
          	left: -8,
          	right: -8,
          	top: 0,
          	botttom: 0
          },
          labelMargin: 0,
           axisMargin: 0,
          hoverable: true,
          clickable: true,
          tickColor: "rgba(0, 0, 0, 0)",
          borderWidth: 0
        },
        colors: [color1,"#1fb594"],
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

    //World Map 1
    function world_map(){

      var color = App.color.alt1;

      $('#world-map').vectorMap({
        map: 'world_mill_en',
        backgroundColor: 'transparent',
        regionStyle: {
          initial: {
            fill: color,
          },
          hover: {
            "fill-opacity": 0.8
          }
        },
        markerStyle:{
            initial:{
              r: 10
            },
             hover: {
              r: 12,
              stroke: 'rgba(255,255,255,0.8)',
              "stroke-width": 4
            }
          },
          markers: [
              {latLng: [41.90, 12.45], name: '1.512 Visits', style: {fill: '#F07878',stroke:'rgba(255,255,255,0.7)',"stroke-width": 3}},
              {latLng: [1.3, 103.8], name: '940 Visits', style: {fill: '#F07878',stroke:'rgba(255,255,255,0.7)',"stroke-width": 3}},
              {latLng: [51.511214, -0.119824], name: '530 Visits', style: {fill: '#F07878',stroke:'rgba(255,255,255,0.7)',"stroke-width": 3}},
              {latLng: [40.714353, -74.005973], name: '340 Visits', style: {fill: '#F07878',stroke:'rgba(255,255,255,0.7)',"stroke-width": 3}},
              {latLng: [-22.913395, -43.200710], name: '1.800 Visits', style: {fill: '#F07878',stroke:'rgba(255,255,255,0.7)',"stroke-width": 3}}
          ]
      });
    }

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

      var color = App.color.alt2;

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
        colors: [color, color],
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

      var color1 = tinycolor( App.color.primary ).lighten( 23 ).toString();
      var color2 = tinycolor( App.color.primary ).brighten( 5 ).toString();

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

    //Radar 1
    function radar_chart(){

      var color1 = tinycolor( App.color.primary ).lighten( 6 );
      var color2 = tinycolor( App.color.alt4 ).lighten( 6.5 );

      var data = {
        labels: ["January", "February", "March", "April", "May", "June", "July"],
        datasets: [
          {
            label: "My First dataset",
            fillColor: color2.setAlpha(.5).toString(),
            pointColor: color2.setAlpha(.8).toString(),
            strokeColor: color2.setAlpha(.8).toString(),
            highlightFill: color2.setAlpha(.75).toString(),
            highlightStroke: color2.toString(),
            data: [65, 59, 80, 81, 56, 55, 40]
          },
          {
            label: "My Second dataset",
            fillColor: color1.setAlpha(.5).toString(),
            pointColor: color1.setAlpha(.8).toString(),
            strokeColor: color1.setAlpha(.8).toString(),
            highlightFill: color1.setAlpha(.75).toString(),
            highlightStroke: color1.toString(),
            data: [28, 48, 40, 19, 86, 27, 90]
          }
        ]
      };

      var radarChart = new Chart( $("#radar-chart1").get(0).getContext("2d") ).Radar(data, {
        scaleShowLine : true,
        responsive: true,
        maintainAspectRatio: false,
        legendTemplate : "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>"
      });
    }

    //CounterUp Init
    counter();

    //Row 1
	    widget_top_1();
	    widget_top_2();
	    widget_top_3();

	    //Sparklines
      var spk1_color = App.color.alt2;
      var spk2_color = tinycolor( App.color.primary ).lighten( 5 ).toString();
	    $("#spk1").sparkline([2,4,3,6,7,5,8,9,4,2,10,], { type: 'bar', width: '80px', height: '30px', barColor: spk1_color});
	    $("#spk2").sparkline([5,3,5,6,5,7,4,8,6,9,8,], { type: 'bar', width: '80px', height: '30px', barColor: spk2_color});

	  //Row 2
	  	calendar_widget();
	  	line_chart1();

	  //Row 3
      line_chart2();
      bar_chart();
      world_map();
      radar_chart();

  };

  return App;
})(App || {});
