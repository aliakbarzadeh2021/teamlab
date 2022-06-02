function ToDoItem() {
    this.id = 0;
    this.title = "";
    this.from = null;
    this.to = null;
    this.isClosed = false;

    this.renderInfo = { balloonX: 0,
        balloonY: 0,
        balloonBackGroundColor: "black",
        balloonBorderColor: "black",
        arcSequence: 0,
        arcBeginAngle: 0,
        arcEndAngle: 0
    };

    this.initArcAngle = function() {

        this.renderInfo.arcBeginAngle = this.from.getHours() * Math.PI / 6 + this.from.getMinutes() * ((2 * Math.PI / 12) / 60);
        this.renderInfo.arcEndAngle = this.to.getHours() * Math.PI / 6 + this.to.getMinutes() * ((2 * Math.PI / 12) / 60);
    };

    this.init = function(title, timeFrom, timeTo) {
        this.title = title;

        var fromDate = new Date();
              
        fromDate.setHours(timeFrom.hour, timeFrom.minutes, 0, 0);
       
        this.from = fromDate;

        var toDate = new Date();
        toDate.setHours(timeTo.hour, timeTo.minutes, 0, 0);
        this.to = toDate;

        this.initArcAngle();
        
    };

}

function Clock(id) {

    this.id = id;
    this.canvas = document.getElementById(id);
    this.context = this.canvas.getContext('2d');
    this.canvas.__object__ = this;

    this.properties = [];
    this.properties['clock.numbers.style.lines.linewidth'] = 13;
    this.properties['clock.face.strokewidth'] = 20;
    this.properties['clock.face.hourtickswidth'] = 30;
    this.properties['clock.face.minutetickswidth'] = 15;
    this.properties['clock.numbers.size'] = 35;

    this.properties['clock.face.minuteshandwidth'] = 8;
    this.properties['clock.face.hourhandwidth'] = 14;
    this.properties['clock.customsegment'] = null;
    this.properties['items'] = new Array();
    this.properties['sequencestep'] = 10;

    this.properties['todo.stroke.linewidth'] = 8;

    this.properties['clock.face.hourticksheight'] = 5;
    this.properties['clock.face.minutesticksheight'] = 5;
    this.properties['clock.radius'] = 250;


    this.centerx = this.canvas.getAttribute("width") / 2;
    this.centery = this.canvas.getAttribute("height") / 2;

    this.customSegmentRadius = function() { return this.properties['clock.radius'] - this.properties['clock.face.strokewidth'] / 2; };
}

/**
* A setter
* 
* @param name  string The name of the property to set
* @param value mixed  The value of the property
*/
Clock.prototype.Set = function(name, value) {
    this.properties[name.toLowerCase()] = value;
}


/**
* A getter
* 
* @param name  string The name of the property to get
*/
Clock.prototype.Get = function(name) {
    return this.properties[name.toLowerCase()];
}


/**
* Progressively draws the clock
*/
Clock.prototype.Draw = function() {

    this.canvas.width = this.canvas.width;

    this.context.save();

    this.context.translate(this.centerx, this.centery);

    this.DrawFace();
    this.DrawHour();
    this.DrawMinute();
    this.DrawNumbers();
    this.DrawCustomSegment();
    this.DrawToDoItems();

    this.context.restore();

    // this.DrawAxis();

}

Clock.prototype.DrawToDoItems = function() {

    this.context.save();
    this.context.rotate(-Math.PI / 2);

    var items = this.Get('items');

    var sequenceStep = this.Get('sequencestep');

    for (var index = 0; index < items.length; index++) {

        var item = items[index];

        this.context.beginPath();

        this.context.lineWidth = this.Get('todo.stroke.lineWidth');
        this.context.lineCap = 'round';
        this.context.strokeStyle = item.renderInfo.balloonBackGroundColor;
        this.context.arc(0, 0, this.Get('clock.radius') + sequenceStep * item.renderInfo.arcSequence, item.renderInfo.arcBeginAngle, item.renderInfo.arcEndAngle, false);
        this.context.stroke();

        this.context.closePath();

    }


    this.context.restore();
}

Clock.prototype.DrawCustomSegment = function() {


    var selectedSegment = this.Get('clock.customsegment');

    if (selectedSegment == null) return;

    var clockwise = selectedSegment.endAngle > selectedSegment.beginAngle;

    this.context.save();

    this.context.rotate(-Math.PI / 2);

    this.context.beginPath();
    this.context.moveTo(0, 0);
    this.context.fillStyle = "rgba(0,85,127,0.5)";
    this.context.lineTo(this.customSegmentRadius() * Math.sin(selectedSegment.beginAngle), this.customSegmentRadius() * Math.cos(selectedSegment.beginAngle));
    this.context.arc(0, 0, this.customSegmentRadius(), Math.PI / 2 - selectedSegment.beginAngle, Math.PI / 2 - selectedSegment.endAngle, clockwise);
    this.context.lineTo(0, 0);

    this.context.fill();
    this.context.restore();
}

Clock.prototype.DrawAxis = function() {


    this.context.save();
    this.context.beginPath();
    this.context.strokeStyle = "white";
    this.context.lineWidth = 2;
    this.context.moveTo(-centerAreaLength, 0);
    this.context.lineTo(centerAreaLength, 0);

    this.context.stroke();
    this.context.closePath();

    this.context.beginPath();
    this.context.moveTo(0, -100);
    this.context.lineTo(0, 100);

    this.context.stroke();
    this.context.closePath();
    this.context.restore();

}

