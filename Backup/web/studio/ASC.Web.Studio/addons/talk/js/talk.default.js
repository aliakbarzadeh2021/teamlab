﻿
/* (function () {
  var
    consoleId = 'console-' + Math.floor(Math.random() * 1000000),
    _log = [];

  if (typeof window.console == 'undefined') {
    window.console = {};
  }
  if (typeof window.console.log === 'undefined') {
    window.console.log = function (message) {
      return undefined;
      var console = document.getElementById(consoleId);
      if (!console) {
        console = document.createElement('textarea');
        document.body.appendChild(console);
        console.id = consoleId;
        console.setAttribute('readonly', 'readonly');
        console.style.position = 'absolute';
        console.style.zIndex = '6666';
        console.style.left = '0';
        console.style.bottom = '0';
        console.style.width = '50%';
        console.style.height = '200px';
        console.style.overflowY = 'scroll';
        console.style.whiteSpace = 'pre';
        console.style.background = '#FFF';
        console.style.border = '0';
      }
      console.value += message + '\n';
    };
  }

  window.console._log = function (msg) {
    _log.push(msg);
  };
  window.console._printLog = function () {
    for (var i = 0, n = _log.length; i < n; i++) {
      window.console.log(_log[i]);
    }
    _log = [];
  };
})(); */

