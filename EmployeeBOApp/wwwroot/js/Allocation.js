$(document).ready(function () {
    // When the Start Date is selected, set the minimum allowed value for the End Date
    $('#StartDate').change(function () {
        var startDate = $(this).val(); 
        console.log("Selected Start Date:", startDate);

        if (startDate) {
            $('#EndDate').attr('min', startDate); 
        }
    });

    // When the ShortProjectName dropdown changes, fetch project details
    $('#ShortProjectName').change(function () {
        var shortName = $(this).val();
        console.log("Selected ShortProjectName:", shortName);

        if (shortName) {
            $.ajax({
                url: '/Allocation/GetProjectDetails',
                type: 'GET',
                data: { shortProjectName: shortName },
                success: function (data) {
                    console.log("Response from server:", data);
                    if (data.success) {
                        $('#ProjectCode').val(data.projectCode);
                        $('#ProjectName').val(data.projectName);
                        $('#ProjectManager').val(data.projectManager);
                        $('#DeliveryManager').val(data.deliveryManager);
                        $('#RequestedBy').val(data.pmEmail);
                        $('#ApprovedBy').val(data.dmEmail);
                    } else {
                        alert(data.message);
                    }
                },
                error: function () {
                    alert('Error fetching project details');
                }
            });
        }
    });

    // Submit form
    $('#allocationForm').submit(function (e) {
        e.preventDefault();

        // Hide previous messages
        $('#successMessage, #errorMessage').hide();

        // Get start and end dates
        const startDate = $('#StartDate').val();
        const endDate = $('#EndDate').val();

        // Prepare data for submission
        const requestData = {
            EmpId: $('#EmpID').val(),
            EmpName: $('#Name').val(),
            RequestedBy: $('#RequestedBy').val(),
            EndDate: endDate,
            StartDate: startDate,
            ProjectId: $('#ProjectCode').val()
        };

        // Disable the submit button during the request
        const $submitButton = $('button[type="submit"]');
        $submitButton.prop('disabled', true);

        // Submit the form data via AJAX
        $.ajax({
            url: '/Allocation/SubmitRequest',
            type: 'POST',
            data: requestData,
            success: function (response) {
                if (response.success) {
                    $('#successMessage')
                        .text(response.message)
                        .fadeIn();
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                    setTimeout(() => $('#successMessage').fadeOut(), 4000);

                    // Reset form
                    $('#allocationForm')[0].reset();

                    // Reset ShortProjectName Select2 dropdown
                    $('#ShortProjectName').val('').trigger('change.select2');

                    // Reset Employee Name Select2 dropdown
                    $('#EmployeeName').select2('destroy').empty().append('<option value="">-- Select Employee --</option>').select2({
                        placeholder: "-- Select Employee --",
                        allowClear: true,
                        width: '100%'
                    });

                    // Clear other fields
                    $('#EmployeeID, #ProjectCode, #ProjectName, #RequestedBy, #ProjectManager, #DeliveryManager, #StartDate, #EndDate').val('');
                } else {
                    $('#errorMessage')
                        .text(response.message)
                        .fadeIn();
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                    setTimeout(() => $('#errorMessage').fadeOut(), 4000);
                }

                $submitButton.prop('disabled', false); // Re-enable the submit button
            },
            error: function () {
                $('#errorMessage')
                    .text('Something went wrong!')
                    .fadeIn();
                $('html, body').animate({ scrollTop: 0 }, 'fast');
                setTimeout(() => $('#errorMessage').fadeOut(), 4000);
                $submitButton.prop('disabled', false);
            }
        });
    });

    // Validate EmpID input (ensure it starts with "PO" and is followed by exactly 6 digits)
    const empIdInput = document.getElementById("EmpID");
    const errorMessage = document.getElementById("errorMessage");

    empIdInput.addEventListener("input", function () {
        if (!this.value.toUpperCase().startsWith("P")) {
            this.value = "P";
        }

        this.value = this.value.substring(0, 2) + this.value.substring(2).replace(/\D/g, '');

        if (this.value.length > 8) {
            this.value = this.value.substring(0, 8);
        }
    });

    // Validate EmpID when form is submitted
    document.getElementById("allocationForm").addEventListener("submit", function (e) {
        const empId = empIdInput.value;
        const pattern = /^P\d{7}$/;

        if (!pattern.test(empId)) {
            e.preventDefault();
            errorMessage.innerText = "Emp ID must start with 'P' and be followed by exactly 6 digits (e.g., PO123456)";
            errorMessage.style.display = "block";
        } else {
            errorMessage.style.display = "none";
        }
    });

    // Clear button functionality
    $('#clearButton1').click(function () {
        $('#allocationForm')[0].reset();

        $('#ShortProjectName').val('').trigger('change.select2');

        $('#EmployeeName').select2('destroy').empty().append('<option value="">-- Select Employee --</option>').select2({
            placeholder: "-- Select Employee --",
            allowClear: true,
            width: '100%'
        });

        $('#EmployeeID, #ProjectCode, #ProjectName, #RequestedBy, #ProjectManager, #DeliveryManager, #StartDate, #EndDate').val('');

        $('#successMessage, #errorMessage').hide();
    });
});
