import { clamp, rectsOverlap } from './math.js';

const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');
const scoreLabel = document.getElementById('scoreLabel');
const bestLabel = document.getElementById('bestLabel');

const OBSTACLE_INTERVAL_MIN = 0.16;
const OBSTACLE_INTERVAL_MAX = 0.42;

const state = {
  running: true,
  paused: false,
  score: 0,
  best: Number(localStorage.getItem('star-dodge-best') ?? 0),
  keys: new Set(),
  player: { x: 380, y: 440, width: 40, height: 40, speed: 420 },
  obstacles: [],
  stars: Array.from({ length: 45 }, () => ({
    x: Math.random() * canvas.width,
    y: Math.random() * canvas.height,
    radius: Math.random() * 1.8 + 0.6,
    speed: Math.random() * 22 + 10,
  })),
  obstacleTimer: 0,
};

bestLabel.textContent = `Best: ${state.best}`;

window.addEventListener('keydown', (event) => {
  const key = event.key.toLowerCase();
  state.keys.add(key);

  if (!state.running && event.code === 'Space') restart();
  if (key === 'p' && state.running) state.paused = !state.paused;
});

window.addEventListener('keyup', (event) => {
  state.keys.delete(event.key.toLowerCase());
});

function getSpawnInterval() {
  const ratio = Math.min(1, state.score / 300);
  return OBSTACLE_INTERVAL_MAX - (OBSTACLE_INTERVAL_MAX - OBSTACLE_INTERVAL_MIN) * ratio;
}

function spawnObstacle() {
  const ratio = Math.min(1, state.score / 360);
  state.obstacles.push({
    x: Math.random() * (canvas.width - 32),
    y: -40,
    width: 32,
    height: 32,
    speed: 160 + Math.random() * 140 + ratio * 180,
  });
}

function update(deltaTime) {
  if (!state.running || state.paused) return;

  const left = state.keys.has('arrowleft') || state.keys.has('a');
  const right = state.keys.has('arrowright') || state.keys.has('d');

  if (left) state.player.x -= state.player.speed * deltaTime;
  if (right) state.player.x += state.player.speed * deltaTime;

  state.player.x = clamp(state.player.x, 0, canvas.width - state.player.width);

  state.obstacleTimer += deltaTime;
  if (state.obstacleTimer >= getSpawnInterval()) {
    spawnObstacle();
    state.obstacleTimer = 0;
  }

  state.obstacles = state.obstacles.filter((obstacle) => {
    obstacle.y += obstacle.speed * deltaTime;
    const hit = rectsOverlap(state.player, obstacle);
    if (hit) {
      state.running = false;
      state.best = Math.max(state.best, Math.floor(state.score));
      localStorage.setItem('star-dodge-best', String(state.best));
      bestLabel.textContent = `Best: ${state.best}`;
    }

    return obstacle.y <= canvas.height + 40 && !hit;
  });

  state.score += deltaTime * 10;
  scoreLabel.textContent = `Score: ${Math.floor(state.score)}`;
}

function draw() {
  ctx.clearRect(0, 0, canvas.width, canvas.height);

  ctx.fillStyle = '#101522';
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  drawStars();

  ctx.fillStyle = '#4fd1c5';
  ctx.fillRect(state.player.x, state.player.y, state.player.width, state.player.height);

  ctx.fillStyle = '#f56565';
  state.obstacles.forEach((obstacle) => {
    ctx.fillRect(obstacle.x, obstacle.y, obstacle.width, obstacle.height);
  });

  if (state.paused) {
    drawOverlay('Paused', 'Press P to continue');
  }

  if (!state.running) {
    drawOverlay('Game Over', 'Press Space to restart');
  }
}

function drawStars() {
  state.stars.forEach((star) => {
    star.y += star.speed / 60;
    if (star.y > canvas.height) {
      star.y = -2;
      star.x = Math.random() * canvas.width;
    }

    ctx.fillStyle = 'rgba(226, 232, 240, 0.7)';
    ctx.beginPath();
    ctx.arc(star.x, star.y, star.radius, 0, Math.PI * 2);
    ctx.fill();
  });
}

function drawOverlay(title, subtitle) {
  ctx.fillStyle = 'rgba(0, 0, 0, 0.65)';
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  ctx.fillStyle = '#edf2f7';
  ctx.font = '700 36px system-ui';
  ctx.textAlign = 'center';
  ctx.fillText(title, canvas.width / 2, canvas.height / 2 - 20);
  ctx.font = '400 20px system-ui';
  ctx.fillText(subtitle, canvas.width / 2, canvas.height / 2 + 24);
}

function restart() {
  state.running = true;
  state.paused = false;
  state.score = 0;
  state.obstacles = [];
  state.obstacleTimer = 0;
  state.player.x = 380;
}

let previous = performance.now();
function frame(now) {
  const deltaTime = Math.min((now - previous) / 1000, 0.032);
  previous = now;

  update(deltaTime);
  draw();
  requestAnimationFrame(frame);
}

requestAnimationFrame(frame);
