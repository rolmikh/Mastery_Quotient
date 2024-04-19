document.addEventListener('DOMContentLoaded', function () {
    const deadlineCheck = document.getElementById('deadlineCheck');
    const deadlineDate = document.getElementById('deadlineDate');

    // Прослушивание изменений состояния флажка
    deadlineCheck.addEventListener('change', function () {
        if (this.checked) {
            // Если флажок выбран, скрыть поле даты
            deadlineDate.style.display = 'none';
        } else {
            // Если флажок не выбран, показать поле даты
            deadlineDate.style.display = 'block';
        }
    });

    // Проверка состояния флажка при загрузке страницы
    if (deadlineCheck.checked) {
        // Если флажок выбран, скрыть поле даты
        deadlineDate.style.display = 'none';
    } else {
        // Если флажок не выбран, показать поле даты
        deadlineDate.style.display = 'block';
    }
});

