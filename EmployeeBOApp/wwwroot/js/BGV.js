    function enableEdit(ticketId) {
        var bgvInput = document.getElementById('bgvInput_' + ticketId);
    var submitButton = document.getElementById('submitBtn_' + ticketId);

    // Enable the BGV ID input field and submit button
    if (bgvInput) {
        bgvInput.removeAttribute('disabled');
        }
    if (submitButton) {
        submitButton.removeAttribute('disabled');
    submitButton.classList.remove('btn-secondary');
    submitButton.classList.add('btn-success');
        }

    // Optional: Send update to server to mark as InProgress and save initial BGV ID
    $.ajax({
        url: '/BGVView/EditTicket',
    type: 'POST',
    data: {
        id: ticketId,
    bgvId: bgvInput ? bgvInput.value : ""
            },
    success: function (response) {
        console.log("Ticket status updated to InProgress");
            },
    error: function () {
        alert("An error occurred while updating the ticket.");
            }
        });
    }