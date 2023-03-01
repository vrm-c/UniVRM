mergeInto(LibraryManager.library, {
    WebGLFileDialog: function () {
        const file_input_id = "file-input";
        var file_input = document.getElementById(file_input_id);
        if (!file_input) {
            file_input = document.createElement('input');
            file_input.setAttribute('type', 'file');
            file_input.setAttribute('id', file_input_id);
            // file_input.setAttribute('accept', '.vrm')
            file_input.style.visibility = 'hidden';
            file_input.onclick = function (event) {
                event.target.value = null;
            };
            file_input.onchange = function (event) {
                SendMessage('Canvas', 'FileSelected', URL.createObjectURL(event.target.files[0]));
            }
            document.body.appendChild(file_input);
        }
        file_input.click();
    },
});