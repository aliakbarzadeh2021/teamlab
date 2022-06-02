/*******************************************************************************
   JQuery Extension
*******************************************************************************/

jQuery.extend({
/**
* Returns get parameters.
*
* If the desired param does not exist, null will be returned
*
* @example value = $.getURLParam("paramName");
*/ 
 getURLParam: function(strParamName){
	 
	  strParamName = strParamName.toLowerCase(); 
	
	  var strReturn = "";
	  var strHref = window.location.href.toLowerCase();
	  var bFound=false;
	  
	  var cmpstring = strParamName + "=";
	  var cmplen = cmpstring.length;

	  if (strHref.indexOf("?") > -1 ){
	    var strQueryString = strHref.substr(strHref.indexOf("?")+1);
	    var aQueryString = strQueryString.split("&");
	    for ( var iParam = 0; iParam < aQueryString.length; iParam++ ){
	      if (aQueryString[iParam].substr(0,cmplen)==cmpstring){
	        var aParam = aQueryString[iParam].split("=");
	        strReturn = aParam[1];
	        bFound=true;	        	        	        
	        break;
	      }
	      
	    }
	  }
	  if (bFound==false) return null;
	  
	  if (strReturn.indexOf("#") > -1 )
	    return strReturn.split("#")[0];
	  
	  return strReturn;
	}, 


	
/*

  var result = $.format("Hello, {0}.", "world");
  //result -> "Hello, world."

*/
format:  function jQuery_dotnet_string_format(text) {
  //check if there are two arguments in the arguments list
  if (arguments.length <= 1) {
    //if there are not 2 or more arguments there's nothing to replace
    //just return the text
    return text;
  }
  //decrement to move to the second argument in the array
  var tokenCount = arguments.length - 2;
  for (var token = 0; token <= tokenCount; ++token) {
    //iterate through the tokens and replace their placeholders from the text in order
    text = text.replace(new RegExp("\\{" + token + "\\}", "gi"), arguments[token + 1]);
  }
  return text;
}
}
);

(function($) {
   $.fn.yellowFade = function() {
    return (this.css({backgroundColor: "#ffffcc"}).animate(
            {
                backgroundColor: "#ffffff"
            }, 1500));
   }
 })(jQuery);


//jQuery.fn.yellowFade = function() {

// this.animate( { backgroundColor: "#ffffcc" }, 1 ).animate( { backgroundColor: "#ffffff" }, 5000 );


//      };	

jQuery.fn.swap = function(b) {
    b = jQuery(b)[0];
    var a = this[0],
        a2 = a.cloneNode(true),
        b2 = b.cloneNode(true),
        stack = this;

    a.parentNode.replaceChild(b2, a);
    b.parentNode.replaceChild(a2, b);

    stack[0] = a2;
    return this.pushStack( stack );
};



/*******************************************************************************/

if (typeof ASC === 'undefined') 
ASC ={};

ASC.Projects = (function() { // Private Section

return { // Public Section

   

}

})();



ASC.Projects.Constants = 
{
   DatePattern: "" ,
   MINI_LOADER_IMG: "", 
   IMAGES_FOLDER_PRODUCT_PATH: "", 
   IMAGES_FOLDER_COMMON_PATH: ""
}

ASC.Projects.Common =(function() { // Private Section

return { // Public Section
   
   
    tooltip: function (target_items, name, isNew)
    {
       
       if(typeof(isNew)!="undefined" && jq(target_items).length == 1)
       {
            var id = jq.trim(jq(target_items).attr('id')).split('_')[1];
            var title =  jq.trim(jq(target_items).attr('title'));
            if (title == "" || title == null) return;
            
            if(isNew)
            {
                jq("body").append("<div style='display:none;' class='borderBase tintMedium "+name+"' id='"+name+id+"'><p>" + title + "</p></div>");
            }
            else
            {
                jq("#"+name+id).html("<p>" + title + "</p>");
            }    
            
            var tooltip = jq("#"+name+id);

            jq(target_items).removeAttr("title")
            .mouseover(function(){tooltip.show();})
            .mousemove(function(kmouse){tooltip.css({left:kmouse.pageX+15, top:kmouse.pageY+15});})
            .mouseout(function(){tooltip.hide();});
            
            return false; 
       }
        
       jq(target_items).each(function()
       {
            var id = jq.trim(jq(this).attr('id')).split('_')[1];
            var title = jq.trim(jq(this).attr('title'));
            if (title == "" || title == null) return;

            jq("body").append("<div style='display:none;' class='borderBase tintMedium "+name+"' id='"+name+id+"'><p>" + title + "</p></div>");

            var my_tooltip = jq("#"+name+id);

            jq(this).removeAttr("title")
            .mouseover(function(){my_tooltip.show();})
            .mousemove(function(kmouse){my_tooltip.css({left:kmouse.pageX+15, top:kmouse.pageY+15});})
            .mouseout(function(){my_tooltip.hide();});
       });
                   
    },
    changeImage: function (target_items, srcMouseOver, srcMouseOut)
    {

       jq(target_items).each(function(i)
       {
          jq(this)
          .mouseover(
                     function()
                     {
                       jq(this).attr('src',srcMouseOver);
                     }
                    )
          .mousemove(
                     function(kmouse)
                     {
                     }
                    )
           .mouseout(
                     function()
                     {
                       jq(this).attr('src',srcMouseOut);
                     }
                    );
          });
                   
    },
    dropdownToggle: function(switcherUI, dropdownID, addTop, addLeft)
    {
        
      var top = addTop == null? 0 : addTop;
      var left = addLeft == null? 0 : addLeft;
      
      var  targetPos = jq(switcherUI).offset();
      var  dropdownItem = jq("#" + dropdownID);
      
      var elemPosTop = targetPos.top + jq(switcherUI).outerHeight();
      var elemPosLeft = targetPos.left;
      
      var w = jq(window);
      var TopPadding =w.scrollTop();
      var LeftPadding=w.scrollLeft();
      var ScrWidth=w.width();
      var ScrHeight=w.height();
      
      if((targetPos.left + dropdownItem.width())> (LeftPadding+ScrWidth))
        elemPosLeft -= targetPos.left + dropdownItem.width() - (LeftPadding+ScrWidth);
      
      if ((targetPos.top + dropdownItem.outerHeight()) > (TopPadding + ScrHeight))
        elemPosTop = targetPos.top - dropdownItem.outerHeight();
                         
      dropdownItem.css(
                       {
                          
                          'position' : 'absolute',
                          'top' : elemPosTop + top,
                          'left' : elemPosLeft + left                             
                       }                            
                    ); 
                    
      dropdownItem.toggle(); 
     
    },
    dropdownRegisterAutoHide: function(event,switcherID, dropdownID)
    {
             
        if (!jq((event.target) ? event.target : event.srcElement)
              .parents()
              .andSelf()
              .is(switcherID + ", " + dropdownID + "'"))
              jq(dropdownID).hide();       
    
    },
    IAmIsManager: false         
}

})();

/*******************************************************************************
             
*******************************************************************************/


ASC.Projects.Error = (function() { // Private Section

return { // Public Section



}

})();






