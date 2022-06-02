﻿jq(document).ready (function()
{
    jq('#simpleTextArea').keyup(function(event){ 
    var code;
    if (!e) var e = event;
    
    if (e.keyCode) 
        code = e.keyCode;
    else if (e.which) 
        code = e.which; 
    
    if(code==13 && e.ctrlKey)
        CommentsManagerObj.AddComment_Click();
    });
});

var TextHelper = new function() {

     
    var trimN = function (str) {
        if (typeof str !== 'string' || str.length === 0) {
            return '';
        }
        return str.replace(/^\n+|\n+$/g, '');
    }

    var isBlockNode = function (node) {
        if (!node || typeof node === 'undefined' || typeof node.nodeName === 'undefined') {
            return false;
        }
        switch (node.nodeName.toLowerCase()) {
            case 'p':
            case 'h1':
            case 'h2':
            case 'h3':
            case 'h4':
            case 'h5':
            case 'h6':
            case 'ul':
            case 'ol':
            case 'li':
            case 'tr':
            case 'div':
            case 'table':
                return true;
            default:
                return false;
        }
        return false;
    }

    var isNonblockNode = function (node) {
        if (!node || typeof node == 'undefined' || typeof node.nodeName == 'undefined') {
            return false;
        }
        return !isBlockNode(node);
    }

    this.Html2FormattedText = function(node) {
        var content = '', child = null, childrens = node.childNodes;
        for (var i = 0, n = childrens.length; i < n; i++) {
            child = childrens.item(i);
            switch (child.nodeType) {
                case 1:
                case 5:
                    switch (child.nodeName.toLowerCase()) {
                        case 'br':
                            if (child.getAttribute('type') !== 'moz_') {
                                content += '\n';
                            }
                            break;
                        case 'a':
                            var attr = child.getAttribute('href');
                            if (attr) {
                                content += attr;
                            }
                            break;
                        case 'img':
                            var attr = child.getAttribute('alt');
                            if (attr) {
                                content += attr;
                            }
                            break;
                        case 'p':
                        case 'h1':
                        case 'h2':
                        case 'h3':
                        case 'h4':
                        case 'h5':
                        case 'h6':
                        case 'ul':
                        case 'ol':
                        case 'li':
                        case 'tr':
                        case 'div':
                        case 'table':
                            var childContent = trimN(arguments.callee(child));
                            content += (i !== 0 ? '\n' : '') + childContent;
                            if (childContent) {
                                content += isNonblockNode(child.nextSibling) ? '\n' : '';
                            }
                            break;
                        default:
                            content += arguments.callee(child);
                            break;
                    }
                    break;
                case 2:
                case 3:
                case 4:
                    content += child.nodeValue;
                    break;
                default:
                    break;
            }
        }
        return content;
    }

    var htmlEncode = function(source, display, tabs) {
        var i, s, ch, peek, line, result,
		next, endline, push,
		spaces;

        // Stash the next character and advance the pointer
        next = function() {
            peek = source.charAt(i);
            i += 1;
        };

        // Start a new "line" of output, to be joined later by <br />
        endline = function() {
            line = line.join('');
            if (display) {
                // If a line starts or ends with a space, it evaporates in html
                // unless it's an nbsp.
                line = line.replace(/(^ )|( $)/g, '&nbsp;');
            }
            result.push(line);
            line = [];
        };

        // Push a character or its entity onto the current line
        push = function() {
            if (ch < ' ' || ch > '~') {
                line.push('&#' + ch.charCodeAt(0) + ';');
            } else {
                line.push(ch);
            }
        };

        // Use only integer part of tabs, and default to 4
        tabs = (tabs >= 0) ? Math.floor(tabs) : 4;

        result = [];
        line = [];

        i = 0;
        next();
        while (i <= source.length) { // less than or equal, because i is always one ahead
            ch = peek;
            next();

            // HTML special chars.
            switch (ch) {
                case '<':
                    line.push('&lt;');
                    break;
                case '>':
                    line.push('&gt;');
                    break;
                case '&':
                    line.push('&amp;');
                    break;
                case '"':
                    line.push('&quot;');
                    break;
                case "'":
                    line.push('&#39;');
                    break;
                default:
                    // If the output is intended for display,
                    // then end lines on newlines, and replace tabs with spaces.
                    if (display) {
                        switch (ch) {
                            case '\r':
                                // If this \r is the beginning of a \r\n, skip over the \n part.
                                if (peek === '\n') {
                                    next();
                                }
                                endline();
                                break;
                            case '\n':
                                endline();
                                break;
                            case '\t':
                                // expand tabs
                                spaces = tabs - (line.length % tabs);
                                for (s = 0; s < spaces; s += 1) {
                                    line.push(' ');
                                }
                                break;
                            default:
                                // All other characters can be dealt with generically.
                                push();
                        }
                    } else {
                        // If the output is not for display,
                        // then none of the characters need special treatment.
                        push();
                    }
            }
        }
        endline();

        // If you can't beat 'em, join 'em.
        result = result.join('<br />');

        if (display) {
            // Break up contiguous blocks of spaces with non-breaking spaces
            result = result.replace(/ {2}/g, ' &nbsp;');
        }

        // tada!
        return result;
    };

    this.Text2EncodedHtml = function(text) {
        return htmlEncode(text, true, 4);
    }  
}