window.TMTalk = (function ($) {
  var
    isInit = false,
    $body = null,
    onblurTimeoutHandler = 0,
    maxlengthMessage = 50,
    setDefaultIcon = false,
    originalTitle = '',
    indicatorTitle = '***',
    postfixTitle = '',
    maxIndicatorLength = 3,
    fileUploader = null,
    fileUploaderPath = '',
    properties = {
      focused : false
    },
    customEvents = {
      pageBlur : 'onpageblur',
      pageFocus : 'onpagefocus'
    },
    eventManager = new CustomEvent(customEvents);

  var init = function (fileuploaderpath) {
    if (isInit === true) {
      return undefined;
    }
    isInit = true;
    // TODO
    properties.focused = true;
    originalTitle = document.title;
    if (!window.name) {
      try {window.name = ASC.Controls.JabberClient.winName} catch (err) {}
    }

    postfixTitle = ASC.TMTalk.Resources.labelNewMessage || '';

    var
      fld = '',
      engine = {},
      browser = {},
      version = -1,
      $body = $(document.body),
      ua = navigator.userAgent;
    version = (ua.match(/.+(?:rv|it|ra|ie|ox|me|on|id|os)[\/:\s]([\d._]+)/i)||[0,'0'])[1].replace('_', '');
    version = isFinite(parseFloat(version)) ? parseFloat(version) : version;
    engine = {
      'engn-ios'    : /iphone|ipad/i.test(ua),
      'engn-gecko'  : /gecko/i.test(ua),
      'engn-webkit' : /webkit/i.test(ua)
    };
    browser = {
      'brwr-msie'     : '\v' == 'v' || /msie/i.test(ua),
      'brwr-opera'    : window.opera ? true : false,
      'brwr-chrome'   : window.chrome ? true : false,
      'brwr-safari'   : /safari/i.test(ua) && !window.chrome,
      'brwr-firefox'  : /firefox/i.test(ua)
    };
    for (fld in engine) {
      if (engine.hasOwnProperty(fld)) {
        if (engine[fld]) {
          $body.addClass(fld);
        }
      }
    }
    for (fld in browser) {
      if (browser.hasOwnProperty(fld)) {
        if (browser[fld]) {
          $body.addClass(fld);
          $body.addClass(fld + '-' + version);
        }
      }
    }

    if (typeof fileuploaderpath === 'string' && fileuploaderpath.length > 0) {
      fileUploaderPath = fileuploaderpath;
    }

    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.recvInvite, onRecvInvite);
    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.acceptInvite, onCloseInvite);
    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.declineInvite, onCloseInvite);

    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.recvMessageFromChat, onRecvMessageFromChat);

    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.createRoom, onCreateConference);

    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.openRoom, onOpenRoom);
    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.closeRoom, onCloseRoom);

    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.connected, onClientConnected);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.disconnected, onClientDisconnected);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.connectingFailed, onClientConnectingFailed);

    TMTalk.bind(TMTalk.events.pageFocus, onPageFocus);

    ASC.TMTalk.indicator.bind(ASC.TMTalk.indicator.events.start, onStartIndicator);
    ASC.TMTalk.indicator.bind(ASC.TMTalk.indicator.events.show, onShowIndicator);
    ASC.TMTalk.indicator.bind(ASC.TMTalk.indicator.events.stop, onStopIndicator);
  };

  var bind = function (eventName, handler, params) {
    return eventManager.bind(eventName, handler, params);
  };

  var unbind = function (handlerId) {
    return eventManager.unbind(handlerId);
  };

  var blur = function (evt) {
    clearTimeout(onblurTimeoutHandler);
    onblurTimeoutHandler = setTimeout(TMTalk.blurcallback, 1);
  };

  var focus = function (evt) {
    clearTimeout(onblurTimeoutHandler);

    if (properties.focused === false) {
      properties.focused = true;
      eventManager.call(customEvents.pageFocus, window, [evt]);
    }
  };

  var blurcallback = function (evt) {
    if (properties.focused === true) {
      properties.focused = false;
      eventManager.call(customEvents.pageBlur, window, [evt]);
    }
  };

  var click = function (evt) {
    clearTimeout(onblurTimeoutHandler);
    setTimeout(TMTalk.focus, 2);
  };

  var clickcallback = function (evt) {
    if (window.getSelection) {
      window.getSelection().removeAllRanges();
    }

    $(document).trigger('click');
  };

  var onStartIndicator = function () {
    setDefaultIcon = true;
  };

  var onShowIndicator = function () {
    //var currenttitle = document.title;
    //if (currenttitle !== indicatorTitle + ' ' + originalTitle) { 
    //  originalTitle = currenttitle;
    //}
    indicatorTitle += indicatorTitle.charAt(0);
    if (indicatorTitle.length > maxIndicatorLength) {
      indicatorTitle = indicatorTitle.charAt(0);
    }
    document.title = indicatorTitle + postfixTitle + ' ' + originalTitle;

    if (setDefaultIcon) {
      ASC.TMTalk.iconManager.set(ASC.TMTalk.Resources.iconNewMessage);
    } else {
      ASC.TMTalk.iconManager.reset();
    }
    setDefaultIcon = !setDefaultIcon;
  };

  var onStopIndicator = function () {
    indicatorTitle = '***';
    document.title = originalTitle;
    ASC.TMTalk.iconManager.reset();
    setDefaultIcon = true;
  };

  var onPageFocus = function () {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData !== null && currentRoomData.type === 'chat') {
      ASC.TMTalk.notifications.hide(currentRoomData.id);
    }
  };

  var onClientConnected = function () {
    $(document.body).addClass('connected');
  };

  var onClientDisconnected = function () {
    $(document.body).removeClass('connected');
  };

  var onClientConnectingFailed = function () {
    $(document.body).removeClass('connected');
  };

  var onCreateConference = function (roomjid, room) {
    ASC.TMTalk.mucManager.setSubject(roomjid, ASC.TMTalk.stringFormat(ASC.TMTalk.Resources.defaultConferenceSubjectTemplate, room.name));
  };

  var onOpenRoom = function (roomId, data, inBackground) {
    if (inBackground !== true) {
      if (TMTalk.properties.focused && data.type === 'chat') {
        ASC.TMTalk.notifications.hide(data.id);
      }

      $('#talkContentContainer').removeClass('disabled');
    }
  };

  var onCloseRoom = function (roomId, data, isCurrent) {
    if (isCurrent === true) {
      $('#talkContentContainer').addClass('disabled');
    }
  };

  var onRecvInvite = function (roomjid, inviterjid, reason) {
    if (TMTalk.properties.focused === false) {
      ASC.TMTalk.notifications.show(
        roomjid,
        ASC.TMTalk.Resources.titleRecvInvite + ' ' + ASC.TMTalk.mucManager.getConferenceName(roomjid),
        ASC.TMTalk.mucManager.getContactName(inviterjid) + ' ' + ASC.TMTalk.Resources.labelRecvInvite
      );
    }
  };

  var onCloseInvite = function (roomjid, name, inviterjid) {
    ASC.TMTalk.notifications.hide(roomjid);
  };

  var onRecvMessageFromChat = function (jid, displayname, displaydate, date, body) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (TMTalk.properties.focused === false || currentRoomData !== null && currentRoomData.type === 'chat' && currentRoomData.id !== jid) {
      ASC.TMTalk.notifications.show(
        jid,
        ASC.TMTalk.contactsManager.getContactName(jid),
        body.length < maxlengthMessage ? body : body.substring(0, maxlengthMessage) + '...'
      );
    }
  };

  var showDialog = function (name, titleval) {
    if (typeof name !== 'string') {
      return undefined;
    }
    name = name.toLowerCase();
    switch (name) {
      case 'create-room' :
      case 'remove-room' :
      case 'recv-invite' :
      case 'kick-occupant' :
      case 'create-mailing' :
      case 'browser-notifications' :
        hideAllDialogs();

        if (typeof titleval === 'string') {
          $('#talkDialogsContainer')
            .find('div.dialog.' + name + ':first')
            .find('div.head:first div.title:first span.value:first').empty().html(titleval);
        }

        $('#talkDialogsContainer')
          .attr('class', name)
          .find('div.dialog.' + name + ':first')
            .css({opacity : '0', display : 'block'})
            .animate({opacity : 1}, $.browser.msie && $.browser.version < 9 ? 0 : 'middle', function () {
              // TODO:
              var $block = $(this);
              $block.find('div.textfield:first input:first').focus();
              if ($.browser.msie && $.browser.version < 9 && $block.length !== 0) {
                var
                  prefix = ' ',
			            block = $block.get(0),
			            cssText = prefix + block.style.cssText,
			            startPos = cssText.toLowerCase().indexOf(prefix + 'filter:'),
			            endPos = cssText.indexOf(';', startPos);
			          if (startPos !== -1) {
			            if (endPos !== -1) {
			              block.style.cssText = [cssText.substring(prefix.length, startPos), cssText.substring(endPos + 1)].join('');
			            } else {
			              blockUI.style.cssText = cssText.substring(prefix.length, startPos);
			            }
			          }
			        }
            });
        break
    }
  };

  var hideDialog = function () {
    $('#talkDialogsContainer').find('div.dialog:visible').animate({opacity : 0}, $.browser.msie && $.browser.version < 9 ? 0 : 'middle', function () {
      $(this).hide();

      var container = document.getElementById('talkDialogsContainer');
      if (container) {
        container.className = '';
      }
    });
  };

  var hideAllDialogs = function () {
    $('#talkDialogsContainer').find('div.dialog').hide();
    var container = document.getElementById('talkDialogsContainer');
    if (container) {
      container.className = '';
    }
  };

  var createFileUploader = function (buttonid, onbegin, oncomplete) {
    //if (typeof ASC !== 'undefined' && typeof ASC.Controls !== 'undefined' && typeof ASC.Controls.FileUploader !== 'undefined') {
    //  fileUploader = new ASC.Controls.FileUploader({
    //    FileUploadHandler : 'ASC.Web.Talk.UploadFileHanler,ASC.Web.Talk',
    //    FileSizeLimit : ASC.TMTalk.properties.item('maxUploadSizeInKB'),
    //    AutoSubmit : true,
    //    SingleUploader : true,
    //    DisableFlash : ASC.TMTalk.flashPlayer.isCorrect === false || ASC.TMTalk.properties.item('fileTransportType') !== 'flash',
    //    UploadButtonID : 'talkFileUploader',
    //    OnBegin : onbegin,
    //    OnUploadComplete : oncomplete
    //  });
    //  var nodes = ASC.TMTalk.dom.getElementsByClassName(document.body, 'fileuploadinput', 'input');
    //  if (nodes.length > 0) {
    //    ASC.TMTalk.dom.addClass(nodes[0], 'disabled');
    //  }
    //}

    if (typeof FileHtml5Uploader !== 'undefined') {
      fileUploader = FileHtml5Uploader.InitFileUploader({
        FileUploadHandler: 'ASC.Web.Talk.UploadFileHanler,ASC.Web.Talk',
        AutoSubmit : true,
        SingleUploader : true,
        DisableFlash : ASC.TMTalk.flashPlayer.isCorrect === false || ASC.TMTalk.properties.item('fileTransportType') !== 'flash',
        UploadButtonID : buttonid,
        FileSizeLimit : ASC.TMTalk.properties.item('maxUploadSizeInKB'),
        Data : {ownjid : ASC.TMTalk.connectionManager.getNativeJid()},
        OnBegin : onbegin,
        OnUploadComplete : oncomplete
      });
      if (fileUploader) {
        fileUploader.__onBeginHandler = onbegin;
        fileUploader.__onCompleteHandler = oncomplete;
      }
      var nodes = ASC.TMTalk.dom.getElementsByClassName(document.body, 'fileuploadinput', 'input');
      if (nodes.length > 0) {
        ASC.TMTalk.dom.addClass(nodes[0], 'disabled');
      }
    }
    return fileUploader;
  };

  var sendFile = function (files) {
    return undefined;

    if (files.length > 0) {
      var xhr = null;
      if (window.XMLHttpRequest) {
        xhr = new XMLHttpRequest();
      }
      if (xhr && fileUploader && fileUploaderPath.length > 0) {
        xhr.onreadystatechange = (function (xhr, callback) {
          return function () {
            var responce = {Success : false};
            if (xhr.readyState == 4) {
              if (xhr.status == 200) {
                responce.Success = true;
                responce.FileName = '';
                responce.FileURL = '';
                // TODO:
                
              }
              if (callback) {
                callback(responce);
              }
              delete xhr;
            }
          };
        })(xhr, fileUploader.__onCompleteHandler ? fileUploader.__onCompleteHandler : null);
        fileUploader.__onBeginHandler();
        xhr.open('POST', fileUploaderPath, true);
        xhr.send(files[0]);
      }
    }
  };

  return {
    events      : customEvents,
    properties  : properties,

    init    : init,
    bind    : bind,
    unbind  : unbind,

    focus         : focus,
    blur          : blur,
    click         : click,
    blurcallback  : blurcallback,
    clickcallback : clickcallback,

    showDialog      : showDialog,
    hideDialog      : hideDialog,
    hideAllDialogs  : hideAllDialogs,

    sendFile            : sendFile,
    createFileUploader  : createFileUploader
  };
})(jQuery);