Clock.prototype.DrawMinute = function() {

    var date = new Date();
    var minutes = date.getMinutes();

    this.context.save();


    var minutesHandWidth = this.Get('clock.face.minuteshandwidth');
    var minutesHandLength = this.Get('clock.radius') - this.Get('clock.face.strokewidth') - this.Get('clock.face.hourtickswidth') - this.Get('clock.numbers.size') - 3 * minutesHandWidth;

    this.context.beginPath();
    this.context.rotate(-Math.PI / 2 + minutes * Math.PI / 30);
    this.context.fillStyle = "white";
    this.context.lineWidth = 2;

    var translateOX = minutesHandLength / 3;

    this.context.translate(-translateOX, 0);

    minutesHandLength += translateOX;

    this.context.moveTo(0, -minutesHandWidth / 2);
    this.context.lineTo(minutesHandLength, -minutesHandWidth / 2);
    this.context.lineTo(minutesHandLength + minutesHandWidth / 2, 0);
    this.context.lineTo(minutesHandLength, minutesHandWidth / 2);
    this.context.lineTo(0, minutesHandWidth / 2);

    this.context.fill();
    this.context.closePath();
    this.context.restore();


    this.context.save();
    this.context.beginPath();
    this.context.moveTo(0, 0);
    this.context.fillStyle = "white";
    this.context.arc(0, 0, 2 * minutesHandWidth, 0, 2 * Math.PI, true);
    this.context.fill();
    this.context.closePath();

    this.context.beginPath();
    this.context.fillStyle = "#3FA5D6";

    this.context.arc(0, 0, minutesHandWidth, 0, 2 * Math.PI, true);

    this.context.fill();

    this.context.restore();
}


/**
* Draws the hour hand
*/
Clock.prototype.DrawHour = function() {
    var date = new Date();
    var hour = date.getHours();
    var minutes = date.getMinutes();

    var hourHandWidth = this.Get('clock.face.hourhandwidth');

    var hourHandLength = this.Get('clock.radius') - this.Get('clock.face.strokewidth') - this.Get('clock.face.hourtickswidth') - this.Get('clock.numbers.size') - 3 * hourHandWidth;

    if (hour >= 12) {
        hour -= 12;
    }

    this.context.save();
    this.context.beginPath();
    this.context.rotate(-Math.PI / 2 + hour * Math.PI / 6 + minutes * ((2 * Math.PI / 12) / 60));
    this.context.fillStyle = "#c3ebff";
    this.context.lineWidth = 2;

    var translateOX = hourHandLength / 3;

    this.context.translate(-translateOX, 0);

    hourHandLength += translateOX / 2;

    this.context.moveTo(0, -hourHandWidth / 2);
    this.context.lineTo(hourHandLength, -hourHandWidth / 2);
    this.context.lineTo(hourHandLength + hourHandWidth / 2, 0);
    this.context.lineTo(hourHandLength, hourHandWidth / 2);
    this.context.lineTo(0, hourHandWidth / 2);

    this.context.fill();
    this.context.closePath();

    this.context.restore();

}


