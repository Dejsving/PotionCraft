// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

function apiUrl(path) {
    return (window._apiBase || '') + path;
}