(function ($) {
  var
    constants = {
      propertySidebarWidth : 'sbw',
      propertyMeseditorHeight : 'meh'
    },
    minPageHeight = 300,
    offsetSidebarContainer = 0,
    offsetMeseditorContainer = 0,
    minMeseditorContainer = 60,
    minSidebarContainerWidth = 250,
    mcHeightOffset = 0,
    ccWidthOffset = 0,
    ccHeightOffset = 0,
    rcHeightOffset = 0,
    vsBottomOffset = 0,
    ssHeightOffset = 0,
    $window = null,
    horSlider = null, $horSlider = null,
    vertSlider = null, $vertSlider = null,
    startSplash = null, $startSplash = null,
    mainContainer = null, $mainContainer = null,
    contentContainer = null, $contentContainer = null,
    roomsContainer = null, $roomsContainer = null,
    meseditorContainer = null, $meseditorContainer = null,
    sidebarContainer = null, $sidebarContainer = null,
    contactsContainer = null, $contactsContainer = null,
    contactToolbarContainer = null, $contactToolbarContainer = null,
    meseditorToolbarContainer = null, $meseditorToolbarContainer = null;

  var platform = (function () {
    var
      ua = navigator.userAgent,
      version = (ua.match(/.+(?:rv|it|ra|ie|ox|me|on|id|os)[\/:\s]([\d._]+)/i)||[0,'0'])[1].replace('_', '');

    return {
      version : isFinite(parseFloat(version)) ? parseFloat(version) : version,
      android : /android/i.test(ua),
      ios     : /iphone|ipad/i.test(ua)
    }
  })();

  $.extend($, {platform : platform});

  $.extend(
    $.support,
    {
      webapp            : window.innerHeight === window.screen.availHeight,
      platform          : platform,
      orientation       : 'orientation' in window,
      touch             : 'ontouchend' in document,
      csstransitions    : 'WebKitTransitionEvent' in window,
      pushState         : !!history.pushState,
      cssPositionFixed  : !('ontouchend' in document),
      iscroll           : $.platform.ios,
      svg               : !($.browser.mozilla === true),
      dataimage         : !($.browser.msie && $.browser.version < 9)
    }
  );

//-------------------------------------------------------------------------------------------
  function onDragStart () {
    return false;
  }

  function onSelectStart () {
    return false;
  }
//-------------------------------------------------------------------------------------------
  function onMouseMoveVertSlider (evt) {
    var
      contentContainerHeight = $contentContainer.height(),
      newMeseditorHeight = document.body.offsetHeight - evt.pageY - offsetMeseditorContainer;

    if (newMeseditorHeight * 3 > contentContainerHeight) { // if more 33%
      newMeseditorHeight = Math.floor(contentContainerHeight / 3);
    }
    if (newMeseditorHeight < minMeseditorContainer) {
      newMeseditorHeight = minMeseditorContainer;
    }
    meseditorContainer.style.height = newMeseditorHeight + 'px';
    //$meseditorContainer.height(newMeseditorHeight);
    vertSlider.style.bottom = newMeseditorHeight + vsBottomOffset + 'px';
    //$vertSlider.css('bottom', newMeseditorHeight + vsBottomOffset + 'px');
    roomsContainer.style.height = contentContainerHeight - meseditorContainer.offsetHeight - roomsContainer.offsetTop - rcHeightOffset + 'px';
    //$roomsContainer.height(contentContainerHeight - $meseditorContainer.height() - parseInt($roomsContainer.css('top')) - rcHeightOffset);

    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData !== null && currentRoomData.type === 'conference' && currentRoomData.minimized === false) {
      var nodes = ASC.TMTalk.dom.getElementsByClassName(roomsContainer, 'room conference current', 'li');
      if (nodes.length > 0) {
        var roomHeight = nodes[0].offsetHeight;
        nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'sub-panel', 'div');
        if (nodes.length > 0) {
          nodes[0].style.height = Math.ceil(roomHeight / 2) - 5 + 'px';
        }
      }
    }
    return false;
  }

  function onMouseUpVertSlider (evt) {
    ASC.TMTalk.properties.item(constants.propertyMeseditorHeight, meseditorContainer.offsetHeight, true);
    //ASC.TMTalk.properties.item(constants.propertyMeseditorHeight, $meseditorContainer.height(), true);
    $meseditorContainer.removeClass('blocked');
    $window.focus();

    $(document).unbind('dragstart', onDragStart);
    $(document).unbind('mouseup', onMouseUpVertSlider);
    $(document).unbind('mousemove', onMouseMoveVertSlider);
    $(document.body).unbind('selectstart', onSelectStart);
    return false;
  }

  function dragVertSlider (evt) {
    offsetMeseditorContainer = document.body.offsetHeight - evt.pageY - meseditorContainer.offsetHeight;
    //offsetMeseditorContainer = document.body.offsetHeight - evt.pageY - $meseditorContainer.height();
    $meseditorContainer.addClass('blocked');

    $(document).bind('dragstart', onDragStart);
    $(document).bind('mouseup', onMouseUpVertSlider);
    $(document).bind('mousemove', onMouseMoveVertSlider);
    $(document.body).bind('selectstart', onSelectStart);
    return false;
  }
