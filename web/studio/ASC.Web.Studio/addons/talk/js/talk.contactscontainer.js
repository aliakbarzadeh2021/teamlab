﻿
window.ASC = window.ASC || {};

window.ASC.TMTalk = window.ASC.TMTalk || {};

window.ASC.TMTalk.contactsContainer = (function ($) {
  var
    isInit = false,
    lastFilterValue = '',
    roomIds = {},
    clScroller = null,
    reconnectTimeout = 10,
    openingconferenceTimeout = 20,
    grouphashesSeparator = '##',
    groupvaluesSeparator = '#',
    unreadMessages = [],
    filterField = null,
    toolbarContainer = null,
    defaultGroup = null,
    defaultContact = null,
    groupsContainer = null,
    contactsContainer = null,
    contactlistContainer = null,
    constants = {
      propertyStatusId : 'sid'
    },
    conferenceIds = {};

  var translateSymbols = function (str, toText) {
    var
      symbols = [
        ['&lt;',  '<'],
        ['&gt;',  '>'],
        ['&and;', '\\^'],
        ['&sim;', '~'],
        ['&amp;', '&']
      ];

    if (typeof str !== 'string' || str.length === 0) {
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
  };

  var setNodes = function () {
    if (contactlistContainer === null) {
      var nodes = null;
      contactlistContainer = document.getElementById('talkContactsContainer');
      nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'grouplist', 'ul');
      groupsContainer = nodes.length > 0 ? nodes[0] : null;
      nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contactlist', 'ul');
      contactsContainer = nodes.length > 0 ? nodes[0] : null;
      filterField = document.getElementById('filterValue');
      toolbarContainer = document.getElementById('talkContactToolbarContainer');
    }
  };

  var createGroup = function (name, type, allcontacts) {
    var
      nodes = null,
      groupnode = null;

    if (defaultGroup === null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group default', 'li');
      defaultGroup = nodes.length > 0 ? nodes[0] : null;
      defaultGroup = defaultGroup.cloneNode(true);
      defaultGroup.className = defaultGroup.className.replace(/\s*default\s*/, ' ').replace(/^\s+|\s+$/g, '');
    }

    groupnode = defaultGroup.cloneNode(true);
    if (type !== null) {
      groupnode.className += ' ' + type;
    }
    groupnode.setAttribute('data-name', name);

    allcontacts = isFinite(+allcontacts) ? +allcontacts : 0;
    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'size', 'div');
    var sizenode = nodes.length > 0 ? nodes[0] : null;
    if (sizenode !== null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'all', 'span');
      if (nodes.length > 0) {
        nodes[0].innerHTML = allcontacts;
      }
      nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'online', 'span');
      if (nodes.length > 0) {
        nodes[0].innerHTML = 0;
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'title', 'div');
    if (nodes.length > 0) {
      nodes[0].className += ' group-title';
      nodes[0].innerHTML = translateSymbols(name, false);
      nodes[0].setAttribute('title', name);
    }

    return groupnode;
  };

  var createContact = function (name, type, jid) {
    var
      nodes = null,
      contactnode = null;

    if (defaultContact === null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact default', 'li');
      defaultContact = nodes.length > 0 ? nodes[0] : null;
      defaultContact = defaultContact.cloneNode(true);
      defaultContact.className = defaultContact.className.replace(/\s*default\s*/, ' ').replace(/^\s+|\s+$/g, '');
    }

    contactnode = defaultContact.cloneNode(true);

    contactnode.setAttribute('data-cid', jid);

    if (jid === ASC.TMTalk.connectionManager.getDomain()) {
      ASC.TMTalk.dom.addClass(contactnode, 'master');
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactnode, 'title', 'div');
    if (nodes.length > 0) {
      nodes[0].className += ' ' + type + '-title';
      nodes[0].innerHTML = translateSymbols(name, false);
      nodes[0].setAttribute('title', name);
    }

    return contactnode;
  };

  var insertNode = function (item, items) {
    var
      title = '',
      nodes = null;

    nodes = ASC.TMTalk.dom.getElementsByClassName(item, 'title', 'div');
    if (nodes.length > 0) {
      title = nodes[0].innerHTML;
    }

    var
      el = null,
      itemsInd = items.length;
    while (itemsInd--) {
      el = items[itemsInd];
      if (ASC.TMTalk.dom.hasClass(el, 'default') || ASC.TMTalk.dom.hasClass(el, 'master')) {
        continue;
      }
      nodes = ASC.TMTalk.dom.getElementsByClassName(el, 'title', 'div');
      if (nodes.length > 0 && title > nodes[0].innerHTML) {
        item.parentNode.insertBefore(item, ASC.TMTalk.dom.nextElementSibling(el));
        break;
      }
    }

    if (itemsInd === -1) {
      for (var i = 0, n = items.length; i < n; i++) {
        if (!ASC.TMTalk.dom.hasClass(items[i], 'default') && !ASC.TMTalk.dom.hasClass(items[i], 'master') && item !== items[i]) {
          item.parentNode.insertBefore(item, items[i]);
          break;
        }
      }
    }
  };

  var unserializeContactlistHashes = function (str) {
    var
      value = null,
      hashes = null,
      hashesInd = 0;
    if (typeof str !== 'string') {
      return [];
    }
    hashes = str.split(grouphashesSeparator);
    hashesInd = hashes.length;
    while (hashesInd--) {
      value = hashes[hashesInd].split(groupvaluesSeparator);
      hashes[hashesInd] = {hash : value[0], isOpen : value[1]};
    }
    return hashes;
  };

  var readMessages = function (jid) {
    var
      nodes = null,
      nodesInd = 0;

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-cid') === jid) {
        ASC.TMTalk.dom.removeClass(nodes[nodesInd], 'new-message');
      }
    }
    var unreadmessagesInd = unreadMessages.length;
    while (unreadmessagesInd--) {
      if (unreadMessages[unreadmessagesInd].cid === jid) {
        unreadMessages.splice(unreadmessagesInd, 1);
      }
    }
    nodes = ASC.TMTalk.dom.getElementsByClassName(toolbarContainer, 'unread-messages', 'div');
    if (nodes.length > 0) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'read-new-message', 'div');
      if (nodes.length > 0) {
        nodes[0].innerHTML = unreadMessages.length;
      }
    }
    if (unreadMessages.length === 0) {
      ASC.TMTalk.dom.removeClass(toolbarContainer, 'unread-messages');
      setTimeout(function () {ASC.TMTalk.indicator.stop()}, 300);
    }
  };

  var getOpeningConferenceCallback = function (cid) {
    return function () {
      var
        contacts = null,
        contactsInd = 0;

      contacts = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact', 'li');
      contactsInd = contacts.length;
      while (contactsInd--) {
        if (contacts[contactsInd].getAttribute('data-cid') === cid) {
          ASC.TMTalk.dom.removeClass(contacts[contactsInd], 'opening');
          break;
        }
      }
    };
  };

  var init = function () {
    if (isInit === true) {
      return undefined;
    }
    isInit = true;
    // TODO
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.connecting, onClientConnecting);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.connected, onClientConnected);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.disconnecting, onClientDisconnecting);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.disconnected, onClientDisconnected);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.connectingFailed, onClientConnectingFailed);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.updateStatus, onUpdateStatus);
    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.retrievesCommand, onRetrievesCommand);

    ASC.TMTalk.contactsManager.bind(ASC.TMTalk.contactsManager.events.updateContacts, onUpdateContacts);
    ASC.TMTalk.contactsManager.bind(ASC.TMTalk.contactsManager.events.comeContact, onComeContact);
    ASC.TMTalk.contactsManager.bind(ASC.TMTalk.contactsManager.events.leftContact, onLeftContact);
    ASC.TMTalk.contactsManager.bind(ASC.TMTalk.contactsManager.events.addContact, onAddContact);
    ASC.TMTalk.contactsManager.bind(ASC.TMTalk.contactsManager.events.removeContact, onRemoveContact);

    ASC.TMTalk.msManager.bind(ASC.TMTalk.msManager.events.getLists, onGetLists);
    ASC.TMTalk.msManager.bind(ASC.TMTalk.msManager.events.createList, onCreateList);
    ASC.TMTalk.msManager.bind(ASC.TMTalk.msManager.events.removeList, onRemoveList);
    ASC.TMTalk.msManager.bind(ASC.TMTalk.msManager.events.openList, onOpenList);

    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.getRooms, onGetRooms);
    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.createRoom, onCreateConference);
    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.destroyRoom, onDestroyConference);
    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.openRoom, onOpenConference);
    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.closeRoom, onCloseConference);
    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.updateRoom, onUpdateConference);
    ASC.TMTalk.mucManager.bind(ASC.TMTalk.mucManager.events.acceptInvite, onAcceptInvite);

    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.createRoom, onCreateRoom);
    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.openRoom, onOpenRoom);
    ASC.TMTalk.roomsManager.bind(ASC.TMTalk.roomsManager.events.closeRoom, onCloseRoom);

    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.recvMessageFromConference, onRecvMessageFromConferencePlaySound);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.recvMessageFromConference, onRecvMessageFromConference);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.sentMessageToChat, onSentMessage);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.recvMessageFromChat, onRecvMessage);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.recvMessageFromChat, onRecvMessageFromChat);
    ASC.TMTalk.messagesManager.bind(ASC.TMTalk.messagesManager.events.recvOfflineMessagesFromChat, onRecvMessageFromChat);

    ASC.TMTalk.indicator.bind(ASC.TMTalk.indicator.events.start, onStartIndicator);
    ASC.TMTalk.indicator.bind(ASC.TMTalk.indicator.events.show, onShowIndicator);
    ASC.TMTalk.indicator.bind(ASC.TMTalk.indicator.events.stop, onStopIndicator);

    ASC.TMTalk.properties.bind(ASC.TMTalk.properties.events.updateProperty, onUpdateProperty);

    TMTalk.bind(TMTalk.events.pageFocus, onPageFocus);
  };

  var getContactlistHash = function () {
    var
      nodes = null,
      group = null,
      groups = null,
      groupsInd = 0,
      hashes = [];
    groups = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group', 'li');
    groupsInd = groups.length;
    while (groupsInd--) {
      group = groups[groupsInd];
      if (ASC.TMTalk.dom.hasClass(group, 'default')) {
        continue;
      }
      hashes.unshift(ASC.TMTalk.getHash(group.getAttribute('data-name')) + '' + groupvaluesSeparator + (ASC.TMTalk.dom.hasClass(group, 'open') ? '1' : '0'));
    }
    return hashes.join(grouphashesSeparator);
  };

  var getContactOffsetTop = function (contact, lastclass) {
    var
      node = contact,
      offsettop = node.offsetTop;
    while ((node = node.offsetParent) !== null && !ASC.TMTalk.dom.hasClass(node, lastclass)) {
      offsettop += node.offsetTop;
    }
    return offsettop;
  }

  var resetContactlist = function () {
    var
      nodes = null,
      nodesInd = 0,
      group = null,
      groups = null,
      groupsInd = 0,
      validcontacts = null;

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      ASC.TMTalk.dom.removeClass(nodes[nodesInd], 'checked-by-filter');
    }
    groups = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group', 'li');
    groupsInd = groups.length;
    while (groupsInd--) {
      group = groups[groupsInd];
      ASC.TMTalk.dom.removeClass(group, 'checked-by-filter');
      validcontacts = ASC.TMTalk.dom.getElementsByClassName(group, 'contact online', 'li').length;
      nodes = ASC.TMTalk.dom.getElementsByClassName(group, 'online', 'span');
      if (nodes.length > 0) {
        nodes[0].innerHTML = validcontacts;
      }
      if (validcontacts > 0) {
        ASC.TMTalk.dom.removeClass(group, 'empty');
      }
    }
    lastFilterValue = '';
  };

  var filteringContactlist = function (evt) {
    var resetSelected = false;

    if (typeof evt !== 'undefined') {
      switch (evt.keyCode) {
        case 13 :
          var container = null;
          //if ($('#talkSidebarContainer').hasClass('grouplist')) {
          //  container = groupsContainer;
          //} else if ($('#talkSidebarContainer').hasClass('contactlist')) {
          //  container = contactsContainer;
          //}
          container = contactsContainer;
          if (container !== null) {
            var
              nodes = null,
              selcontact = null;
            nodes = ASC.TMTalk.dom.getElementsByClassName(container, 'contact selected', 'li');
            if (nodes.length > 0) {
              selcontact = nodes[0];
              nodes = ASC.TMTalk.dom.getElementsByClassName(selcontact, 'title', 'div');
              if (nodes.length > 0) {
                if (ASC.TMTalk.dom.hasClass(nodes[0], 'room-title')) {
                  var jid = selcontact.getAttribute('data-cid');
                  if (jid) {
                    ASC.TMTalk.dom.removeClass(selcontact, 'selected');
                    ASC.TMTalk.contactsContainer.openConference(jid);
                  }
                }
                if (ASC.TMTalk.dom.hasClass(nodes[0], 'contact-title')) {
                  var jid = selcontact.getAttribute('data-cid');
                  if (jid) {
                    ASC.TMTalk.dom.removeClass(selcontact, 'selected');
                    ASC.TMTalk.contactsContainer.openRoom(jid);
                  }
                }
              }
            }
          }
          return undefined;
        case 27 :
          if (filterField !== null) {
            filterField.value = '';
          }
          ASC.TMTalk.properties.item('enblfltr', '0');
          return undefined;
        case 32 :
          return undefined;
        case 38 :
          var classname = '', container = null;
          //if ($('#talkSidebarContainer').hasClass('grouplist')) {
          //  classname = 'grouplist';
          //  container = groupsContainer;
          //} else if ($('#talkSidebarContainer').hasClass('contactlist')) {
          //  classname = 'contactlist';
          //  container = contactsContainer;
          //}
          classname = 'contactlist';
          container = contactsContainer;

         if (container !== null) {
            var
              nodes = null,
              nodesInd = 0,
              selcontact = null,
              nextcontact = null;
            nodes = ASC.TMTalk.dom.getElementsByClassName(container, 'contact', 'li');
            nodesInd = nodes.length;
            while (nodesInd--) {
              if (ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'selected')) {
                selcontact = nodes[nodesInd];
                break;
              }
            }
            nodesInd = nodesInd === -1 ? 0 : nodesInd;
            while (nodesInd--) {
              if (nodes[nodesInd].offsetHeight !== 0 && ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'checked-by-filter') && !ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'default')) {
                nextcontact = nodes[nodesInd];
                ASC.TMTalk.dom.removeClass(selcontact, 'selected');
                ASC.TMTalk.dom.addClass(nextcontact, 'selected');
                break;
              }
            }
            // TODO
            if (nextcontact !== null && classname !== '') {
              var contactOffsetTop = getContactOffsetTop(nextcontact, classname);

              if (container.scrollTop > contactOffsetTop) {
                container.scrollTop = contactOffsetTop;
              }

              if (container.scrollTop + container.offsetHeight < contactOffsetTop + nextcontact.offsetHeight) {
                container.scrollTop = contactOffsetTop + nextcontact.offsetHeight - container.offsetHeight;
              }
            }
          }
          return undefined;
        case 40 :
          var classname = '', container = null;
          //if ($('#talkSidebarContainer').hasClass('grouplist')) {
          //  classname = 'grouplist';
          //  container = groupsContainer;
          //} else if ($('#talkSidebarContainer').hasClass('contactlist')) {
          //  classname = 'contactlist';
          //  container = contactsContainer;
          //}
          classname = 'contactlist';
          container = contactsContainer;

          if (container !== null) {
            var
              nodes = null,
              nodesInd = 0,
              selcontact = null,
              nextcontact = null;
            nodes = ASC.TMTalk.dom.getElementsByClassName(container, 'contact', 'li');
            nodesInd = nodes.length;
            while (nodesInd--) {
              if (ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'selected')) {
                selcontact = nodes[nodesInd];
                break;
              }
            }
            nodesInd = nodesInd === -1 ? 0 : nodesInd + 1;
            for (var n = nodes.length; nodesInd < n; nodesInd++) {
              if (nodes[nodesInd].offsetHeight !== 0 && ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'checked-by-filter') && !ASC.TMTalk.dom.hasClass(nodes[nodesInd], 'default')) {
                nextcontact = nodes[nodesInd];
                ASC.TMTalk.dom.removeClass(selcontact, 'selected');
                ASC.TMTalk.dom.addClass(nextcontact, 'selected');
                break;
              }
            }
            // TODO:
            if (nextcontact !== null && classname !== '') {
              var contactOffsetTop = getContactOffsetTop(nextcontact, classname);

              if (container.scrollTop > contactOffsetTop) {
                container.scrollTop = contactOffsetTop;
              }

              if (container.scrollTop + container.offsetHeight < contactOffsetTop + nextcontact.offsetHeight) {
                container.scrollTop = contactOffsetTop + nextcontact.offsetHeight - container.offsetHeight;
              }
            }
          }
          return undefined;
        default :
          resetSelected = true;
          break;
      }
    }

    var
      nodes = null,
      group = null,
      groups = null,
      groupsInd = null,
      contact = null,
      contacts = null,
      container = null,
      contactsInd = 0,
      contactname = '',
      validcontacts = 0,
      filterValue = filterField.value;

    if (typeof filterValue !== 'string') {
      return undefined;
    }

    if ((filterValue = (filterValue.replace(/^\s+|\s+$/g, '')).toLowerCase()) === lastFilterValue && typeof evt !== 'undefined') {
      return undefined;
    }

    //container = contactlistContainer;
    container = contactsContainer;

    if (clScroller) {
      clScroller.scrollTo(0, 0, 0);
    }

    contacts = ASC.TMTalk.dom.getElementsByClassName(container, 'contact', 'li');
    contactsInd = contacts.length;
    while (contactsInd--) {
      contact = contacts[contactsInd];

      nodes = ASC.TMTalk.dom.getElementsByClassName(contact, 'title', 'div');
      contactname = nodes.length > 0 ? nodes[0].innerHTML : '';
      if (contactname === '') {
        continue;
      }

      if (contactname.toLowerCase().indexOf(filterValue) === -1) {
        ASC.TMTalk.dom.removeClass(contact, 'checked-by-filter');
        ASC.TMTalk.dom.removeClass(contact, 'selected');
      } else {
        ASC.TMTalk.dom.addClass(contact, 'checked-by-filter');
        if (resetSelected === true) {
          ASC.TMTalk.dom.removeClass(contact, 'selected');
        }
      }
    }

    //container = groupsContainer;

    //groups = ASC.TMTalk.dom.getElementsByClassName(container, 'group', 'li');
    //groupsInd = groups.length;
    //while (groupsInd--) {
    //  group = groups[groupsInd];
    //  if (ASC.TMTalk.dom.hasClass(group, 'default')) {
    //    continue;
    //  }
    //  validcontacts = ASC.TMTalk.dom.getElementsByClassName(group, 'contact checked-by-filter', 'li').length;
    //  if (validcontacts === 0) {
    //    ASC.TMTalk.dom.removeClass(group, 'checked-by-filter');
    //  } else {
    //    ASC.TMTalk.dom.addClass(group, 'checked-by-filter');
    //  }

    //  validcontacts = ASC.TMTalk.dom.getElementsByClassName(group, 'contact online checked-by-filter', 'li').length;
    //  nodes = ASC.TMTalk.dom.getElementsByClassName(group, 'online', 'span');
    //  if (nodes.length > 0) {
    //    nodes[0].innerHTML = validcontacts;
    //  }

    //  if (validcontacts === 0) {
    //    ASC.TMTalk.dom.addClass(group, 'empty');
    //  } else {
    //    ASC.TMTalk.dom.removeClass(group, 'empty');
    //  }
    //}

    lastFilterValue = filterValue;
  };

  var showFirstUnreadMessage = function () {
    if (unreadMessages.length === 0) {
      return undefined;
    }
    switch (unreadMessages[0].type.toLowerCase()) {
      case 'chat' :
        openRoom(unreadMessages[0].cid);
        break;
      case 'conference' :
        openConference(unreadMessages[0].cid);
        break;
    }
    
  };

  var onPageFocus = function () {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (currentRoomData !== null) {
      readMessages(currentRoomData.id);
    }
  };

  var onUpdateProperty = function (name, value, oldvalue) {
    switch (name) {
      case 'shwoffusrs' :
        //var
        //  nodes = null,
        //  nodesInd = 0;

        //nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact selected', 'li');
        //nodesInd = nodes.length;
        //while (nodesInd--) {
        //  ASC.TMTalk.dom.removeClass(nodes[nodesInd], 'selected');
        //}

        if (clScroller) {
          clScroller.refresh();
        }
        break;
      case 'shwgrps' :
        if (value === '1') {
          $('#talkSidebarContainer').removeClass('contactlist').addClass('grouplist');
        } else {
          $('#talkSidebarContainer').removeClass('grouplist').addClass('contactlist');
        }

        //var
        //  nodes = null,
        //  nodesInd = 0;

        //nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact selected', 'li');
        //nodesInd = nodes.length;
        //while (nodesInd--) {
        //  ASC.TMTalk.dom.removeClass(nodes[nodesInd], 'selected');
        //}

        break;
      case 'enblsnds' :
        if (value === '1') {
          ASC.TMTalk.sounds.enable();
        } else {
          ASC.TMTalk.sounds.disable();
        }
        break;
      case 'spnt' :
        if (value === '1') {
          $('#talkContactToolbarContainer div.button-container.toggle-notifications:first').removeClass('disabled');
        } else {
          $('#talkContactToolbarContainer div.button-container.toggle-notifications:first').addClass('disabled');
        }
        break;
      case 'enblfltr' :
        if (value === '1') {
          $('#talkSidebarContainer').addClass('enable-filter');
          $(window).resize();

          $('#filterValue').bind('keyup', ASC.TMTalk.contactsContainer.filteringContactlist);
          ASC.TMTalk.contactsContainer.filteringContactlist();

          filterField.focus();
        } else {
          $('#talkSidebarContainer').removeClass('enable-filter');
          $(window).resize();

          $('#filterValue').unbind('keyup', ASC.TMTalk.contactsContainer.filteringContactlist);
          ASC.TMTalk.contactsContainer.resetContactlist();

          window.focus();
        }

        var
          nodes = null,
          nodesInd = 0;

        nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact selected', 'li');
        nodesInd = nodes.length;
        while (nodesInd--) {
          ASC.TMTalk.dom.removeClass(nodes[nodesInd], 'selected');
        }

        break;
    }
  };

  var onShowIndicator = function () {
    var nodes = ASC.TMTalk.dom.getElementsByClassName(toolbarContainer, 'unread-messages', 'div');
    if (nodes.length > 0) {
      ASC.TMTalk.dom.toggleClass(nodes[0], 'blik');
    }
  };

  var onStartIndicator = function () {
    ASC.TMTalk.dom.addClass(toolbarContainer, 'unread-messages');
  };

  var onStopIndicator = function () {
    ASC.TMTalk.dom.removeClass(toolbarContainer, 'unread-messages');
  };

  var onSentMessage = function (jid) {
    ASC.TMTalk.sounds.play('sndmsg');
  };

  var onRecvMessage = function (jid) {
    ASC.TMTalk.sounds.play('incmsg');
  };

  var onRecvMessageFromConferencePlaySound = function (roomjid, name, displaydate, date, body, isMine) {
    if (isMine && date.getTime() > ASC.TMTalk.mucManager.getOpeningDate(roomjid).getTime()) {
      ASC.TMTalk.sounds.play('sndmsg');
    }
  };

  var onRecvMessageFromChat = function (jid) {
    if (!roomIds.hasOwnProperty(jid)) {
      var inBackground = false;
      for (var fld in roomIds) {
        inBackground = true;
        break;
      }
      openRoom(jid, inBackground);
    };

    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (TMTalk.properties.focused === false || currentRoomData !== null && currentRoomData.id !== jid) {
      var
        nodes = null,
        nodesInd = 0;
      nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact', 'li');
      nodesInd = nodes.length;
      while (nodesInd--) {
        if (nodes[nodesInd].getAttribute('data-cid') === jid) {
          ASC.TMTalk.dom.addClass(nodes[nodesInd], 'new-message');
        }
      }
      unreadMessages.push({cid : jid, type : 'chat'});
      nodes = ASC.TMTalk.dom.getElementsByClassName(toolbarContainer, 'unread-messages', 'div');
      if (nodes.length > 0) {
        nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'read-new-message', 'div');
        if (nodes.length > 0) {
          nodes[0].innerHTML = unreadMessages.length;
        }
      }
      if (unreadMessages.length > 0) {
        setTimeout(function () {ASC.TMTalk.indicator.start()}, 300);
      }
    }
  };

  var onRecvMessageFromConference = function (roomjid, name, displaydate, date, body, isMine) {
    var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
    if (TMTalk.properties.focused === false || currentRoomData !== null && currentRoomData.id !== roomjid) {
      var
        nodes = null,
        nodesInd = 0;
      nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact', 'li');
      nodesInd = nodes.length;
      while (nodesInd--) {
        if (nodes[nodesInd].getAttribute('data-cid') === roomjid) {
          ASC.TMTalk.dom.addClass(nodes[nodesInd], 'new-message');
        }
      }
      unreadMessages.push({cid : roomjid, type : 'conference'});
      nodes = ASC.TMTalk.dom.getElementsByClassName(toolbarContainer, 'unread-messages', 'div');
      if (nodes.length > 0) {
        nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'read-new-message', 'div');
        if (nodes.length > 0) {
          nodes[0].innerHTML = unreadMessages.length;
        }
      }
      if (unreadMessages.length > 0) {
        setTimeout(function () {ASC.TMTalk.indicator.start()}, 300);
      }
    }
  };

  var onAcceptInvite = function (roomjid, roomname, inviterjid) {
    var
      allcontacts = 0,
      nodes = null,
      nodesInd = 0,
      sizenode = null,
      groupnode = null,
      conferencenode = null,
      contactlistnode = null;

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group rooms', 'li');
    var groupnode = nodes.length > 0 ? nodes[0] : null;

    if (groupnode === null) {
      groupnode = createGroup(groupname, 'rooms', 1);
      nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group default', 'li');
      var defaultgroup = nodes.length > 0 ? nodes[0] : null;
      if (defaultgroup !== null) {
        groupsContainer.insertBefore(groupnode, ASC.TMTalk.dom.nextElementSibling(defaultgroup));
      } else {
        groupsContainer.appendChild(groupnode);
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-cid') === roomjid) {
        break;
      }
    }

    if (nodesInd === -1) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'size', 'div');
      sizenode = nodes.length > 0 ? nodes[0] : null;
      if (sizenode !== null) {
        nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'all', 'span');
        if (nodes.length > 0) {
          var allcontacts = nodes[0].innerHTML;
          allcontacts = isFinite(+allcontacts) ? +allcontacts : 0;
          nodes[0].innerHTML = ++allcontacts;
        }
      }

      nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contactlist', 'ul');
      contactlistnode = nodes.length > 0 ? nodes[0] : null;

      conferencenode = createContact(roomname, 'room', roomjid);
      contactlistnode.appendChild(conferencenode);

      nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contact offline', 'li');
      var contactlist = nodes.length > 0 ? nodes[0] : null;

      if (nodes.length > 0) {
        insertNode(conferencenode, nodes);
      }
    }

    openConference(roomjid);
  };

  var onCreateConference = function (roomjid, room) {
    var
      groupname = '',
      nodes = null,
      nodesInd = 0,
      sizenode = null,
      groupnode = null,
      conferencenode = null,
      contactlistnode = null;

    groupname = ASC.TMTalk.Resources.conferenceGroupName;

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group rooms', 'li');
    groupnode = nodes.length > 0 ? nodes[0] : null;

    if (groupnode === null) {
      groupnode = createGroup(groupname, 'rooms', 0);
      nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group default', 'li');
      var defaultgroup = nodes.length > 0 ? nodes[0] : null;
      if (defaultgroup !== null) {
        groupsContainer.insertBefore(groupnode, ASC.TMTalk.dom.nextElementSibling(defaultgroup));
      } else {
        groupsContainer.appendChild(groupnode);
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-cid') === room.jid) {
        return undefined;
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'size', 'div');
    sizenode = nodes.length > 0 ? nodes[0] : null;
    if (sizenode !== null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'all', 'span');
      if (nodes.length > 0) {
        var allcontacts = nodes[0].innerHTML;
        allcontacts = isFinite(+allcontacts) ? +allcontacts : 0;
        nodes[0].innerHTML = ++allcontacts;
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contactlist', 'ul');
    contactlistnode = nodes.length > 0 ? nodes[0] : null;

    conferencenode = createContact(room.name, 'room', room.jid);
    if (contactlistnode !== null) {
      contactlistnode.appendChild(conferencenode);
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contact offline', 'li');
    if (nodes.length > 0) {
      insertNode(conferencenode, nodes);
    }

    if (ASC.TMTalk.properties.item('enblfltr') === '1') {
      filteringContactlist();
    }
  };

  var onDestroyConference = function (roomjid, jid) {
    var
      nodes = null,
      nodesInd = 0,
      groupnode = null;

    readMessages(roomjid);

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-cid') === roomjid) {
        nodes[nodesInd].parentNode.removeChild(nodes[nodesInd]);
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group rooms', 'li');
    groupnode = nodes.length > 0 ? nodes[0] : null;

    if (groupnode !== null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'size', 'div');
      var sizenode = nodes.length > 0 ? nodes[0] : null;
      if (sizenode !== null) {
        nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'all', 'span');
        if (nodes.length > 0) {
          var allcontacts = nodes[0].innerHTML;
          allcontacts = isFinite(+allcontacts) ? +allcontacts : 0;
          if (allcontacts > 0) {
            nodes[0].innerHTML = --allcontacts;
          }
        }
      }
    }

    if (roomIds.hasOwnProperty(roomjid)) {
      ASC.TMTalk.roomsManager.closeRoom(roomIds[roomjid].id);
    }
  };

  var onOpenConference = function (roomjid, room) {
    if (!roomIds.hasOwnProperty(roomjid)) {
      ASC.TMTalk.roomsManager.createRoom({
        type : 'conference',
        id : roomjid,
        jid : room.ownjid,
        role : room.ownrole,
        affiliation : room.ownaffiliation,
        classname : room.ownrole + ' ' + room.ownaffiliation,
        minimized : false
      });
    }

    // after creating the room, we has the id
    if (roomIds.hasOwnProperty(roomjid)) {
      ASC.TMTalk.roomsManager.openRoom(roomIds[roomjid].id);
    }

    var
      wasOffline = false,
      classname = '',
      allcontacts = 0,
      onlinecontacts = 0,
      nodes = null,
      groupnode = null,
      conference = null,
      contacts = null,
      contactsInd = 0;

    contacts = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact', 'li');
    contactsInd = contacts.length;
    while (contactsInd--) {
      if (contacts[contactsInd].getAttribute('data-cid') !== roomjid) {
        continue;
      }
      conference = contacts[contactsInd];
      wasOffline = ASC.TMTalk.dom.hasClass(conference, 'offline');

      if (wasOffline === true) {
        groupnode = conference.parentNode.parentNode;
        if (groupnode && ASC.TMTalk.dom.hasClass(groupnode, 'group')) {
          ASC.TMTalk.dom.removeClass(groupnode, 'empty');
          nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'size', 'div');
          var sizenode = nodes.length > 0 ? nodes[0] : null;
          if (sizenode !== null) {
            nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'all', 'span');
            if (nodes.length > 0) {
              allcontacts = nodes[0].innerHTML;
              allcontacts = isFinite(+allcontacts) ? +allcontacts : allcontacts;
              nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'online', 'span');
              if (nodes.length > 0) {
                onlinecontacts = nodes[0].innerHTML;
                onlinecontacts = isFinite(+onlinecontacts) ? +onlinecontacts : onlinecontacts;
                if (onlinecontacts + 1 <= allcontacts) {
                  nodes[0].innerHTML = ++onlinecontacts;
                }
              }
            }
          }
        }
        nodes = ASC.TMTalk.dom.getElementsByClassName(conference.parentNode, 'contact online', 'li');
        if (nodes.length > 0) {
          insertNode(conference, nodes);
        } else {
          nodes = ASC.TMTalk.dom.getElementsByClassName(conference.parentNode, 'contact default', 'li');
          var defaultcontact = nodes.length > 0 ? nodes[0] : null;
          conference.parentNode.insertBefore(conference, ASC.TMTalk.dom.nextElementSibling(defaultcontact));
        }
      }

      classname = 'contact online';
      if (ASC.TMTalk.dom.hasClass(conference, 'master')) {
        classname += ' master';
      }
      if (ASC.TMTalk.dom.hasClass(conference, 'new-message')) {
        classname += ' new-message';
      }
      if (ASC.TMTalk.dom.hasClass(conference, 'selected')) {
        classname += ' selected';
      }
      if (ASC.TMTalk.dom.hasClass(conference, 'checked-by-filter')) {
        classname += ' checked-by-filter';
      }
      conference.className = classname;
    }
  };

  var onCloseConference = function (roomjid, jid) {
    if (roomIds.hasOwnProperty(roomjid)) {
      ASC.TMTalk.roomsManager.closeRoom(roomIds[roomjid].id);
    }

    var
      allcontacts = 0,
      onlinecontacts = 0,
      nodes = null,
      conference = null,
      contacts = null,
      contactsInd = 0,
      groupnode = null;

    contacts = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact', 'li');
    contactsInd = contacts.length;
    while (contactsInd--) {
      if (contacts[contactsInd].getAttribute('data-cid') !== roomjid) {
        continue;
      }
      conference = contacts[contactsInd];

      classname = 'contact offline';
      if (ASC.TMTalk.dom.hasClass(conference, 'master')) {
        classname += ' master';
      }
      if (ASC.TMTalk.dom.hasClass(conference, 'new-message')) {
        classname += ' new-message';
      }
      if (ASC.TMTalk.dom.hasClass(conference, 'selected')) {
        classname += ' selected';
      }
      if (ASC.TMTalk.dom.hasClass(conference, 'checked-by-filter')) {
        classname += ' checked-by-filter';
      }
      conference.className = classname;

      groupnode = conference.parentNode.parentNode;
      if (groupnode && ASC.TMTalk.dom.hasClass(groupnode, 'group')) {
        if (ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contact online').length === 0) {
          ASC.TMTalk.dom.addClass(groupnode, 'empty');
        }
        nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'size', 'div');
        var sizenode = nodes.length > 0 ? nodes[0] : null;
        if (sizenode !== null) {
          nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'all', 'span');
          if (nodes.length > 0) {
            allcontacts = nodes[0].innerHTML;
            allcontacts = isFinite(+allcontacts) ? +allcontacts : allcontacts;
            nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'online', 'span');
            if (nodes.length > 0) {
              onlinecontacts = nodes[0].innerHTML;
              onlinecontacts = isFinite(+onlinecontacts) ? +onlinecontacts : onlinecontacts;
              if (onlinecontacts - 1 >= 0) {
                nodes[0].innerHTML = --onlinecontacts;
              }
            }
          }
        }
      }

      nodes = ASC.TMTalk.dom.getElementsByClassName(conference.parentNode, 'contact offline', 'li');
      if (nodes.length > 0) {
        insertNode(conference, nodes);
      } else {
        conference.parentNode.appendChild(conference);
      }
    }
  };

  var onUpdateConference = function (roomjid, room) {
    var
      nodes = null,
      contacts = null,
      contactsInd = 0,
      conference = null;

    contacts = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact', 'li');
    contactsInd = contacts.length;
    while (contactsInd--) {
      if (contacts[contactsInd].getAttribute('data-cid') === roomjid) {
        conference = contacts[contactsInd];
        nodes = ASC.TMTalk.dom.getElementsByClassName(conference, 'room-title', 'div');
        if (nodes.length > 0) {
          nodes[0].innerHTML = room.name;
        }
      }
    }
  };

  var openConference = function (jid) {
    if (jid) {
      var
        contacts = null,
        contactsInd = 0;

      contacts = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact', 'li');
      contactsInd = contacts.length;
      while (contactsInd--) {
        if (contacts[contactsInd].getAttribute('data-cid') === jid && !ASC.TMTalk.dom.hasClass(contacts[contactsInd], 'opening')) {
          if (!ASC.TMTalk.mucManager.roomIsOpening(jid)) {
            ASC.TMTalk.dom.addClass(contacts[contactsInd], 'opening');
            setTimeout(getOpeningConferenceCallback(jid), openingconferenceTimeout * 1000);
          }
          ASC.TMTalk.mucManager.openRoom(jid);
          break;
        }
      }
    }
  };

  var openMailing = function (listId) {
    if (listId) {
      ASC.TMTalk.msManager.openList(listId);
    }
  };

  var openRoom = function (jid, inBackground) {
    if (!roomIds.hasOwnProperty(jid)) {
      ASC.TMTalk.roomsManager.createRoom({
        type : 'chat',
        id : jid
      });
    }
    // after creating the room, we has id
    if (roomIds.hasOwnProperty(jid)) {
      ASC.TMTalk.roomsManager.openRoom(roomIds[jid].id, inBackground === true);
    }
  };

  var openItem = function (cid, inBackground) {
    if (ASC.TMTalk.contactsManager.getContact(cid) !== null) {
      openRoom(cid, inBackground);
    }
  };

  var onCreateRoom = function (roomId, data) {
    switch (data.type) {
      case 'chat' :
        roomIds[data.id] = {
          id   : roomId,
          data  : data
        };
      case 'conference' :
        roomIds[data.id] = {
          id   : roomId,
          data  : data
        };
        break;
      case 'mailing' :
        roomIds[data.id] = {
          id   : roomId,
          data  : data
        };
        break;
    }
  };

  var onOpenRoom = function (roomId, data, inBackground) {
    var jid = null;
    for (var fld in roomIds) {
      if (roomIds.hasOwnProperty(fld)) {
        if (roomIds[fld].id === roomId) {
          jid = fld;
          break;
        }
      }
    }
    if (jid === null) {
      return undefined;
    }
    switch (data.type) {
      case 'chat' :
        if (inBackground !== true) {
          if (TMTalk.properties.focused) {
            readMessages(data.id);
          }

          var contact = ASC.TMTalk.contactsManager.getContact(jid);
          if (contact !== null) {
            ASC.TMTalk.tabsContainer.resetStatus('chat', {department : contact.group, message : contact.status, show : contact.show}, 1);
          }

          ASC.TMTalk.dom.removeClass(contactlistContainer, 'mailing');
          ASC.TMTalk.dom.removeClass(contactlistContainer, 'conference');
        }
        break;
      case 'conference' :
        if (inBackground !== true) {
          if (TMTalk.properties.focused) {
            readMessages(data.id);
          }

          if (data.minimized === false) {
            ASC.TMTalk.tabsContainer.setStatus('hint', {hint : ASC.TMTalk.Resources.hintSendInvite}, 2);
          } else {
            ASC.TMTalk.tabsContainer.resetStatus('conference', {confsubject : ASC.TMTalk.mucManager.getConferenceSubject(jid)}, 1);
          }

          ASC.TMTalk.dom.removeClass(contactlistContainer, 'mailing');
          ASC.TMTalk.dom.addClass(contactlistContainer, 'conference');
        }
        break;
      case 'mailing' :
        if (inBackground !== true) {
          ASC.TMTalk.tabsContainer.setStatus('hint', {hint : ASC.TMTalk.Resources.hintSendInvite}, 2);

          ASC.TMTalk.dom.removeClass(contactlistContainer, 'conference');
          ASC.TMTalk.dom.addClass(contactlistContainer, 'mailing');
        }
        break;
    }
  };

  var onCloseRoom = function (roomId, data, isCurrent) {
    switch (data.type) {
      case 'chat' :
        if (roomIds.hasOwnProperty(data.id)) {
          delete roomIds[data.id];
        }
      case 'conference' :
        if (roomIds.hasOwnProperty(data.id)) {
          var roomjid = data.id;
          delete roomIds[data.id];
          ASC.TMTalk.mucManager.closeRoom(roomjid);
          if (isCurrent === true) {
            ASC.TMTalk.dom.removeClass(contactlistContainer, 'conference');
          }
        }
        break;
      case 'mailing' :
        if (roomIds.hasOwnProperty(data.id)) {
          var roomjid = data.id;
          delete roomIds[data.id];
          if (isCurrent === true) {
            ASC.TMTalk.dom.removeClass(contactlistContainer, 'mailing');
          }
        }
        break;
    }
  };

  var onClientConnecting = function () {
    $('#talkStatusMenu').addClass('processing');
    $('#talkContactsContainer').removeClass('processing');
    ASC.TMTalk.tabsContainer.setStatus('information', {title : ASC.TMTalk.Resources.hintClientConnecting});
  };

  var onClientConnected = function () {
    $('#talkStatusMenu').removeClass('processing');
    if ($('#talkContactsContainer').find('li.contact:not(.default)').length === 0) {
      $('#talkContactsContainer').addClass('processing');
    }

    ASC.TMTalk.tabsContainer.setStatus('information', {title : ASC.TMTalk.Resources.hintSelectContact});

    ASC.TMTalk.sounds.play('startup');
  };

  var onClientDisconnecting = function () {
    $('#talkStatusMenu').addClass('processing');
    $('#talkContactsContainer').removeClass('processing');
  };

  var onClientDisconnected = function () {
    $('#talkStatusMenu').removeClass('processing');
    $('#talkContactsContainer').removeClass('processing');

    ASC.TMTalk.tabsContainer.setStatus('information', {title : ASC.TMTalk.Resources.hintClientDisconnected});
    
    ASC.TMTalk.sounds.play('letup');
  };

  var onClientConnectingFailed = function () {
    $('#talkStatusMenu').removeClass('processing');
    $('#talkContactsContainer').removeClass('processing');
    ASC.TMTalk.tabsContainer.setStatus('information', {title : ASC.TMTalk.Resources.hintClientDisconnected});

    ASC.TMTalk.connectionManager.bind(ASC.TMTalk.connectionManager.events.disconnected, (function (status, timeout) {
      return function () {
        if (status !== null) {
          setTimeout(function(){ASC.TMTalk.connectionManager.status(status.id)}, timeout * 1000);
        }
      };
    })(ASC.TMTalk.connectionManager.status(), reconnectTimeout), {once : true});
  };

  var onUpdateStatus = function (status, wasOffline) {
    var
      $statuses = $('#talkStatusMenu li.status').removeClass('current'),
      statusInd = $statuses.length;
    while (statusInd--) {
      if ($($statuses[statusInd]).hasClass(status.className)) {
        var $status = $($statuses[statusInd]);
        $status.addClass('current');
        $('#talkCurrentStatus').attr('class', status.className).find('div.title:first').html($status.text());
        break;
      }
    }
    ASC.TMTalk.properties.item(constants.propertyStatusId, status.id, true);

    if (wasOffline === false && status.show !== 'offline') {
      
      ASC.TMTalk.sounds.play('status');
    }
  };

  var onRetrievesCommand = function (from, cmd) {
    switch (cmd.toLocaleString()) {
      case 'open' :
        openRoom(from);
        break;
    }
  };

  var onUpdateContacts = function (groups, contacts) {
    var
      tempgroups = [],
      grouphashes = unserializeContactlistHashes(ASC.TMTalk.properties.item('grps')),
      i = 0, n = 0, j = 0, m = 0,
      node = null,
      nodes = null,
      nodesInd = 0,
      groupname = '',
      sortedgroups = [];

    if (ASC.TMTalk.properties.item('enabledMassend') === 'true' && ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group mailings', 'li').length === 0) {
      tempgroups.push({
        name      : ASC.TMTalk.Resources.mailingsGroupName,
        type      : 'mailings',
        isEmpty   : true,
        isFixed   : true,
        contacts  : []
      });
    }

    if (ASC.TMTalk.properties.item('enabledConferences') === 'true' && ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group rooms', 'li').length === 0) {
      tempgroups.push({
        name      : ASC.TMTalk.Resources.conferenceGroupName,
        type      : 'rooms',
        isEmpty   : true,
        isFixed   : true,
        contacts  : []
      });
    }

    for (i = 0, n = groups.length; i < n; i++) {
      tempgroups.push({
        name      : groups[i].name,
        type      : null,
        isEmpty   : groups[i].name !== '',
        isFixed   : false,
        contacts  : groups[i].contacts
      });
    }

    for (i = 0, n = grouphashes.length; i < n; i++) {
      for (j = 0, m = tempgroups.length; j < m; j++) {
        if (tempgroups[j].name !== '' && grouphashes[i].hash.toString() === ASC.TMTalk.getHash(tempgroups[j].name).toString()) {
          sortedgroups.push(tempgroups[j]);
          sortedgroups[sortedgroups.length - 1].isOpen = grouphashes[i].isOpen === '1';
          tempgroups.splice(j, 1);
          break;
        }
      }
    }

    for (i = 0, n = tempgroups.length; i < n; i++) {
      sortedgroups.push(tempgroups[i]);
      sortedgroups[sortedgroups.length - 1].isOpen = false;
    }

    var
      groupname = '',
      classname = '',
      sortedgroupitem = null,
      groupcontacts = null,
      groupnode = null,
      contactnode = null,
      contactlistnode = null,
      mastercontactnode = null,
      grouplistfragment = document.createDocumentFragment(),
      contactlistfragment = document.createDocumentFragment();

    for (i = 0, n = sortedgroups.length; i < n; i++) {
      sortedgroupitem = sortedgroups[i];
      groupname = sortedgroupitem.name;
      groupnode = createGroup(groupname, sortedgroupitem.type, sortedgroups[i].contacts.length);
      classname = groupnode.className;
      if (sortedgroupitem.isFixed === true) {
        classname += ' fixed';
      }
      if (sortedgroupitem.isEmpty === true) {
        classname += ' empty';
      }
      if (sortedgroupitem.name === '') {
        classname += ' none open';
      }
      if (sortedgroupitem.isOpen === true) {
        classname += ' open';
      }
      groupnode.className = classname;
      nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contactlist', 'ul');
      contactlistnode = nodes.length > 0 ? nodes[0] : null;
      groupcontacts = sortedgroupitem.contacts;
      mastercontactnode = null;
      for (j = 0, m = groupcontacts.length; j < m; j++) {
        contactnode = createContact(groupcontacts[j].name, 'contact', groupcontacts[j].jid);
        if (contactlistnode !== null) {
          contactlistnode.appendChild(contactnode);
          if (ASC.TMTalk.dom.hasClass(contactnode, 'master')) {
            mastercontactnode = contactnode;
          }
        }
      }
      if (mastercontactnode !== null) {
        contactlistnode.appendChild(mastercontactnode);
      }
      grouplistfragment.appendChild(groupnode);
    }

    mastercontactnode = null;
    for (i = 0, n = contacts.length; i < n; i++) {
      contactnode = createContact(contacts[i].name, 'contact', contacts[i].jid);
      contactlistfragment.appendChild(contactnode);
      if (ASC.TMTalk.dom.hasClass(contactnode, 'master')) {
        mastercontactnode = contactnode;
      }
    }
    if (mastercontactnode !== null) {
      contactlistfragment.appendChild(mastercontactnode);
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      node = nodes[nodesInd];
      if (!ASC.TMTalk.dom.hasClass(node, 'default') && !ASC.TMTalk.dom.hasClass(node, 'rooms') && !ASC.TMTalk.dom.hasClass(node, 'mailings')) {
        node.parentNode.removeChild(node);
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactsContainer, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      node = nodes[nodesInd];
      if (!ASC.TMTalk.dom.hasClass(node, 'default')) {
        node.parentNode.removeChild(node);
      }
    }

    ASC.TMTalk.dom.removeClass(contactlistContainer, 'processing');
    groupsContainer.appendChild(grouplistfragment);
    contactsContainer.appendChild(contactlistfragment);

    var cid = location.hash.charAt(0) === '#' ? location.hash.substring(1) : location.hash;
    if (cid) {
      location.hash = '';
      var domain = ASC.TMTalk.connectionManager.getDomain();
      cid = cid.indexOf(domain) !== -1 ? cid : cid + '@' + domain;
      if (cid.toLowerCase() !== ASC.TMTalk.connectionManager.getJid() && ASC.TMTalk.contactsManager.getContact(cid) !== null) {
        openRoom(jid);
      }
    }

    if (ASC.TMTalk.properties.item('enblfltr') === '1') {
      filteringContactlist();
    }

    if ($.support.iscroll) {
      clScroller = new iScroll('talkContactsContainer');
    }
  };

  var onGetLists = function (mailinglists) {
    var
      node = null,
      nodes = null,
      nodesInd = null,
      classname = '',
      groupname = '',
      groupnode = null,
      mailingnode = null,
      contactlistnode = null,
      contactlistfragment = document.createDocumentFragment();

    groupname = ASC.TMTalk.Resources.conferenceGroupName;

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group mailings', 'li');
    var groupnode = nodes.length > 0 ? nodes[0] : null;

    if (groupnode === null) {
      return undefined;
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'size', 'div');
    var sizenode = nodes.length > 0 ? nodes[0] : null;
    if (sizenode !== null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'all', 'span');
      if (nodes.length > 0) {
        nodes[0].innerHTML = mailinglists.length;
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      node = nodes[nodesInd];
      if (!ASC.TMTalk.dom.hasClass(node, 'default')) {
        node.parentNode.removeChild(node);
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contactlist', 'ul');
    if (nodes.length > 0) {
      contactlistnode = nodes[0];
    }

    for (var i = 0, n = mailinglists.length; i < n; i++) {
      mailingnode = createContact(mailinglists[i].name, 'mailing', mailinglists[i].id);
      contactlistfragment.appendChild(mailingnode);
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contactlist', 'ul');
    if (nodes.length > 0) {
      nodes[0].appendChild(contactlistfragment);
    }

    if (ASC.TMTalk.properties.item('enblfltr') === '1') {
      filteringContactlist();
    }
  };

  var onGetRooms = function (discoitems, rooms) {
    var
      node = null,
      nodes = null,
      nodesInd = null,
      classname = '',
      groupname = '',
      groupnode = null,
      conferencenode = null,
      contactlistnode = null,
      contactlistfragment = document.createDocumentFragment();

    groupname = ASC.TMTalk.Resources.conferenceGroupName;

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group rooms', 'li');
    var groupnode = nodes.length > 0 ? nodes[0] : null;

    if (groupnode === null) {
      var
        grouphashes = unserializeContactlistHashes(ASC.TMTalk.properties.item('grps')),
        grouphash = ASC.TMTalk.getHash(groupname).toString(),
        grouphashesInd = 0,
        wasAdded = false,
        isOpen = false;

      grouphashesInd = grouphashes.length;
      while (grouphashesInd--) {
        if (grouphash === grouphashes[grouphashesInd].hash) {
          isOpen = grouphashes[grouphashesInd].isOpen === '1';
          break;
        }
      }

      groupnode = createGroup(groupname, 'rooms', 0);
      classname = groupnode.className;
      if (isOpen === true) {
        classname += ' open';
      }
      classname += ' empty';
      groupnode.className = classname;

      wasAdded = false;
      if (grouphashesInd !== -1 && grouphashesInd !== 0 && grouphashesInd !== grouphashes.length - 1) {
        var nextgrouphash = grouphashes[grouphashesInd + 1].hash;
        nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group', 'li');
        nodesInd = nodes.length;
        while (nodesInd--) {
          if (ASC.TMTalk.getHash(nodes[nodesInd].getAttribute('data-name')).toString() === nextgrouphash) {
            groupsContainer.insertBefore(groupnode, nodes[nodesInd]);
            wasAdded = true;
            break;
          }
        }
      }

      if (wasAdded === false) {
        nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group default', 'li');
        var defaultgroup = nodes.length > 0 ? nodes[0] : null;
        if (defaultgroup !== null) {
          groupsContainer.insertBefore(groupnode, ASC.TMTalk.dom.nextElementSibling(defaultgroup));
        } else {
          groupsContainer.appendChild(groupNode);
        }
      }
      ASC.TMTalk.properties.item('grps', ASC.TMTalk.contactsContainer.getContactlistHash(), true);
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'size', 'div');
    var sizenode = nodes.length > 0 ? nodes[0] : null;
    if (sizenode !== null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'all', 'span');
      if (nodes.length > 0) {
        nodes[0].innerHTML = rooms.length;
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      node = nodes[nodesInd];
      if (!ASC.TMTalk.dom.hasClass(node, 'default')) {
        node.parentNode.removeChild(node);
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contactlist', 'ul');
    if (nodes.length > 0) {
      contactlistnode = nodes[0];
    }

    for (var i = 0, n = rooms.length; i < n; i++) {
      conferencenode = createContact(rooms[i].name, 'room', rooms[i].jid);
      contactlistfragment.appendChild(conferencenode);
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contactlist', 'ul');
    if (nodes.length > 0) {
      nodes[0].appendChild(contactlistfragment);
    }

    if (ASC.TMTalk.properties.item('enblfltr') === '1') {
      filteringContactlist();
    }
  };

  var onComeContact = function (jid, status) {
    var
      wasOffline = false,
      allcontacts = 0,
      onlinecontacts = 0,
      classname = '',
      nodes = null,
      groupnode = null,
      contact = null,
      contacts = null,
      contactsInd = 0;

    contacts = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact', 'li');
    contactsInd = contacts.length;
    while (contactsInd--) {
      if (contacts[contactsInd].getAttribute('data-cid') !== jid) {
        continue;
      }
      contact = contacts[contactsInd];

      if (ASC.TMTalk.dom.hasClass(contact, 'master')) {
        continue;
      }

      wasOffline = ASC.TMTalk.dom.hasClass(contact, 'offline');

      if (wasOffline === true) {
        groupnode = contact.parentNode.parentNode;
        if (groupnode && ASC.TMTalk.dom.hasClass(groupnode, 'group')) {
          ASC.TMTalk.dom.removeClass(groupnode, 'empty');
          nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'size', 'div');
          var sizenode = nodes.length > 0 ? nodes[0] : null;
          if (sizenode !== null) {
            nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'all', 'span');
            if (nodes.length > 0) {
              allcontacts = nodes[0].innerHTML;
              allcontacts = isFinite(+allcontacts) ? +allcontacts : allcontacts;
              nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'online', 'span');
              if (nodes.length > 0) {
                onlinecontacts = nodes[0].innerHTML;
                onlinecontacts = isFinite(+onlinecontacts) ? +onlinecontacts : onlinecontacts;
                if (onlinecontacts + 1 <= allcontacts) {
                  nodes[0].innerHTML = ++onlinecontacts;
                }
              }
            }
          }
        }
        nodes = ASC.TMTalk.dom.getElementsByClassName(contact.parentNode, 'contact online', 'li');
        if (nodes.length > 0) {
          insertNode(contact, nodes);
        } else {
          nodes = ASC.TMTalk.dom.getElementsByClassName(contact.parentNode, 'contact default', 'li');
          var defaultcontact = nodes.length > 0 ? nodes[0] : null;
          contact.parentNode.insertBefore(contact, ASC.TMTalk.dom.nextElementSibling(defaultcontact));
        }
      }

      classname = 'contact online ' + status.className;
      if (ASC.TMTalk.dom.hasClass(contact, 'master')) {
        classname += ' master';
      }
      if (ASC.TMTalk.dom.hasClass(contact, 'new-message')) {
        classname += ' new-message';
      }
      if (ASC.TMTalk.dom.hasClass(contact, 'selected')) {
        classname += ' selected';
      }
      if (ASC.TMTalk.dom.hasClass(contact, 'checked-by-filter')) {
        classname += ' checked-by-filter';
      }
      contact.className = classname;
    }

    if (ASC.TMTalk.properties.item('enblfltr') === '1') {
      filteringContactlist();
    }
  };

  var onLeftContact = function (jid, status) {
    var
      allcontacts = 0,
      onlinecontacts = 0,
      classname = '',
      nodes = null,
      groupnode = null,
      contact = null,
      contacts = null,
      contactsInd = 0;

    contacts = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact', 'li');
    contactsInd = contacts.length;
    while (contactsInd--) {
      if (contacts[contactsInd].getAttribute('data-cid') !== jid) {
        continue;
      }
      contact = contacts[contactsInd];

      if (ASC.TMTalk.dom.hasClass(contact, 'master')) {
        continue;
      }

      classname = 'contact ' + status.className;
      if (ASC.TMTalk.dom.hasClass(contact, 'master')) {
        classname += ' master';
      }
      if (ASC.TMTalk.dom.hasClass(contact, 'new-message')) {
        classname += ' new-message';
      }
      if (ASC.TMTalk.dom.hasClass(contact, 'selected')) {
        classname += ' selected';
      }
      if (ASC.TMTalk.dom.hasClass(contact, 'checked-by-filter')) {
        classname += ' checked-by-filter';
      }
      contact.className = classname;

      groupnode = contact.parentNode.parentNode;
      if (groupnode && ASC.TMTalk.dom.hasClass(groupnode, 'group')) {
        if (ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contact online').length === 0) {
          ASC.TMTalk.dom.addClass(groupnode, 'empty');
        }
        nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'size', 'div');
        var sizenode = nodes.length > 0 ? nodes[0] : null;
        if (sizenode !== null) {
          nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'all', 'span');
          if (nodes.length > 0) {
            allcontacts = nodes[0].innerHTML;
            allcontacts = isFinite(+allcontacts) ? +allcontacts : allcontacts;
            nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'online', 'span');
            if (nodes.length > 0) {
              onlinecontacts = nodes[0].innerHTML;
              onlinecontacts = isFinite(+onlinecontacts) ? +onlinecontacts : onlinecontacts;
              if (onlinecontacts - 1 >= 0) {
                nodes[0].innerHTML = --onlinecontacts;
              }
            }
          }
        }
      }

      nodes = ASC.TMTalk.dom.getElementsByClassName(contact.parentNode, 'contact offline', 'li');
      if (nodes.length > 0) {
        insertNode(contact, nodes);
      } else {
        contact.parentNode.appendChild(contact);
      }
    }

    if (ASC.TMTalk.properties.item('enblfltr') === '1') {
      filteringContactlist();
    }
  };

  var onAddContact = function () {
    
  };

  var onRemoveContact = function () {
    
  };

  var onCreateList = function (listId, list) {
    var
      groupname = '',
      nodes = null,
      nodesInd = 0,
      sizenode = null,
      groupnode = null,
      mailingnode = null,
      contactlistnode = null;

    groupname = ASC.TMTalk.Resources.mailingsGroupName;

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group mailings', 'li');
    groupnode = nodes.length > 0 ? nodes[0] : null;

    if (groupnode === null) {
      groupnode = createGroup(groupname, 'mailings', 0);
      nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group default', 'li');
      var defaultgroup = nodes.length > 0 ? nodes[0] : null;
      if (defaultgroup !== null) {
        groupsContainer.insertBefore(groupnode, ASC.TMTalk.dom.nextElementSibling(defaultgroup));
      } else {
        groupsContainer.appendChild(groupnode);
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-cid') === list.id) {
        return undefined;
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'size', 'div');
    sizenode = nodes.length > 0 ? nodes[0] : null;
    if (sizenode !== null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'all', 'span');
      if (nodes.length > 0) {
        var allcontacts = nodes[0].innerHTML;
        allcontacts = isFinite(+allcontacts) ? +allcontacts : 0;
        nodes[0].innerHTML = ++allcontacts;
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contactlist', 'ul');
    contactlistnode = nodes.length > 0 ? nodes[0] : null;

    mailingnode = createContact(list.name, 'mailing', list.id);
    if (contactlistnode !== null) {
      contactlistnode.appendChild(mailingnode);
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'contact offline', 'li');
    if (nodes.length > 0) {
      insertNode(mailingnode, nodes);
    }

    if (ASC.TMTalk.properties.item('enblfltr') === '1') {
      filteringContactlist();
    }
  };

  var onRemoveList = function (listId) {
    var
      nodes = null,
      nodesInd = 0,
      groupnode = null;

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'contact', 'li');
    nodesInd = nodes.length;
    while (nodesInd--) {
      if (nodes[nodesInd].getAttribute('data-cid') === listId) {
        nodes[nodesInd].parentNode.removeChild(nodes[nodesInd]);
      }
    }

    nodes = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group mailings', 'li');
    groupnode = nodes.length > 0 ? nodes[0] : null;

    if (groupnode !== null) {
      nodes = ASC.TMTalk.dom.getElementsByClassName(groupnode, 'size', 'div');
      var sizenode = nodes.length > 0 ? nodes[0] : null;
      if (sizenode !== null) {
        nodes = ASC.TMTalk.dom.getElementsByClassName(sizenode, 'all', 'span');
        if (nodes.length > 0) {
          var allcontacts = nodes[0].innerHTML;
          allcontacts = isFinite(+allcontacts) ? +allcontacts : 0;
          if (allcontacts > 0) {
            nodes[0].innerHTML = --allcontacts;
          }
        }
      }
    }

    if (roomIds.hasOwnProperty(listId)) {
      ASC.TMTalk.roomsManager.closeRoom(roomIds[listId].id);
    }
  };

  var onOpenList = function (listId, list) {
    if (!roomIds.hasOwnProperty(listId)) {
      ASC.TMTalk.roomsManager.createRoom({
        type : 'mailing',
        id : listId,
        minimized : false
      });
    }

    // after creating the room, we has the id
    if (roomIds.hasOwnProperty(listId)) {
      ASC.TMTalk.roomsManager.openRoom(roomIds[listId].id);
    }
  };

  return {
    init  : init,
    nodes : setNodes,

    constants : constants,

    getContactlistHash  : getContactlistHash,

    resetContactlist      : resetContactlist,
    filteringContactlist  : filteringContactlist,

    showFirstUnreadMessage  : showFirstUnreadMessage,

    openItem        : openItem,
    openRoom        : openRoom,
    openMailing     : openMailing,
    openConference  : openConference
  };
})(jQuery);

