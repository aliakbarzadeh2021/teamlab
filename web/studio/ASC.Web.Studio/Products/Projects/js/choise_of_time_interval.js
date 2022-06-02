ASC.Projects.TimeInterval = (function() {
    // Private Section

    return { // Public Section

        validDate: function() {
            try {
                jq('.textEditCalendar').each(function() {
                    if (jq.datepick.parseDate(jq.datepick.dateFormat, jq(this).val()) == null || jq(this).val() == "")
                        throw '';
                });
            }
            catch (e) {
                return false;
            }

            return true;
        },

        keyPress: function(event) {
            var code;
            if (!e) var e = event;
            if (!e) var e = window.event;
            if (e.keyCode) {
                code = e.keyCode;
            }
            else if (e.which) {
                code = e.which;
            }

            if (code >= 48 && code <= 57)
            {
                jq("#8").attr('selected', 'selected');
                jq("[id$=hiddenCurrentTimeRange]").val(8);
            }

        },

        changeDateRange: function() {
            jq("#8").attr('selected', 'selected');
            jq("[id$=hiddenCurrentTimeRange]").val(8);
        },

        changeTimeRange: function(DateTimeFormat, mask) {
            var index = jq('#selectDate option:selected').val();
            var currentTime = new Date();

            var from_date = jq.datepick.today();
            var to_date = jq.datepick.today();

            switch (parseInt(index)) {
                case 0:   // Today
                    from_date = jq.datepick.formatDate(currentTime);
                    to_date = jq.datepick.formatDate(currentTime);
                    break;

                case 1:  // Yesterday                         
                    currentTime.setDate(currentTime.getDate() - 1);
                    from_date = jq.datepick.formatDate(currentTime);
                    to_date = jq.datepick.formatDate(currentTime);
                    break;

                case 2:  // This week
                    currentTime.setDate(-currentTime.getDay() + currentTime.getDate() + 1);
                    from_date = jq.datepick.formatDate(currentTime);
                    currentTime.setDate(currentTime.getDate() + 6);
                    to_date = jq.datepick.formatDate(currentTime);
                    break;

                case 3:  // Last Week
                    currentTime.setDate(-currentTime.getDay() + 1 + currentTime.getDate() - 7);
                    from_date = jq.datepick.formatDate(currentTime);
                    currentTime.setDate(currentTime.getDate() + 6);
                    to_date = jq.datepick.formatDate(currentTime);
                    break;

                case 4: // This month
                    currentTime.setMonth(currentTime.getMonth() + 1);
                    currentTime.setDate(-1);
                    currentTime.setDate(currentTime.getDate() + 1);
                    to_date = jq.datepick.formatDate(currentTime);
                    currentTime.setDate(1);
                    from_date = jq.datepick.formatDate(currentTime);
                    break;

                case 5: // Last month
                    currentTime.setMonth(currentTime.getMonth());
                    currentTime.setDate(-1);
                    currentTime.setDate(currentTime.getDate() + 1);
                    to_date = jq.datepick.formatDate(currentTime);
                    currentTime.setDate(1);
                    from_date = jq.datepick.formatDate(currentTime);
                    break;

                case 6: // This Year            
                    currentTime.setFullYear(currentTime.getFullYear(), 0, 1);
                    from_date = jq.datepick.formatDate(currentTime);
                    currentTime.setFullYear(currentTime.getFullYear() + 1, 0, 1);
                    currentTime.setDate(0);
                    to_date = jq.datepick.formatDate(currentTime);
                    break;

                case 7: // Last Year
                    currentTime.setFullYear(currentTime.getFullYear() - 1, 0, 1);
                    from_date = jq.datepick.formatDate(currentTime);
                    currentTime.setFullYear(currentTime.getFullYear() + 1, 0, 1);
                    currentTime.setDate(0);
                    to_date = jq.datepick.formatDate(currentTime);
                    break;

                case 8: // Other
                    from_date = jq.datepick.parseDate(jq.datepick.dateFormat, jq('[id$=tbxFromDate]').val());
                    to_date = jq.datepick.parseDate(jq.datepick.dateFormat, jq('[id$=tbxToDate]').val());
                    break;
            }

            jq('[id$=tbxFromDate],[id$=tbxToDate]').datepick('option', 'minDate', null);
            jq('[id$=tbxFromDate],[id$=tbxToDate]').datepick('option', 'maxDate', null);

            jq("[id$=tbxFromDate]").datepick('setDate', from_date);
            jq("[id$=tbxToDate]").datepick('setDate', to_date);

            jq("[id$=hiddenCurrentTimeRange]").val(index);
            jq("#" + index).attr('selected', 'true');

        }
    }
})();