//-------------------------------------------------------------------------------------------
  function onMouseMoveHorSlider (evt) {
    var
      mainContainerWidth = $mainContainer.width(),
      newSidebarWidth = document.body.offsetWidth - evt.pageX - offsetSidebarContainer;

    if (newSidebarWidth * 2 > mainContainerWidth) { // if more 50%
      newSidebarWidth = Math.floor(mainContainerWidth / 2);
    }
    if (newSidebarWidth < minSidebarContainerWidth) {
      newSidebarWidth = minSidebarContainerWidth;
    }
    $sidebarContainer.width(newSidebarWidth);
    $horSlider.css('right', newSidebarWidth + 'px');
    $contentContainer.width(mainContainerWidth - newSidebarWidth - ccWidthOffset);
    return false;
  }

  function onMouseUpHorSlider (evt) {
    ASC.TMTalk.properties.item(constants.propertySidebarWidth, sidebarContainer.offsetWidth, true);
    //ASC.TMTalk.properties.item(constants.propertySidebarWidth, $sidebarContainer.width(), true);
    $meseditorContainer.removeClass('blocked');
    $window.focus();

    $(document).unbind('dragstart', onDragStart);
    $(document).unbind('mouseup', onMouseUpHorSlider);
    $(document).unbind('mousemove', onMouseMoveHorSlider);
    $(document.body).unbind('selectstart', onSelectStart);
    return false;
  }

  function dragHorSlider (evt) {
    offsetSidebarContainer = document.body.offsetWidth - evt.pageX - sidebarContainer.offsetWidth;
    //offsetSidebarContainer = document.body.offsetWidth - evt.pageX - $sidebarContainer.width();
    $meseditorContainer.addClass('blocked');

    $(document).bind('dragstart', onDragStart);
    $(document).bind('mouseup', onMouseUpHorSlider);
    $(document).bind('mousemove', onMouseMoveHorSlider);
    $(document.body).bind('selectstart', onSelectStart);
    return false;
  }
