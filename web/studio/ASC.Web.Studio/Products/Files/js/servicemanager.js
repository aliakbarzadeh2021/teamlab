if (typeof window.serviceManager === "undefined") {
    window.serviceManager = (function($) {
        var 
            isInit = false,
            servicePath = "",
            docServicePath = "",
            cmdSeparator = '/',
            requestTimeout = 60 * 1000,
            customEvents = {};

        var check_ready = true;
        var queue = 0;
        var ajax_stek = new Array();

        var init = function(path, path2) {
            if (typeof path !== 'string' || path.length === 0) {
                throw 'incorrect service path';
            }

            if (isInit === false) {
                isInit = true;
                servicePath = path[path.length - 1] === '/' ? path : path + '/';
                docServicePath = path2;
            }
        };

        var getRandomId = function(prefix) {
            return (typeof prefix !== "undefined" ? prefix + '-' : "") + Math.floor(Math.random() * 1000000);
        };

        var getUniqueId = function(o, prefix) {
            var iterCount = 0,
                maxIterations = 1000,
                uniqueId = getRandomId();

            while (o.hasOwnProperty(uniqueId) && iterCount++ < maxIterations) {
                uniqueId = getRandomId(prefix);
            }
            return uniqueId;
        };

        var jsonToXml = function(o, parent) {
            var 
                attr = "",
                xml = "";

            if (typeof o === "object") {
                if (o.constructor.toString().indexOf('Array') !== -1) {
                    for (i = 0, n = o.length; i < n; i++) {
                        xml += '<' + parent + '>' + o[i] + '</' + parent + '>';
                    }
                } else {
                    for (var i in o) {
                        if (i.charAt(0) === '_') {
                            attr += ' ' + i.substring(1) + '="' + o[i] + '"';
                        } else {
                            xml += arguments.callee(o[i], i);
                        }
                    }
                    if (typeof parent !== "undefined") {
                        xml = '<' + parent + attr + '>' + xml + '</' + parent + '>';
                    }
                }
            } else if (typeof o === 'string') {
                xml = o;
                if (typeof parent !== "undefined") {
                    xml = '<' + parent + attr + '>' + xml + '</' + parent + '>';
                }
            } else if (typeof o !== "undefined" && typeof o.toString !== "undefined") {
                xml = o.toString();
                if (typeof parent !== "undefined") {
                    xml = '<' + parent + attr + '>' + xml + '</' + parent + '>';
                }
            }
            return xml;
        };

        var execCustomEvent = function(eventType, thisArg, argsArray) {
            eventType = eventType.toLowerCase();
            thisArg = thisArg || window;
            argsArray = argsArray || [];

            if (!customEvents.hasOwnProperty(eventType)) {
                return undefined;
            }
            var customEvent = customEvents[eventType];

            for (var eventId in customEvent) {
                if (customEvent.hasOwnProperty(eventId)) {
                    customEvent[eventId].handler.apply(thisArg, argsArray);
                    if (customEvent[eventId].type & 1) {
                        delete customEvent[eventId];
                    }
                }
            }
        };

        var addCustomEvent = function(eventType, handler, params) {
            if (typeof eventType !== 'string' || typeof handler !== 'function') {
                return undefined;
            }

            eventType = eventType.toLowerCase();

            if (typeof params !== "object") {
                params = {};
            }
            var isOnceExec = params.hasOwnProperty('once') ? params.once : false;

            // collect the flags mask the new handler
            var handlerType = 0;
            handlerType |= +isOnceExec * 1;  // isOnceExec - process once and delete

            if (!customEvents.hasOwnProperty(eventType)) {
                customEvents[eventType] = {};
            }

            var eventId = getUniqueId(customEvents[eventType]);

            customEvents[eventType][eventId] = {
                handler: handler,
                type: handlerType
            };

            return eventId;
        };

        var removeCustomEvent = function(eventType, eventId) {
            if (typeof eventType !== 'string' || typeof eventId === "undefined") {
                return false;
            }

            if (customEvents(eventType) && customEvents[eventType].hasOwnProperty(eventId)) {
                delete userEventHandlers[eventType][eventId];
            }
            return true;
        };

        var getUrl = function() {
            var i = 0,
                n = 0,
                url = servicePath;

            if (arguments.length === 0) {
                return url;
            }
            for (i = 0, n = arguments.length - 1; i < n; i++) {
                url += arguments[i] + cmdSeparator;
            }
            var res = url + arguments[i];

            var now = new Date();
            if (res.search(/\?/) > 0)
                res += "&_=" + now.getTime();
            else
                res += "?_=" + now.getTime();

            return res;
        };

        var getNodeContent = function(o) {
            if (!o || typeof o !== "object") {
                return "";
            }

            return o.text || o.textContent || (function(o) {
                var result = "",
                    childrens = o.childNodes;

                if (!childrens) {
                    return result;
                }
                for (var i = 0, n = childrens.length; i < n; i++) {
                    var child = childrens.item(i);
                    switch (child.nodeType) {
                        case 1:
                        case 5:
                            result += arguments.callee(child);
                            break;
                        case 3:
                        case 2:
                        case 4:
                            result += child.nodeValue;
                            break;
                        default:
                            break;
                    }
                }
                return result;
            })(o);
        };

        var completeRequest = function(eventType, params, dataType, xmlHttpRequest, textStatus) {

            check_ready = true;
            if (ASC.Files.UI && ASC.Files.UI.hideLoading)
                ASC.Files.UI.hideLoading();

            if (textStatus === 'error') {
                var 
                errorMessage = "",
                commentMessage = "",
                messageNode = null,
                innerNode = null;
                innerMessageNode = null;

                if (xmlHttpRequest.responseXML) {
                    messageNode = xmlHttpRequest.responseXML.getElementsByTagName('message')[0];
                    innerNode = xmlHttpRequest.responseXML.getElementsByTagName('inner')[0];
                    if (innerNode) {
                        innerMessageNode = innerNode.getElementsByTagName('message')[0];
                    }
                    if (errorMessage === "") {
                        try {
                            errorMessage = eval('[' + xmlHttpRequest.responseText + ']')[0].Detail;
                        }
                        catch (e) {
                            var div = document.createElement("div");
                            errorMessage = jq("#content", jq(div).html(xmlHttpRequest.responseText)).text();
                        };
                    }
                }
                else if (xmlHttpRequest.responseText) {
                    var div = document.createElement("div");
                    errorMessage = jq("#content", jq(div).html(xmlHttpRequest.responseText)).text();
                    if (errorMessage === "") {
                        try {
                            errorMessage = eval('[' + xmlHttpRequest.responseText + ']')[0].Detail;
                        }
                        catch (e) { };
                    }
                }
                if (messageNode && typeof messageNode === "object") {
                    errorMessage = getNodeContent(messageNode);
                }
                if (innerMessageNode && typeof innerMessageNode === "object") {
                    commentMessage = getNodeContent(innerMessageNode);
                }

                execCustomEvent(eventType, window, [undefined, params, errorMessage, commentMessage]);
                return undefined;
            }

            var data = null;

            try {
                switch (dataType) {
                    case "xml":
                        data = ASC.Controls.XSLTManager.createXML(xmlHttpRequest.responseText);
                        break;

                    case "json":
                        data = jq.parseJSON(xmlHttpRequest.responseText);
                        break;

                    default:
                        data = ASC.Controls.XSLTManager.createXML(xmlHttpRequest.responseXML.xml)
                                || jq.parseJSON(xmlHttpRequest.responseText);
                }
            } catch (e) {
                data = xmlHttpRequest.responseText;
            };

            execCustomEvent(eventType, window, [data, params]);

            if (ajax_stek.length != 0 && check_ready == true) {
                var req = ajax_stek.shift();
                check_ready = false;
                execAjax(req);
            }
            return;
        };

        var getCompleteCallbackMethod = function(eventType, params, dataType) {
            return function() {
                var 
          argsArray = [eventType, params, dataType];
                for (var i = 0, n = arguments.length; i < n; i++) {
                    argsArray.push(arguments[i]);
                }
                completeRequest.apply(this, argsArray);
            };
        };

        var request = function(type, dataType, eventType, params) {

            if (typeof type === "undefined" || typeof dataType === "undefined" || typeof eventType !== 'string') {
                return undefined;
            }

            if (ASC.Files.UI == undefined || ASC.Files.UI.displayLoading == undefined)
                params.showLoading = false;

            var 
            data = {},
            argsArray = [];
            var contentType = (params.ajaxcontentType || "text/xml");

            if (typeof params !== "object") {
                params = {};
            }

            switch (type.toLowerCase()) {
                case "delete":
                case "get":
                    for (var i = 4, n = arguments.length; i < n; i++) {
                        argsArray.push(arguments[i]);
                    }
                    break;
                case "post":
                    data = (contentType == "text/xml" ? jsonToXml(arguments[4]) : arguments[4]);

                    for (var i = 5, n = arguments.length; i < n; i++) {
                        argsArray.push(arguments[i]);
                    }
                    break;
                default:
                    return undefined;
            }

            var req = {
                async: (params.ajaxsync != true),
                data: data,
                type: type,
                dataType: dataType,
                contentType: contentType,
                mimeType: 'text/xml',
                cache: true,
                url: getUrl.apply(this, argsArray),
                timeout: requestTimeout,
                beforeSend: params.showLoading ? ASC.Files.UI.displayLoading() : null,
                complete: getCompleteCallbackMethod(eventType, params, dataType)
            };

            if (ajax_stek.length == 0 && check_ready == true) {
                check_ready = false;
                execAjax(req);
            }
            else {
                ajax_stek.push(req);
            }
        };

        var execAjax = function(req) {
            $.ajax({
                async: req.async,
                data: req.data,
                type: req.type,
                dataType: req.dataType,
                contentType: req.contentType,
                cache: req.cache,
                url: req.url,
                timeout: req.timeout,
                beforeSend: req.beforeSend,
                complete: req.complete
            });
        }

        var createFolder = function(eventType, params) {
            params.ajaxsync = true;
            request("get", "xml", eventType, params, 'folders', params.parentFolderID, 'create?title=' + encodeURIComponent(params.title));
        };

        var getFolderInfo = function(eventType, params) {
            params.ajaxsync = true;
            request("get", "xml", eventType, params, 'folders', params.id);
        };

        var getFile = function(eventType, params) {
            params.ajaxsync = true;
            request("get", "xml", eventType, params, 'folders', 'files', params.id, 'lastversion?setThumbnailURLBool=' + (params.setThumbnailURLBool == true));
        };

        var getFolderItems = function(eventType, params, data) {
            params.ajaxsync = params.append != true;
            params.showLoading = true;
            request("post", "xml", eventType, params, data, 'folders', params.id + '?from=' + params.from + '&count=' + params.count +
                    '&filter=' + params.filter + '&subjectID=' + params.subject + '&setThumbnailURLBool=' + params.setThumbnailURLBool);
        };

        var renameFolder = function(eventType, params) {
            request("get", "xml", eventType, params, 'folders', params.id, 'rename?title=' + encodeURIComponent(params.newname));
        };

        var renameFile = function(eventType, params) {
            request("get", "xml", eventType, params, 'folders', 'files', params.id, 'rename?title=' + encodeURIComponent(params.newname));
        };

        var deleteItem = function(eventType, params, data) {
            request("post", "json", eventType, params, data, 'folders', 'files?action=delete');
        }

        var emptyTrash = function(eventType, params) {
            request("get", "json", eventType, params, 'emptytrash');
        }

        var getFileHistory = function(eventType, params, data) {
            request("post", "xml", eventType, params, data, 'folders', 'files', params.id, 'history');
        };

        var moveItems = function(eventType, params, data) {
            request("post", "json", eventType, params, data, 'moveorcopy?dfID=' + params.destID + '&ow=' + (params.overwrite == true) + "&ic=" + (params.isCopyOperation == true));
        };

        var moveFilesCheck = function(eventType, params, data) {

            request("post", "json", eventType, params, data, 'folders', 'files', params.destID, 'moveOrCopyFilesCheck');
        };

        var download = function(eventType, params, data) {
            params.showLoading = true;
            request("post", "json", eventType, params, data, 'bulkdownload');
        };

        var terminateTasks = function(eventType, params) {
            request("get", "json", eventType, params, 'tasks?terminate');
        };

        var getTasksStatuses = function(eventType, params) {
            request("get", "json", eventType, params, 'tasks', 'statuses');
        };

        var setCurrentVersion = function(eventType, params) {
            request("get", "xml", eventType, params, 'folders', 'files', params.id, 'updateToVersion?version=' + params.version);
        };

        var createNewFile = function(eventType, params) {
            params.ajaxsync = true;
            request("get", "xml", eventType, params, 'folders', params.folderID, 'files', 'createfile?title=' + encodeURIComponent(params.fileTitle));
        };

        var checkEditFile = function(eventType, params) {
            request("get", "json", eventType, params, 'makeeditfile?fileID=' + params.fileID);
        };

        var trackEditFile = function(eventType, params) {
            request("get", "json", eventType, params, 'trackeditfile?fileID=' + params.fileID + "&docKey=" + params.docKey + "&isFinish=" + (params.finish == true));
        };

        /////////////////////
        var openFile = function(id, title, ticket) {
            if (ASC.Files.Plugin == undefined) return;

            var sitePath = ASC.Files.Common.getSitePath();

            var url = jq.format(servicePath + "folders/files?id={0}&ticket={1}&title={2}", id, ticket, encodeURIComponent(ASC.Files.Utils.clippingTitle(title, false, false, true)));

            ASC.Files.Plugin.RunOpenOfficeApp(sitePath + url, id);
        };
        ////////////////////

        var getDocPath = function() {
            return docServicePath;
        }
        var getServicePath = function() {
            return servicePath;
        }

        return {
            init: init,
            bind: addCustomEvent,
            unbind: removeCustomEvent,
            jsonToXml: jsonToXml,

            request: request,

            createFolder: createFolder,
            createNewFile: createNewFile,

            getFile: getFile,
            getFolderInfo: getFolderInfo,
            getFolderItems: getFolderItems,

            renameFolder: renameFolder,
            renameFile: renameFile,
            deleteItem: deleteItem,
            emptyTrash: emptyTrash,

            download: download,
            terminateTasks: terminateTasks,

            getFileHistory: getFileHistory,
            setCurrentVersion: setCurrentVersion,
            moveFilesCheck: moveFilesCheck,
            moveItems: moveItems,

            getTasksStatuses: getTasksStatuses,

            checkEditFile: checkEditFile,
            trackEditFile: trackEditFile,
            openFile: openFile,

            getDocPath: getDocPath,
            getServicePath: getServicePath
        };
    })(jQuery);
};