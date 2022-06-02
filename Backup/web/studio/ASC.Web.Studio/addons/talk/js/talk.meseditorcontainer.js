﻿
window.ASC = window.ASC || {};

window.ASC.TMTalk = window.ASC.TMTalk || {};

window.ASC.TMTalk.meseditorContainer = (function ($) {
  var
    isInit = false,
    taWindow = null,
    taStylePath = '',
    maRange = null,
    maRangeMark = null,
    composingMessages = {},
    pausedMesssagesTimeout = 5,
    simpleEditor = false,
    browser = (function () {
      var
        ua = navigator.userAgent,
        browser = null,
        version = (ua.match(/.+(?:rv|it|ra|ie|ox|me|on|id|os)[\/:\s]([\d._]+)/i)||[0,'0'])[1].replace('_', '');

      browser = {
        version : isFinite(parseFloat(version)) ? parseFloat(version) : version,
        // browsers
        msie    : '\v' == 'v' || /msie/i.test(ua),
        opera   : window.opera ? true : false,
        chrome  : window.chrome ? true : false,
        safari  : /safari/i.test(ua) && !window.chrome,
        firefox : /firefox/i.test(ua),
        mozilla : /mozilla/i.test(ua) && !/(compatible|webkit)/.test(ua),
        // engine
        ios     : /iphone|ipad/i.test(ua),
        gecko   : /gecko/i.test(ua),
        webkit  : /webkit/i.test(ua)
      };

      if (browser.msie && window.addEventListener && browser.version < 9) {
        browser.version = 9;
      }

      return browser;
    })();

  simpleEditor = simpleEditor || browser.ios;

  function trimS (str) {
    if (typeof str !== 'string' || str.length === 0) {
      return '';
    }
    return str.replace(/^\s+|\s+$/g, '');
  }

  function trimN (str) {
    if (typeof str !== 'string' || str.length === 0) {
      return '';
    }
    return str.replace(/^\n+|\n+$/g, '');
  }

  function trim (str) {
    if (typeof str != 'string' || str.length === 0) {
      return '';
    }
    return str.replace(/^[\n\s]+|[\n\s]+$/g, '');
  }

  function translateSymbols (str, toText) { 
    var
      symbols = [
        ['&lt;',  '<'],
        ['&gt;',  '>'],
        ['&and;', '\\^'],
        ['&sim;', '~'],
        ['&amp;', '&']
      ];

    if (typeof str !== 'string') {
      return '';
    }

    // replace html to symbols
    if (typeof toText === 'undefined' || toText === true) {
      var symInd = symbols.length;
      while (symInd--) {
        str = str.replace(new RegExp(symbols[symInd][0], 'g'), symbols[symInd][1]); 
      }
    // replace symbols to html
    } else {
      var symInd = symbols.length;
      while (symInd--) {
        str = str.replace(new RegExp(symbols[symInd][1], 'g'), symbols[symInd][0]);
      }
    }
    return str;
  }

  function isBlockNode (node) {
    if (!node || typeof node == 'undefined' || typeof node.nodeName == 'undefined') {
      return false;
    }
    switch (node.nodeName.toLowerCase()) {
      case 'p' :
      case 'h1' :
      case 'h2' :
      case 'h3' :
      case 'h4' :
      case 'h5' :
      case 'h6' :
      case 'ul' :
      case 'ol' :
      case 'li' :
      case 'tr' :
      case 'div' :
      case 'table' :
        return true;
      default :
        return false;
    }
    return false;
  }

  function isNonblockNode (node) {
    if (!node || typeof node == 'undefined' || typeof node.nodeName == 'undefined') {
      return false;
    }
    return !isBlockNode(node);
  }

  function getNodeContent (node) {
    var
      content = '',
      child = null,
      childrens = node.childNodes;

    for (var i = 0, n = childrens.length; i < n; i++) {
      child = childrens.item(i);
      switch (child.nodeType) {
        case 1 :
        case 5 :
          switch (child.nodeName.toLowerCase()) {
            case 'br' :
              if (child.getAttribute('type') !== 'moz_') {
                content += '\n';
              }
              break;
            case 'a' :
              var attr = child.getAttribute('href');
              if (attr) {
                content += attr;
              }
              break;
            case 'img' :
              var attr = child.getAttribute('alt');
              if (attr) {
                content += attr;
              }
              break;
            case 'p' :
            case 'h1' :
            case 'h2' :
            case 'h3' :
            case 'h4' :
            case 'h5' :
            case 'h6' :
            case 'ul' :
            case 'ol' :
            case 'li' :
            case 'tr' :
            case 'div' :
            case 'table' :
              var childContent = trimN(arguments.callee(child));
              content += (i !== 0 ? '\n' : '') + childContent;
              if (childContent) {
                content += isNonblockNode(child.nextSibling) ? '\n' : '';
              }
              break;
            default :
              content += arguments.callee(child);
              break;
          }
          break;
        case 2 :
        case 3 :
        case 4 :
          content += child.nodeValue;
          break;
        default :
          break;
      }
    }
    return content;
  }

  function setSelected () {
    if (browser.msie && maRange && maRangeMark) {
      maRange.moveToBookmark(maRangeMark);
      maRange.select();
    }
  }

  function setFocusToTextarea (wnd) {
    window.focus();
    wnd.focus();
  }

  var insertNewLineToTextarea = (function () {
    if (simpleEditor === true) {
      return function (wnd, fin) {
        if (document.selection) {
          var s = document.selection.createRange(); 
          if (s.text) {
            s.text = '\n';
	          s.select();
          }
        } else if (typeof wnd.selectionStart === 'number') {
          var
            start = wnd.selectionStart,
            end = wnd.selectionEnd;

          wnd.value = wnd.value.substring(0, start) + '\n' + wnd.value.substring(end);
          wnd.setSelectionRange(start + 1, start + 1);
        }
      };
    }
    return function (wnd, fin) {
      if (browser.msie && browser.version < 9) {
        var range = wnd.document.selection.createRange();
        range.pasteHTML('&nbsp;<p></p>');
        range.collapse(true);
      } else {
        var el = wnd.document.createElement('br');
        var selectionRange = wnd.getSelection().getRangeAt(0);
        selectionRange.deleteContents();
        selectionRange.insertNode(el);

        // fucking webkit
        if (browser.webkit && fin !== true && el.previousSibling && el.previousSibling.nodeType === 3 && (!el.nextSibling || !el.nextSibling.data)) {
          try {insertNewLineToTextarea(wnd, true)} catch (err) {}
        }

        if (el.parentNode.tagName.toLowerCase() === 'a') {
          el.parentNode.parentNode.insertBefore(el, el.parentNode.nextElementSibling);
        }

        wnd.getSelection().collapseToEnd();
        wnd.getSelection().removeAllRanges();

        selectionRange = wnd.document.createRange();
        selectionRange.selectNode(el);
        wnd.getSelection().addRange(selectionRange);
        wnd.getSelection().collapseToEnd();
      }
      wnd.document.body.scrollTop = ASC.TMTalk.dom.maxScrollTop(wnd.document.body);
    };
  })();

  var insertSmileToTextarea = (function () {
    if (simpleEditor === true) {
      return function (wnd, src, title) {
        wnd.focus();
        if (document.selection) {
          var s = document.selection.createRange(); 
          s.text = title;
          s.select();
        } else if (typeof wnd.selectionStart === 'number') {
          var
            start = wnd.selectionStart,
            end = wnd.selectionEnd;

          wnd.value = wnd.value.substring(0, start) + title + wnd.value.substring(end);
          wnd.setSelectionRange(start + title.length, start + title.length);
        }
      };
    }
    return function (wnd, src, title) {
      setSelected();
      setFocusToTextarea(wnd);
      var selectionRange = null;
      var el = null;

      if (browser.msie) {
        el = '<img src="' + src + '" alt="' + title + '" contentEditable="false" onResizeStart="return false">';
        selectionRange = wnd.document.selection.createRange();
        selectionRange.pasteHTML(el);
        selectionRange.collapse(true);
      } else {
        el = wnd.document.createElement('img');
        el.src = src;
        el.alt = title;

        selectionRange = wnd.getSelection().getRangeAt(0);
        selectionRange.deleteContents();
        selectionRange.insertNode(el);
        wnd.getSelection().collapseToEnd();
        wnd.getSelection().removeAllRanges();

        selectionRange = wnd.document.createRange();
        selectionRange.selectNode(el);
        wnd.getSelection().addRange(selectionRange);
        wnd.getSelection().collapseToEnd();
      }
    };
  })();


  var clearTextarea = (function () {
    if (simpleEditor === true) {
      return function (wnd) {
        wnd.value = '';
      };
    }
    return function (wnd) {
      var o = wnd.document.body;
	    while (o.firstChild) {
	      o.removeChild(o.firstChild);
	    }

      if (!browser.msie) {
        var bogus = wnd.document.createElement('br');
        bogus.setAttribute('type', 'moz_');
        wnd.document.body.appendChild(bogus);

        var selectionRange = wnd.document.createRange();
        selectionRange.selectNode(bogus);
        wnd.getSelection().addRange(selectionRange);
        wnd.getSelection().collapseToStart();
      }
    };
  })();

  var getTextareaContent = (function () {
    if (simpleEditor === true) {
      return function (wnd) {
        return wnd.value;
      };
    }
    return function (wnd) {
      return wnd.document.body.innerHTML.replace(/\n/g, '');
    };
  })();

  var init = function (id, cssfile) {
    if (isInit === true) {
      return undefined;
    }
    isInit = true;
    // TODO
    taStylePath = cssfile;

    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.createRoom, onCreateRoom);
    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.openRoom, onOpenRoom);
    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.closeRoom, onCloseRoom);

    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.openHistory, onOpenHistory);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.closeHistory, onCloseHistory);

    ASC.TMTalk.msManager.bind(ASC.TMTalk.msManager.events.addContact, onAddContact);
    ASC.TMTalk.msManager.bind(ASC.TMTalk.msManager.events.removeContact, onRemoveContact);

    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.connected, onClientConnected);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.disconnected, onClientDisconnected);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.connectingFailed, onClientConnectingFailed);
  };

  var createTextarea = (function () {
    if (simpleEditor === true) {
      return function (iframe) {
        if (typeof iframe === 'string') {
          var
            textareasContainer = document.getElementById('talkTextareaContainer'),
            roomId = iframe,
            node = null,
            nodes = null,
            nodesInd = 0;

          nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textarea', 'li');
          nodesInd = nodes.length;
          while (nodesInd--) {
            node = nodes[nodesInd];
            if (node.getAttribute('data-roomid') === roomId) {
              nodes = node.getElementsByTagName('iframe');
              if (nodes.length > 0) {
                iframe = nodes[0];
              }
              break;
            }
          }
        }

        if (!iframe) {
          return undefined;
        }

        var textarea = document.createElement('textarea');
        iframe.parentNode.insertBefore(textarea, iframe);
        iframe.parentNode.removeChild(iframe);

        jQuery(textarea)
          .keypress(ASC.TMTalk.meseditorContainer.keyPress);
      };
    }
    return function (iframe) {
      if (typeof iframe === 'string') {
        var
          textareasContainer = document.getElementById('talkTextareaContainer'),
          roomId = iframe,
          node = null,
          nodes = null,
          nodesInd = 0;

        nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textarea', 'li');
        nodesInd = nodes.length;
        while (nodesInd--) {
          node = nodes[nodesInd];
          if (node.getAttribute('data-roomid') === roomId) {
            nodes = node.getElementsByTagName('iframe');
            if (nodes.length > 0) {
              iframe = nodes[0];
            }
            break;
          }
        }
      }

      if (!iframe) {
        return undefined;
      }
      iframe.setAttribute('src', 'javascript:false;');

      var html = [
        '<html xmlns="http://www.w3.org/1999/xhtml">',
          '<head>',
            '<link rel="stylesheet" type="text/css" href="' + taStylePath + '" />',
          '</head>',
          '<body',
            ' contentEditable="true"',
            jQuery.browser.mozilla ? '' : ' onkeypress="return parent.ASC.TMTalk.meseditorContainer.keyPress(event)"',
            jQuery.browser.mozilla ? '' : ' onblur="return parent.TMTalk.blur(event)"',
            jQuery.browser.mozilla ? '' : ' onfocus="return parent.TMTalk.focus(event)"',
            jQuery.browser.mozilla ? '' : ' onclick="return parent.TMTalk.clickcallback(event)"',
          '></body>',
        '</html>'
      ].join('');

      iframe.contentWindow.document.open();
      iframe.contentWindow.document.write(html);
      iframe.contentWindow.document.close();

      if (jQuery.browser.mozilla) {
        iframe.contentWindow.document.designMode = 'on';

        jQuery(iframe.contentWindow.document)
          .keypress(ASC.TMTalk.meseditorContainer.keyPress)
          .blur(TMTalk.blur).focus(TMTalk.focus).click(parent.TMTalk.clickcallback);
      }

      var
        o = null,
        wnd = iframe.contentWindow;
      o = wnd.document.body;
      while (o.firstChild) {
        o.removeChild(o.firstChild);
      }
      if (!jQuery.browser.msie) {
        var bogus = wnd.document.createElement('br');
        bogus.setAttribute('type', 'moz_');
        wnd.document.body.appendChild(bogus);
      }

      nodes = wnd.document.getElementsByTagName('html');
      if (nodes.length > 0) {
        nodes[0].setAttribute('dir', 'ltr');
      }

      try { wnd.document.execCommand('undo', false, null); } catch (err) {}
      try { wnd.document.execCommand('useCSS', false, true); } catch (err) {}
      try { wnd.document.execCommand('styleWithCSS',false, true); } catch (err) {}
      try { wnd.document.execCommand('enableObjectResizing', false, false); } catch (err) {}
    };
  })();

  var insertSmile = (function () {
    if (simpleEditor === true) {
      return function (src, title) {
        if (taWindow) {
          insertSmileToTextarea(taWindow, src, title);
        }
      };
    }
    return function (src, title) {
      if (taWindow) {
        insertSmileToTextarea(taWindow, src, title);
      }
    };
  })();

  var setRange = (function () {
    if (simpleEditor === true) {
      return function () {
        if (taWindow !== null) {
          maRange = document.selection.createRange();
          maRangeMark = maRange.getBookmark();
        }
      };
    }
    return function () {
      if (taWindow !== null) {
        maRange = taWindow.document.selection.createRange();
        maRangeMark = maRange.getBookmark();
      }
    };
  })();

  var setCursor = (function () {
    if (browser.ios) {
      return function (wnd) {
        return undefined;
      };
    }
    if (simpleEditor === true) {
      return function (wnd) {
        window.focus();
        wnd.focus();
        if (wnd.createTextRange) {
          var r = wnd.createTextRange();
          r.collapse(false);
          r.select();
        }
      };
    }
    return function (wnd)  {
      if (!wnd || typeof wnd !== 'object') {
        var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
        if (currentRoomData === null) {
          return undefined;
        }

        var
          textareasContainer = document.getElementById('talkTextareaContainer'),
          roomId = currentRoomData.roomId,
          wnd = null,
          node = null,
          nodes = null,
          nodesInd = 0;

        nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textarea', 'li');
        nodesInd = nodes.length;
        while (nodesInd--) {
          node = nodes[nodesInd];
          if (node.getAttribute('data-roomid') === roomId) {
            nodes = node.getElementsByTagName('iframe');
            if (nodes.length > 0) {
              wnd = nodes[0].contentWindow;
            }
            break;
          }
        }
      }

      if (!wnd) {
        return undefined;
      }

      window.focus();
      wnd.focus();
      if (jQuery.browser.msie) {
        var range = wnd.document.body.createTextRange(), textnode = wnd.document.createElement('span');

        textnode.appendChild(wnd.document.createTextNode('text'));
        var lastelement = ASC.TMTalk.dom.lastElementChild(wnd.document.body);
        (lastelement ? lastelement : wnd.document.body).appendChild(textnode);
        range.moveToElementText(textnode);
        range.select();

        range.pasteHTML('');
        range.collapse(true);
      } else {
        var range = wnd.document.createRange(), textnode = wnd.document.createElement('span');

        textnode.appendChild(wnd.document.createTextNode(''));
        wnd.document.body.insertBefore(textnode, ASC.TMTalk.dom.lastElementChild(wnd.document.body));
        range.selectNode(textnode);
        wnd.getSelection().addRange(range);
        wnd.getSelection().collapseToStart();
        wnd.document.body.removeChild(textnode);
      }
    };
  })();


  var pausedMessage = function (jid) {
    if (ASC.TMTalk.connectionManager.connected() && composingMessages.hasOwnProperty(jid)) {
      clearTimeout(composingMessages[jid].handlerTimeout);
      delete composingMessages[jid];
      ASC.TMTalk.connectionManager.pausedMessage(jid);
    }
  };

  var pausedMessageCallback = function (jid) {
    return function () {
      pausedMessage(jid);
    }
  };

  var keyPress = function (evt) {
    switch (evt.keyCode) {
      case 9 :
        if (evt.shiftKey) {
          ASC.TMTalk.tabsContainer.nextTab();
          return false;
        }
        break;
      case 10 :
      case 13 :
        // if send by ctrl + enter
        if (ASC.TMTalk.properties.item('sndbyctrlentr') === '1') {
          // if pressed ctrl + enter
          if (evt.ctrlKey) {
            sendMessage();
          // or \n
          } else {
            return undefined;
          }
        // if send by enter
        } else {
          // set \n
          if (evt.ctrlKey) {
            insertNewLineToTextarea(taWindow);
          // if press ctrl
          } else {
            sendMessage();
          }
        }
        // short variant
        // if (evt.ctrlKey === (+ASC.TMTalk.properties.item('sndbyctrlentr') === 1)) {
        //  sendMessage();
        //} else {
        //  echoNewLineToTextArea(maWindow);
        //}
        return false;
      default :
        var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
        if (currentRoomData !== null && currentRoomData.type === 'chat') {
          var jid = currentRoomData.id;
          if (ASC.TMTalk.connectionManager.connected()) {
            if (composingMessages.hasOwnProperty(jid)) {
              clearTimeout(composingMessages[jid].handlerTimeout);
              composingMessages[jid].handlerTimeout = setTimeout(pausedMessageCallback(jid), pausedMesssagesTimeout * 1000);
            } else {
              composingMessages[jid] = {
                jid             : jid,
                handlerTimeout  : null
              };
              ASC.TMTalk.connectionManager.composingMessage(jid);
              composingMessages[jid].handlerTimeout = setTimeout(pausedMessageCallback(jid), pausedMesssagesTimeout * 1000);
            }
          }
        }
        break;
    }
  };

  var showMessage = function (cid, type, message) {
    switch (type.toLowerCase()) {
      case 'error' :
        var o = document.createElement('div');
        o.innerHTML = message.replace(/\n/g, '');
        ASC.TMTalk.messagesManager.sendMessageToChat(cid, translateSymbols(trim(getNodeContent(o))), true);
        break;
    }
  };

  var sendMessage = function (cid, type, message) {
    if (typeof cid === 'string') {
      switch (type.toLowerCase()) {
        case 'chat' :
          var o = document.createElement('div');
          o.innerHTML = message.replace(/\n/g, '');
          pausedMessage(cid);
          ASC.TMTalk.messagesManager.sendMessageToChat(cid, translateSymbols(trim(getNodeContent(o))));
          break;
        case 'mailing' :
          var o = document.createElement('div');
          o.innerHTML = message.replace(/\n/g, '');
          ASC.TMTalk.msManager.sendMessage(cid, translateSymbols(trim(getNodeContent(o))));
          break;
        case 'conference' :
          var o = document.createElement('div');
          o.innerHTML = message.replace(/\n/g, '');
          ASC.TMTalk.messagesManager.sendMessageToConference(cid, translateSymbols(trim(getNodeContent(o))));
          break;
      }
      return undefined;
    }

    if (taWindow) {
      var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
      if (currentRoomData !== null) {
        switch (currentRoomData.type) {
          case 'chat' :
            if (ASC.TMTalk.connectionManager.connected()) {
              var o = document.createElement('div');
              o.innerHTML = getTextareaContent(taWindow);
              pausedMessage(currentRoomData.id);
              ASC.TMTalk.messagesManager.sendMessageToChat(currentRoomData.id, translateSymbols(trim(getNodeContent(o))));

              clearTextarea(taWindow);
            }
            break;
          case 'mailing' :
            if (ASC.TMTalk.connectionManager.connected() && ASC.TMTalk.msManager.getContacts(currentRoomData.id).length > 0) {
              var o = document.createElement('div');
              o.innerHTML = getTextareaContent(taWindow);
              ASC.TMTalk.msManager.sendMessage(currentRoomData.id, translateSymbols(trim(getNodeContent(o))));

              clearTextarea(taWindow);
            }
            break;
          case 'conference' :
            if (ASC.TMTalk.connectionManager.connected()) {
              var o = document.createElement('div');
              o.innerHTML = getTextareaContent(taWindow);
              ASC.TMTalk.messagesManager.sendMessageToConference(currentRoomData.id, translateSymbols(trim(getNodeContent(o))));

              clearTextarea(taWindow);
            }
            break;
        }
      }
    }
  };

  var onClientConnected = function () {
    ASC.TMTalk.dom.addClass('talkMeseditorContainer', 'connected');
  };

  var onClientDisconnected = function () {
    for (var fld in composingMessages) {
      if (composingMessages.hasOwnProperty(fld)) {
        delete composingMessages[fld];
      }
    }
    ASC.TMTalk.dom.removeClass('talkMeseditorContainer', 'connected');
  };

  var onClientConnectingFailed = function () {
    for (var fld in composingMessages) {
      if (composingMessages.hasOwnProperty(fld)) {
        delete composingMessages[fld];
      }
    }
    ASC.TMTalk.dom.removeClass('talkMeseditorContainer', 'connected');
  };

  var onAddContact = function (listId, contactjid) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (listId === currentRoomData.id) {
      ASC.TMTalk.dom.removeClass('talkMeseditorContainer', 'unavailable');
      ASC.TMTalk.dom.removeClass(document.body, 'disable-fileuploadinput');
    }
  };

  var onRemoveContact = function (listId, contactjid) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (listId === currentRoomData.id) {
      if (ASC.TMTalk.msManager.getContacts(listId).length === 0) {
        ASC.TMTalk.dom.addClass('talkMeseditorContainer', 'unavailable');
        ASC.TMTalk.dom.addClass(document.body, 'disable-fileuploadinput');
      } else {
        ASC.TMTalk.dom.removeClass('talkMeseditorContainer', 'unavailable');
        ASC.TMTalk.dom.removeClass(document.body, 'disable-fileuploadinput');
      }
    }
  };

  var onCreateRoom = function (roomId, data) {
    var
      textareasContainer = document.getElementById('talkTextareaContainer'),
      nodes = null,
      newTextarea = null,
      defaultTextarea = null;

    nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textarea default', 'li');
    if (nodes.length > 0) {
      defaultTextarea = nodes[0];
    }

    if (!defaultTextarea) {
      throw 'no templates';
    }

    newTextarea = defaultTextarea.cloneNode(true);
    newTextarea.className = newTextarea.className.replace(/\s*default\s*/, ' ').replace(/^\s+|\s+$/g, '');

    newTextarea.setAttribute('data-roomid', roomId);

    switch (data.type) {
      case 'chat' :
        break;
      case 'conference' :
        newTextarea.className += ' conference';
        break;
      case 'mailing' :
        break;
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textareas', 'ul');
    if (nodes.length > 0) {
      nodes[0].appendChild(newTextarea);
    }

    nodes = newTextarea.getElementsByTagName('iframe');
    if (nodes.length > 0) {
      if ($.browser.mozilla && simpleEditor === false) {
        setTimeout((function (id) {
          return function () {
            ASC.TMTalk.meseditorContainer.createTextarea(id);
          };
        })(roomId), 100);
      } else {
        ASC.TMTalk.meseditorContainer.createTextarea(nodes[0]);
      }
    }
  };

  var onOpenRoom = function (roomId, data, inBackground) {
    var
      textareasContainer = document.getElementById('talkTextareaContainer'),
      currentTextarea = null,
      node = null,
      nodes = null,
      nodesInd = 0;
    nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textarea', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      node = nodes[nodesInd];
      if (node.getAttribute('data-roomid') === roomId) {
        currentTextarea = node;
        if (inBackground !== true) {
          ASC.TMTalk.dom.addClass(node, 'current');
        }
      } else {
        if (inBackground !== true) {
          ASC.TMTalk.dom.removeClass(node, 'current');
        }
      }
    }

    var meseditorContainer = document.getElementById('talkMeseditorContainer');
    ASC.TMTalk.dom.removeClass(meseditorContainer, 'disabled');
    nodes = ASC.TMTalk.dom.getElementsByClassName(document.body, 'fileuploadinput', 'input');
    if (nodes.length > 0) {
      ASC.TMTalk.dom.removeClass(nodes[0], 'disabled');
    }

    if (inBackground !== true) {
      taWindow = null;
      if (currentTextarea !== null) {
        nodes = currentTextarea.getElementsByTagName('textarea');
        if (nodes.length > 0) {
          taWindow = nodes[0];
        }
        if (taWindow === null) {
          nodes = currentTextarea.getElementsByTagName('iframe');
          if (nodes.length > 0) {
            taWindow = nodes[0].contentWindow;
          }
        }
        if (!taWindow) {
          throw 'can\'t get textarea';
        }
        // my street magic. HOLY FUCKING SHIT FUCKS IE9.
        if (nodes.length > 0) {
          nodes[0].style.position = 'relative';
          nodes[0].style.position = 'absolute';
        }
      }

      switch (data.type) {
        case 'chat' :
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'conference');
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'mailing');
          ASC.TMTalk.dom.addClass(meseditorContainer, 'chat');

          ASC.TMTalk.dom.removeClass(meseditorContainer, 'unavailable');
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'owner');

          ASC.TMTalk.dom.removeClass(document.body, 'disable-fileuploadinput');
          break;
        case 'mailing' :
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'chat');
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'conference');
          ASC.TMTalk.dom.addClass(meseditorContainer, 'mailing');

          ASC.TMTalk.dom.removeClass(meseditorContainer, 'owner');
          if (ASC.TMTalk.msManager.getContacts(data.id).length === 0) {
            ASC.TMTalk.dom.addClass(meseditorContainer, 'unavailable');
            ASC.TMTalk.dom.addClass(document.body, 'disable-fileuploadinput');
          } else {
            ASC.TMTalk.dom.removeClass(meseditorContainer, 'unavailable');
            ASC.TMTalk.dom.removeClass(document.body, 'disable-fileuploadinput');
          }
          break;
        case 'conference' :
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'chat');
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'mailing');
          ASC.TMTalk.dom.addClass(meseditorContainer, 'conference');

          if (data.affiliation === 'owner') {
            ASC.TMTalk.dom.addClass(meseditorContainer, 'owner');
          } else {
            ASC.TMTalk.dom.removeClass(meseditorContainer, 'owner');
          }
          ASC.TMTalk.dom.removeClass(meseditorContainer, 'unavailable');

          ASC.TMTalk.dom.removeClass(document.body, 'disable-fileuploadinput');
          break;
      }
      ASC.TMTalk.dom.removeClass(meseditorContainer, 'history');
      if (TMTalk.properties.focused) {
        if ($.browser.mozilla && simpleEditor === false) {
          setTimeout(ASC.TMTalk.meseditorContainer.setCursor, 100);
        } else {
          ASC.TMTalk.meseditorContainer.setCursor(taWindow);
        }
      }
    }
  };

  var onCloseRoom = function (roomId, data) {
    var
      textareasContainer = document.getElementById('talkTextareaContainer'),
      isCurrent = false,
      nodes = null,
      nodesInd = 0;
    nodes = ASC.TMTalk.dom.getElementsByClassName(textareasContainer, 'textarea', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-roomid') === roomId) {
        isCurrent = ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'current');
        nodes[nodesInd].parentNode.removeChild(nodes[nodesInd]);
        break;
      }
    }

    if (isCurrent === true) {
      var meseditorContainer = document.getElementById('talkMeseditorContainer');
      ASC.TMTalk.dom.addClass(meseditorContainer, 'disabled');
      if (meseditorContainer) {
        nodes = ASC.TMTalk.dom.getElementsByClassName(document.body, 'fileuploadinput', 'input');
        if (nodes.length > 0) {
          ASC.TMTalk.dom.addClass(nodes[0], 'disabled');
        }
      }

      ASC.TMTalk.dom.removeClass(meseditorContainer, 'chat');
      ASC.TMTalk.dom.removeClass(meseditorContainer, 'mailing');
      ASC.TMTalk.dom.removeClass(meseditorContainer, 'conference');

      ASC.TMTalk.dom.removeClass(meseditorContainer, 'owner');
      ASC.TMTalk.dom.removeClass(meseditorContainer, 'history');
      ASC.TMTalk.dom.removeClass(meseditorContainer, 'unavailable');

      ASC.TMTalk.dom.removeClass(document.body, 'disable-fileuploadinput');

      taWindow = null;
    }
  };

  var onOpenHistory = function (jid) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData.type === 'chat' && currentRoomData.id === jid) {
      ASC.TMTalk.dom.addClass('talkMeseditorContainer', 'history');
    }
  };

  var onCloseHistory = function (jid) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData.type === 'chat' && currentRoomData.id === jid) {
      ASC.TMTalk.dom.removeClass('talkMeseditorContainer', 'history');
      if ($.browser.mozilla && simpleEditor === false) {
        setTimeout(ASC.TMTalk.meseditorContainer.setCursor, 100);
      } else {
        ASC.TMTalk.meseditorContainer.setCursor(taWindow);
      }
    }
  };

  var onSendFileStart = function () {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData !== null) {
      jQuery('#talkMeseditorToolbarContainer div.button-container.send-file').addClass('sending');
      ASC.TMTalk.properties.item('dstcontact', {cid : currentRoomData.id, type : currentRoomData.type});
    }
  }

  var onSendFileComplete = function (response) {
    jQuery('#talkMeseditorToolbarContainer div.button-container.send-file').removeClass('sending');
    if (response.Success === true) {
      var link = response.FileURL.substring(0, response.FileURL.lastIndexOf('/')) + '/' + encodeURIComponent(response.FileName);
      var dstcontact = ASC.TMTalk.properties.item('dstcontact');
      if (dstcontact && typeof dstcontact === 'object') {
        ASC.TMTalk.meseditorContainer.sendMessage(dstcontact.cid, dstcontact.type, ASC.TMTalk.stringFormat(ASC.TMTalk.Resources.sendFileMessage, link, response.Data));
        ASC.TMTalk.properties.item('dstcontact', null);
      }
    } else {
      var dstcontact = ASC.TMTalk.properties.item('dstcontact');
      if (dstcontact && typeof dstcontact === 'object') {
        ASC.TMTalk.meseditorContainer.showMessage(dstcontact.cid, 'error', response.Message);
      }
    }
  }

  return {
    init  : init,

    createTextarea  : createTextarea,
    insertSmile     : insertSmile,
    setCursor       : setCursor,
    setRange        : setRange,

    keyPress      : keyPress,

    sendMessage : sendMessage,
    showMessage : showMessage,

    onSendFileStart     : onSendFileStart,
    onSendFileComplete  : onSendFileComplete
  };
})(jQuery);

