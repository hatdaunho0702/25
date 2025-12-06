//$(document).ready(function () {
//    $("#chatBody").append(`<div class="bot">Xin chào! Tôi là trợ lý AI 😊</div>`);
//});

//function toggleChat() {
//    document.getElementById("chatbox").classList.toggle("hidden");
//}

//document.getElementById("chatToggle").onclick = toggleChat;

//function sendMessage() {
//    let msg = $("#userMessage").val().trim();
//    if (!msg) return;

//    $("#chatBody").append(`<div class="user">${msg}</div>`);
//    $("#userMessage").val("");

//    $("#chatBody").scrollTop($("#chatBody")[0].scrollHeight);

//    $.post("/Chat/SendMessage", { message: msg }, function (reply) {

//        $("#chatBody").append(`<div class="bot">${reply}</div>`);
//        $("#chatBody").scrollTop($("#chatBody")[0].scrollHeight);

//    }).fail(function () {
//        $("#chatBody").append(`<div class="bot">Lỗi kết nối đến server!</div>`);
//    });
//}

//$("#userMessage").keyup(function (event) {
//    if (event.key === "Enter") {
//        sendMessage();
//    }
//});

$(document).ready(function () {
    $("#chatBody").append(`<div class="bot">Xin chào! Tôi là trợ lý AI 😊</div>`);
});

function toggleChat() {
    $("#chatbox").toggleClass("hidden");
}

$("#chatToggle").click(toggleChat);

function sendMessage() {
    let msg = $("#userMessage").val().trim();
    if (!msg) return;

    $("#chatBody").append(`
    <div class="user-wrapper">
        <div class="user">${msg}</div>
    </div>
`);
    $("#userMessage").val("");

    $("#chatBody").scrollTop($("#chatBody")[0].scrollHeight);

    $.post("/Chat/SendMessage", { message: msg }, function (reply) {

        $("#chatBody").append(`
    <div class="bot-wrapper">
        <div class="bot">${reply}</div>
    </div>
`);
        $("#chatBody").scrollTop($("#chatBody")[0].scrollHeight);

    }).fail(function () {
        $("#chatBody").append(`<div class="bot">⚠ Lỗi kết nối server!</div>`);
    });
}

$("#userMessage").keyup(function (event) {
    if (event.key === "Enter") sendMessage();
});
