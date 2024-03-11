// Получаем ссылки на модальное окно и кнопки
var modal = document.getElementById('myModal');
var confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
var confirmBackBtn = document.getElementById('confirmBack');

// Функция для открытия модального окна
confirmDeleteBtn.onclick = function () {
    modal.style.display = "block";
}

// Функция для закрытия модального окна при нажатии на кнопку "Отмена"
confirmBackBtn.onclick = function () {
    modal.style.display = "none";
}

// Закрыть модальное окно, если пользователь кликнул вне его области
window.onclick = function (event) {
    if (event.target == modal) {
        modal.style.display = "none";
    }
}
