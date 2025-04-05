// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(".nav-link").click(function (e) {
    e.preventDefault();
    var url = $(this).attr("href");

    $("#main-content").load(url); // This loads the view into the #main-content div
});