Clock.prototype.DrawFace = function() {


    this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);

    // Draw the main circle
    this.context.beginPath();

    var fillStyleGradient = this.context.createLinearGradient(0, -this.Get('clock.radius'), 0, this.Get('clock.radius'));
    fillStyleGradient.addColorStop(0, "#42aadd");
    fillStyleGradient.addColorStop(1, "#3186af");

    this.context.fillStyle = fillStyleGradient;

    var strokeStyleGradient = this.context.createLinearGradient(0, -this.Get('clock.radius'), 0, this.Get('clock.radius'));


    strokeStyleGradient.addColorStop(0, "#e5e5e5");
    strokeStyleGradient.addColorStop(1, "#bababa");
    this.context.lineWidth = this.Get('clock.face.strokewidth');
    this.context.strokeStyle = strokeStyleGradient;

    if (jq.browser.msie)
        this.context.strokeStyle = "#e5e5e5";

    this.context.arc(0, 0, this.Get('clock.radius'), 0, Math.PI * 2, true);
    this.context.fill();
    this.context.stroke();
    this.context.closePath();

    var img = new Image();
    img.src = "data:image/png;base64, iVBORw0KGgoAAAANSUhEUgAAAMgAAAAgCAYAAABXTzdxAAAACXBIWXMAAA7DAAAOwwHHb6hkAAAEf0lEQVR4nO1Yi46jMAwMdP//i29hVWmj8079jE2hbUaK4thOCHSGmC5togrLgfP3xLqZuR+P7I/6DuCegfRctOflfZaVz9xDfi1HinH+jxTauwoE7ys71vwVoqlEVBAeXzTnbcT0qgJZBFuLefNGxpLPE6tCRBhZwntzI2tcElcXyAJ9xo7OG82L5FSg4lR4lj0ipFNxFYF4CX2Ur0JQXjFx8ORYxKkSQIUvGotc66k4SyBZ0ko5kTkL45P8HgFlSjYulvmWyJL36D6bi/ZheJZARkjvFYNXAB6BeOZ45nr2j9B+C00Qo4S3fNl4JKb10ZxSHCmQSlFEyey1Iz4uR9pT5JSKwkusjYw1knNjze9dx7I1X6ZHO4UjBJIRQe+jYvCOV8afbXRNa33cP3f/CM+bdGs8Ya22tUeS0lhjbK1hTgvGNB/GpDHXD6NSIEcIgfN53+hRklu+1eg5uwuGW186hTho5N+YcSf0BjnYc2vQnMbkYkzaV0Q8FafMIUKpEIhUQvQ+KhRLGB5xWISUBMCRnfpv4MMY+jBPEpMkEo8wNtJwbLWdsSWfJsioODR/M2zN5+lD+BqZRMAJzCuO0ZMCx9ETQhIBEloi+L3dhL7bkp8TCycUCk0QvX2D/W34MYZtFez9d487jL0i4e7NOjkXsDWfpw8hK5AOSQharuRDopwpDiQ1JX5vC4ylhvO+mDX7WndQQVDC/2t/CY+CkFq/JzreiE1bL536PlZi09+l/e5xhRgFJaX0AqDxHXIlQdB8rwjCIlntlMtguI48aD3tzWjl0Toey4ZOdq7saIyN14ncV9Uzrf5tLoPsCWIdaZ45jbGbY+wFvgFXiLX2+Aake8cSZyX7vftu7T+xpdKLK688JRYVh/WNgWWVVmbtTS+xtG8S/O6gzwgbnipW6YUvAe2bA58PxjicVmL1i3NHnacmHKoPhet30lFfI7G9/SVm/yFX0ksl2DcZSx/s1ncMV+41pu/77z1HPiQvV5Z1H45HxDDygc6dhJrfsjWfpw+hQiCWELJr4ynDjdGPMS53g7H0t+7It0xrj4LQWiO99Bwkgnn+skUBaPMs0kvxRmLcPjm/dG+cjT2eJJwQUuK4o/oEyczlBKbZTbA9Y4us0l+wnHAs0mtiQFFwIsHn6iEbR9jo2z56Ckgxac+WjX3khEgLo0N7a1WtvSi+SD/is8SCMa/PuybaTbA9v4OHCJpw0B8Rmpf00X1Y94C53vwyHCkQ6TqWGDw5HOky4rLmjgiYy9NsD7jTBG2r9Ii8qavnVvRWrBTPEoh0XY8oRmNH9dEcaWz5ERIJRkRj+bS39ug8rY/G0D4MZwkEESlBzvB5iT8ikCw4okhEsmxvvEKEI9d9Oq4iEAke8kXKmIwdiWk+zR+BJQzJ5zlxcDx6SmXt03F1gWjIktoieHQs+SLxEViEsgTiyfGIKBK7lAg0vLJALBwhAMl/hjAooiLR/NWCemm8s0AiqCyPos80+5E+mp8RzcdgCqQeV3mmH0vqiYmJiYmJiYmJiYmJV8UPRLHRA+54w1QAAAAASUVORK5CYII=";

    this.context.moveTo(0, 0);
    var ctx = this.context;
    var radius = this.Get('clock.radius');
    var centerx = this.centerx;
    var centery = this.centery;
    

   // img.onload = function() {

   // ctx.drawImage(img, centerx - 100, centery + radius);
    //}

    // Draw the small ticks
    this.context.save();
    this.context.rotate(-Math.PI / 2);
    this.context.strokeStyle = "white";
    this.context.lineWidth = this.Get('clock.face.hourticksheight');



    this.context.save();
    this.context.beginPath();

    for (var i = 0; i < 12; i++) {

        this.context.rotate(Math.PI / 6);

        this.context.moveTo(this.Get('clock.radius') - this.Get('clock.face.strokewidth') - this.Get('clock.face.hourtickswidth'), 0);
        this.context.lineTo(this.Get('clock.radius') - this.Get('clock.face.strokewidth'), 0);

    }

    this.context.stroke();
    this.context.restore();

    this.context.lineWidth = this.Get('clock.face.minutesticksheight');


    this.context.save();
    this.context.beginPath();
    this.context.strokeStyle = "#a6d1e6";

    for (var i = 0; i < 60; i++) {
        if (i % 5 != 0) {

            this.context.moveTo(this.Get('clock.radius') - this.Get('clock.face.strokewidth') - this.Get('clock.face.minutetickswidth'), 0);
            this.context.lineTo(this.Get('clock.radius') - this.Get('clock.face.strokewidth'), 0);
        }

        this.context.rotate(Math.PI / 30);
    }
    this.context.stroke();
    this.context.restore();
    this.context.restore();

}

Clock.prototype.DrawNumbers = function() {

    var centerAreaLength = this.Get('clock.radius') - this.Get('clock.face.strokewidth') - this.Get('clock.face.hourtickswidth');
    centerAreaLength -= 5;

    this.Drawtext("12", 0, -centerAreaLength);
    this.Drawtext("15", centerAreaLength, 0);
    this.Drawtext("18", 0, centerAreaLength);
    this.Drawtext("9", -centerAreaLength, 0);

}

/**
* A function which draws some text
*/
Clock.prototype.Drawtext = function(text, x, y) {


    this.context.font = 'bold ' + this.Get('clock.numbers.size') + 'pt Arial';

    var halign = arguments[4] ? arguments[4] : 'left';
    var valign = arguments[5] ? arguments[5] : 'bottom';

    if (x > 0)
        x -= (this.context.measureText(text).width);

    if (x == 0)
        x -= (this.context.measureText(text).width / 2);

    if (y == 0)
        y += (this.Get('clock.numbers.size') / 2);

    if (y < 0)
        y += (this.Get('clock.numbers.size'));


    this.context.save();
    this.context.beginPath();
    this.context.translate(x, y);

    this.context.shadowOffsetX = 1;
    this.context.shadowOffsetY = 1;
    this.context.shadowBlur = 5;
    this.context.shadowColor = 'rgba(255, 0, 0, 0.5)';

    var numbersGradient = this.context.createLinearGradient(this.context.measureText(text).width / 2, -this.Get('clock.numbers.size'), this.context.measureText(text).width / 2, 0);
    numbersGradient.addColorStop(0, "#ffffff");
    numbersGradient.addColorStop(1, "#b8e8ff");

    this.context.fillStyle = numbersGradient;

    this.context.fillText(text, 0, 0);

    this.context.stroke();
    this.context.fill();

    this.context.restore();
}