//-------------------------------------------------------------------------------------------
  $(window).blur(TMTalk.blur).focus(TMTalk.focus);
  $(document).click(TMTalk.click);

  $(function () {
    //$(document.body).addClass('focused');

    if (ASC.TMTalk.flashPlayer.isCorrect) {
      var
        o = document.createElement('div'),
        soundsContainerId = 'talkSoundsContainer-' + Math.floor(Math.random() * 1000000);

      o.setAttribute('id', soundsContainerId);
      document.body.appendChild(o);
      swfobject.embedSWF(
        ASC.TMTalk.properties.item('sounds'),
        soundsContainerId,
        1,
        1,
        '9.0.0',
        ASC.TMTalk.properties.item('expressInstall'),
        {apiInit : function (id) {ASC.TMTalk.sounds.init(id);}, apiId : soundsContainerId},
        {allowScriptAccess : 'always', wmode : 'transparent'},
        {styleclass : 'soundsContainer', wmode: 'transparent'}
      );
    }

    $('#talkWrapper').show();
    $window = $(window);
    horSlider = document.getElementById('talkHorSlider'); $horSlider = $(horSlider);
    vertSlider = document.getElementById('talkVertSlider'); $vertSlider = $(vertSlider);
    startSplash = document.getElementById('talkStartSplash'); $startSplash = $(startSplash);
    mainContainer = document.getElementById('talkMainContainer'); $mainContainer = $(mainContainer);
    sidebarContainer = document.getElementById('talkSidebarContainer'); $sidebarContainer = $(sidebarContainer);
    contentContainer = document.getElementById('talkContentContainer'); $contentContainer = $(contentContainer);
    meseditorContainer = document.getElementById('talkMeseditorContainer'); $meseditorContainer = $(meseditorContainer);
    roomsContainer = document.getElementById('talkRoomsContainer'); $roomsContainer = $(roomsContainer);
    contactsContainer = document.getElementById('talkContactsContainer'); $contactsContainer = $(contactsContainer);
    contactToolbarContainer = document.getElementById('talkContactToolbarContainer'); $contactToolbarContainer = $(contactToolbarContainer);
    meseditorToolbarContainer = document.getElementById('talkMeseditorToolbarContainer'); $meseditorToolbarContainer = $(meseditorToolbarContainer);

    mcHeightOffset = $('#talkTabContainer').height() + 10 * 2 + 10 * 2 + 1;
    ccWidthOffset = $horSlider.width();
    ccHeightOffset = $('#talkStatusContainer').outerHeight(true) + 1 * 2;
    rcHeightOffset = parseInt($meseditorContainer.css('bottom')) + $vertSlider.height() + 1 * 2 + 1;
    vsBottomOffset = $meseditorToolbarContainer.outerHeight(true) + 1;
    ssHeightOffset = $meseditorToolbarContainer.outerHeight(true) + 10 + 1;

    var windowHeight = $window.height();
    if (windowHeight < minPageHeight) {
      windowHeight = minPageHeight;
    }

    if ($.browser.msie && $.browser.version < 9) {
      $(document.body).add('#studioContent').css('backgroundColor', '#0A4462');
      $startSplash.find('div.background').css('backgroundColor', '#4A8CAC');
    }

    var mainContainerHeight = windowHeight - mcHeightOffset;
    mainContainer.style.height = mainContainerHeight + 'px';
    //$mainContainer.height(windowHeight - mcHeightOffset);
    contactsContainer.style.height = mainContainerHeight - contactToolbarContainer.offsetHeight - ccHeightOffset + 'px';
    //$contactsContainer.height($mainContainer.height() - $contactToolbarContainer.height() - ccHeightOffset);

    var
      contentContainerHeight = contentContainer.offsetHeight,
      //contentContainerHeight = $contentContainer.height(),
      meseditorHeight = ASC.TMTalk.properties.item(constants.propertyMeseditorHeight);
    meseditorHeight = isFinite(+meseditorHeight) ? +meseditorHeight : minMeseditorContainer;
    if (meseditorHeight * 3 > contentContainerHeight) { // if more 33%
      meseditorHeight = Math.floor(contentContainerHeight / 3);
    }
    if (meseditorHeight < minMeseditorContainer) {
      meseditorHeight = minMeseditorContainer;
    }
    meseditorContainer.style.height = meseditorHeight + 'px';
    //$meseditorContainer.height(meseditorHeight);
    vertSlider.style.bottom = meseditorHeight + vsBottomOffset + 'px';
    //$vertSlider.css('bottom', meseditorHeight + vsBottomOffset + 'px');

    var roomsContainerHeight = contentContainerHeight - roomsContainer.offsetTop - ($roomsContainer.hasClass('history') ? 2 : meseditorContainer.offsetHeight + rcHeightOffset);
    if (roomsContainerHeight < 0) {
      roomsContainerHeight = -roomsContainerHeight;
    }
    roomsContainer.style.height = roomsContainerHeight + 'px';
    //$roomsContainer.height(contentContainerHeight - parseInt($roomsContainer.css('top')) - ($roomsContainer.hasClass('history') ? 2 : $meseditorContainer.height() + rcHeightOffset));

    var
      mainContainerWidth = $mainContainer.width(),
      sidebarWidth = ASC.TMTalk.properties.item(constants.propertySidebarWidth);
    sidebarWidth = isFinite(+sidebarWidth) ? +sidebarWidth : minSidebarContainerWidth;
    if (sidebarWidth * 2 > mainContainerWidth) { // if more 50%
      sidebarWidth = Math.floor(mainContainerWidth / 2);
    }
    if (sidebarWidth < minSidebarContainerWidth) {
      sidebarWidth = minSidebarContainerWidth;
    }
    sidebarContainer.style.width = sidebarWidth + 'px';
    //$sidebarContainer.width(sidebarWidth);
    horSlider.style.right = sidebarWidth + 'px';
    //$horSlider.css('right', sidebarWidth + 'px');
    contentContainer.style.width = mainContainerWidth - sidebarWidth - ccWidthOffset + 'px';
    //$contentContainer.width(mainContainerWidth - sidebarWidth - ccWidthOffset);

    startSplash.style.height = contentContainer.offsetHeight - ssHeightOffset + 'px';
    //$startSplash.height($contentContainer.height() - ssHeightOffset);

    if (ASC.TMTalk.properties.item('hidscd') !== '1') {
      try {
        google.gears.factory.create('beta.desktop').createShortcut(
          ASC.TMTalk.Resources.productName,
          location.href,
          {
            '16x16'   : ASC.TMTalk.Resources.addonIcon16,
            '32x32'   : ASC.TMTalk.Resources.addonIcon32,
            '48x48'   : ASC.TMTalk.Resources.addonIcon48,
            '128x128' : ASC.TMTalk.Resources.addonIcon128
          },
          ASC.TMTalk.Resources.hintCreateShortcutDialog
        );
      } catch (err) {}
    }
    ASC.TMTalk.properties.item('hidscd', '1', true);

    if ($.browser.safari) {
      $('#talkStartSplash').mousedown(function () {
        if (window.getSelection) {
          window.getSelection().removeAllRanges();
        }
        return false;
      });
    }

    //if (ASC.TMTalk.properties.item('hidnd') !== '1') {
    //  if (ASC.TMTalk.notifications.supported.current === true) {
    //    if (ASC.TMTalk.notifications.enabled() === true) {
    //      $('#cbxToggleNotifications').removeClass('disabled');
    //    } else {
    //      $('#cbxToggleNotifications').addClass('disabled');
    //    }
    //    TMTalk.showDialog('browser-notifications');
    //  }
    //}

    //switch (ASC.TMTalk.properties.item('hidnd')) {
    //  case '0' :
    //    $('#cbxToggleNotificationsDialog').attr('checked', false);
    //    break;
    //  case '1' :
    //    $('#cbxToggleNotificationsDialog').attr('checked', true);
    //    break;
    //  default :
    //    $('#cbxToggleNotificationsDialog').attr('checked', true);
    //    ASC.TMTalk.properties.item('hidnd', '1', true);
    //    break;
    //}

    $(document).keydown(function (evt) {
      // shift + tab
      if (evt.shiftKey === true && evt.keyCode === 9) {
        // TODO :
        ASC.TMTalk.tabsContainer.nextTab();
        return false;
      }
      // esc
      if (evt.keyCode === 27) {
        // TODO :
        TMTalk.hideAllDialogs();
        return false;
      }
      // shift + F
      if (evt.shiftKey === true && evt.keyCode === 70) {
         // TODO :
        if (ASC.TMTalk.properties.item('enblfltr') === '0') {
          ASC.TMTalk.properties.item('enblfltr', '1');
        }
        return false;
      }
    });

    $(window)
      .keypress(function (evt) {
        if (evt.ctrlKey && evt.charCode === 119 && ASC.TMTalk.connectionManager.connected()) {
          ASC.TMTalk.connectionManager.terminate();
        }
      })
      .bind('beforeunload', function(evt) {
        if (ASC.TMTalk.connectionManager.connected()) {
          ASC.TMTalk.connectionManager.terminate();
        }
      })
      .resize(function () {
        var windowHeight = $window.height();
        if (windowHeight < minPageHeight) {
          windowHeight = minPageHeight;
        }

        var
          containerHeight = windowHeight - mcHeightOffset,
          contactsContainerHeight = containerHeight - contactToolbarContainer.offsetHeight - ccHeightOffset,
          roomsContainerHeight = containerHeight - roomsContainer.offsetTop - ($roomsContainer.hasClass('history') ? 2 : meseditorContainer.offsetHeight + rcHeightOffset);

        if (containerHeight < 0) {
          containerHeight = -containerHeight;
        }
        if (contactsContainerHeight < 0) {
          contactsContainerHeight = -contactsContainerHeight;
        }
        if (roomsContainerHeight < 0) {
          roomsContainerHeight = -roomsContainerHeight;
        }

        mainContainer.style.height = containerHeight + 'px';
        //$mainContainer.height(containerHeight);
        contactsContainer.style.height = contactsContainerHeight + 'px';
        //$contactsContainer.height(containerHeight - $contactToolbarContainer.height() - ccHeightOffset);
        roomsContainer.style.height = roomsContainerHeight + 'px';
        //$roomsContainer.height(containerHeight - parseInt($roomsContainer.css('top')) - ($roomsContainer.hasClass('history') ? 2 : $meseditorContainer.height() + rcHeightOffset));

        var
          sidebarWidth = sidebarContainer.offsetWidth,
          mainContainerWidth = mainContainer.offsetWidth;
          //sidebarWidth = $sidebarContainer.width(),
          //mainContainerWidth = $mainContainer.width();

        sidebarWidth = isFinite(+sidebarWidth) ? +sidebarWidth : minSidebarContainerWidth;
        if (sidebarWidth * 2 > mainContainerWidth) { // if more 50%
          sidebarWidth = Math.floor(mainContainerWidth / 2);
        }
        if (sidebarWidth < minSidebarContainerWidth) {
          sidebarWidth = minSidebarContainerWidth;
        }
        sidebarContainer.style.width = sidebarWidth + 'px';
        //$sidebarContainer.width(sidebarWidth);
        horSlider.style.right = sidebarWidth + 'px';
        //$horSlider.css('right', sidebarWidth + 'px');
        contentContainer.style.width = mainContainerWidth - sidebarWidth - ccWidthOffset + 'px';
        //$contentContainer.width(mainContainerWidth - sidebarWidth - ccWidthOffset);

        startSplash.style.height = contentContainer.offsetHeight - ssHeightOffset + 'px';
        //$startSplash.height($contentContainer.height() - ssHeightOffset);

        var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
        if (currentRoomData !== null && currentRoomData.type === 'conference' && currentRoomData.minimized === false) {
          var nodes = ASC.TMTalk.dom.getElementsByClassName(roomsContainer, 'room conference current', 'li');
          if (nodes.length > 0) {
            var roomHeight = nodes[0].offsetHeight;
            nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'sub-panel', 'div');
            if (nodes.length > 0) {
              nodes[0].style.height = Math.ceil(roomHeight / 2) - 5 + 'px';
            }
          }
        }
      });

    $('#talkHorSlider').mousedown(function (evt) {
      return dragHorSlider(evt);
    });

    $('#talkVertSlider').mousedown(function (evt) {
      return dragVertSlider(evt);
    });

    $('#talkDialogsContainer')
      .keydown(function (evt) {
        evt.originalEvent.stopPropagation ? evt.originalEvent.stopPropagation() : evt.originalEvent.cancelBubble = true;
      })
      .keypress(function (evt) {
        switch (evt.keyCode) {
          case 13 :
            $('#talkDialogsContainer').find('div.dialog:visible:first div.toolbar:first div.button:first').click();
            return false;
          case 27 :
            TMTalk.hideDialog();
            break;
          default :
            if (evt.target.tagName.toLowerCase() === 'input') {
              $(evt.target).parents('div.textfield:first').removeClass('invalid-field');
            }
        }
      })
      .click(function (evt) {
        var $target = $(evt.target);
        if ($target.hasClass('button') && $target.hasClass('close-dialog')) {
          TMTalk.hideDialog();
        }
      });

    //$('#cbxToggleNotifications').click(function (evt) {
    //  var $target = $(evt.target);
    //  if ($target.hasClass('button')) {
    //    var $this = $(this);
    //    if ($this.toggleClass('disabled').hasClass('disabled')) {
    //      $this.addClass('disabled');
    //      ASC.TMTalk.notifications.disable();
    //    } else {
    //      $this.addClass('disabled');
    //      ASC.TMTalk.notifications.enable(function (val) {
    //        if (val === true) {
    //          $('#cbxToggleNotifications').removeClass('disabled');
    //        } else {
    //          $('#cbxToggleNotifications').addClass('disabled');
    //        }
    //      });
    //    }
    //  }
    //});

    //$('#cbxToggleNotificationsDialog').click(function (evt) {
    //  ASC.TMTalk.properties.item('hidnd', $(this).is(':checked') ? '1' : '0', true);
    //});
  });
})(jQuery);
