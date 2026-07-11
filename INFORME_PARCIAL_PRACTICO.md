# Informe breve - Parcial Practico Shooter 3D

Proyecto creado tomando como base la configuracion del proyecto `Shooter3DDoom` del docente (`Packages` y `ProjectSettings`) y agregando una escena jugable nueva en `Assets/Scenes/ParcialShooter3D.unity`.

## Funcionalidades implementadas

- Movimiento FPS con mouse y teclado mediante `PlayerMotor`.
- Arma con municion limitada, disparo por raycast, recarga con `R`, tiempo de espera y HUD de balas en `PlayerWeapon`.
- HUD de municion con cargador, reserva, balas gastadas y cantidad sumada en la ultima recarga.
- Enemigos con `NavMeshAgent`, persecucion hacia el jugador y ataque a distancia por raycast en `EnemyController`.
- Contador de enemigos y condicion doble de victoria: se gana solo al eliminar todos los enemigos y entrar a la zona verde de meta.
- Game Over con panel, cursor liberado y boton `Reintentar`.
- Feedback de dano con parpadeo rojo de pantalla usando una imagen UI transparente.
- Barra de vida visible: baja cuando el jugador recibe disparos y sube al tomar el botiquin.
- Botiquin opcional funcional: cura al jugador hasta el maximo y desaparece al tocarlo.

## Decisiones

La escena usa geometria simple de Unity para que el proyecto sea portable y no dependa de assets externos. El arma y los enemigos usan raycasts porque es una forma clara de demostrar disparo directo tipo shooter clasico. Los enemigos incluyen `NavMeshAgent` y tambien tienen un movimiento de respaldo si el agente no esta sobre NavMesh, para que la demo siga funcionando incluso si Unity no regenera la navegacion al abrir el proyecto.

Los sonidos se guardan en `Assets/Sounds`. Se dejaron beeps generados como base, pero pueden reemplazarse desde el Inspector en los componentes `PlayerWeapon`, `PlayerHealth` y `EnemyController`.

## Controles

- `WASD`: movimiento
- Mouse: mirar
- Click izquierdo: disparar
- `R`: recargar

La escena principal esta agregada al Build Settings.
