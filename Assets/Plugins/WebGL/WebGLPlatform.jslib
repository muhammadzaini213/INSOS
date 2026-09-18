mergeInto(LibraryManager.library, {

    RequestFullscreenWebGL: function () {
        var canvas = document.getElementById('unity-canvas');
        if (!canvas) return;

        if (canvas.webkitRequestFullscreen) {
            canvas.webkitRequestFullscreen();
        } else if (canvas.requestFullscreen) {
            canvas.requestFullscreen();
        }
    }

});
