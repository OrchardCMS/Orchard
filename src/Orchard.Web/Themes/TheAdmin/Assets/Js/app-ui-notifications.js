var App = (function () {
  'use strict';
  
  App.uiNotifications = function( ){  
    
    $('#not-basic').click(function(){
      $.gritter.add({
        title: 'Samantha new msg!',
        text: "You have a new Thomas message, let's checkout your inbox.",
        image: App.conf.assetsPath + '/' +  App.conf.imgPath + '/avatar.jpg',
        time: '',
        class_name: 'img-rounded'
      });
      return false;
    });
    
    $('#not-theme').click(function(){
      $.gritter.add({
        title: 'Welcome home!',
        text: 'You can start your day checking the new messages.',
        image: App.conf.assetsPath + '/' +  App.conf.imgPath + '/avatar6.jpg',
        class_name: 'clean img-rounded',
        time: '',
      });
      return false;
    });
    
    $('#not-sticky').click(function(){
      $.gritter.add({
        title: 'Sticky Note',
        text: "Your daily goal is 130 new code lines, don't forget to update your work.",
        image: App.conf.assetsPath + '/' +  App.conf.imgPath + '/slack_logo.png',
        class_name: 'clean',
        sticky: true, 
        time: ''
      });
      return false;
    });
    
    $('#not-text').click(function(){
      $.gritter.add({
        title: 'Just Text',
        text: 'This is a simple Gritter Notification. Etiam efficitur efficitur nisl eu dictum, nullam non orci elementum.',
        class_name: 'clean',
        time: ''
      });
      return false;
    });

    /*Positions*/
    $('#not-tr').click(function(){
      $.extend($.gritter.options, { position: 'top-right' });

      $.gritter.add({
        title: 'Top Right',
        text: 'This is a simple Gritter Notification. Etiam efficitur efficitur nisl eu dictum, nullam non orci elementum',
        class_name: 'clean'
      });

      return false;
    });
    
    $('#not-tl').click(function(){
      $.extend($.gritter.options, { position: 'top-left' });

      $.gritter.add({
        title: 'Top Left',
        text: 'This is a simple Gritter Notification. Etiam efficitur efficitur nisl eu dictum, nullam non orci elementum',
        class_name: 'clean'
      });

      return false;
    });
    
    $('#not-bl').click(function(){

      $.extend($.gritter.options, { position: 'bottom-left' });

      $.gritter.add({
        title: 'Bottom Left',
        text: 'This is a simple Gritter Notification. Etiam efficitur efficitur nisl eu dictum, nullam non orci elementum',
        class_name: 'clean'
      });

      return false;
    });
    
    $('#not-br').click(function(){

      $.extend($.gritter.options, { position: 'bottom-right' });
      
      $.gritter.add({
        title: 'Bottom Right',
        text: 'This is a simple Gritter Notification. Etiam efficitur efficitur nisl eu dictum, nullam non orci elementum',
        class_name: 'clean'
      });

      return false;
    });

    /*Social Gritters*/
    $('#not-facebook').click(function(){
      $.gritter.add({
        title: 'You have comments!',
        text: 'You can start your day checking the new messages.',
        image: App.conf.assetsPath + '/' +  App.conf.imgPath + '/fb-icon.png',
        class_name: 'color facebook'
      });
      return false;
    });
    
    $('#not-twitter').click(function(){
      $.gritter.add({
        title: 'You have new followers!',
        text: 'You can start your day checking the new messages.',
        image: App.conf.assetsPath + '/' +  App.conf.imgPath + '/tw-icon.png',
        class_name: 'color twitter'
      });
      return false;
    });
    
    $('#not-google-plus').click(function(){
      $.gritter.add({
        title: 'You have new +1!',
        text: 'You can start your day checking the new messages.',
        image: App.conf.assetsPath + '/' +  App.conf.imgPath + '/gp-icon.png',
        class_name: 'color google-plus'
      });
      return false;
    });
    
    $('#not-dribbble').click(function(){
      $.gritter.add({
        title: 'You have new comments!',
        text: 'You can start your day checking the new comments.',
        image: App.conf.assetsPath + '/' +  App.conf.imgPath + '/db-icon.png',
        class_name: 'color dribbble'
      });
      return false;
    });
    
    $('#not-flickr').click(function(){
      $.gritter.add({
        title: 'You have new comments!',
        text: 'You can start your day checking the new comments.',
        image: App.conf.assetsPath + '/' +  App.conf.imgPath + '/fl-icon.png',
        class_name: 'color flickr'
      });
      return false;
    });
    
    $('#not-linkedin').click(function(){
      $.gritter.add({
        title: 'You have new comments!',
        text: 'You can start your day checking the new comments.',
        image: App.conf.assetsPath + '/' +  App.conf.imgPath + '/in-icon.png',
        class_name: 'color linkedin'
      });
      return false;
    });
    
    $('#not-youtube').click(function(){
      $.gritter.add({
        title: 'You have new comments!',
        text: 'You can start your day checking the new comments.',
        image: App.conf.assetsPath + '/' +  App.conf.imgPath + '/yt-icon.png',
        class_name: 'color youtube'
      });
      return false;
    });
    
    $('#not-pinterest').click(function(){
      $.gritter.add({
        title: 'You have new comments!',
        text: 'You can start your day checking the new comments.',
        image: App.conf.assetsPath + '/' +  App.conf.imgPath + '/pi-icon.png',
        class_name: 'color pinterest'
      });
      return false;
    });  
    
    $('#not-github').click(function(){
      $.gritter.add({
        title: 'You have new forks!',
        text: 'You can start your day checking the new comments.',
        image: App.conf.assetsPath + '/' +  App.conf.imgPath + '/gh-icon.png',
        class_name: 'color github'
      });
      return false;
    });    
    
    $('#not-tumblr').click(function(){
      $.gritter.add({
        title: 'You have new comments!',
        text: 'You can start your day checking the new comments.',
        image: App.conf.assetsPath + '/' +  App.conf.imgPath + '/tu-icon.png',
        class_name: 'color tumblr'
      });
      return false;
    });

    /*Colors*/
    $('#not-primary').click(function(){
      $.gritter.add({
        title: 'Primary',
        text: 'This is a simple Gritter Notification.',
        class_name: 'color primary'
      });
    });
    
    $('#not-success').click(function(){
      $.gritter.add({
        title: 'Success',
        text: 'This is a simple Gritter Notification.',
        class_name: 'color success'
      });
    });
    
    $('#not-info').click(function(){
      $.gritter.add({
        title: 'Info',
        text: 'This is a simple Gritter Notification.',
        class_name: 'color info'
      });
    });
    
    $('#not-warning').click(function(){
      $.gritter.add({
        title: 'Warning',
        text: 'This is a simple Gritter Notification.',
        class_name: 'color warning'
      });
    });
    
    $('#not-danger').click(function(){
      $.gritter.add({
        title: 'Danger',
        text: 'This is a simple Gritter Notification.',
        class_name: 'color danger'
      });
    });

    /*Alt Colors*/
    $('#not-ac1').click(function(){
      $.gritter.add({
        title: 'Alt Color 1',
        text: 'This is a simple Gritter Notification.',
        class_name: 'color alt1'
      });
    });
    
    $('#not-ac2').click(function(){
      $.gritter.add({
        title: 'Alt Color 2',
        text: 'This is a simple Gritter Notification.',
        class_name: 'color alt2'
      });
    });
    
    $('#not-ac3').click(function(){
      $.gritter.add({
        title: 'Alt Color 3',
        text: 'This is a simple Gritter Notification.',
        class_name: 'color alt3'
      });
    });
    
    $('#not-ac4').click(function(){
      $.gritter.add({
        title: 'Alt Color 4',
        text: 'This is a simple Gritter Notification.',
        class_name: 'color alt4'
      });
    });
    
    $('#not-dark').click(function(){
      $.gritter.add({
        title: 'Dark Color',
        text: 'This is a simple Gritter Notification.',
        class_name: 'color dark'
      });
    });

  };

  return App;
})(App || {});
