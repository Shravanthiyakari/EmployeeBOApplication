$(document).ready(function () {
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

        $('#successMessage, #errorMessage').hide();

        const requestData = {
            EmpId: $('#EmpID').val(), // fixed ID name here too!
            EmpName: $('#Name').val(),
            RequestedBy: $('#RequestedBy').val(),
            EndDate: $('#EndDate').val(),
            EndDate: $('#StartDate').val(),
            ProjectId: $('#ProjectCode').val() // ✅ Match the backend property name
        };

        const $submitButton = $('button[type="submit"]');
        $submitButton.prop('disabled', true);

        $.ajax({
            url: '/Allocation/SubmitRequest',
            type: 'POST',
            data: requestData,
            success: function (response) {
                if (response.success) {
                    $('#successMessage')
                        .text(response.message)
                        .fadeIn(); // 👈 fade in
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                    setTimeout(() => $('#successMessage').fadeOut(), 4000); // 👈 fade out after 4s

                    $('#allocationForm')[0].reset();
                } else {
                    $('#errorMessage')
                        .text(response.message)
                        .fadeIn(); // 👈 fade in
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                    setTimeout(() => $('#errorMessage').fadeOut(), 4000); // 👈 fade out after 4s
                }
                $submitButton.prop('disabled', false);
            },
            error: function () {
                $('#errorMessage')
                    .text('Something went wrong!')
                    .fadeIn(); // 👈 fade in
                $('html, body').animate({ scrollTop: 0 }, 'fast');
                setTimeout(() => $('#errorMessage').fadeOut(), 4000); // 👈 fade out after 4s
                $submitButton.prop('disabled', false);
            }
        });
    });
    const empIdInput = document.getElementById("EmpID");
    const errorMessage = document.getElementById("errorMessage");

    empIdInput.addEventListener("input", function () {
        // Always keep "PO" uppercase and at the start
        if (!this.value.toUpperCase().startsWith("P0")) {
            this.value = "P0";
        }

        // Remove any characters after "PO" that aren't digits
        this.value = this.value.substring(0, 2) + this.value.substring(2).replace(/\D/g, '');

        // Enforce max 6 digits after "PO"
        if (this.value.length > 8) {
            this.value = this.value.substring(0, 8);
        }
    });

    document.getElementById("allocationForm").addEventListener("submit", function (e) {
        const empId = empIdInput.value;
        const pattern = /^P0\d{6}$/;

        if (!pattern.test(empId)) {
            e.preventDefault();
            errorMessage.innerText = "Emp ID must start with 'PO' and be followed by exactly 6 digits (e.g., PO123456)";
            errorMessage.style.display = "block";
        } else {
            errorMessage.style.display = "none";
        }
    });
    
    // Clear button
    $('#clearButton1').click(function () {
        $('#allocationForm')[0].reset();
        $('#ShortProjectName').val('').trigger('change.select2');
        $('#EmployeeName').select2('destroy').empty().append('<option value="">-- Select Employee --</option>').select2({
            placeholder: "-- Select Employee --",
            allowClear: true,
            width: '100%'
        });

        $('#EmployeeID, #ProjectCode, #ProjectName, #RequestedBy, #ProjectManager, #DeliveryManager,#StartDate #EndDate').val('');
        $('#successMessage, #errorMessage').hide();
    });
});