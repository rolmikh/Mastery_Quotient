$(document).ready(function () {
    // Инициализация слайдера
    $('.slider').slick({
        infinite: true, // Бесконечный слайдер
        slidesToShow: 1, // Отображение одного слайда за раз
        slidesToScroll: 1, // Прокрутка одного слайда за раз
        dots: true // Включение точек для навигации
    });
});

document.addEventListener('DOMContentLoaded', function () {
    const addAnswerOptionButton = document.getElementById('addAnswerOptionRadio');
    const answerOptionsContainer = document.getElementById('answerOptionsRadio');

    addAnswerOptionButton.addEventListener('click', function () {
        const answerOptionDiv = document.createElement('div');
        answerOptionDiv.classList.add('answer-optionRadio');

        const answerOptionInput = document.createElement('input');
        answerOptionInput.type = 'text';
        answerOptionInput.name = 'answerOptionRadio[]';

        const correctAnswerInput = document.createElement('input');
        correctAnswerInput.type = 'radio';
        correctAnswerInput.name = 'correctAnswerRadio';

        answerOptionDiv.appendChild(answerOptionInput);
        answerOptionDiv.appendChild(correctAnswerInput);

        answerOptionsContainer.appendChild(answerOptionDiv);
    });
});


document.addEventListener('DOMContentLoaded', function () {
    const addAnswerOptionButton = document.getElementById('addAnswerOptionCheck');
    const answerOptionsContainer = document.getElementById('answerOptionsCheck');

    addAnswerOptionButton.addEventListener('click', function () {
        const answerOptionDiv = document.createElement('div');
        answerOptionDiv.classList.add('answer-option');

        const answerOptionInput = document.createElement('input');
        answerOptionInput.type = 'text';
        answerOptionInput.name = 'answerOptionCheck[]';

        const correctAnswerInput = document.createElement('input');
        correctAnswerInput.type = 'checkbox';
        correctAnswerInput.name = 'correctAnswerCheck';

        answerOptionDiv.appendChild(answerOptionInput);
        answerOptionDiv.appendChild(correctAnswerInput);

        answerOptionsContainer.appendChild(answerOptionDiv);
    });
});