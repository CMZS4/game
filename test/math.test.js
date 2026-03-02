import test from 'node:test';
import assert from 'node:assert/strict';
import { clamp, rectsOverlap } from '../src/math.js';

test('clamp keeps numbers within boundaries', () => {
  assert.equal(clamp(5, 0, 10), 5);
  assert.equal(clamp(-3, 0, 10), 0);
  assert.equal(clamp(20, 0, 10), 10);
});

test('rectsOverlap detects collisions correctly', () => {
  const player = { x: 20, y: 20, width: 40, height: 40 };
  const near = { x: 45, y: 45, width: 20, height: 20 };
  const far = { x: 120, y: 130, width: 20, height: 20 };

  assert.equal(rectsOverlap(player, near), true);
  assert.equal(rectsOverlap(player, far), false);
});
