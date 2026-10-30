# Rodolfo was alone

**TDPV_MO3D_TP05_Rodriguez_Droz**: Manipulación de Objetos en 3D, Unidad III, Actividad 5.

Versión de un solo nivel inspirada en *Thomas Was Alone*, hecha solo con primitivas y materiales de Unity.

## Objetivo

Llevar a **Rodolfo** (un rectángulo vertical naranja con un triángulo blanco encima) desde el punto de inicio hasta el **marcador blanco** del final del nivel. El nivel termina cuando el cuerpo de Rodolfo queda completamente dentro del marcador.

## Controles

| Acción | Teclas |
|---|---|
| Moverse a la izquierda | Flecha izquierda o `A` |
| Moverse a la derecha | Flecha derecha o `D` |
| Saltar | Barra espaciadora |

## Cómo abrir y ejecutar

1. Abrir **Unity Hub**, elegir *Add project from disk* y seleccionar la carpeta del proyecto.
2. Usar Unity **6000.6.6f1** (Unity 6.6).
3. Abrir la escena `Assets/Scenes/RodolfoWasAlone.unity` y presionar **Play**.

La escena también está configurada como la primera de *Build Settings*.

## Estructura de carpetas

```
Assets/
├── Art/GUI/
│   ├── Materials/   Materiales Unlit (bloques, Rodolfo, indicador, marcador, fondo) y RodolfoPhysics (sin fricción)
│   ├── Prefabs/     Rodolfo.prefab
│   └── Textures/    IndicatorTriangle.png (triángulo invertido con fondo transparente)
├── Scenes/          RodolfoWasAlone.unity (escena del juego), SampleScene.unity (plantilla)
├── Scripts/         RodolfoBehaviour, RodolfoController, CameraFollow, LevelEndMarker
├── Settings/        Configuración de URP de la plantilla
└── Welcome/         Archivos de la plantilla 2D de Unity
```

### Escena `RodolfoWasAlone`

- **Main Camera**: cámara ortográfica con `CameraFollow`.
- **Background**: plano de color y franjas más claras, detrás del nivel (capa de fondo).
- **Level**: todos los bloques negros del nivel (suelo, paredes, escalones, plataformas, torre final).
- **StartPoint**: punto de inicio. **Rodolfo**: instancia del prefab ubicada ahí.
- **LevelEndMarker**: marcador blanco de fin de nivel sobre la torre final.
- **HUD**: textos de controles y mensaje de éxito (oculto hasta terminar).

## Prefab Rodolfo

- `Rodolfo` (raíz): `Rigidbody`, `BoxCollider` de 1 × 2, `RodolfoBehaviour`, `RodolfoController`.
  - `Body`: cubo escalado a 1 × 2 con color naranja.
  - `Indicator`: Quad con la textura del triángulo blanco invertido (recorte por alfa), ubicado encima del cuerpo.

## Implementación

### Movimiento (`RodolfoBehaviour` + `RodolfoController`)

Se sigue el mismo esquema que el TP anterior: el *Behaviour* expone una interfaz pública y el *Controller* lee el teclado (Input System) y la usa.

- El movimiento usa un `Rigidbody` con gravedad. En `FixedUpdate` la velocidad horizontal se acerca a la deseada con una aceleración (sin arranques bruscos).
- El `Rigidbody` congela la posición en Z y todas las rotaciones, así que Rodolfo solo se mueve en el plano lateral y no puede desplazarse en profundidad.
- La detección de colisión es continua, para no atravesar plataformas.
- **Salto**: la velocidad vertical se calcula para alcanzar `jumpHeight` (3,5 unidades). Solo se aplica si Rodolfo está apoyado. Para saberlo se revisa una franja delgada debajo de los pies con `Physics.OverlapBox`. Durante 0,15 s después de saltar se ignora el suelo, así no se puede encadenar un segundo salto al despegar. Un pedido de salto en el aire se descarta, por lo que no hay saltos infinitos.
- Rodolfo usa un material físico sin fricción para no quedar "pegado" a las paredes.

### Cámara (`CameraFollow`)

- La cámara es ortográfica y tiene vista lateral.
- No sigue a Rodolfo todo el tiempo: solo se desplaza cuando Rodolfo queda a menos de `horizontalEdgeMargin` (4 u) de un borde lateral o a menos de `verticalEdgeMargin` (2,5 u) del borde superior o inferior.
- El movimiento se suaviza con `Vector3.SmoothDamp`.
- Los límites del nivel se calculan al iniciar uniendo los `Renderer` de todos los bloques de `Level`. La posición de la cámara se limita para que la vista nunca salga de esos límites.

### Fin de nivel (`LevelEndMarker`)

- El marcador es un rectángulo blanco con las mismas dimensiones que el cuerpo de Rodolfo (1 × 2). Su área es un `BoxCollider` en modo *trigger* del mismo tamaño.
- En `OnTriggerStay` se comparan los límites del cuerpo de Rodolfo con los del marcador. El nivel se completa solo cuando el cuerpo está **completamente contenido**, con una tolerancia de 0,15 u porque ambos miden lo mismo y encajarlos al milímetro sería imposible. Un toque parcial no alcanza.
- Al completarse se desactiva `RodolfoController`, se congela a Rodolfo y se muestra el mensaje de éxito una sola vez.
