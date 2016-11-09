window.addEventListener('load', function () {
  // zmiana kontekstu uzytkownika
  document.querySelector('.user-selection').addEventListener('change', function (e) {
    location.href = '/?user=' + e.target.value;
  });
});