var clockManager = (function() {

    var rememberMousePosition;
    var clockObject;
    var selectedSegment;
    var colorArray = [{ color: "#c9e5d0", borderColor: "#bddfc6" },
                      { color: "#ddefd0", borderColor: "#d3e9c4" },
                      { color: "#f4f7cf", borderColor: "#edf1c2" },
                      { color: "#fef7cf", borderColor: "#f9f0c1" },
                      { color: "#fee7c7", borderColor: "#fadeb8" },
                      { color: "#fbd7c1", borderColor: "#f8cdb2" },
                      { color: "#f8cdc1", borderColor: "#f4c1b2" },
                      { color: "#edcddf", borderColor: "#e8c1d6" },
                      { color: "#d2c4de", borderColor: "#cab9d8" },
                      { color: "#c6c6df", borderColor: "#bbbbd9"}];

    var settingsObj;

    function addDocumentEventHandlers() {
        document.onmouseup = mouseUp;
        document.onmousemove = mouseMove;
        document.body.onselectstart = function() { return false }
        document.ondragstart = function() { return false }

    }

    function removeDocumentEventHandlers() {

        document.onmouseup = null;
        document.onmousemove = null;
        document.body.onselectstart = null;
        document.ondragstart = null;
    }

    function pointInArea(x, y) {

        var minRadiusVectorLenght = 100;
        var maxRadiusVectorLenght = clockObject.Get('clock.radius');

        var radiusVectorLength = Math.sqrt(Math.pow(x, 2) + Math.pow(y, 2));

        return (minRadiusVectorLenght <= radiusVectorLength) && (radiusVectorLength <= maxRadiusVectorLenght);

    }



    function getPosition(e) {
        var left = 0
        var top = 0

        while (e.offsetParent) {
            left += e.offsetLeft
            top += e.offsetTop
            e = e.offsetParent
        }

        left += e.offsetLeft
        top += e.offsetTop

        return { x: left, y: top }
    }

    function getRelativeCoordinates(pageX, pageY) {

        var canvasOffset = getPosition(clockObject.canvas);

        var clickOX = pageX - canvasOffset.x - clockObject.centerx;
        var clickOY = pageY - canvasOffset.y - clockObject.centery;

        return { x: -clickOY, y: clickOX };

    }

    function getAbsoluteCoordinates(relativeX, relativeY) {

        var canvasOffset = getPosition(clockObject.canvas);

        var relativeXX = relativeY;
        var relativeYY = -relativeX;

        var absoluteX = canvasOffset.x + relativeXX + clockObject.centerx;
        var absoluteY = canvasOffset.y + relativeYY + clockObject.centery;

        return { x: absoluteX, y: absoluteY };

    }

    function getAngle(pageX, pageY) {

        var angle = 0;

        var relativeCoordinates = getRelativeCoordinates(pageX, pageY);

        if (relativeCoordinates.y < 0 && relativeCoordinates.x > 0)
            angle = Math.atan((relativeCoordinates.x / relativeCoordinates.y)) + Math.PI;
        else if (relativeCoordinates.y < 0 && relativeCoordinates.x < 0)
            angle = Math.atan((relativeCoordinates.x / relativeCoordinates.y)) - Math.PI;
        else
            angle = Math.atan((relativeCoordinates.x / relativeCoordinates.y));

        return angle;

    }

    function angleToTime(angle) {


        var hourDouble = angle / (Math.PI / 6) + 12;

        var hour = parseInt(hourDouble);
        var minutes = Math.round((hourDouble - hour) * 60);


        if (minutes > 55) {
            hour++;
            minutes = 0;
        }

        if (minutes % 5 != 0)
            minutes = Math.round(minutes / 10) * 10;



        return { hour: hour, minutes: minutes };
    }


    function getSelectedTimeInterval() {

        if (selectedSegment == null) return null;

        if (selectedSegment.beginAngle > selectedSegment.endAngle)
            return { begin: angleToTime(Math.PI / 2 - selectedSegment.beginAngle), end: angleToTime(Math.PI / 2 - selectedSegment.endAngle) };
        else
            return { begin: angleToTime(Math.PI / 2 - selectedSegment.endAngle), end: angleToTime(Math.PI / 2 - selectedSegment.beginAngle) };

    }

    function getQuadrans(angel) {

        while (angel > 2 * Math.PI) angel -= 2 * Math.PI;

        if (0 <= angel && angel < (Math.PI / 2)) return 1;

        if ((Math.PI / 2) <= angel && angel < Math.PI) return 2;

        if (Math.PI <= angel && angel < (3 * Math.PI / 2)) return 3;


        if ((3 * Math.PI / 2) <= angel && angel < Math.PI * 2) return 4;


    }

    function timeToString(time) {

        var hour = time.hour;
        var minutes = time.minutes;

        if (time.hour < 10)
            hour = "0" + time.hour;

        if (time.minutes < 10)
            minutes = "0" + time.minutes;

        return hour + ":" + minutes;
    }

    function mouseUp(e) {

        removeDocumentEventHandlers();

        var selectedTime = getSelectedTimeInterval();

        if (selectedTime == null) return false;

        var middleAngle = (selectedSegment.endAngle + selectedSegment.beginAngle) / 2;

        var middleOX = Math.sin(middleAngle) * clockObject.customSegmentRadius();
        var middleOY = Math.cos(middleAngle) * clockObject.customSegmentRadius();

        var absoluteCoordinate = getAbsoluteCoordinates(middleOX, middleOY);

        var createToDoWidth = jq("#create_todo").width();
        var createToDoHeight = jq("#create_todo").height();

        var maxZIndex = 10000;

        jq("div.todo-balloons").each(
           function(index) {
               maxZIndex = Math.max(jq(this).css("z-index"), maxZIndex);
           }
        );


        jq("#create_todo").css(
                       {
                           'position': 'absolute',
                           'top': absoluteCoordinate.y,
                           'left': absoluteCoordinate.x,
                           'z-index': maxZIndex + 1
                       }
                    );

        jq("#create_todo").show();

        jq("#create_todo input.field_time").val("Сегодня с " + timeToString(selectedTime.begin) + "-" + timeToString(selectedTime.end));

        return false;

    }

    function refreshClock(e) {


        clockObject.Set('clock.customsegment', selectedSegment);
        clockObject.Draw();

    }

    function mouseMove(e) {

        var oneSecondAngle = Math.PI / 30;
        e = fixEvent(e);

        var currentAngle = getAngle(e.pageX, e.pageY);
        var prevAngle = getAngle(rememberMousePosition.x, rememberMousePosition.y);

        if (Math.abs(prevAngle - currentAngle) < Math.PI / 30) return;

        if (selectedSegment == null) {

            jq("#create_todo input[type=text]").val("");
            jq("#create_todo").hide();

            selectedSegment = { beginAngle: getAngle(rememberMousePosition.x, rememberMousePosition.y),
                endAngle: 0
            };
        }

        selectedSegment.endAngle = getAngle(e.pageX, e.pageY);

        refreshClock(e);

        return false;
    }

    function mouseDown(e) {

        e = fixEvent(e);

        if (e.which != 1) return;

        var relativeCoordinates = getRelativeCoordinates(e.pageX, e.pageY);

        if (!pointInArea(relativeCoordinates.x, relativeCoordinates.y)) return;

        selectedSegment = null;

        rememberMousePosition = { x: e.pageX, y: e.pageY };

        addDocumentEventHandlers();

        return false;

    }

    function TimeCompare(dateOX, dateOY) {

        var hoursX = dateOX.getHours();
        var hoursY = dateOY.getHours();
        var minutesX = dateOX.getMinutes();
        var minutesY = dateOY.getMinutes();

        if ((hoursX == hoursY) && (minutesX == minutesY)) return 0;

        if (hoursY > hoursX) return -1;
        if (hoursY < hoursX) return 1;

        if (hoursY == hoursX)
            if (minutesY > minutesX)
            return -1;
        else
            return 1;

    }

    function sortToDoItems(todoX, todoY) {

        var timeCompareFrom = TimeCompare(todoX.from, todoY.from);

        if (timeCompareFrom > 0) return 1;
        if (timeCompareFrom < 0) return -1;

        var timeCompareTo = TimeCompare(todoX.to, todoY.to);

        if (timeCompareTo > 0)
            return 1;
        else if (timeCompareTo < 0)
            return -1;

        return 0;

    }

    function preparationToDoItems(items) {

        var colorIndex = 0;

        items.sort(sortToDoItems);

        var IsSetSequence = false;

        for (var i = 0; i < items.length; i++) {

            var incomer = items[i];

            IsSetSequence = false;

            incomer.sequence = 1;

            for (var j = 0; j < i; j++) {
                var comparedItem = items[j];
                if (incomer.from > comparedItem.to) {
                    incomer.sequence = comparedItem.sequence;
                    IsSetSequence = true;
                    break;
                }
            }

            if (!IsSetSequence && i > 0)
                incomer.sequence = items[i - 1].sequence + 1;

            var fromHours = incomer.from.getHours();
            var fromMinutes = incomer.from.getMinutes();
            var toHours = incomer.to.getHours();
            var toMinutes = incomer.to.getMinutes();


            incomer.beginAngle = fromHours * Math.PI / 6 + fromMinutes * ((2 * Math.PI / 12) / 60);

            incomer.endAngle = toHours * Math.PI / 6 + toMinutes * ((2 * Math.PI / 12) / 60);


            // console.log(timeToString(angleToTime(incomer.beginAngle)) + " - " + timeToString(angleToTime(incomer.endAngle)));


            var middleAngle = (incomer.beginAngle + incomer.endAngle) / 2;

            var hypotenuse = clockObject.Get('clock.radius') + (incomer.sequence + 1) * clockObject.Get('sequencestep');

            var middleOX = Math.sin(middleAngle) * hypotenuse;
            var middleOY = Math.cos(middleAngle) * hypotenuse;

            var absoluteCoordinate = getAbsoluteCoordinates(middleOY, middleOX);

            incomer.pageX = absoluteCoordinate.x;
            incomer.pageY = absoluteCoordinate.y;

            incomer.color = colorArray[colorIndex].color;
            incomer.borderColor = colorArray[colorIndex].borderColor;

            colorIndex++;

            if (colorIndex >= colorArray.length) colorIndex = 0;

        }

        return items;
    }

    function buildBalloonHTML(item) {

        var content = item.title;
        var time = "c " + fromDateToString(item.from) + " до " + fromDateToString(item.to);

        var html = "<div  class='todo-balloons round' >";
        html += "<div style='float: left'>";
        html += "  <input type='checkbox' onclick='clockManager.changeStatusToDoItem(this)' />";
        html += "    </div>";
        html += "    <div style='float: left; width:280px'>";

        if (item.isClosed)
            html += "   <div class='text' style='text-decoration:line-through'>";
        else
            html += "   <div class='text'>";

        html += content
        html += "       </div>";
        html += "        <div class='time textMediumDescribe'>";
        html += time;
        html += "       </div>";
        html += "   </div>";
        html += "    <br style='clear:both'/>";
        html += "  </div>";

        return html;


    }

    function fromDateToString(date) {

        var hour = date.getHours();
        var minutes = date.getMinutes();

        return timeToString({ hour: hour, minutes: minutes });

    }

    function drawBalloon(item, includeBalloonOffset) {

        var balloon = jq(buildBalloonHTML(item));

        balloon.data("item", item);

        var dragObject = new DragObject(balloon[0]);
        dragObject.onDragSuccessCallBack = settingsObj.drag_end_handler;

        var settings = {
            tl: { radius: 10 },
            tr: { radius: 10 },
            bl: { radius: 10 },
            br: { radius: 10 }

        }

        var balloonWidth = parseInt(balloon.css("width").replace("px", ""));
      //  var balloonHeight = parseInt(balloon.css("width").replace("px", ""));
      
        // curvyCorners(settings, dragObject);



        if (includeBalloonOffset) {
            var quadran = getQuadrans((item.renderInfo.arcBeginAngle + item.renderInfo.arcEndAngle) / 2);

            switch (quadran) {
                case 1:
                    item.renderInfo.balloonY -= 2 * balloon.height();
                    break;
                case 3:
                    item.renderInfo.balloonX -= balloonWidth;
                    break;
                case 4:
                    item.renderInfo.balloonY -= balloon.height();
                    item.renderInfo.balloonX -= balloonWidth;
                    break;
                case 2:
                    break;
            }
        }

        balloon.css(
                       {
                           'position': 'absolute',
                           'top': item.renderInfo.balloonY,
                           'left': item.renderInfo.balloonX,
                           'display': 'block',
                           'background-color': item.renderInfo.balloonBackGroundColor,
                           'border': '3px solid ' + item.renderInfo.balloonBorderColor,
                           'z-index': 10000 + item.id
                       }
                    );

        jq("div.todo-balloons-list").append(balloon);

        return balloon;

    }

    function HasIntersection(moveItems, elem) {

        for (var i = 0; i < moveItems.length; i++) {

            var comparedItem = moveItems[i];

            if (comparedItem.renderInfo.arcSequence != elem.renderInfo.arcSequence) continue;

            if ((TimeCompare(comparedItem.from, elem.to) > 0) || (TimeCompare(elem.from, comparedItem.to) > 0)) continue;

            return true;

        }


        return false;

    }

    function setSequence(todoItems) {

        //  console.log(timeToString(angleToTime(incomer.renderInfo.arcBeginAngle)) + " - " + timeToString(angleToTime(incomer.renderInfo.arcEndAngle)));

        todoItems.sort(sortToDoItems);

        var moveItems = new Array();
        var moveItemsIndex = new Array();

        for (var i = 0; i < todoItems.length; i++) {

            var incomer = todoItems[i];

            incomer.renderInfo.arcSequence = 1;

            var prevSequence = 0;
            var IsSetSequence = false;
            var firstIntersectionSequence = 0;

            for (var j = i - 1; j >= 0; j--) {
                var comparedItem = todoItems[j];
                if (TimeCompare(incomer.from, comparedItem.to) > 0) continue;

                if (prevSequence - comparedItem.renderInfo.arcSequence > 1) {

                    incomer.renderInfo.arcSequence = comparedItem.renderInfo.arcSequence + 1;
                    if (HasIntersection(moveItems, incomer)) {
                        incomer.renderInfo.arcSequence = 1;
                        continue;
                    }

                    IsSetSequence = true;
                    moveItems.push(incomer);

                    break;
                }
                if (firstIntersectionSequence == 0)
                    firstIntersectionSequence = comparedItem.renderInfo.arcSequence;


                prevSequence = comparedItem.renderInfo.arcSequence;

            }


            if (i > 0 && !IsSetSequence && prevSequence != 0) {
                if (TimeCompare(incomer.from, todoItems[i - 1].to) > 0)
                    incomer.renderInfo.arcSequence = todoItems[i - 1].renderInfo.arcSequence;
                // else if (prevSequence > 1)
                //      incomer.renderInfo.arcSequence = 1;
                else
                    incomer.renderInfo.arcSequence = firstIntersectionSequence + 1;

            }

            // console.log(timeToString(angleToTime(incomer.renderInfo.arcBeginAngle)) + " - " + timeToString(angleToTime(incomer.renderInfo.arcEndAngle)));

        }
    }

    function setArcRenderInfo(todoItems) {

        var IsSetSequence = false;

        items.sort(sortToDoItems);

        for (var i = 0; i < todoItems.length; i++) {

            var incomer = todoItems[i];

            IsSetSequence = false;

            incomer.sequence = 1;

            for (var j = 0; j < i; j++) {
                var comparedItem = todoItems[j];
                if (incomer.from > comparedItem.to) {
                    incomer.sequence = comparedItem.sequence;
                    IsSetSequence = true;
                    break;
                }
            }

            if (!IsSetSequence && i > 0)
                incomer.sequence = todoItems[i - 1].sequence + 1;


            var fromHours = incomer.from.getHours();
            var fromMinutes = incomer.from.getMinutes();
            var toHours = incomer.to.getHours();
            var toMinutes = incomer.to.getMinutes();

            incomer.beginAngle = fromHours * Math.PI / 6 + fromMinutes * ((2 * Math.PI / 12) / 60);
            incomer.endAngle = toHours * Math.PI / 6 + toMinutes * ((2 * Math.PI / 12) / 60);

        }


    }

    function setBallonRenderInfo(todoItem) {

        var items = clockObject.Get('items');

        var colorIndex = items.length;


        while (colorIndex >= colorArray.length)
            colorIndex -= colorArray.length;

        todoItem.renderInfo.balloonBackGroundColor = colorArray[colorIndex].color;
        todoItem.renderInfo.balloonBorderColor = colorArray[colorIndex].borderColor;

        var middleAngle = (todoItem.renderInfo.arcBeginAngle + todoItem.renderInfo.arcEndAngle) / 2;

        var hypotenuse = clockObject.Get('clock.radius') + (todoItem.renderInfo.arcSequence + 1) * clockObject.Get('sequencestep');

        var middleOX = Math.sin(middleAngle) * hypotenuse;
        var middleOY = Math.cos(middleAngle) * hypotenuse;

        var absoluteCoordinate = getAbsoluteCoordinates(middleOY, middleOX);

        todoItem.renderInfo.balloonX = absoluteCoordinate.x;
        todoItem.renderInfo.balloonY = absoluteCoordinate.y;

    }

    return {

        init: function(customSettings) {

            settingsObj = customSettings;
            clockObject = settingsObj.clockObj;

            var todoItems = customSettings.data;

            selectedSegment = clockObject.Get('clock.customsegment');
            setSequence(todoItems);

            for (var index = 0; index < todoItems.length; index++) {

                var todoItem = todoItems[index];
                todoItem.initArcAngle();
                setBallonRenderInfo(todoItem);

                drawBalloon(todoItem);
            }

            clockObject.Set('items', todoItems);
            clockObject.canvas.onmousedown = mouseDown;

        },
        resetClock: function() {

            clockObject.Set('clock.customsegment', null);
            clockObject.Draw();
            jq("#create_todo input[type=text]").val("");
            jq("#create_todo").hide();
        },
        changeStatusToDoItem: function(ui) {

            var balloon = jq(ui).parents("div.todo-balloons")
            var todoItem = balloon.data("item");

            todoItem.isClosed = jq(ui).is(":checked");

            if (todoItem.isClosed)
                balloon.find("div.text").css("text-decoration", "line-through");
            else
                balloon.find("div.text").css("text-decoration", "none");

            settingsObj.todo_change_status_handler(todoItem, todoItem.isClosed);

        },
        insertToDoItem: function() {

            var todoItem = new ToDoItem();
            var selectedTime = getSelectedTimeInterval();

            if (selectedTime == null) return;

            todoItem.init(jq("#create_todo .field_content").val(), selectedTime.begin, selectedTime.end);

            if (jq.trim(todoItem.title) == "") return;
            

            var items = clockObject.Get('items');

            items[items.length] = todoItem;
            

            setSequence(items);
            setBallonRenderInfo(todoItem);

            drawBalloon(todoItem, true);

            var settings = {
                tl: { radius: 10 },
                tr: { radius: 10 },
                bl: { radius: 10 },
                br: { radius: 10 }

            }


            //curvyCorners(settings, '.todo-balloons');


            clockManager.resetClock();


            settingsObj.todo_insert_handler(todoItem);

        },

        insertToDoItemByInfo: function(id, title, timeFrom, timeTo) {


            var todo = new ToDoItem();
           
            todo.id = id;
            todo.init(title, { hour: timeFrom.getHours(), minutes: timeFrom.getMinutes() }, { hour: timeTo.getHours(), minutes: timeTo.getMinutes() });
            todo.initArcAngle();
            var items = clockObject.Get('items');

            items[items.length] = todo;

            setSequence(items);
            setBallonRenderInfo(todo);

            drawBalloon(todo, true);

            var settings = {
                tl: { radius: 10 },
                tr: { radius: 10 },
                bl: { radius: 10 },
                br: { radius: 10 }

            }


            //curvyCorners(settings, '.todo-balloons');


            clockManager.resetClock();

            // settingsObj.todo_insert_handler(todo);

            return todo;

        },

        removeToDoItem: function(id) {


        }

    }
} ());




