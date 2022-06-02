ASC.Files.ImageViewer = (function($) {

    var isInit = false;
    var scalingInProcess = false;
    var action;
    var imageCollection;

    var odimensions = {};
    var ndimensions = {};
    var imgRef;
    var oScale = 0;
    var nScale = 0;
    var scaleStep = 15;
    var rotateAngel = 0;
    var mouseOffset;
    var windowDimensions = {};
    var imageAreaDimensions = {};
    var centerArea = {};
    var dimensionsOffset = {};


    var displayLoading = function() {
        ASC.Files.UI.displayLoading();
        jq("#loading_container").addClass("loading_container_wrapper");
    };

    var hideLoading = function() {
        ASC.Files.UI.hideLoading(true);
        jq("#loading_container").removeClass("loading_container_wrapper");
    };

    var prepareWorkspace = function() {
        ASC.Files.UI.hideLoading(true);
        displayLoading();
        ASC.Files.UI.finishMoveTo({}, {});
        jq("body").addClass("scroll_fix");

        if (jq.browser.msie && jQuery.browser.version <= 7) {
            jq("html").addClass("scroll_fix");
            jq("<style type='text/css'> .studioContentClassOverride{ position: static !important; } </style>").appendTo("head");
            jq("#studioContent").addClass("studioContentClassOverride");
        }

        jq(document).bind('mousewheel', mouseWheelEvent);
        jq(document).keydown(keyDownEvent);
        jq(window).bind('resize', positionParts);

        jq.blockUI({ message: null,
            css: {
                left: '50%',
                top: '50%',
                opacity: '1',
                border: 'none',
                padding: '0px'

            },
            overlayCSS: {
                backgroundColor: 'Black',
                cursor: 'default',
                opacity: '0.5'
            },
            allowBodyStretch: false
        });

        jq("#imageViewerToolbox, #imageViewerClose").show();

        ASC.Files.ImageViewer.fixMobileWidth();
    };

    var resetWorkspace = function() {
        jq("#imageViewerInfo, #imageViewerToolbox, #imageViewerContainer, #imageViewerClose, #other_actions").hide();
        jq("body").removeClass('scroll_fix');
        jq("#loading_container").removeClass("loading_container_wrapper");

        if (jq.browser.msie && jQuery.browser.version <= 7) {
            jq("html").removeClass("scroll_fix");
            jq("#studioContent").removeClass("studioContentClassOverride");

        }
        hideLoading();
        jq.unblockUI();

        ASC.Files.ImageViewer.fixMobileWidth();
    };

    var getnewSize = function(side, nvalue) {
        var otherside = (side == "w") ? "h" : "w";

        if (typeof nvalue == "undefined" || nvalue == null)
            var newSize = ndimensions[otherside] * odimensions[side] / odimensions[otherside];
        else
            var newSize = (/%/.test(nvalue)) ? parseInt(nvalue) / 100 * odimensions[side] : parseInt(nvalue);

        ndimensions[side] = Math.round(newSize);
    };

    //    var foo = function() {

    //        if (nScale == 100) {
    //           jq("#imageViewerToolbox div.toolboxWrapper ul li:nth-child(2)")
    //        }
    //    };

    var setNewDimensions = function(setting, callbackHandle) {

        var sortbysize = (odimensions.w > odimensions.h) ? ["w", "h"] : ["h", "w"];
        setting[sortbysize[0]] = setting.ls;
        setting[sortbysize[1]] = null;

        var sortbyavail = (setting.w) ? ["w", "h"] : (setting.h) ? ["h", "w"] : [];

        var oldDimensions = { w: ndimensions.w, h: ndimensions.h };

        getnewSize(sortbyavail[0], setting[sortbyavail[0]]);
        getnewSize(sortbyavail[1], setting[sortbyavail[1]]);

        scalingInProcess = true;

        if (typeof callbackHandle == "undefined" || callbackHandle == null)
            callbackHandle = function() {
                scalingInProcess = false;
                jq("#imageViewerInfo").hide();
                setTimeout(function() { ASC.Files.ImageViewer.fixMobileWidth(); }, 300);
            };

        if (typeof setting.speed == "undefined" || setting.speed == null)
            setting.speed = 250;

        if (setting.speed != 0) {

            var scaleSign = 1;

            var getDimensionOffset = function(side) {
                var diff = (ndimensions[side] - oldDimensions[side]) / 2;

                if (diff < 0) scaleSign = -1;

                if (diff > 0)
                    return "-=" + diff + "px";
                else
                    return "+=" + (-diff) + "px";
            };

            imgRef.animate({ width: ndimensions.w + 'px', height: ndimensions.h + 'px', left: getDimensionOffset('w'), top: getDimensionOffset('h') },
                       {
                           duration: setting.speed,
                           easing: 'linear',
                           complete: callbackHandle,
                           step: function(now, obj) {
                               jq("#imageViewerInfo span").text(Math.round(nScale + scaleSign * scaleStep * obj.pos) + '%');
                           }
                       });
        }
        else {
            imgRef.css({ width: ndimensions.w + 'px', height: ndimensions.h + 'px' });
            callbackHandle();
        }
    };

    var imageOnLoad = function() {
        hideLoading();

        imgRef[0].style.cssText = "";

        var tempimg = jq('<img src="' + imgRef.attr('src') + '" style="position:absolute; top:0; left:0; visibility:hidden" />').prependTo('body');

        odimensions = { w: tempimg.width(), h: tempimg.height() };

        if (odimensions.w == 0 && odimensions.h == 0) {
            odimensions.w = imgRef.attr("naturalWidth");
            odimensions.h = imgRef.attr("naturalHeight");
        }

        var similarityFactors = { w: odimensions.w / imageAreaDimensions.w, h: odimensions.h / imageAreaDimensions.h };
        var lsf = (similarityFactors.w > similarityFactors.h) ? "w" : "h";

        if (similarityFactors[lsf] > 1)
            oScale = Math.round(imageAreaDimensions[lsf] * 100.0 / odimensions[lsf]);
        else
            oScale = 100;

        nScale = oScale;

        if (action == 'open') {
            ndimensions = { w: 10, h: 10 };

            imgRef.css({
                'display': 'block',
                'left': centerArea.w - ndimensions.w / 2,
                'top': centerArea.h - ndimensions.h / 2
            });

            setNewDimensions({ ls: oScale + "%" });
        }
        else {
            setNewDimensions({ ls: oScale + "%", speed: 0 }, function() {
                scalingInProcess = false;

                imgRef.css({
                    'display': 'block',
                    'left': centerArea.w - ndimensions.w / 2,
                    'top': centerArea.h - ndimensions.h / 2
                });
            });
        }

        tempimg.remove();

        resetImageInfo();

        ASC.Files.ImageViewer.fixMobileWidth();

        //        var fileID = "file_" + imageCollection[imageCollection.selectedIndex].Key;

        //        if (ASC.Files.UI.accessAdmin(fileID))
        //            jq("#other_actions ul li:last").show();
        //        else
        //            jq("#other_actions ul li:last").hide();
    };

    var resetImageInfo = function() {
        if (isImageLoadCompleted())
            jq("#imageViewerToolbox div.imageInfo").text(jq.format("{0} ({1}x{2})", imageCollection[imageCollection.selectedIndex].Value, odimensions.w, odimensions.h));
        else
            jq("#imageViewerToolbox div.imageInfo").html("&nbsp");
    };

    var isImageLoadCompleted = function() {
        return !jQuery.isEmptyObject(odimensions);
    };

    var showImage = function() {
        odimensions = {};
        ndimensions = {};
        rotateAngel = 0;

        jq("#imageViewerInfo, #imageViewerContainer").hide();

        displayLoading();

        var imageID = imageCollection[imageCollection.selectedIndex].Key;

        imgRef[0].onload = imageOnLoad;

        imgRef.attr('src', ASC.Files.Utils.fileViewUrl(imageID));

        if (ASC.Files.UI.accessAdmin("file_" + imageID))
            jq("#other_actions .action_delete").show();
        else
            jq("#other_actions .action_delete").hide();

        if (ASC.Files.Favorites != undefined) {
            if (ASC.Files.Favorites.favoriteId("file_" + imageID) == undefined) {
                initAddFavorite(imageID);
            }
            else {
                initRemoveFromFavorite(imageID);
            }
        }
    };

    var initAddFavorite = function(imageID) {
        var img = jq("#imageViewerFavorite img");
        var button = jq("#imageViewerFavorite");
        var originSrc = img.attr("src").replace("_selected_hover.png", ".png")
                                        .replace("_selected.png", ".png");

        img.attr("src", originSrc)
                    .attr("alt", ASC.Files.Resources.AddToFavorites)
                    .attr("title", ASC.Files.Resources.AddToFavorites);

        button.unbind("click").click(function() {
            ASC.Files.Favorites.saveFavorite({ path: ("file_" + imageID), title: imageCollection[imageCollection.selectedIndex].Value });

            initRemoveFromFavorite(imageID);
        });

    };

    var initRemoveFromFavorite = function(imageID, favID) {

        var img = jq("#imageViewerFavorite img");
        var button = jq("#imageViewerFavorite");
        var originSrc = img.attr("src").replace("_hover.png", ".png")
                                        .replace("_selected.png", ".png")
                                        .replace(".png", "_selected.png");

        img.attr("src", originSrc)
                    .attr("alt", ASC.Files.Resources.RemoveFavorites)
                    .attr("title", ASC.Files.Resources.RemoveFavorites);

        button.unbind("click").click(function() {
            var favID = ASC.Files.Favorites.favoriteId("file_" + imageID);
            if (favID == null)
                return;

            ASC.Files.Favorites.removeFavorite(favID);

            initAddFavorite(imageID);
        });
    };

    var mouseDownEvent = function(event) {
        mouseOffset = { x: event.pageX - imgRef.offset().left, y: event.pageY - imgRef.offset().top };

        jq(document).bind("mouseup", mouseUpEvent);
        jq(document).bind("mousemove", mouseMoveEvent);

        document.ondragstart = function() { return false; }
        document.body.onselectstart = function() { return false; }
        imgRef[0].ondragstart = function() { return false; }

        jq("#imageViewerToolbox, #other_actions").hide();
    };

    var mouseMoveEvent = function(event) {
        imgRef.css({
            'cursor': 'pointer',
            'top': event.pageY - mouseOffset.y,
            'left': event.pageX - mouseOffset.x
        });
    };

    var mouseUpEvent = function() {
        imgRef.css('cursor', 'move');
        jq(document).unbind("mouseup");
        jq(document).unbind("mousemove");

        imgRef[0].ondragstart = null;
        document.ondragstart = null;
        document.body.onselectstart = null;

        jq("#imageViewerToolbox").show();
    };

    var mouseWheelEvent = function(event, delta) {
        if (scalingInProcess) return;

        if (delta > 0)
            ASC.Files.ImageViewer.zoomIn();
        else
            ASC.Files.ImageViewer.zoomOut();

        return false;
    };

    var mouseDoubleClickEvent = function() {
        if (scalingInProcess) return;

        var ls = windowDimensions.w > windowDimensions.h ? windowDimensions.w : windowDimensions.h;

        setNewDimensions({ ls: ls + 'px' });
    }

    var keyDownEvent = function(e) {
        var keyCode = e.keyCode || e.which,
                arrow = { left: 37, up: 38, right: 39, down: 40, esc: 27, home: 36, end: 35, pageUP: 33, pageDown: 34, spaceBar: 32, deleteKey: 46 };

        switch (keyCode) {
            case arrow.left:
                ASC.Files.ImageViewer.prevImage();
                return false;
            case arrow.spaceBar:
            case arrow.right:
                ASC.Files.ImageViewer.nextImage();
                return false;
            case arrow.up:
                ASC.Files.ImageViewer.zoomIn();
                return false;
            case arrow.down:
                ASC.Files.ImageViewer.zoomOut();
                return false;
            case arrow.esc:
                ASC.Files.ImageViewer.closeImageViewer();
                return false;
            case arrow.deleteKey:
                ASC.Files.ImageViewer.deleteImage();
                return false;
            case arrow.home:
            case arrow.end:
            case arrow.pageDown:
            case arrow.pageUP:
                return false;
        }
    }

    var rotateImage = function() {
        if (!jq.browser.msie) {
            var rotateCssAttr = jq.format("rotate({0}deg)", rotateAngel);

            imgRef.css({
                "-moz-transform": rotateCssAttr,
                "-o-transform": rotateCssAttr,
                "-webkit-transform": rotateCssAttr,
                "transform": rotateCssAttr
            });
        }
        else {
            var rad = (rotateAngel * Math.PI) / 180.0;
            var filter = 'progid:DXImageTransform.Microsoft.Matrix(sizingMethod="auto expand", M11 = ' + Math.cos(rad) + ', M12 = ' + (-Math.sin(rad)) + ', M21 = ' + Math.sin(rad) + ', M22 = ' + Math.cos(rad) + ')';

            var imgOffset = imgRef.offset();

            imgRef.css(
            {
                "-ms-filter": filter,
                "filter": filter

            });

            var rotateOffset = { left: -Math.round((imgRef.width() - ndimensions.w) / 2), top: -Math.round((imgRef.height() - ndimensions.h) / 2) };

            imgRef.css(
            {
                'left': imgOffset.left + rotateOffset.left,
                'top': imgOffset.top + rotateOffset.top
            });

            ndimensions.w = imgRef.width();
            ndimensions.h = imgRef.height();
        }

        ASC.Files.ImageViewer.fixMobileWidth();
    };

    var calculateDimensions = function() {
        windowDimensions = { w: jq(window).width(), h: jq(window).height() };

        var centerAreaOX = windowDimensions.w / 2 + jq(window).scrollLeft();
        var centerAreaOY = windowDimensions.h / 2 + jq(window).scrollTop() - jq("#imageViewerToolbox").height() / 2;

        centerArea = { w: centerAreaOX, h: centerAreaOY };

        imageAreaDimensions = { w: windowDimensions.w, h: windowDimensions.h - jq("#imageViewerToolbox").height() };
    };

    var positionParts = function() {
        calculateDimensions();

        jq("#imageViewerInfo").css({
            'left': centerArea.w,
            'top': centerArea.h
        });

        jq("#imageViewerClose").css({
            'left': (centerArea.w - 15) * 2,
            'top': jq(window).scrollTop() + 15
        });

        jq("#imageViewerToolbox").css({
            'top': windowDimensions.h + jq(window).scrollTop() - jq("#imageViewerToolbox").height(),
            'left': '0px',
            "width": (ASC.Controls.Constants.isMobileAgent && jq(document).width() <= 1000) ?
                        "1000px" : ""
        });

        if (jq("#other_actions").is(':visible'))
            ASC.Files.ImageViewer.showOtherActionsPanel();
        //jq("#other_actions_switch").click();

    };

    var toBatchLoader = function() {
        var idList = jq("#imageBatchLoader").data("idList");

        if (jQuery.inArray(imageID, idList) != -1) return;

        idList.push(imageID);

        jq("#imageBatchLoader").append(jq("<img/>").attr("src", ASC.Files.Utils.fileViewUrl(imageID)));
    };

    var fetchImage = function(asc) {
        if (scalingInProcess) return;

        if (imageCollection.length == 0 || imageCollection.selectedIndex < 0) {
            ASC.Files.ImageViewer.closeImageViewer();
            return;
        }

        if (!asc) {
            imageCollection.selectedIndex--;

            if (imageCollection.selectedIndex < 0) {
                imageCollection.selectedIndex = imageCollection.length - 1;
            }

            action = "prevImage";

            var fileId = imageCollection[imageCollection.selectedIndex].Key;
            ASC.Controls.AnchorController.safemove(ASC.Files.ImageViewer.getPreviewUrl(fileId));
        }
        else {
            imageCollection.selectedIndex++;

            if (imageCollection.selectedIndex > imageCollection.length - 1) {
                imageCollection.selectedIndex = 0;
            }

            action = "nextImage";

            var fileId = imageCollection[imageCollection.selectedIndex].Key;
            ASC.Controls.AnchorController.safemove(ASC.Files.ImageViewer.getPreviewUrl(fileId));
        }

        showImage();
    };

    var onGetImageViewerData = function(jsonData, params, errorMessage, commentMessage) {
        if ((typeof errorMessage != "undefined") || jsonData == undefined || (jsonData.length == 0)) {
            ASC.Files.UI.displayInfoPanel(commentMessage || errorMessage, true);
            ASC.Files.ImageViewer.closeImageViewer();
            return undefined;
        }

        imageCollection = jsonData;

        for (var i = 0; i < imageCollection.length; i++) {
            if (imageCollection[i].Key == params.imageID) {
                imageCollection.selectedIndex = i;
                break;
            }
        }

        if (i >= imageCollection.length) {
            ASC.Files.ImageViewer.closeImageViewer();
            return;
        }

        showImage();


    };

    return {
        init: function(imageID) {
            if (isInit === false) {
                isInit = true;

                serviceManager.bind(ASC.Files.TemplateManager.events.GetSiblingsFile, onGetImageViewerData);
                imgRef = jq("#imageViewerContainer");
                imgRef.dblclick(mouseDoubleClickEvent);
                imgRef.mousedown(mouseDownEvent);

                jq(document).click(function(event) {
                    ASC.Files.Common.dropdownRegisterAutoHide(event, "#other_actions_switch", "#other_actions");
                });

                if (ASC.Files.Favorites == undefined)
                    jq("#imageViewerFavorite").remove();

                jq("#imageViewerToolbox ul li img").hover(
                    function() {
                        var img = jq(this);
                        var imgSrc = img.attr("src");
                        if (imgSrc.indexOf("_hover.png") != -1) return;

                        var hoverSrc = imgSrc.replace(".png", "_hover.png");
                        img.attr("src", hoverSrc);

                        jq("#imageViewerToolbox div.imageInfo").text(img.attr("title"));
                    },

                    function() {
                        var img = jq(this);
                        var originSrc = img.attr("src").replace("_hover.png", ".png");
                        img.attr("src", originSrc);

                        resetImageInfo();
                    }
                );
            }

            prepareWorkspace();
            positionParts();

            action = "open";

            serviceManager.request("post",
                               "json",
                               ASC.Files.TemplateManager.events.GetSiblingsFile,
                               { imageID: imageID },
                               { orderBy: ASC.Files.Folders.getOrderByAnchor() },
                               'folders',
                               'files',
                               imageID,
                               'siblings');
        },

        getPreviewUrl: function(fileId) {
            return jq.format("#preview/{0}", fileId);
        },

        showOtherActionsPanel: function() {

            ASC.Files.Common.dropdownToggle(jq("#imageViewerToolbox .toolboxWrapper li:last")[0], 'other_actions');

        },
        closeImageViewer: function() {
            if (isImageLoadCompleted()) {
                setNewDimensions({ ls: '10px', speed: 500 }, resetWorkspace);
            }
            else {
                imgRef[0].onload = null;
                imgRef.attr("src", "");
                resetWorkspace();
            }

            jq(document).unbind('mousewheel');
            jq(document).unbind("keydown");
            jq(window).unbind('resize');

            ASC.Files.UI.madeAnchor(null, ASC.Files.Folders.currentFolderId != 0);
        },

        prevImage: function() {
            fetchImage(false);
        },
        nextImage: function() {
            fetchImage(true);
        },

        rotateLeft: function() {
            if (scalingInProcess) return;

            rotateAngel -= 90;
            rotateImage();
        },

        rotateRight: function() {
            if (scalingInProcess) return;

            rotateAngel += 90;
            rotateImage();
        },

        fullScale: function() {
            if (scalingInProcess || nScale == 100) return;

            nScale = 100;
            jq("#imageViewerInfo span").text(nScale + '%');
            jq("#imageViewerInfo").show();
            setNewDimensions({ ls: nScale + '%' });
        },

        zoomIn: function() {
            if (scalingInProcess) return;

            nScale += scaleStep;
            if (nScale > 1000) nScale = 1000;

            jq("#imageViewerInfo span").text(nScale + '%');
            jq("#imageViewerInfo").show();
            setNewDimensions({ ls: nScale + '%' });
        },

        zoomOut: function() {
            if (scalingInProcess) return;

            nScale -= scaleStep;
            if (nScale < scaleStep) {
                nScale = scaleStep;
                return;
            }

            jq("#imageViewerInfo span").text(nScale + '%');
            jq("#imageViewerInfo").show();
            setNewDimensions({ ls: nScale + '%' });
        },

        deleteImage: function() {
            if (scalingInProcess)
                return;

            var imageID = imageCollection[imageCollection.selectedIndex].Key;

            if (!ASC.Files.UI.accessAdmin("file_" + imageID))
                return;

            var data = {};
            data.entry = new Array();
            ASC.Files.UI.blockItem("content_file_" + imageID, true, ASC.Files.Resources.DescriptRemove);
            data.entry.push("file_" + imageID);

            serviceManager.deleteItem(ASC.Files.TemplateManager.events.DeleteItem, { list: [imageID] }, { stringList: data });

            imageCollection.splice(imageCollection.selectedIndex, 1);
            imageCollection.selectedIndex--;

            ASC.Files.ImageViewer.nextImage();

        },

        downloadImage: function() {
            if (scalingInProcess) return;

            var imageID = imageCollection[imageCollection.selectedIndex].Key;
            window.open(ASC.Files.Utils.fileViewUrl(imageID), 'new', 'fullscreen = 1, resizable = 1, location=1, toolbar=1');

        },

        fixMobileWidth: function() {
            if (!ASC.Controls.Constants.isMobileAgent) return;

            jq("#studioFooter, div.studioTopNavigationPanel, div.blockUI.blockOverlay").css('width', jq(document).width() <= 1000 ? "1000px" : "100%");
        }
    };
})(jQuery);

//        batchLoaderBound = { left: imageCollection.selectedIndex - 5, right: imageCollection.selectedIndex + 5 };

//        if (batchLoaderBound.left < 0) batchLoaderBound.left = 0;
//        if (batchLoaderBound.right > imageCollection.length - 1) batchLoaderBound.right = imageCollection.length - 1;

//        for (var index = batchLoaderBound.left; index < batchLoaderBound.right; index++)
//            toBatchLoader(imageCollection[index]);

//            jq("#imageBatchLoader").data("idList", new Array());
//            toBatchLoader(imageID);
