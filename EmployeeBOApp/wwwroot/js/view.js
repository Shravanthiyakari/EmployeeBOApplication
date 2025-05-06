$(document).ready(function () {
    $('#searchQuery').on('keyup', function () {
        liveSearch();
    });
});
function liveSearch() {
    var query = $("#searchQuery").val();

    $.ajax({
        url: '/View/Index', // use URL directly here
        type: 'GET',
        data: { searchQuery: query, page: 1 },
        success: function (result) {
            var newHtml = $(result).find('#requestsTableContainer').html();
            $('#requestsTableContainer').html(newHtml);
            $('.pagination').html($(result).find('.pagination').html());
        }
    });
}