function DragObject(element) {
 
    element.dragObject = this;
    dragMaster.makeDraggable(element);

    var rememberPosition;
    var mouseOffset;        
            
    this.onDragStart = function(offset) {
        var s = element.style
        rememberPosition = { top: s.top, left: s.left, position: s.position }
        s.position = 'absolute'

        mouseOffset = offset
    }

    this.hide = function() {
        element.style.display = 'none';
    }

    this.show = function() {
        element.style.display = '';
    }

    this.onDragMove = function(x, y) {
        element.style.top = y - mouseOffset.y + 'px';
        element.style.left = x - mouseOffset.x + 'px';
    }

    this.onDragSuccessCallBack = function() { };

    this.onDragSuccess = function(dropTarget) {

        var todoItem = jq(element).data("item");

        todoItem.renderInfo.balloonX = element.style.left;
        todoItem.renderInfo.balloonY = element.style.top;

        //alert("ping");
        this.onDragSuccessCallBack(todoItem, element.style.left, element.style.top);
        //alert("ping");
        //   jq(this).trigger("onDragSuccess");

    }

    this.onDragFail = function() {
    
        var s = element.style;
        s.top = rememberPosition.top;
        s.left = rememberPosition.left;
        s.position = rememberPosition.position;
    }

    this.toString = function() {
        return element.id;
    }

   
    
};
function DropTarget(element) {

    element.dropTarget = this

    this.canAccept = function(dragObject) {
        return true
    }

    this.accept = function(dragObject) {
        this.onLeave()

        dragObject.hide()

        alert("Акцептор '" + this + "': принял объект '" + dragObject + "'")
    }

    this.onLeave = function() {
        element.className = ''
    }

    this.onEnter = function() {
        element.className = 'uponMe'
    }

    this.toString = function() {
        return element.id
    }
};

