window.quillInterop = {
    editors: {},

    initQuill: function (elementId, placeholder, dotNetRef) {
        var options = {
            theme: 'snow',
            placeholder: placeholder || 'Write something...',
            modules: {
                toolbar: [
                    [{ 'header': [1, 2, 3, false] }],
                    ['bold', 'italic', 'underline', 'strike'],
                    ['blockquote', 'code-block'],
                    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                    [{ 'color': [] }, { 'background': [] }],
                    ['clean']
                ]
            }
        };

        var quill = new Quill('#' + elementId, options);
        this.editors[elementId] = quill;

        // Listen for text changes and notify Blazor
        quill.on('text-change', function () {
            var html = quill.root.innerHTML;
            dotNetRef.invokeMethodAsync('OnContentChanged', html);
        });
    },

    setQuillContent: function (elementId, content) {
        if (this.editors[elementId]) {
            var quill = this.editors[elementId];
            if (quill.root.innerHTML !== content) {
                quill.root.innerHTML = content;
            }
        }
    },

    getQuillContent: function (elementId) {
        if (this.editors[elementId]) {
            return this.editors[elementId].root.innerHTML;
        }
        return "";
    }
};
