﻿
if (!ASC.Controls.NavigationItem) {
  ASC.Controls.NavigationItem = {
    checkMessagesTimeout : 1000,
    currentValue : -1,
    handlerPath : '',
    init : function (path, timeout) {
      if (typeof path === 'string') {
        this.handlerPath = path;
      }
      if (isFinite(+timeout)) {
        this.checkMessagesTimeout = +timeout * 1000;
        setTimeout(this.checkNewMessages, 0);
        if (+timeout > 0) {
          setInterval(this.checkNewMessages, this.checkMessagesTimeout);
        }
      }
    },
    getNodeContent : (function () {
      if (typeof document.createElement('div').textContent === 'string') {
        return function (o) {
          if (typeof o === 'string') {
            o = document.getElementById(o);
          }
          if (!o) {
            return '';
          }
          return o.textContent;
        };
      } else if (typeof document.createElement('div').text === 'string') {
        return function (o) {
          if (typeof o === 'string') {
            o = document.getElementById(o);
          }
          if (!o) {
            return '';
          }
          return o.text;
        };
      }
      return function (o) {
        if (typeof o === 'string') {
          o = document.getElementById(o);
        }
        if (!o) {
          return '';
        }
        result = '';
        var childrens = o.childNodes;
        if (!childrens) {
          return result;
        }
        for (var i = 0, n = childrens.length; i < n; i++) {
          var child = childrens.item(i);
          switch (child.nodeType) {
            case 1 :
            case 5 :
              result += arguments.callee(child);
              break;
            case 3 :
            case 2 :
            case 4 :
              result += child.nodeValue;
              break;
          }
        }
        return result;
      };
    })(),
    updateValue : function (value) {
      if (this.currentValue === value) {
        return undefined;
      }
      this.currentValue = value;
      var o = document.getElementById('talkMsgCount');
      if (o) {
        o.innerHTML = value;
        jQuery('#talkMsgLabel')[value > 0 ? 'addClass' : 'removeClass']('message-blink');
      }
    },
    getResponce : function (xmlHttpRequest, textStatus) {
      if (textStatus === 'success') {
        var
          nodes = null,
          responseXML = null;

        responseXML = xmlHttpRequest.responseXML;
        nodes = xmlHttpRequest.responseXML.getElementsByTagName('value');
        if (nodes.length > 0) {
          value = ASC.Controls.NavigationItem.getNodeContent(nodes[0]);
          value = isFinite(+value) ? +value : null;
          if (value !== null) {
            ASC.Controls.NavigationItem.updateValue(value);
          }
        }
      }
    },
    checkNewMessages : function () {
      if (jQuery('#talkMsgLabel').length > 0) {
        jQuery.ajax({
          async       : true,
          type        : 'get',
          contentType : 'text/xml',
          cache       : false,
          url         : ASC.Controls.NavigationItem.handlerPath,
          complete    : ASC.Controls.NavigationItem.getResponce
        });
      }
    }
  };
}

if (!ASC.Controls.JabberClient) {
  ASC.Controls.JabberClient = {
    username        : null,
    jid             : null,
    winName         : 'ASCJabberClient' + location.hostname,
    helperId        : 'JabberClientHelper-' + Math.floor(Math.random() * 1000000),
    params          : 'width=1000,height=600,status=no,toolbar=no,menubar=no,resizable=yes,scrollbars=no',
    pathCmdHandler  : '',
    pathWebTalk     : '',
    init : function (name, talkpath, cmdpath) {
      if (typeof name === 'string' && name.length > 0) {
        this.username = name;
      }
      if (typeof talkpath === 'string' && talkpath.length > 0) {
        this.pathWebTalk = talkpath;
      }
      if (typeof cmdpath === 'string' && cmdpath.length > 0) {
        this.pathCmdHandler = cmdpath;
      }
      var a = this.winName.match(/\w+/g);
      this.winName = a ? a.join('') : this.winName;
    },
    open : function(jid) {
      var
        hWnd = null,
        isExist = false;

      try {hWnd = window.open('', this.winName, this.params)} catch (err) {}

      try {
        isExist = typeof hWnd.ASC === 'undefined' ? false : true;
      } catch (err) {
        isExist = true;
      }

      if (!isExist && typeof this.pathWebTalk) {
        hWnd = window.open(this.pathWebTalk + (typeof jid === 'string' ? '#' + jid : ''), this.winName, this.params);
        isExist = true;
      }

      if (!isExist) {
        return undefined;
      }

      try {hWnd.focus()} catch (err) {}

      try {ASC.Controls.NavigationItem.updateValue(0)} catch (err) {}

      if (typeof jid === 'string' && jid.length > 0) {
        this.openContact(jid);
      }
    },
    openContact : function (jid) {
      if (typeof this.username === 'string' && this.username.length > 0) {
        var name = jid.substring(0, jid.indexOf('@'));
        name = name || jid;
        this.jid = jid;

        jQuery.ajax({
          async       : true,
          type        : 'get',
          contentType : 'text/xml',
          cache       : false,
          url         : this.pathCmdHandler,
          data        : {from : name, to : this.username},
          complete    : null
        });
      }
    }
  };
}

if (!ASC.Controls.SoundManager) {
  ASC.Controls.SoundManager = {
    isEnabled : null,
    soundContainer : null,
    soundPath : null,
    installPath : null,
    init : function (sound, install) {
      if (typeof sound === 'string') {
        this.soundPath = sound;
      }
      if (typeof install === 'string') {
        this.installPath = install;
      }
    },
    onLoad : function (o) {
      if (typeof o === 'string') {
        o = document.getElementById(o);
      }
      if (!o || typeof o === 'undefined') {
        return null;
      }
      this.soundContainer = o;
      if (this.isEnabled === null) {
        this.isEnabled = true;
      }
      this.play('incmsg');
    },
    alarm : function () {
      if (this.soundContainer === null && this.swfPath !== null) {
        if (this.soundPath !== null) {
          try {
            var soundContainerId = 'talkSoundContainer-' + Math.floor(Math.random() * 1000000);
            var o = document.createElement('div');
            o.setAttribute('id', soundContainerId);
            document.body.appendChild(o);
            swfobject.embedSWF(
              this.soundPath,
              soundContainerId,
              1,
              1,
              '9.0.0',
              this.installPath,
              {apiInit : function (id) {ASC.Controls.SoundManager.onLoad(id);}, apiId : soundContainerId},
              {allowScriptAccess : 'always', wmode : 'transparent'},
              {styleclass : 'sound-container', wmode: 'transparent'}
            );
          } catch (err) {
          }
        }
        this.soundContainer = undefined;
        return undefined;
      }
      this.play('incmsg');
    },
    play : function (soundname) {
      if (this.soundContainer !== null && this.isEnabled === true && typeof soundname === 'string') {
        try { this.soundContainer.playSound(soundname) } catch (err) {}
      }
    }
  };
}
