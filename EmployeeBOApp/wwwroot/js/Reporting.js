$(document).ready(function () {
    // Initialize select2 for all dropdowns
    $('.select2').select2();

    // Handle Short Project Name change
    $('#ShortProjectName').change(function () {
        const selectedProject = $(this).val();

        if (selectedProject) {
            // Fetch project details
            $.get('/Reporting/GetProjectDetails', { shortProjectName: selectedProject }, function (data) {
                if (data) {
                    $('#ProjectID').val(data.projectId);
                    $('#ProjectName').val(data.projectName);
                    $('#DepartmentID').val(data.departmentId);
                    $('#ReportingManager').val(data.reportingManager);
                    $('#ProjectManager').val(data.projectManager);
                }
            });

            // Fetch employees for the selected project
            $.get('/Reporting/GetEmployeesByProject', { shortProjectName: selectedProject }, function (data) {
                let options = '<option value="">-- Select Employee --</option>';
                data.forEach(emp => {
                    options += `<option value="${emp.empName}" data-empid="${emp.empId}">${emp.empName}</option>`;
                });
                $('#EmpName').html(options).select2();

                $('#EmpID').val('');
            });
        } else {
            // Reset fields if no project is selected
            $('#EmpName').html('<option value="">-- Select Employee --</option>').select2();
            $('#EmpID').val('');
            $('#ProjectID').val('');
            $('#ProjectName').val('');
            $('#DepartmentID').val('');
            $('#ReportingManager').val('');
            $('#ProjectManager').val('');
            $('#StartDate').val('');
        }
    });

    // Set Employee ID on EmpName change
    $('#EmpName').change(function () {
        const empId = $(this).find(':selected').data('empid');
        $('#EmpID').val(empId || '');
    });

    // Clear button
    $('#clearButton2').click(function () {
        $('#reportingForm')[0].reset();
        $('#ShortProjectName').val('').trigger('change.select2');
        $('#EmpName').html('<option value="">-- Select Employee --</option>').select2();
        $('#EmpID').val('');
        $('#successMessage, #errorMessage').hide();
        $('#RequestType').trigger('change'); // Ensure readonly logic re-applies
    });

    // Submit form
    $('#reportingForm').submit(function (e) {
        e.preventDefault();

        $('#successMessage, #errorMessage').hide();

        const requestData = {
            EmpId: $('#EmpID').val(),
            EmpName: $('#Name').val(),
            RequestedBy: $('#RequestedBy').val(),
            ReportingManager: $('#ReportingManager').val(),
            ProjectManager: $('#ProjectManager').val(),
            ProjectId: $('#ProjectCode').val(),
            ProjectName: $('#ProjectName').val(),
            RequestType: $('#RequestType').val(),
            StartDate: $('#StartDate').val()
        };

        const $submitButton = $('button[type="submit"]');
        $submitButton.prop('disabled', true);

        $.ajax({
            url: '/Reporting/SubmitRequest',
            type: 'POST',
            data: requestData,
            success: function (response) {
                if (response.success) {
                    $('#successMessage').text(response.message).fadeIn();
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                    setTimeout(() => $('#successMessage').fadeOut(), 4000);

                    // Clear the form after success
                    $('#reportingForm')[0].reset();
                    $('#ShortProjectName').val('').trigger('change.select2');
                    $('#EmpName').html('<option value="">-- Select Employee --</option>').select2();
                    $('#EmpID').val('');
                    $('#RequestType').trigger('change'); // Ensure readonly logic re-applies
                }

                else {
                    $('#errorMessage').text(response.message).fadeIn();
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                    setTimeout(() => $('#errorMessage').fadeOut(), 4000);
                }
                $submitButton.prop('disabled', false);
            },
            error: function () {
                $('#errorMessage').text('Something went wrong!').fadeIn();
                $('html, body').animate({ scrollTop: 0 }, 'fast');
                setTimeout(() => $('#errorMessage').fadeOut(), 4000);
                $submitButton.prop('disabled', false);
            }
        });
    });

    // Enable DepartmentID input only if RequestType is DepartmentChange
    $('#RequestType').on('change', function () {
        const selectedValue = $(this).val();
        if (selectedValue === 'DepartmentChange') {
            $('#DepartmentID').prop('readonly', false);
        } else {
            $('#DepartmentID').prop('readonly', true);
        }
    });

    // Trigger change event on page load
    $('#RequestType').trigger('change');
});