var dragMaster = (function() {

    var dragObject
    var mouseDownAt

    var currentDropTarget


    function mouseDown(e) {
        e = fixEvent(e)
        if (e.which != 1) return

        mouseDownAt = { x: e.pageX, y: e.pageY, element: this }

        var maxZIndex = 0;
        jq("div.todo-balloons").each(
           function(index) {
               maxZIndex = Math.max(jq(this).css("z-index"), maxZIndex);
           }
        );

        jq(this).css("z-index", maxZIndex + 1);




        addDocumentEventHandlers();

        return false;
    }


    function mouseMove(e) {
        e = fixEvent(e)

        // (1)
        if (mouseDownAt) {
            if (Math.abs(mouseDownAt.x - e.pageX) < 5 && Math.abs(mouseDownAt.y - e.pageY) < 5) {
                return false
            }
            // Начать перенос
            var elem = mouseDownAt.element
            // текущий объект для переноса
            dragObject = elem.dragObject

            // запомнить, с каких относительных координат начался перенос
            var mouseOffset = getMouseOffset(elem, mouseDownAt.x, mouseDownAt.y)
            mouseDownAt = null // запомненное значение больше не нужно, сдвиг уже вычислен

            dragObject.onDragStart(mouseOffset) // начали

        }

        // (2)
        dragObject.onDragMove(e.pageX, e.pageY)

        // (3)
        var newTarget = getCurrentTarget(e)

        // (4)
        if (currentDropTarget != newTarget) {
            if (currentDropTarget) {
                currentDropTarget.onLeave()
            }
            if (newTarget) {
                newTarget.onEnter()
            }
            currentDropTarget = newTarget

        }

        // (5)
        return false
    }


    function mouseUp() {
        if (!dragObject) { // (1)
            mouseDownAt = null
        } else {

            dragObject.onDragSuccess(currentDropTarget);
            //            // (2)
            //            if (currentDropTarget) {
            //                currentDropTarget.accept(dragObject)
            //                dragObject.onDragSuccess(currentDropTarget)
            //            } else {
            //                dragObject.onDragFail()
            //            }

            dragObject = null
        }

        // (3)
        removeDocumentEventHandlers()
    }


    function getMouseOffset(target, x, y) {
        var docPos = getOffset(target)
        return { x: x - docPos.left, y: y - docPos.top }
    }


    function getCurrentTarget(e) {
        // спрятать объект, получить элемент под ним - и тут же показать опять

        if (navigator.userAgent.match('MSIE') || navigator.userAgent.match('Gecko')) {
            var x = e.clientX, y = e.clientY
        } else {
            var x = e.pageX, y = e.pageY
        }
        // чтобы не было заметно мигание - максимально снизим время от hide до show
        dragObject.hide()
        var elem = document.elementFromPoint(x, y)
        dragObject.show()

        // найти самую вложенную dropTarget
        while (elem) {
            // которая может принять dragObject
            if (elem.dropTarget && elem.dropTarget.canAccept(dragObject)) {
                return elem.dropTarget
            }
            elem = elem.parentNode
        }

        // dropTarget не нашли
        return null
    }


    function addDocumentEventHandlers() {
        document.onmousemove = mouseMove
        document.onmouseup = mouseUp
        document.ondragstart = document.body.onselectstart = function() { return false }
    }
    function removeDocumentEventHandlers() {
        document.onmousemove = document.onmouseup = document.ondragstart = document.body.onselectstart = null
    }


    return {

        makeDraggable: function(element) {
            element.onmousedown = mouseDown
        }
    }
} ());


