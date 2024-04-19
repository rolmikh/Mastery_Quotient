

$(document).ready(function () {
    // Инициализация слайдера
    $('.slider').slick({
        infinite: true, // Бесконечный слайдер
        slidesToShow: 1, // Отображение одного слайда за раз
        slidesToScroll: 1, // Прокрутка одного слайда за раз
        dots: true, // Включение точек для навигации
        pauseOnFocus: true // Пауза при фокусе
    });
});

document.addEventListener('DOMContentLoaded', function () {
    initRadioButtons();
    initCheckBoxes();
});

function initRadioButtons() {
    const addAnswerOptionButton = document.getElementById('addAnswerOptionRadio');
    const answerOptionsContainer = document.getElementById('answerOptionsRadio');
    let answerOptionIndex = 0;  // Начальное значение индекса

    addAnswerOptionButton.addEventListener('click', function () {
        const answerOptionDiv = document.createElement('div');
        answerOptionDiv.classList.add('answer-option');

        const answerOptionInput = document.createElement('input');
        answerOptionInput.type = 'text';
        answerOptionInput.name = 'AnswerOptionViewModels[' + answerOptionIndex + '].AnswerOption';

        const correctAnswerInput = document.createElement('input');
        correctAnswerInput.type = 'hidden';
        correctAnswerInput.name = 'AnswerOptionViewModels[' + answerOptionIndex + '].IsCorrectAnswer';
        correctAnswerInput.value = 'false';  // Устанавливаем значение false по умолчанию

        const radioInput = document.createElement('input');
        radioInput.type = 'radio';
        radioInput.name = 'correctAnswerRadio';  // Устанавливаем одинаковое имя для всех радиокнопок
        radioInput.value = answerOptionIndex.toString(); // Устанавливаем значение радиокнопки равным индексу
        radioInput.addEventListener('click', function () {
            const isChecked = this.checked;
            const allCorrectAnswerInputs = document.querySelectorAll('input[name^="AnswerOptionViewModels"][name$="IsCorrectAnswer"]');
            allCorrectAnswerInputs.forEach(function (input) {
                input.value = 'false';
            });
            correctAnswerInput.value = isChecked ? 'true' : 'false';
        });

        const removeButton = document.createElement('button');
        removeButton.type = 'button';
        removeButton.classList.add('remove-answer-option', "buttonPlus");
        removeButton.textContent = '-';
        removeButton.addEventListener('click', function () {
            answerOptionsContainer.removeChild(answerOptionDiv);
            answerOptionIndex--;  // Уменьшаем индекс при удалении поля
        });

        answerOptionDiv.appendChild(answerOptionInput);
        answerOptionDiv.appendChild(correctAnswerInput);
        answerOptionDiv.appendChild(radioInput);
        answerOptionDiv.appendChild(removeButton);

        answerOptionsContainer.appendChild(answerOptionDiv);

        answerOptionIndex++; // Увеличиваем индекс для нового варианта ответа
    });
}

function initCheckBoxes() {
    const addAnswerOptionButton = document.getElementById('addAnswerOptionCheck');
    const answerOptionsContainer = document.getElementById('answerOptionsCheck');
    let answerOptionIndex = 0;  // Начальное значение индекса

    addAnswerOptionButton.addEventListener('click', function () {
        const answerOptionDivCheck = document.createElement('div');
        answerOptionDivCheck.classList.add('answer-option');

        const answerOptionInputCheck = document.createElement('input');
        answerOptionInputCheck.type = 'text';
        answerOptionInputCheck.name = 'AnswerOptionViewModels[' + answerOptionIndex + '].AnswerOption';


        const correctAnswerInputCheck = document.createElement('input');
        correctAnswerInputCheck.type = 'hidden';
        correctAnswerInputCheck.name = 'AnswerOptionViewModels[' + answerOptionIndex + '].IsCorrectAnswer';
        correctAnswerInputCheck.value = 'false';  // Устанавливаем значение false по умолчанию

        const checkboxInputCheck = document.createElement('input');
        checkboxInputCheck.type = 'checkbox';
        checkboxInputCheck.name = 'correctAnswerCheck';  // Устанавливаем одинаковое имя для всех чекбоксов
        checkboxInputCheck.value = answerOptionIndex.toString(); // Устанавливаем значение чекбокса равным индексу
        checkboxInputCheck.addEventListener('change', function () {
            const isChecked = this.checked;
            correctAnswerInputCheck.value = isChecked ? 'true' : 'false';
        });

        const removeButton = document.createElement('button');
        removeButton.type = 'button';
        removeButton.classList.add('remove-answer-option', "buttonPlus");
        removeButton.textContent = '-';
        removeButton.addEventListener('click', function () {
            answerOptionsContainer.removeChild(answerOptionDivCheck);
            answerOptionIndex--;  // Уменьшаем индекс при удалении поля
        });

        answerOptionDivCheck.appendChild(answerOptionInputCheck);
        answerOptionDivCheck.appendChild(correctAnswerInputCheck);
        answerOptionDivCheck.appendChild(checkboxInputCheck);
        answerOptionDivCheck.appendChild(removeButton);

        answerOptionsContainer.appendChild(answerOptionDivCheck);

        answerOptionIndex++; // Увеличиваем индекс для нового варианта ответа
    });
}