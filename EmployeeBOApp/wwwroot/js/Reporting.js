
        $(document).ready(function () {
            $('#reportingForm').submit(function (e) {
                e.preventDefault();

                var formData = $(this).serialize();

                $.ajax({
                    type: 'POST',
                    url: '@Url.Action("SubmitRequest", "Reporting")',
                    data: formData,
                    success: function (response) {
                        if (response.success) {
                            $('#successMessage').show();
                            $('#reportingForm')[0].reset();
                        } else {
                            alert("Submission failed.");
                        }
                    },
                    error: function () {
                        alert("Error occurred.");
                    }
                });
            });

            $('#clearButton2').click(function () {
                $('#reportingForm')[0].reset();
                $('#successMessage').hide();
            });
        });