function getOffset(elem) {
    if (elem.getBoundingClientRect) {
        // "правильный" вариант
        return getOffsetRect(elem)
    } else {
        // пусть работает хоть как-то
        return getOffsetSum(elem)
    }
}

function getOffsetSum(elem) {
    var top = 0, left = 0
    while (elem) {
        top = top + parseInt(elem.offsetTop)
        left = left + parseInt(elem.offsetLeft)
        elem = elem.offsetParent
    }

    return { top: top, left: left }
}

function getOffsetRect(elem) {
    // (1)
    var box = elem.getBoundingClientRect()

    // (2)
    var body = document.body
    var docElem = document.documentElement

    // (3)
    var scrollTop = window.pageYOffset || docElem.scrollTop || body.scrollTop
    var scrollLeft = window.pageXOffset || docElem.scrollLeft || body.scrollLeft

    // (4)
    var clientTop = docElem.clientTop || body.clientTop || 0
    var clientLeft = docElem.clientLeft || body.clientLeft || 0

    // (5)
    var top = box.top + scrollTop - clientTop
    var left = box.left + scrollLeft - clientLeft

    return { top: Math.round(top), left: Math.round(left) }
}


function fixEvent(e) {
    // получить объект событие для IE
    e = e || window.event

    // добавить pageX/pageY для IE
    if (e.pageX == null && e.clientX != null) {
        var html = document.documentElement
        var body = document.body
        e.pageX = e.clientX + (html && html.scrollLeft || body && body.scrollLeft || 0) - (html.clientLeft || 0)
        e.pageY = e.clientY + (html && html.scrollTop || body && body.scrollTop || 0) - (html.clientTop || 0)
    }

    // добавить which для IE
    if (!e.which && e.button) {
        e.which = e.button & 1 ? 1 : (e.button & 2 ? 3 : (e.button & 4 ? 2 : 0))
    }

    return e
}