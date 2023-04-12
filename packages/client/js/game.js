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

  function newRandomArray(numSquares) {
    let a = [];
    for (let i = 0; i < numSquares; i++) {
      a.push(i);
    }
    shuffle(a);
    return a;
  }

  // We will visualize it as first index picks out a row,
  // and 2nd index picks a column.

  function newFilledPositions(dimensions) {
    const numSquares = dimensions * dimensions;
    const arr = new Array(numSquares);
    arr.fill(0);

    const randomArr = newRandomArray(numSquares);
    // const numPositive = Math.floor(Math.random() * 3) + 12;
    const numPositive =
      Math.floor(Math.random() * 3) + Math.floor(numSquares / 2);

    for (let i = 0; i < numPositive; i++) {
      arr[randomArr[i]] = 1;
    }

    return arr;
  }

  const CellStatus = {
    untouched: 0,
    positive: 1,
    negative: 2,
  };

  class GameInstance {
    constructor(dimensions) {
      this.dimensions = dimensions;
      this.newGame();
    }

    newGame() {
      this.answer = newFilledPositions(this.dimensions);
      this.userAnswer = new Array(this.dimensions * this.dimensions);
      this.userAnswer.fill(CellStatus.untouched);
    }

    getRowHeaderNumbers(arr, rowIndex) {
      let streak = 0;
      const results = [];

      for (let i = 0; i < this.dimensions; i++) {
        if (arr[rowIndex * this.dimensions + i] === CellStatus.positive) {
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

      for (let i = 0; i < this.dimensions; i++) {
        if (arr[colIndex + i * this.dimensions] === CellStatus.positive) {
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

      for (let i = 0; i < this.dimensions; i++) {
        results.push(this.getColumnHeaderNumbers(arr, i));
      }

      for (let i = 0; i < this.dimensions; i++) {
        results.push(this.getRowHeaderNumbers(arr, i));
      }

      return results;
    }

    getIsSolved() {
      const solution = this.getHeaderNumbers();
      const fromUser = this.getHeaderNumbers(this.userAnswer);

      for (let i = 0; i < this.dimensions * 2; i++) {
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

    resetGame() {
      const numSquares = this.dimensions * this.dimensions;
      for (let i = 0; i < numSquares; i++) {
        this.userAnswer[i] = CellStatus.untouched;
      }
    }
  }

  function newElWithClass(tag, className) {
    const el = document.createElement(tag);
    el.className = className;
    return el;
  }

  function rebuildPicrossTableDom(dimensions) {
    const $tbody = $('.picrossTable > tbody');
    $tbody.empty();
    const tbody = $tbody[0];

    const topRow = newElWithClass('tr', 'picrossHeaderRow');
    for (let i = 0; i < dimensions + 1; i++) {
      let el;
      if (i === 0) {
        el = newElWithClass('td', 'headerTopLeft');
      } else if (i === dimensions) {
        el = newElWithClass('td', 'pcrsHeader headerTopRight');
      } else {
        el = newElWithClass('td', 'pcrsHeader headerTop');
      }
      topRow.appendChild(el);
    }
    tbody.appendChild(topRow);

    for (let rowIdx = 0; rowIdx < dimensions; rowIdx++) {
      const rowEl = newElWithClass('tr', `picrossRow${rowIdx}`);
      if (rowIdx === dimensions - 1) {
        $(rowEl).addClass('bottomRow');
      }

      for (let colIdx = 0; colIdx < dimensions + 1; colIdx++) {
        let el;
        if (colIdx === 0) {
          el = newElWithClass('td', 'pcrsHeader');
          if (rowIdx === dimensions - 1) {
            $(el).addClass('headerBottomLeft');
          } else {
            $(el).addClass('headerLeft');
          }
        } else {
          el = document.createElement('td');
          const dataPos = rowIdx * dimensions + colIdx - 1;
          el.setAttribute('data-pos', dataPos);
        }
        rowEl.appendChild(el);
      }

      tbody.appendChild(rowEl);
    }
  }

  window.addEventListener('DOMContentLoaded', () => {
    const picrossDimensionsEl = document.getElementById('picrossDimensions');

    let dimensions = parseInt(picrossDimensionsEl.value, 10);
    let gameInstance = new GameInstance(dimensions);
    let solved = false;
    rebuildPicrossTableDom(dimensions);

    picrossDimensionsEl.addEventListener('change', (e) => {
      dimensions = parseInt(e.target.value, 10);
      gameInstance = new GameInstance(dimensions);
      solved = false;
      rebuildPicrossTableDom(dimensions);
      initNewGame();
    });

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
      for (let i = 0; i < dimensions * 2; i++) {
        headerEls[i].innerHTML = headerNumbers[i].join(
          i < dimensions ? '<br>' : ' '
        );
      }
    }

    function checkGameSolved() {
      if (gameInstance.getIsSolved()) {
        solved = true;

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
      gameInstance.resetGame();

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

    $('.picrossTable').on('contextmenu', function (e) {
      if (e.target.nodeName !== 'TD' || !e.target.hasAttribute('data-pos')) {
        return;
      }

      e.preventDefault();
      if (solved) {
        return;
      }

      const index = parseInt(e.target.getAttribute('data-pos'), 10);

      if (gameInstance.userAnswer[index] === CellStatus.negative) {
        gameInstance.userAnswer[index] = CellStatus.untouched;
        $(e.target).removeClass('positive negative');
      } else {
        gameInstance.userAnswer[index] = CellStatus.negative;
        $(e.target).removeClass('positive').addClass('negative');
      }

      checkGameSolved();
    });

    $('.picrossTable').on('click', function (e) {
      if (e.target.nodeName !== 'TD' || !e.target.hasAttribute('data-pos')) {
        return;
      }

      if (solved) {
        return;
      }

      const index = parseInt(e.target.getAttribute('data-pos'), 10);

      if (gameInstance.userAnswer[index] === CellStatus.positive) {
        gameInstance.userAnswer[index] = CellStatus.untouched;
        $(e.target).removeClass('positive negative');
      } else {
        gameInstance.userAnswer[index] = CellStatus.positive;
        $(e.target).removeClass('negative').addClass('positive');
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
