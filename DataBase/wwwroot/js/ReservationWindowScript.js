// Відкриття модального вікна
function openBookingModal() {
    document.getElementById('bookingModal').style.display = 'block';
    document.getElementById('step1').classList.add('active');
}

// Закриття модального вікна
function closeBookingModal() {
    document.getElementById('bookingModal').style.display = 'none';
    resetSteps();
}

// Скидання етапів форми
function resetSteps() {
    document.querySelectorAll('.form-step').forEach(step => step.classList.remove('active'));
}

// Перехід на наступний крок
function submitClientData() {
    const passportNumber = document.getElementById('passportNumber').value;

    fetch('/Booking/CheckClient', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            FirstName: document.getElementById('firstName').value,
            LastName: document.getElementById('lastName').value,
            Birthday: document.getElementById('birthday').value,
            Phone: document.getElementById('phone').value,
            Email: document.getElementById('email').value,
            PassportNumber: passportNumber,
            Address: document.getElementById('address').value
        })
    }).then(response => response.json())
        .then(data => {
            if (data.success) {
                document.getElementById('step1').classList.remove('active');
                document.getElementById('step2').classList.add('active');
            } else {
                alert('Помилка: ' + data.message);
            }
        });
}

// Повернення до попереднього кроку
function previousStep() {
    document.getElementById('step2').classList.remove('active');
    document.getElementById('step1').classList.add('active');
}

// Завершення бронювання
function submitBooking() {
    fetch('/Booking/ReserveRoom', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            StartDate: document.getElementById('startDate').value,
            EndDate: document.getElementById('endDate').value
        })
    }).then(response => response.json())
        .then(data => {
            if (data.success) {
                document.getElementById('step2').classList.remove('active');
                document.getElementById('step3').classList.add('active');
            } else {
                alert('Помилка: ' + data.message);
            }
        });
}
