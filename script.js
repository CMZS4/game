const symbols = ["🍎", "🍌", "🍇", "🍉", "🍒", "🥝", "🍍", "🥥"];

const board = document.getElementById("game-board");
const statusText = document.getElementById("status");
const moveCountEl = document.getElementById("move-count");
const timerEl = document.getElementById("timer");
const restartBtn = document.getElementById("restart-btn");

let cards = [];
let openedCards = [];
let matchedPairs = 0;
let moveCount = 0;
let timer = 0;
let timerId = null;
let lockBoard = false;

function shuffle(array) {
  const copy = [...array];
  for (let i = copy.length - 1; i > 0; i -= 1) {
    const j = Math.floor(Math.random() * (i + 1));
    [copy[i], copy[j]] = [copy[j], copy[i]];
  }
  return copy;
}

function startTimer() {
  if (timerId !== null) return;
  timerId = setInterval(() => {
    timer += 1;
    timerEl.textContent = String(timer);
  }, 1000);
}

function stopTimer() {
  clearInterval(timerId);
  timerId = null;
}

function updateMoves() {
  moveCountEl.textContent = String(moveCount);
}

function checkWin() {
  if (matchedPairs !== symbols.length) return;
  stopTimer();
  statusText.textContent = `Tebrikler! ${moveCount} hamlede ${timer} saniyede kazandınız 🎉`;
}

function closeOpenedCards() {
  openedCards.forEach((card) => {
    card.classList.remove("open");
  });
  openedCards = [];
}

function handleCardClick(event) {
  const card = event.currentTarget;

  if (lockBoard || card.classList.contains("open") || card.classList.contains("matched")) {
    return;
  }

  if (timerId === null) {
    startTimer();
  }

  card.classList.add("open");
  openedCards.push(card);

  if (openedCards.length < 2) {
    statusText.textContent = "İkinci kartı seçin.";
    return;
  }

  moveCount += 1;
  updateMoves();

  const [first, second] = openedCards;

  if (first.dataset.symbol === second.dataset.symbol) {
    first.classList.add("matched");
    second.classList.add("matched");
    matchedPairs += 1;
    openedCards = [];
    statusText.textContent = "Eşleşme bulundu ✅";
    checkWin();
    return;
  }

  lockBoard = true;
  statusText.textContent = "Eşleşmedi, tekrar deneyin.";

  setTimeout(() => {
    closeOpenedCards();
    lockBoard = false;
  }, 700);
}

function createCard(symbol) {
  const card = document.createElement("button");
  card.type = "button";
  card.className = "card";
  card.dataset.symbol = symbol;
  card.textContent = symbol;
  card.setAttribute("aria-label", "Oyun kartı");
  card.addEventListener("click", handleCardClick);
  return card;
}

function initGame() {
  stopTimer();
  timer = 0;
  matchedPairs = 0;
  moveCount = 0;
  openedCards = [];
  lockBoard = false;

  timerEl.textContent = "0";
  updateMoves();

  const deck = shuffle([...symbols, ...symbols]);
  cards = deck.map(createCard);

  board.replaceChildren(...cards);
  statusText.textContent = "Oyuna başlamak için bir kart seçin.";
}

restartBtn.addEventListener("click", initGame);

initGame();
