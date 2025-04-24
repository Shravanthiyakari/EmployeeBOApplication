$(document).ready(function () {

    // Initialize Select2 for dropdowns
    $('#ShortProjectName').select2({
        placeholder: "-- Select --",
        allowClear: true,
        width: '100%'
    });

    $('#EmployeeName').select2({
        placeholder: "-- Select Employee --",
        allowClear: true,
        width: '100%'
    });

    // When ShortProjectName changes
    $('#ShortProjectName').change(function () {
        const spn = $(this).val();
        const $empSelect = $('#EmployeeName');

        $('#ShortProjectName').select2('close');

        // Reset Employee dropdown
        $empSelect.select2('destroy').empty().append('<option value="">-- Select Employee --</option>');
        $('#EmployeeID, #ProjectCode, #ProjectName, #RequestedBy, #ProjectManager, #DeliveryManager').val('');

        if (spn) {
            $.getJSON('/Deallocation/GetEmployeesByProject', { shortProjectName: spn }, function (data) {
                if (Array.isArray(data) && data.length > 0) {
                    const uniqueEmployees = [...new Map(data.map(emp => [String(emp.empId), emp])).values()];
                    window.employeeData = uniqueEmployees;

                    uniqueEmployees.forEach(emp => {
                        $empSelect.append(`<option value="${emp.empId}">${emp.empName}</option>`);
                    });

                    const proj = uniqueEmployees[0];
                    $('#ProjectCode').val(proj.projectId);
                    $('#ProjectName').val(proj.projectName);
                    $('#RequestedBy').val(proj.requestedBy);
                    $('#ProjectManager').val(proj.projectManager);
                    $('#DeliveryManager').val(proj.deliveryManager);
                }

                $empSelect.select2({
                    placeholder: "-- Select Employee --",
                    allowClear: true,
                    width: '100%'
                });

            }).fail(function () {
                $('#errorMessage').text("Failed to load employee/project data.").fadeIn().delay(4000).fadeOut();
                $('html, body').animate({ scrollTop: 0 }, 'fast');
            });
        }
    });

    // When EmployeeName changes – autofill EmployeeID
    $('#EmployeeName').change(function () {
        const selectedId = $(this).val();
        const emp = window.employeeData?.find(e => String(e.empId) === selectedId);
        $('#EmployeeID').val(emp ? emp.empId : '');
    });

    // Submit form
    $('#deallocationForm').submit(function (e) {
        e.preventDefault();

        $('#successMessage, #errorMessage').hide();

        const requestData = {
            EmpId: $('#EmployeeID').val(),
            EndDate: $('#EndDate').val(),
            RequestedBy: $('#RequestedBy').val(),
            RequestedDate: new Date().toISOString().split('T')[0]
        };

        const $submitButton = $('button[type="submit"]');
        $submitButton.prop('disabled', true);

        $.ajax({
            url: '/Deallocation/SubmitDeallocation',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function (res) {
                if (res.success) {
                    // Clear form fields
                    $('#deallocationForm')[0].reset(); // Resets the form
                    $('#ShortProjectName').val('').trigger('change.select2');
                    $('#EmployeeName').select2('destroy').empty().append('<option value="">-- Select Employee --</option>').select2({
                        placeholder: "-- Select Employee --",
                        allowClear: true,
                        width: '100%'
                    });

                    // Clear additional fields manually if necessary
                    $('#EmployeeID, #ProjectCode, #ProjectName, #RequestedBy, #ProjectManager, #DeliveryManager').val('');
                    $('#EndDate').val(''); // Assuming EndDate is an input field for date

                    // Display success message
                    $('#successMessage').text(res.message).fadeIn();
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                    setTimeout(() => $('#successMessage').fadeOut(), 4000);
                } else {
                    // Display error message
                    $('#errorMessage').text(res.message || "Submission failed.").fadeIn();
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                    setTimeout(() => $('#errorMessage').fadeOut(), 4000);
                }
            },
            error: function () {
                $('#errorMessage').text("An error occurred during submission.").fadeIn();
                $('html, body').animate({ scrollTop: 0 }, 'fast');
                setTimeout(() => $('#errorMessage').fadeOut(), 4000);
            },
            complete: function () {
                $submitButton.prop('disabled', false);
            }
        });
    });

    // Clear button
    $('#clearButton').click(function () {
        $('#deallocationForm')[0].reset();
        $('#ShortProjectName').val('').trigger('change.select2');
        $('#EmployeeName').select2('destroy').empty().append('<option value="">-- Select Employee --</option>').select2({
            placeholder: "-- Select Employee --",
            allowClear: true,
            width: '100%'
        });

        $('#EmployeeID, #ProjectCode, #ProjectName, #RequestedBy, #ProjectManager, #DeliveryManager, #EndDate').val('');
        $('#successMessage, #errorMessage').hide();
    });
});
