export function closeModal(id) {
    // TODO: For each call to this method, the DOM searches for the modal
    const modalElement = document.getElementById(id);
    const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);
    modal.hide();
}

export function showAlert(id, type, message) {
    // TODO: For each call to this method, the DOM searches for the modal
    const container = document.getElementById(id);
    container.innerHTML = '';
    const alert = document.createElement('div');
    alert.classList.add(`alert`);
    alert.classList.add(`alert-${type}`);
    alert.classList.add(`alert-dismissible`);
    alert.setAttribute('role', 'alert');
    alert.innerHTML = [
        `   <div>${message}</div>`,
        '   <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>',
    ].join('');
    container.append(alert);
}