(function () {
  function shuffle(array) {
    let m = array.length;
    let temp;
    let i;

    // While there remain elements to shuffle…
    while (m) {
      // Pick a remaining element…
      i = Math.floor(Math.random() * m);
      m -= 1;

      // And swap it with the current element.
      temp = array[m];
      array[m] = array[i];
      array[i] = temp;
    }

    return array;
  }

  window.shuffle = shuffle;

  window.newRandomArray = () => {
    let a = [];
    for (let i = 0; i < 25; i++) {
      a.push(i);
    }
    shuffle(a);
    return a;
  };

  window.aaa = () => {
    let v = 0;
    let arr = [];
    for (let i = 0; i < 5; i++) {
      const innerArr = [];
      for (let j = 0; j < 5; j++) {
        innerArr.push(v);
        v += 1;
      }
      arr.push(innerArr);
    }
    return arr;
  };

  // We will visualize it as first index picks out a row,
  // and 2nd index picks a column.

  window.newFilledPositions = () => {
    const arr = new Array(25);
    arr.fill(0);

    const randomArr = newRandomArray();
    const numPositive = Math.floor(Math.random() * 3) + 12;
    // const numPositive = Math.floor(Math.random() * 8) + 12;

    for (let i = 0; i < numPositive; i++) {
      arr[randomArr[i]] = 1;
    }

    return arr;
  };

  const CellStatus = {
    untouched: 0,
    positive: 1,
    negative: 2,
  };

  class GameInstance {
    constructor() {
      this.newGame();
    }

    newGame() {
      this.answer = window.newFilledPositions();
      this.userAnswer = new Array(25);
      this.userAnswer.fill(CellStatus.untouched);
    }

    getRowHeaderNumbers(arr, rowIndex) {
      let streak = 0;
      const results = [];

      for (let i = 0; i < 5; i++) {
        if (arr[rowIndex * 5 + i] === CellStatus.positive) {
          streak += 1;
        } else if (streak > 0) {
          results.push(streak);
          streak = 0;
        }
      }

      if (streak > 0) {
        results.push(streak);
      }

      if (results.length === 0) {
        results.push(0);
      }

      return results;
    }

    getColumnHeaderNumbers(arr, colIndex) {
      let streak = 0;
      const results = [];

      for (let i = 0; i < 5; i++) {
        if (arr[colIndex + i * 5] === CellStatus.positive) {
          streak += 1;
        } else if (streak > 0) {
          results.push(streak);
          streak = 0;
        }
      }

      if (streak > 0) {
        results.push(streak);
      }

      if (results.length === 0) {
        results.push(0);
      }

      return results;
    }

    getHeaderNumbers(arr) {
      if (!arr) {
        arr = this.answer;
      }

      const results = [];

      for (let i = 0; i < 5; i++) {
        results.push(this.getColumnHeaderNumbers(arr, i));
      }

      for (let i = 0; i < 5; i++) {
        results.push(this.getRowHeaderNumbers(arr, i));
      }

      return results;
    }

    getIsSolved() {
      const solution = this.getHeaderNumbers();
      const fromUser = this.getHeaderNumbers(this.userAnswer);

      for (let i = 0; i < 10; i++) {
        const solutionArr = solution[i];
        const userArr = fromUser[i];
        if (solutionArr.length !== userArr.length) {
          return false;
        }

        for (let j = 0; j < solutionArr.length; j++) {
          if (solutionArr[j] !== userArr[j]) {
            return false;
          }
        }
      }

      return true;
    }
  }

  window.addEventListener('DOMContentLoaded', () => {
    const gameInstance = new GameInstance();
    let solved = false;

    console.log(gameInstance.getHeaderNumbers());

    function initNewGame() {
      solved = false;
      gameInstance.newGame();
      $('.picrossTable td[data-pos]').removeClass(
        'positive negative complete example'
      );

      const headerNumbers = gameInstance.getHeaderNumbers();

      const headerEls = document.querySelectorAll(
        '.picrossTable td.pcrsHeader'
      );
      for (let i = 0; i < 10; i++) {
        headerEls[i].innerHTML = headerNumbers[i].join(i < 5 ? '<br>' : ' ');
      }
    }

    function checkGameSolved() {
      if (gameInstance.getIsSolved()) {
        solved = true;
        console.log('DONNNNNNNNNNNNNEEEEEEEEEE');

        $('.picrossTable td[data-pos]').each(function () {
          const $this = $(this);
          if (!$this.hasClass('positive')) {
            $this.addClass('negative complete');
          } else {
            $this.addClass('complete');
          }
        });
      }
    }

    function showExampleGame() {
      initNewGame();
      gameInstance.userAnswer = gameInstance.answer.slice();

      const cells = document.querySelectorAll('.picrossTable td[data-pos]');

      for (let i = 0; i < gameInstance.userAnswer.length; i++) {
        if (gameInstance.userAnswer[i] === CellStatus.positive) {
          $(cells[i]).addClass('positive example');
        }
      }

      checkGameSolved();
    }

    $('#restartPicrossGame').on('click', (e) => {
      e.preventDefault();
      solved = false;
      for (let i = 0; i < 25; i++) {
        gameInstance.userAnswer[i] = CellStatus.untouched;
      }

      $('.picrossTable td[data-pos]').each(function () {
        $(this).removeClass('positive negative complete example');
      });
    });

    $('#showExamplePicrossGame').on('click', (e) => {
      e.preventDefault();
      showExampleGame();
    });

    $('#newPicrossGame').on('click', (e) => {
      e.preventDefault();
      initNewGame();
    });

    $('.picrossTable td[data-pos]').on('contextmenu', function (e) {
      e.preventDefault();
      if (solved) {
        return;
      }

      const index = parseInt(this.getAttribute('data-pos'), 10);

      if (gameInstance.userAnswer[index] === CellStatus.negative) {
        gameInstance.userAnswer[index] = CellStatus.untouched;
        $(this).removeClass('positive negative');
      } else {
        gameInstance.userAnswer[index] = CellStatus.negative;
        $(this).removeClass('positive').addClass('negative');
      }

      checkGameSolved();
    });

    $('.picrossTable td[data-pos]').on('click', function (e) {
      // if (true) {
      //   // temp
      //   return;
      // }

      if (solved) {
        return;
      }

      const index = parseInt(this.getAttribute('data-pos'), 10);

      if (gameInstance.userAnswer[index] === CellStatus.positive) {
        gameInstance.userAnswer[index] = CellStatus.untouched;
        $(this).removeClass('positive negative');
      } else {
        gameInstance.userAnswer[index] = CellStatus.positive;
        $(this).removeClass('negative').addClass('positive');
      }

      checkGameSolved();
    });

    $('#togglePicrossExplanation').on('click', () => {
      $('#picrossExplanationSection').toggle();
    });

    $('#playPicrossBtn').on('click', () => {
      $('#sectionPlayPicross').hide();
      $('#fullGameWrapper').show();
    });

    $('#picrossGoToGeneratedSeed').on('click', () => {
      window.location.reload();
    });

    initNewGame();
    // showExampleGame();
  });
})();
