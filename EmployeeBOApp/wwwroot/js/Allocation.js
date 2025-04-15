$(document).ready(function () {
    $('#successMessage').fadeIn().delay(3000).fadeOut();
    $('#ErrorMessage').fadeIn().delay(3000).fadeOut();
    // Load ShortProjectNames based on the logged-in user
    $.ajax({
        url: '/RequestType/GetUserRelatedProjects',  // Backend endpoint to get projects related to the user
        type: 'GET',
        success: function (data) {
            if (data) {
                // Populate ShortProjectName dropdown with the user-related projects
                var options = '<option value="">Select a project</option>';
                $.each(data, function (index, project) {
                    options += '<option value="' + project.shortProjectName + '">' + project.shortProjectName + '</option>';
                });
                $('#ShortProjectName').html(options);
            }
        }
    });

    // Handle the change event for ShortProjectName
    $('#ShortProjectName').change(function () {
        var selectedProject = $(this).val();

        if (selectedProject) {
            $.ajax({
                url: '/RequestType/GetProjectDetails',
                type: 'GET',
                data: { shortProjectName: selectedProject },
                success: function (data) {
                    if (data) {
                        $('#ProjectID').val(data.projectId);
                        $('#ProjectName').val(data.projectName);
                        $('#ReportingManager').val(data.reportingManager);
                        $('#ProjectManager').val(data.projectManager);
                    } else {
                        $('#ProjectID, #ProjectName, #ReportingManager, #ProjectManager').val('');
                    }
                }
            });
        }
    });

    // Reset the form when the clear button is clicked
    $('#clearButton').click(function () {
        // Reset the entire form
        $('form')[0].reset();

        // Reset the standard select dropdown value
        $('#ShortProjectName').val('').change();  // Reset the value and trigger change

        // Clear all other fields
        $('#ProjectID').val('');
        $('#ProjectName').val('');
        $('#ReportingManager').val('');
        $('#ProjectManager').val('');
        $('#DepartmentID').val('');
        $('#EmpID').val('');
        $('#EmpName').val('');
        $('#StartDate').val('');
        $('#RequestedBy').val('');
        $('#RequestedDate').val('');
    });
});
