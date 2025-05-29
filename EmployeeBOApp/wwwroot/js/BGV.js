 function enableEdit(ticketId) {
     // Enable editing only for BGV ID field; EmpId and EmpName readonly usually
     $('#bgvInput_' + ticketId).prop('readonly', false);
     $('#empNameInput_' + ticketId).prop('readonly', false);
     $('#empIdInput_' + ticketId).prop('readonly', false);

     // Enable submit and delete buttons

     $('#submitBtn_' + ticketId).prop('disabled', false);
     //$('#deleteBtn_' + ticketId).prop('disabled', false);

     // Disable edit button to prevent multiple clicks
     $('#editBtn_' + ticketId).prop('disabled', true);
}
function deleteRequest(ticketId) {
    if (confirm("Are you sure you want to delete this request?")) {
        $.ajax({
            url: '@Url.Action("DeleteRequest", "BGVView")',
            type: 'POST',
            data: {
                id: ticketId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
            },
            success: function () {
                $('#row_' + ticketId).remove();
            },
            error: function () {
                alert("Failed to delete request. Please try again.");
            }
        });
    }
}


    // Optional: Send update to server to mark as InProgress and save initial BGV ID
    $.ajax({
        url: '/BGVView/SubmitTicket',
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