(function ($) {
  function onChoseSendInvite (selectedElement, selectedTargets) {
    if (selectedTargets.length === 0) {
      return undefined;
    }
    if (ASC.TMTalk.dom.hasClass(selectedElement, 'contact')) {
      var
        contactjid = selectedElement.getAttribute('data-cid'),
        currentRoomData = ASC.TMTalk.roomsManager.getRoomData(),
        roomjid = currentRoomData !== null ? currentRoomData.id : '';
      if (roomjid && contactjid) {
        if (ASC.TMTalk.contactsManager.getContactStatus(contactjid).id !== ASC.TMTalk.connectionManager.offlineStatusId) {
          ASC.TMTalk.mucManager.sendInvite(roomjid, contactjid);
        }
      }
    }
  }

  function onChoseAddToMailing (selectedElement, selectedTargets) {
    if (selectedTargets.length === 0) {
      return undefined;
    }
    if (ASC.TMTalk.dom.hasClass(selectedElement, 'contact')) {
      var
        contactjid = selectedElement.getAttribute('data-cid'),
        currentRoomData = ASC.TMTalk.roomsManager.getRoomData(),
        listid = currentRoomData !== null ? currentRoomData.id : '';
      if (listid && contactjid) {
        ASC.TMTalk.msManager.addContact(listid, contactjid);
      }
    }
  }

  function onChoseMoveGroup (selectedElement, selectedTargets) {
    if (selectedTargets.length === 0) {
      return undefined;
    }
    
    // TODO:
    if (ASC.TMTalk.dom.hasClass(selectedTargets[0], 'separator')) {
      var
        selectedGroup = selectedElement,
        targetGroup = selectedTargets[0].parentNode;
      if (ASC.TMTalk.dom.hasClass(selectedGroup, 'group') && ASC.TMTalk.dom.hasClass(targetGroup, 'group')) {
        selectedGroup.parentNode.insertBefore(selectedGroup, targetGroup);
        ASC.TMTalk.properties.item('grps', ASC.TMTalk.contactsContainer.getContactlistHash(), true);
      }
    }
    if (ASC.TMTalk.dom.hasClass(selectedTargets[0], 'sub-panel')) {
      var
        fn = null,
        currentRoomData = ASC.TMTalk.roomsManager.getRoomData(),
        itemid = currentRoomData !== null ? currentRoomData.id : '',
        itemtype = currentRoomData !== null ? currentRoomData.type : '',
        contacts = ASC.TMTalk.contactsManager.getContactsGroupByName(selectedElement.getAttribute('data-name'));
      switch (itemtype) {
        case 'mailing'    : fn = ASC.TMTalk.msManager.addContact; break;
        case 'conference' : fn = ASC.TMTalk.mucManager.sendInvite; break;
      }
      if (fn && itemid && contacts.length > 0) {
        for (var i = 0, n = contacts.length; i < n; i++) {
          fn(itemid, contacts[i].jid);
        }
      }
    }
  }

  $(function () {
    ASC.TMTalk.contactsContainer.nodes();

    if ($.browser.safari) {
      $('#talkContactToolbarContainer div.unread-messages:first div.button-container').mousedown(function () {
        if (window.getSelection) {
          window.getSelection().removeAllRanges();
        }
        return false;
      });

      $('#talkContactToolbarContainer div.toolbar:first div.button-container').mousedown(function () {
        if (window.getSelection) {
          window.getSelection().removeAllRanges();
        }
        return false;
      });

      $('#talkStatusContainer').mousedown(function () {
        if (window.getSelection) {
          window.getSelection().removeAllRanges();
        }
        return false;
      });
    }

    if (ASC.TMTalk.properties.item('requestTransportType') === 'flash' && ASC.TMTalk.flashPlayer.isCorrect === false) {
      ASC.TMTalk.tabsContainer.setStatus('information', {title : ASC.TMTalk.flashPlayer.isInstalled ? ASC.TMTalk.Resources.hintFlastPlayerIncorrect : ASC.TMTalk.Resources.hintNoFlashPlayer});
    } else {
      ASC.TMTalk.tabsContainer.setStatus('information', {title : ASC.TMTalk.Resources.hintClientDisconnected});

      var lastStatusId = ASC.TMTalk.properties.item(ASC.TMTalk.contactsContainer.constants.propertyStatusId);
      lastStatusId = isFinite(+lastStatusId) ? +lastStatusId : ASC.TMTalk.connectionManager.onlineStatusId;
      setTimeout((function (statusId) {return function () {ASC.TMTalk.connectionManager.status(statusId)}})(lastStatusId), 100);
    }

    if (ASC.TMTalk.sounds.supported()) {
      if (ASC.TMTalk.properties.item('enblsnds') === '0') {
        ASC.TMTalk.sounds.disable();
        $('#talkContactToolbarContainer div.button-container.toggle-sounds:first').addClass('disabled');
      } else {
        ASC.TMTalk.sounds.enable();
        $('#talkContactToolbarContainer div.button-container.toggle-sounds:first').removeClass('disabled');
      }
    } else {
      ASC.TMTalk.sounds.disable();
      $('#talkContactToolbarContainer div.button-container.toggle-sounds:first').addClass('disabled').addClass('not-available');
    }

    if (ASC.TMTalk.notifications.supported.current === true) {
      if (ASC.TMTalk.notifications.enabled()) {
        $('#talkContactToolbarContainer div.button-container.toggle-notifications:first').removeClass('disabled');
      } else {
        $('#talkContactToolbarContainer div.button-container.toggle-notifications:first').addClass('disabled');
      }
    } else {
      $('#talkContactToolbarContainer div.button-container.toggle-notifications:first').addClass('disabled').addClass('not-available');
    }

    if ($.support.iscroll) {
      $('#talkContactToolbarContainer div.button-container.toggle-group:first').addClass('disabled').addClass('not-available');
    }

    if (ASC.TMTalk.properties.item('shwoffusrs') === '0') {
      $('#talkSidebarContainer').addClass('hide-offlineusers');
    } else {
      $('#talkSidebarContainer').removeClass('hide-offlineusers');
    }
    if (ASC.TMTalk.properties.item('shwgrps') === '0') {
      $('#talkSidebarContainer').removeClass('grouplist').addClass('contactlist');
    } else {
      $('#talkSidebarContainer').removeClass('contactlist').addClass('grouplist');
    }

    if ($.support.iscroll) {
      $('#talkSidebarContainer').removeClass('grouplist').addClass('contactlist');
    }

    ASC.TMTalk.properties.item('enblfltr', '0');

    $('#talkContactToolbarContainer div.button.toggle-offlineusers:first').click(function () {
      ASC.TMTalk.properties.item('shwoffusrs', $('#talkSidebarContainer').toggleClass('hide-offlineusers').hasClass('hide-offlineusers') ? '0' : '1', true);
    });

    $('#talkContactToolbarContainer div.button.toggle-group:first').click(function () {
      ASC.TMTalk.properties.item('shwgrps', $('#talkSidebarContainer').toggleClass('grouplist').hasClass('grouplist') ? '1' : '0', true);
    });

    $('#talkContactToolbarContainer div.button.toggle-sounds:first').click(function () {
      ASC.TMTalk.properties.item('enblsnds', $('#talkContactToolbarContainer div.button-container.toggle-sounds:first').toggleClass('disabled').hasClass('disabled') ? '0' : '1', true);
    });

    $('#talkContactToolbarContainer div.button.toggle-notifications:first').click(function () {
      if ($('#talkContactToolbarContainer div.button-container.toggle-notifications:first').hasClass('disabled')) {
        ASC.TMTalk.notifications.enable();
      } else {
        ASC.TMTalk.notifications.disable();
      }
    });

    $('#talkContactToolbarContainer div.button.toggle-filter:first').click(function () {
      ASC.TMTalk.properties.item('enblfltr', $('#talkSidebarContainer').toggleClass('enable-filter').hasClass('enable-filter') ? '1' : '0');
    });

    $('#talkContactToolbarContainer div.unread-messages:first').click(function () {
      ASC.TMTalk.contactsContainer.showFirstUnreadMessage();
    });

    $('#filterValue')
      .keydown(function (evt) {
        if (evt.target.tagName.toLowerCase() === 'input') {
          evt.originalEvent.stopPropagation ? evt.originalEvent.stopPropagation() : evt.originalEvent.cancelBubble = true;
        }
      });

    $('#talkContactsContainer')
      .mousedown(function (evt) {
        var element = evt.target;
        if (!element || typeof element !== 'object') {
          return undefined;
        }
        if (ASC.TMTalk.dom.hasClass(element, 'group-title')) {
          var group = element.parentNode.parentNode;
          if (ASC.TMTalk.dom.hasClass(group, 'group')) {
            var
              contactlistContainer = document.getElementById('talkContactsContainer'),
              currentRoomData = ASC.TMTalk.roomsManager.getRoomData(),
              nodes = null,
              groupsInd = 0,
              groups = null,
              targets = [];
            nodes = currentRoomData === null ? [] : ASC.TMTalk.dom.getElementsByClassName('talkRoomsContainer', 'room' + (currentRoomData.type === 'conference' ? ' conference' : ' mailing') + ' current', 'li');
            if (nodes.length > 0) {
              nodes = ASC.TMTalk.dom.getElementsByClassName(nodes[0], 'sub-panel', 'div');
              if (nodes.length > 0) {
                targets.push(nodes[0]);
              }
            }
            groups = ASC.TMTalk.dom.getElementsByClassName(contactlistContainer, 'group', 'li');
            groupsInd = groups.length;
            while (groupsInd--) {
              if (ASC.TMTalk.dom.hasClass(groups[groupsInd], 'default')) {
                continue;
              }
              nodes = ASC.TMTalk.dom.getElementsByClassName(groups[groupsInd], 'separator', 'div');
              if (nodes.length > 0) {
                targets.push(nodes[0]);
              }
            }
            ASC.TMTalk.dragMaster.start(
              evt,
              group,
              targets,
              {onChose : onChoseMoveGroup},
              function (el) {
                var
                  name = '',
                  nodes = null;
                nodes = ASC.TMTalk.dom.getElementsByClassName(el, 'group-title', 'div');
                name = nodes.length > 0 ? nodes[0].innerHTML : 'none';
                if (!name) {
                  name = el.getAttribute('data-name');
                }
                this.className = 'group-dragable';
                this.style.width = el.offsetWidth + 'px';
                nodes = ASC.TMTalk.dom.getElementsByClassName(this, 'title', 'div');
                if (nodes.length > 0) {
                  nodes[0].innerHTML = name;
                } 
              }
            );
          }
        } else if (ASC.TMTalk.dom.hasClass(element, 'contact-title')) {
          var currentRoomData = ASC.TMTalk.roomsManager.getRoomData();
          if (currentRoomData && (currentRoomData.type === 'conference' || currentRoomData.type === 'mailing') && currentRoomData.minimized === false) {
            var contact = element.parentNode;
            if (ASC.TMTalk.dom.hasClass(contact, 'contact') && !ASC.TMTalk.dom.hasClass(contact, 'master') && (currentRoomData.type === 'mailing' || (currentRoomData.type === 'conference' && !ASC.TMTalk.dom.hasClass(contact, 'offline')))) {
              var targets = ASC.TMTalk.dom.getElementsByClassName('talkRoomsContainer', 'room' + (currentRoomData.type === 'conference' ? ' conference' : ' mailing') + ' current', 'li');
              if (targets.length > 0) {
                targets = ASC.TMTalk.dom.getElementsByClassName(targets[0], 'sub-panel', 'div');
              }
              ASC.TMTalk.dragMaster.start(
                evt,
                contact,
                targets.length > 0 ? targets : ['talkRoomsContainer'],
                {onChose : currentRoomData.type === 'conference' ? onChoseSendInvite : onChoseAddToMailing},
                function (el) {
                  var
                    nodes = null,
                    name = '';
                  nodes = ASC.TMTalk.dom.getElementsByClassName(el, 'contact-title', 'div');
                  name = nodes.length > 0 ? nodes[0].innerHTML : 'none';
                  if (!name) {
                    name = el.getAttribute('data-name');
                  }
                  this.className = 'contact-dragable';
                  this.style.width = el.offsetWidth + 'px';
                  nodes = ASC.TMTalk.dom.getElementsByClassName(this, 'title', 'div');
                  if (nodes.length > 0) {
                    nodes[0].innerHTML = name;
                  }
                  var contactjid = el.getAttribute('data-cid');
                  if (contactjid) {
                    var status = ASC.TMTalk.contactsManager.getContactStatus(contactjid);
                    if (status) {
                      this.className += ' ' + status.className;
                    }
                  }
                }
              );
            }
          }
        }
        return false;
      })
      .click(function (evt) {
        var element = evt.target;
        if (!element || typeof element !== 'object') {
          return undefined;
        }
        if (ASC.TMTalk.dom.hasClass(element, 'group-title')) {
          var group = element.parentNode.parentNode;
          if (ASC.TMTalk.dom.hasClass(group, 'group') && !ASC.TMTalk.dom.hasClass(group, 'checked-by-filter')) {
            ASC.TMTalk.dom.toggleClass(group, 'open');
            ASC.TMTalk.properties.item('grps', ASC.TMTalk.contactsContainer.getContactlistHash(), true);
          }
        } else if (ASC.TMTalk.dom.hasClass(element, 'room-title')) {
          var contact = element.parentNode;
          if (ASC.TMTalk.dom.hasClass(contact, 'contact')) {
            var cid = contact.getAttribute('data-cid');
            if (cid) {
              ASC.TMTalk.contactsContainer.openConference(cid);
            }
          }
        } else if (ASC.TMTalk.dom.hasClass(element, 'mailing-title')) {
          var contact = element.parentNode;
          if (ASC.TMTalk.dom.hasClass(contact, 'contact')) {
            var cid = contact.getAttribute('data-cid');
            if (cid) {
              ASC.TMTalk.contactsContainer.openMailing(cid);
            }
          }
        } else if (ASC.TMTalk.dom.hasClass(element, 'contact-title')) {
          var contact = element.parentNode;
          if (ASC.TMTalk.dom.hasClass(contact, 'contact')) {
            var cid = contact.getAttribute('data-cid');
            if (cid) {
              ASC.TMTalk.contactsContainer.openRoom(cid);
            }
          }
        } else if (ASC.TMTalk.dom.hasClass(element, 'button') && ASC.TMTalk.dom.hasClass(element, 'send-invite')) {
          var contact = element.parentNode.parentNode;
          if (ASC.TMTalk.dom.hasClass(contact, 'contact')) {
            var
              contactjid = contact.getAttribute('data-cid'),
              currentRoomData = ASC.TMTalk.roomsManager.getRoomData(),
              roomjid = currentRoomData !== null ? currentRoomData.id : '';

            if (roomjid && contactjid) {
              ASC.TMTalk.mucManager.sendInvite(roomjid, contactjid);
            }
          }
        } else if (ASC.TMTalk.dom.hasClass(element, 'button') && ASC.TMTalk.dom.hasClass(element, 'add-to-mailing')) {
          var contact = element.parentNode.parentNode;
          if (ASC.TMTalk.dom.hasClass(contact, 'contact')) {
            var
              contactjid = contact.getAttribute('data-cid'),
              currentRoomData = ASC.TMTalk.roomsManager.getRoomData(),
              listid = currentRoomData !== null ? currentRoomData.id : '';

            if (listid && contactjid) {
              ASC.TMTalk.msManager.addContact(listid, contactjid);
            }
          }
        }
      });

    $('#talkStatusMenu').click(function (evt) {
      var
        $target = $(evt.target),
        $container = null,
        $selectStatus = null;

      if (($container = $('#talkStatusMenu div.container:first')).is(':hidden')) {
        $container.slideDown('fast', function () {
          $(document).one('click', function () {
            $('#talkStatusMenu div.container:first').hide();
          });
        });
        return undefined;
      }

      if (ASC.TMTalk.properties.item('requestTransportType') === 'flash' && ASC.TMTalk.flashPlayer.isCorrect === false) {
        ASC.TMTalk.tabsContainer.setStatus('information', {title : ASC.TMTalk.flashPlayer.isInstalled ? ASC.TMTalk.Resources.hintFlastPlayerIncorrect : ASC.TMTalk.Resources.hintNoFlashPlayer});
        return undefined;
      }

      if (($selectStatus = $target.parents('li.status:first')).length > 0) {
        var
          statusInd = 0,
          statuses = ASC.TMTalk.connectionManager.statuses;
        statusInd = statuses.length;
        while (statusInd--) {
          if ($selectStatus.hasClass(statuses[statusInd].className)) {
            ASC.TMTalk.connectionManager.status(statuses[statusInd].id);
            break;
          }
        }
      }
    });
  });
})(jQuery);
