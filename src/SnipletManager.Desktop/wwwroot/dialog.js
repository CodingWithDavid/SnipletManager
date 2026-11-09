// Native modal dialogs provide focus trapping, inert background content, and focus restoration.
window.snipletDialog = {
    open(dialog) {
        dialog.oncancel = event => event.preventDefault();
        if (!dialog.open) dialog.showModal();
    },
    close(dialog) {
        if (dialog.open) dialog.close();
    }
};