(function ($) {
  $(function () {
    if ($.browser.safari) {
      $('#talkMeseditorToolbarContainer div.toolbar:first div.button-container').not('.send-file').mousedown(function () {
        if (window.getSelection) {
          window.getSelection().removeAllRanges();
        }
        return false;
      });

      $('#talkSendMenu').mousedown(function () {
        if (window.getSelection) {
          window.getSelection().removeAllRanges();
        }
        return false;
      });
    }

    $(document).mousedown(!$.browser.msie ? null : function () {
      ASC.TMTalk.meseditorContainer.setRange();
    });

    if (ASC.TMTalk.properties.item('sndbyctrlentr') === undefined) {
      ASC.TMTalk.properties.item('sndbyctrlentr', '1', true);
    }

    if (ASC.TMTalk.properties.item('sndbyctrlentr') === '0') {
      $('#talkMeseditorToolbarContainer div.button-container.toggle-sendbutton:first').removeClass('send-by-ctrlenter');
    } else {
      $('#talkMeseditorToolbarContainer div.button-container.toggle-sendbutton:first').addClass('send-by-ctrlenter');
    }

    if ($.support.touch) {
      $('#talkMeseditorToolbarContainer div.button-container.send-file:first').addClass('not-available');
    }

    TMTalk.createFileUploader('talkFileUploader', ASC.TMTalk.meseditorContainer.onSendFileStart, ASC.TMTalk.meseditorContainer.onSendFileComplete);

    $('#talkMeseditorToolbarContainer div.button.create-massend:first').click(function () {
      $('#txtMailingName').val('');
      TMTalk.showDialog('create-mailing');
    });

    $('#talkMeseditorToolbarContainer div.button.create-conference:first').click(function () {
      $('#txtRoomName').val('');
      $('#cbxTemporaryRoom').attr('checked', true);
      TMTalk.showDialog('create-room');
    });

    $('#talkMeseditorToolbarContainer div.button.emotions:first').click(function () {
      var $container = null;
      
      if(($container = $('#talkMeseditorToolbarContainer div.button-container.emotions:first div.container:first')).is(':hidden')) {
        $container.show('fast', function () {
          $(document).one('click', function () {
            $('#talkMeseditorToolbarContainer div.button-container.emotions:first div.container:first').hide();
          });
        });
      }
    });

    $('#talkMeseditorToolbarContainer ul.smiles:first').click(function (evt) {
      var $target = $(evt.target);
      if ($target.hasClass('smile')) {
        var
          title = $target.attr('title'),
          smilesrc = $target.css('backgroundImage');
        title = typeof title === 'string' ? title : '';
        smilesrc = typeof smilesrc === 'string' ? smilesrc.replace(/^url|[\("'\)]+/g, '') : null;
        if (smilesrc) {
          ASC.TMTalk.meseditorContainer.insertSmile(smilesrc, title);
        }
      }
    });

    $('#talkMeseditorToolbarContainer div.button.history:first').click(function () {
      var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
      if (currentRoomData !== null && currentRoomData.type === 'chat') {
        var jid = currentRoomData.id;
        if (jid) {
          ASC.TMTalk.messagesManager.openHistory(jid);
        }
      }
    });

    $('#talkMeseditorToolbarContainer div.button.toggle-sendbutton:first').click(function () {
      ASC.TMTalk.properties.item('sndbyctrlentr', $('#talkMeseditorToolbarContainer div.button-container.toggle-sendbutton:first').toggleClass('send-by-ctrlenter').hasClass('send-by-ctrlenter') ? '1' : '0', true);
    });

    $('#talkMeseditorToolbarContainer div.button.remove-mailing:first').click(function () {
      var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
      if (currentRoomData !== null && currentRoomData.type === 'mailing') {
        var listId = currentRoomData.id;
        if (listId) {
          ASC.TMTalk.msManager.removeList(listId);
        }
      }
    });

    $('#talkMeseditorToolbarContainer div.button.remove-conference:first').click(function () {
      var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
      if (currentRoomData !== null && currentRoomData.type === 'conference' && currentRoomData.affiliation === 'owner') {
        var roomjid = currentRoomData.id;
        if (roomjid) {
          $('#hdnRemoveJid').val(roomjid);
          TMTalk.showDialog('remove-room', ASC.TMTalk.mucManager.getConferenceName(roomjid));
        }
      }
    });

    $('#talkSendMenu div.button.send-message:first').click(function () {
      ASC.TMTalk.meseditorContainer.sendMessage();
    });

    $('#talkDialogsContainer').click(function (evt) {
      var $target = $(evt.target);
      if ($target.hasClass('button') && $target.hasClass('create-room')) {
        var roomname = ASC.TMTalk.trim($('#txtRoomName').val());

        if (roomname) {
          if (ASC.TMTalk.mucManager.isValidName(roomname)) {
            ASC.TMTalk.mucManager.createRoom(roomname, $('#cbxTemporaryRoom').is(':checked'));
            TMTalk.hideDialog();
          } else {
            $('#txtRoomName').parents('div.textfield:first').addClass('invalid-field');
          }
        }
      } else if ($target.hasClass('button') && $target.hasClass('create-mailing')) {
        var mailingname = ASC.TMTalk.trim($('#txtMailingName').val());

        if (mailingname) {
          if (ASC.TMTalk.msManager.isValidName(mailingname)) {
            ASC.TMTalk.msManager.createList(mailingname);
            TMTalk.hideDialog();
          } else {
            $('#txtMailingName').parents('div.textfield:first').addClass('invalid-field');
          }
        }
      }
    });
  });
})(jQuery);
