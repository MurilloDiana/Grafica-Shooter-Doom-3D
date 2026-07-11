# Sonidos del juego

Coloca aqui tus archivos de audio (`.wav`, `.mp3` u `.ogg`) si quieres reemplazar los sonidos generados.

En la escena `Assets/Scenes/ParcialShooter3D.unity` puedes asignarlos desde el Inspector:

- `Player > Player Camera > Weapon > PlayerWeapon`
  - `Shoot Clip`: sonido de disparo.
  - `Reload Clip`: sonido de recarga.
- `Player > PlayerHealth`
  - `Hurt Clip`: sonido cuando el jugador recibe dano.
  - `Heal Clip`: sonido cuando usa botiquin.
- Cada enemigo (`Enemy A`, `Enemy B`, `Enemy C`) > `EnemyController`
  - `Shoot Clip`: sonido de disparo enemigo.

El proyecto ahora usa automaticamente estos archivos si existen en esta carpeta:

- `shootSound.mp3`
- `reloadSound.mp3`
- `hurtSound.mp3`
- `HealSound.mp3`

Si cambias los nombres, reasignalos manualmente en el Inspector o actualiza `Assets/Editor/ExamSceneBuilder.cs`.
