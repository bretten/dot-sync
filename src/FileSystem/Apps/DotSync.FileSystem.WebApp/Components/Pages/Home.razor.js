export const uploadButton = document.getElementById('upload-btn')
export const uploadAlert = document.getElementById('upload-alert')

if (uploadButton) {
    const toast = bootstrap.Toast.getOrCreateInstance(uploadAlert)
    uploadButton.addEventListener('click', () => {
        toast.show()
    })
}