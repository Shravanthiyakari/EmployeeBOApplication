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

        // Fully reset Employee dropdown
        $empSelect.select2('destroy').empty().append('<option value="">-- Select Employee --</option>');

        // Clear all fields except EmployeeName and EmployeeID
        $('#ProjectCode, #ProjectName, #RequestedBy, #ProjectManager, #DeliveryManager').val('');
        $('#EmployeeID').val('');

        if (spn) {
            $.getJSON('/Deallocation/GetEmployeesByProject', { shortProjectName: spn }, function (data) {
                console.log("Employee data from server:", data); // Debugging

                if (Array.isArray(data) && data.length > 0) {
                    // Deduplicate employees by empId
                    const uniqueEmployees = [...new Map(data.map(emp => [String(emp.empId), emp])).values()];
                    window.employeeData = uniqueEmployees;

                    // Populate EmployeeName dropdown
                    uniqueEmployees.forEach(emp => {
                        $empSelect.append(`<option value="${emp.empId}">${emp.empName}</option>`);
                    });

                    // Autofill project-related fields using first record
                    const proj = uniqueEmployees[0];
                    $('#ProjectCode').val(proj.projectId);
                    $('#ProjectName').val(proj.projectName);
                    $('#RequestedBy').val(proj.requestedBy);
                    $('#ProjectManager').val(proj.projectManager);
                    $('#DeliveryManager').val(proj.deliveryManager);
                }

                // Reinitialize Select2
                $empSelect.select2({
                    placeholder: "-- Select Employee --",
                    allowClear: true,
                    width: '100%'
                });

            }).fail(function () {
                alert("Failed to load employee/project data.");
            });
        }
    });

    // When EmployeeName changes – just autofill EmployeeID
    $('#EmployeeName').change(function () {
        const selectedId = $(this).val();
        const emp = window.employeeData?.find(e => String(e.empId) === selectedId);

        if (emp) {
            $('#EmployeeID').val(emp.empId);
        } else {
            $('#EmployeeID').val('');
        }
    });

    // Submit deallocation request
    $('#deallocationForm').submit(function (e) {
        e.preventDefault();
        $('#successMessage, #formMessage').hide();

        const requestData = {
            EmpId: $('#EmployeeID').val(),
            EndDate: $('#EndDate').val(),
            RequestedBy: $('#RequestedBy').val(),
            RequestedDate: new Date().toISOString().split('T')[0]
        };

        const $submitButton = $('button[type="submit"]');
        $submitButton.prop('disabled', true); // Disable the submit button to prevent double submissions

        $.ajax({
            url: '/Deallocation/SubmitDeallocation',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function (res) {
                const $msg = $('#formMessage');

                if (res.success) {
                    $('#deallocationForm')[0].reset();
                    $('#ShortProjectName').val('').trigger('change.select2');

                    const $empSelect = $('#EmployeeName');
                    $empSelect.select2('destroy').empty().append('<option value="">-- Select Employee --</option>');
                    $empSelect.select2({
                        placeholder: "-- Select Employee --",
                        allowClear: true,
                        width: '100%'
                    });

                    $('#EmployeeID, #ProjectCode, #ProjectName, #RequestedBy, #ProjectManager, #DeliveryManager').val('');
                    $msg.hide();
                    $('#successMessage').fadeIn().delay(3000).fadeOut();
                } else {
                    $msg.removeClass('text-success').addClass('text-danger').text('Submission failed. Please try again.').show();
                }
            },
            error: function () {
                $('#formMessage').removeClass('text-success').addClass('text-danger').text('An error occurred during submission.').show();
            },
            complete: function () {
                $submitButton.prop('disabled', false); // Re-enable the submit button after the request completes
            }
        });
    });

    // Clear button
    $('#clearButton').click(function () {
        $('#deallocationForm')[0].reset();
        $('#ShortProjectName').val('').trigger('change.select2');

        const $empSelect = $('#EmployeeName');
        $empSelect.select2('destroy').empty().append('<option value="">-- Select Employee --</option>');
        $empSelect.select2({
            placeholder: "-- Select Employee --",
            allowClear: true,
            width: '100%'
        });

        $('#EmployeeID, #ProjectCode, #ProjectName, #RequestedBy, #ProjectManager, #DeliveryManager, #EndDate').val('');
        $('#formMessage, #successMessage').hide();
    });
});
