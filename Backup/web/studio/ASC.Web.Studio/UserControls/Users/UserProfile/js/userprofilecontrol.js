
if (!window.userProfileControl) {
  window.userProfileControl = {
    openContact : function (name) {
      var tcExist = false;
      try {
        tcExist = !!ASC.Controls.JabberClient;
      } catch (err) {
        tcExist = false;
      }
      if (tcExist === true) {
        try {ASC.Controls.JabberClient.open(name)} catch (err) {}
      }
    }
  };
}

jq(function () {
  var tcExist = false;
  try {
    tcExist = !!ASC.Controls.JabberClient;
  } catch (err) {
    tcExist = false;
  }
  if (tcExist === true) {
    jq('div.userProfileCard:first ul.info:first li.contact span')
      .addClass('link')
      .click(function () {
        var
          search = location.search,
          arr = null,
          ind = 0,
          b = null;
        if (search.charAt(0) === '?') {
          search = search.substring(1);
        }
        arr = search.split('&');
        ind = arr.length;
        while (ind--) {
          b = arr[ind].split('=');
          if (b[0].toLowerCase() !== 'user') {
            continue;
          }
          userProfileControl.openContact(b[1]);
          break;
        }
      });
  }
});


