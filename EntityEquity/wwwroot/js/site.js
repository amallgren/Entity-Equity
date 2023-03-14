// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function HideModal(selector) {
    $(selector).modal('hide');
}
function ShowModal(selector) {
    $(selector).modal('show');
}
function ShowInformationModal(content) {
    $('#informationModal .modal-body').html(content);
    $('#informationModal').modal('show');
}
function ApplyDataTable(selector) {
    $(selector).DataTable();
}