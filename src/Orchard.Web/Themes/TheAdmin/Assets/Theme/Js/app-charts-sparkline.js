var App = (function () {
	'use strict';

	App.chartsSparklines = function( ){

		//Change default charts color (Read the official docs for more info)
		//This change the default sparkline colors, but you can change them individually to each chart
		var color1Src = tinycolor( App.color.primary );
		var color2Src = tinycolor( App.color.alt1 );
		var color3Src = tinycolor( App.color.alt2 );
		var color4Src = tinycolor( App.color.alt3 );
		var color1 = color1Src.toString();
		var color2 = color2Src.toString();
		var color3 = color3Src.toString();
		var color4 = color4Src.toString();
		$.fn.sparkline.defaults.common.lineColor = color1;
		$.fn.sparkline.defaults.common.fillColor = color1Src.setAlpha(.3).toString();

		$.fn.sparkline.defaults.line.spotColor = color1;
		$.fn.sparkline.defaults.line.minSpotColor = color1;
		$.fn.sparkline.defaults.line.maxSpotColor = color1;
		$.fn.sparkline.defaults.line.highlightSpotColor = color1;
		$.fn.sparkline.defaults.line.highlightLineColor = color1;

		$.fn.sparkline.defaults.bar.barColor = color1;
		$.fn.sparkline.defaults.bar.negBarColor = color2;
		$.fn.sparkline.defaults.bar.stackedBarColor[0] = color1;
		$.fn.sparkline.defaults.bar.stackedBarColor[1] = color2;

		$.fn.sparkline.defaults.tristate.posBarColor = color1;
		$.fn.sparkline.defaults.tristate.negBarColor = color2;

		$.fn.sparkline.defaults.discrete.thresholdColor = color2;

		$.fn.sparkline.defaults.bullet.targetColor = color1;
		$.fn.sparkline.defaults.bullet.performanceColor = color2;
		$.fn.sparkline.defaults.bullet.rangeColors[0] = color2Src.setAlpha(.2).toString();
		$.fn.sparkline.defaults.bullet.rangeColors[1] = color2Src.setAlpha(.50).toString();
		$.fn.sparkline.defaults.bullet.rangeColors[2] = color2Src.setAlpha(.45).toString();

		$.fn.sparkline.defaults.pie.sliceColors[0] = color1;
		$.fn.sparkline.defaults.pie.sliceColors[1] = color2;
		$.fn.sparkline.defaults.pie.sliceColors[2] = color3;

		$.fn.sparkline.defaults.box.medianColor = color1;
		$.fn.sparkline.defaults.box.boxFillColor = color2Src.setAlpha(.5).toString();
		$.fn.sparkline.defaults.box.boxLineColor = color1;
		$.fn.sparkline.defaults.box.whiskerColor = color4;

		// Bar + line composite charts
		$(".compositebar").sparkline('html', { type: 'bar', barColor: color2 });
		$(".compositebar").sparkline([4,1,5,7,9,9,8,7,6,6,4,7,8,4,3,2,2,5,6,7], { composite: true, fillColor: false });

 		// Line charts taking their values from the tag
    $('.sparkline').sparkline();

    // Customized line chart
    $('.linecustom').sparkline('html', {height: '1.5em', width: '8em', lineColor: color3, fillColor: color3Src.setAlpha(.4).toString(), minSpotColor: false, maxSpotColor: false, spotColor: color4, spotRadius: 3});

    // Bar charts using inline values
    $('.sparkbar').sparkline('html', {type: 'bar'});

    // Tri-state charts using inline values
    $('.sparktristate').sparkline('html', {type: 'tristate'});

    // Composite line charts, the second using values supplied via javascript
    $('.compositeline').sparkline('html', { fillColor: false, changeRangeMin: 0, chartRangeMax: 10 });
    $('.compositeline').sparkline([4,1,5,7,9,9,8,7,6,6,4,7,8,4,3,2,2,5,6,7], { composite: true, fillColor: false, lineColor: color2, changeRangeMin: 0, chartRangeMax: 10 });

    // Line charts with normal range marker
    $('.normalline').sparkline('html', { fillColor: false, normalRangeMin: -1, normalRangeMax: 8 });

    // Discrete charts
    $('.discrete1').sparkline('html', { type: 'discrete', xwidth: 18 });
    $('.discrete2').sparkline('html', { type: 'discrete', thresholdValue: 4 });

    // Bullet charts
    $('.sparkbullet').sparkline('html', { type: 'bullet' });

    // Pie charts
    $('.sparkpie').sparkline('html', { type: 'pie', height: '1.0em' });

    // Box plots
    $('.sparkboxplot').sparkline('html', { type: 'box'});

    $('#line-chart').sparkline([4,3.5,5,7,9,9,8,7,6,6,5,7,8,4,3,4,4,5,6,7,5,6,7,8,8.2,7.5,7.4,7.2,7],{
    	width: '100%',
    	height: '220px',
    	lineWidth: 1.7,
    	spotRadius: 3,
    	fillColor: color1Src.setAlpha(.2).toString()
    });

    $('#pie-chart').sparkline([2,5,2.5],{
    	type: 'pie',
    	sliceColors:[color3, color2, color1],
    	height: '200px'
    });

    $('#bar-chart').sparkline([4,3.5,5,7,9,9,8,7,8,4,3,4,4,5,6,7,5,6,7,8,8.2,7.5,7.4],{
    	type: 'bar',
    	barColor: color4,
    	barWidth: 8,
    	width: '100%',
    	height: '200px'
    });

	};

	return App;
})(App || {});