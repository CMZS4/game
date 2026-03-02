# Gölge Anahtarları (Unity 2D)

Bu repo, tek sahneli + level prefab sistemi kullanan 2D platformer prototipini içerir.

## İçerik
- Oyuncu: koşu, zıplama, coyote time, jump buffer, wall slide, wall jump.
- Lamba/Spotlight: `Q/E` ile döndürme, mouse ile sürükleme veya oyuncu yakınındayken `E` basılı tutarak taşıma.
- ShadowPlatform: ışıkta **solid**, gölgede **pass-through**.
- Key/Door hedef döngüsü.
- Checkpoint + ölümde respawn.
- Level Manager: `R` reset, `N` sonraki level, `P` önceki level.
- UI: anahtar sayacı.

## Kurulum
1. Unity 2021+ ile projeyi açın (URP gerekmez).
2. Üst menüden **Tools > Golge Anahtarlari > Build Scene & Levels** çalıştırın.
3. `Assets/Scenes/Main.unity` sahnesini açıp Play'e basın.

## Input
- Hareket: `A/D` veya `←/→`
- Zıplama: `Space`
- Lamba döndürme: `Q/E`
- Lamba taşıma: Mouse drag veya lambaya yakınken `E` basılı tut

## Üretilen level prefabları
- Tutorial
- WallJump
- MovingPlatform
- LightGate
- Final
