mergeInto(LibraryManager.library, {
    WebGL_VRM0X_SimpleViewer_FileDialog: function (_target, _message) {
        const target = UTF8ToString(_target);
        const message = UTF8ToString(_message);
        const file_input_id = "file-input";
        var file_input = document.getElementById(file_input_id);
        if (!file_input) {
            file_input = document.createElement('input');
            file_input.setAttribute('type', 'file');
            file_input.setAttribute('id', file_input_id);
            file_input.style.visibility = 'hidden';
            file_input.onclick = function (event) {
                event.target.value = null;
            };
            file_input.onchange = function (event) {
                console.log('SendMessage', target, message);
                SendMessage(target, message, URL.createObjectURL(event.target.files[0]));
            }
            document.body.appendChild(file_input);
        }
        file_input.click();
    },
});
