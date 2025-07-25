﻿ function enableEdit(ticketId) {
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

function submitTicket(event, ticketId) {
    event.preventDefault(); // ⛔ stop normal form submit

    var bgvIdValue = $('#bgvInput_' + ticketId).val();

    $.ajax({
        url: '/BGVView/SubmitTicket',
        type: 'POST',
        data: {
            id: ticketId,
            bgvId: bgvIdValue
        },
        success: function (response) {
            console.log("Ticket status updated to InProgress");
            // Optionally reload or update UI
        },
        error: function () {
            alert("An error occurred while updating the ticket.");
        }
    });
}

